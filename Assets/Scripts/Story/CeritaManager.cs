// =============================================================================
// File        : CeritaManager.cs
// Deskripsi   : Pemutar cutscene utama. Membaca CeritaData, menampilkan tiap
//               baris dialog dengan efek ketik, mengatur portrait karakter,
//               latar, audio, dan efek dramatis, lalu kembali ke scene gameplay.
// Tim         : Gethuk Pisang
// =============================================================================

using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DegreeOfSanity.Cerita
{
    public class CeritaManager : MonoBehaviour
    {
        [Header("Sumber Cerita")]
        [Tooltip("Dipakai hanya bila scene dijalankan langsung dari Editor untuk tes.")]
        [SerializeField] private CeritaData ceritaUjiCoba;
        [SerializeField] private DatabaseKarakter databaseKarakter;

        [Header("UI Panel Dialog")]
        [SerializeField] private CanvasGroup panelDialog;
        [SerializeField] private GameObject wadahNama;
        [SerializeField] private TMP_Text teksNama;
        [SerializeField] private TMP_Text teksIsi;
        [SerializeField] private GameObject indikatorLanjut;

        [Header("UI Judul Bab")]
        [SerializeField] private CanvasGroup panelJudulBab;
        [SerializeField] private TMP_Text teksJudulBab;
        [SerializeField] private TMP_Text teksSubJudul;
        [SerializeField] private float durasiJudulBab = 2.2f;

        [Header("Komponen Pendukung")]
        [SerializeField] private EfekAdegan efek;
        [SerializeField] private PenampilKarakter penampilKarakter;
        [SerializeField] private AudioSource sumberBgm;
        [SerializeField] private AudioSource sumberSfx;
        [SerializeField] private GoyangTeks goyangTeks;

        [Header("Pengaturan Ketik")]
        [SerializeField] private float kecepatanKetik = 0.028f;
        [SerializeField] private float kecepatanKetikSkip = 0.004f;
        [SerializeField] private AudioClip sfxKetik;
        [SerializeField] private float intervalSfxKetik = 0.055f;
        [SerializeField] private float jedaModeAuto = 1.6f;

        [Header("Tombol Opsional")]
        [SerializeField] private Button tombolLewati;
        [SerializeField] private Button tombolAuto;
        [SerializeField] private TMP_Text labelTombolAuto;
        [SerializeField] private GameObject panelKonfirmasiLewati;

        [Header("Riwayat Dialog (Opsional)")]
        [SerializeField] private GameObject panelRiwayat;
        [SerializeField] private TMP_Text teksRiwayat;
        [SerializeField] private int maksBarisRiwayat = 40;

        // ---------------------------------------------------------------------
        // STATUS INTERNAL
        // ---------------------------------------------------------------------
        private CeritaData ceritaAktif;
        private bool sedangMengetik;
        private bool mintaLanjut;
        private bool modeAuto;
        private bool modeLewati;
        private bool ceritaBerjalan;
        private Coroutine rutinFadeBgm;
        private readonly List<string> riwayat = new List<string>();

        // ---------------------------------------------------------------------
        // SIKLUS HIDUP
        // ---------------------------------------------------------------------

        private void Awake()
        {
            // Ambil cerita dari jembatan; kalau kosong (tes langsung di Editor)
            // pakai cerita uji coba yang di-assign di Inspector.
            ceritaAktif = JembatanCerita.ceritaYangDimainkan != null
                ? JembatanCerita.ceritaYangDimainkan
                : ceritaUjiCoba;

            if (panelDialog != null)
            {
                panelDialog.alpha = 0f;
                panelDialog.blocksRaycasts = false;
            }
            if (panelJudulBab != null) panelJudulBab.alpha = 0f;
            if (indikatorLanjut != null) indikatorLanjut.SetActive(false);
            if (panelRiwayat != null) panelRiwayat.SetActive(false);
            if (panelKonfirmasiLewati != null) panelKonfirmasiLewati.SetActive(false);

            if (tombolLewati != null) tombolLewati.onClick.AddListener(MintaLewatiCerita);
            if (tombolAuto != null) tombolAuto.onClick.AddListener(ToggleAuto);
        }

        private void Start()
        {
            if (ceritaAktif == null)
            {
                Debug.LogError("[CeritaManager] Tidak ada CeritaData yang bisa dimainkan. " +
                               "Isi field 'Cerita Uji Coba' atau panggil lewat PemicuCerita.");
                return;
            }
            StartCoroutine(MainkanCerita());
        }

        private void Update()
        {
            if (!ceritaBerjalan) return;
            if (InputLanjutDitekan()) mintaLanjut = true;
        }

        // ---------------------------------------------------------------------
        // ALUR UTAMA
        // ---------------------------------------------------------------------

        private IEnumerator MainkanCerita()
        {
            ceritaBerjalan = true;

            // Layar mulai dari gelap total, lalu perlahan terbuka.
            if (efek != null) efek.SetGelapSeketika(true);
            yield return new WaitForSecondsRealtime(0.25f);

            foreach (var adegan in ceritaAktif.daftarAdegan)
            {
                if (adegan == null) continue;
                yield return StartCoroutine(MainkanAdegan(adegan));
            }

            yield return StartCoroutine(SelesaikanCerita());
        }

        private IEnumerator MainkanAdegan(AdeganData adegan)
        {
            // --- Persiapan adegan: gelapkan layar dulu supaya transisi bab rapi
            if (efek != null) yield return StartCoroutine(efek.FadeKeGelap(0.6f));

            SembunyikanPanelDialog();
            if (penampilKarakter != null) penampilKarakter.SembunyikanSemua(true);

            if (adegan.latarAwal != null && efek != null)
                efek.SetLatarSeketika(adegan.latarAwal);

            if (adegan.bgmAwal != null) GantiBgm(adegan.bgmAwal);

            // --- Kartu judul bab
            if (adegan.tampilkanJudulBab && !string.IsNullOrEmpty(adegan.judulBab) && !modeLewati)
                yield return StartCoroutine(TampilkanJudulBab(adegan.judulBab, adegan.subJudul));

            if (efek != null) yield return StartCoroutine(efek.FadeKeTerang(0.7f));

            // --- Jalankan setiap baris
            foreach (var baris in adegan.barisDialog)
            {
                if (baris == null) continue;
                yield return StartCoroutine(TampilkanBaris(baris));
            }
        }

        private IEnumerator TampilkanBaris(BarisDialog baris)
        {
            // 1. Jeda pembuka
            if (baris.jedaSebelum > 0f && !modeLewati)
                yield return new WaitForSecondsRealtime(baris.jedaSebelum);

            // 2. Audio
            if (baris.hentikanBgm) HentikanBgm();
            if (baris.bgmBaru != null) GantiBgm(baris.bgmBaru);
            if (baris.sfx != null && sumberSfx != null) sumberSfx.PlayOneShot(baris.sfx);

            // 3. Ganti latar
            if (baris.latarBaru != null && efek != null)
            {
                float durasi = modeLewati ? 0.05f : baris.durasiTransisi;
                yield return StartCoroutine(efek.GantiLatar(baris.latarBaru, baris.transisiLatar, durasi));
            }

            // 4. Karakter
            if (penampilKarakter != null)
            {
                if (baris.keluarkanSemuaKarakter) penampilKarakter.SembunyikanSemua(modeLewati);

                if (baris.tipe == TipeBaris.Dialog)
                {
                    KarakterData profil = databaseKarakter != null ? databaseKarakter.Cari(baris.namaKarakter) : null;
                    Sprite sprite = baris.portraitOverride != null
                        ? baris.portraitOverride
                        : (profil != null ? profil.AmbilSprite(baris.ekspresi) : null);

                    PosisiPortrait posisi = baris.posisiPortrait;
                    if (posisi == PosisiPortrait.TanpaPortrait && profil != null)
                        posisi = profil.posisiDefault;

                    if (sprite != null && posisi != PosisiPortrait.TanpaPortrait)
                    {
                        penampilKarakter.Tampilkan(sprite, posisi, modeLewati);
                        penampilKarakter.SorotPembicara(posisi);
                    }
                    else
                    {
                        penampilKarakter.SorotPembicara(PosisiPortrait.TanpaPortrait);
                    }
                }
            }

            // 5. Efek kamera
            if (efek != null && baris.efekKamera != EfekKamera.TidakAda && !modeLewati)
                StartCoroutine(efek.JalankanEfek(baris.efekKamera));

            // 6. Baris tipe Perintah tidak menampilkan teks apa pun
            if (baris.tipe == TipeBaris.Perintah)
            {
                if (baris.jedaSesudah > 0f && !modeLewati)
                    yield return new WaitForSecondsRealtime(baris.jedaSesudah);
                yield break;
            }

            // 7. Siapkan panel & nama
            TampilkanPanelDialog();
            AturNama(baris);

            // 8. Efek getar teks (untuk momen panik / sanity rendah)
            if (goyangTeks != null) goyangTeks.kekuatan = baris.goyangTeks;

            // 9. Ketik teks
            float kecepatan = baris.kecepatanKetikOverride > 0f ? baris.kecepatanKetikOverride : kecepatanKetik;
            if (modeLewati) kecepatan = kecepatanKetikSkip;
            yield return StartCoroutine(KetikTeks(baris.isiTeks, kecepatan));

            CatatRiwayat(baris);

            // 10. Tunggu input pemain
            if (!baris.lanjutOtomatis)
                yield return StartCoroutine(TungguLanjut());
            else if (!modeLewati)
                yield return new WaitForSecondsRealtime(0.45f);

            // 11. Jeda penutup
            if (baris.jedaSesudah > 0f && !modeLewati)
                yield return new WaitForSecondsRealtime(baris.jedaSesudah);
        }

        private IEnumerator SelesaikanCerita()
        {
            ceritaBerjalan = false;
            if (goyangTeks != null) goyangTeks.kekuatan = 0f;

            SembunyikanPanelDialog();
            if (penampilKarakter != null) penampilKarakter.SembunyikanSemua(false);
            if (efek != null) yield return StartCoroutine(efek.FadeKeGelap(1.2f));

            yield return StartCoroutine(FadeVolumeBgm(0f, 1f));

            if (ceritaAktif.tandaiSelesai)
                JembatanCerita.TandaiSelesai(ceritaAktif.idCerita);

            string tujuan = !string.IsNullOrEmpty(ceritaAktif.sceneSetelahSelesai)
                ? ceritaAktif.sceneSetelahSelesai
                : JembatanCerita.sceneKembali;

            yield return new WaitForSecondsRealtime(0.2f);
            SceneManager.LoadScene(tujuan);
        }

        // ---------------------------------------------------------------------
        // EFEK KETIK
        // ---------------------------------------------------------------------

        private IEnumerator KetikTeks(string isi, float kecepatan)
        {
            sedangMengetik = true;
            mintaLanjut = false;
            if (indikatorLanjut != null) indikatorLanjut.SetActive(false);

            // Memakai maxVisibleCharacters agar rich text (<b>, <color>) tetap aman.
            teksIsi.text = isi;
            teksIsi.maxVisibleCharacters = 0;
            teksIsi.ForceMeshUpdate();

            int total = teksIsi.textInfo.characterCount;
            int tampil = 0;
            float timer = 0f;
            float timerSfx = 0f;

            while (tampil < total)
            {
                if (mintaLanjut)
                {
                    tampil = total;
                    break;
                }

                timer += Time.unscaledDeltaTime;
                timerSfx += Time.unscaledDeltaTime;

                while (timer >= kecepatan && tampil < total)
                {
                    timer -= kecepatan;
                    tampil++;
                }

                teksIsi.maxVisibleCharacters = tampil;

                if (sfxKetik != null && sumberSfx != null && timerSfx >= intervalSfxKetik && !modeLewati)
                {
                    timerSfx = 0f;
                    sumberSfx.PlayOneShot(sfxKetik, 0.35f);
                }

                yield return null;
            }

            teksIsi.maxVisibleCharacters = total;
            sedangMengetik = false;
            mintaLanjut = false;
            if (indikatorLanjut != null) indikatorLanjut.SetActive(true);
        }

        private IEnumerator TungguLanjut()
        {
            float timerAuto = 0f;
            while (true)
            {
                if (modeLewati) break;
                if (mintaLanjut) break;

                if (modeAuto)
                {
                    timerAuto += Time.unscaledDeltaTime;
                    if (timerAuto >= jedaModeAuto) break;
                }
                yield return null;
            }
            mintaLanjut = false;
            if (indikatorLanjut != null) indikatorLanjut.SetActive(false);
        }

        // ---------------------------------------------------------------------
        // INPUT
        // ---------------------------------------------------------------------

        /// <summary>Dipanggil oleh tombol transparan "Area Klik" yang menutupi layar.</summary>
        public void KlikLanjut()
        {
            if (panelRiwayat != null && panelRiwayat.activeSelf) return;
            mintaLanjut = true;
        }

        private bool InputLanjutDitekan()
        {
            // Jangan proses klik saat panel riwayat / konfirmasi terbuka.
            if (panelRiwayat != null && panelRiwayat.activeSelf) return false;
            if (panelKonfirmasiLewati != null && panelKonfirmasiLewati.activeSelf) return false;

#if ENABLE_INPUT_SYSTEM
            bool spasi = Keyboard.current != null &&
                         (Keyboard.current.spaceKey.wasPressedThisFrame ||
                          Keyboard.current.enterKey.wasPressedThisFrame);
            bool klik = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            return spasi || klik;
#else
            return Input.GetKeyDown(KeyCode.Space) ||
                   Input.GetKeyDown(KeyCode.Return) ||
                   Input.GetMouseButtonDown(0);
#endif
        }

        public void ToggleAuto()
        {
            modeAuto = !modeAuto;
            if (labelTombolAuto != null)
                labelTombolAuto.text = modeAuto ? "AUTO ►" : "AUTO";
        }

        public void MintaLewatiCerita()
        {
            if (panelKonfirmasiLewati != null) panelKonfirmasiLewati.SetActive(true);
            else KonfirmasiLewati();
        }

        /// <summary>Dipasang di tombol "Ya" pada panel konfirmasi lewati.</summary>
        public void KonfirmasiLewati()
        {
            modeLewati = true;
            mintaLanjut = true;
            if (panelKonfirmasiLewati != null) panelKonfirmasiLewati.SetActive(false);
        }

        /// <summary>Dipasang di tombol "Tidak" pada panel konfirmasi lewati.</summary>
        public void BatalLewati()
        {
            if (panelKonfirmasiLewati != null) panelKonfirmasiLewati.SetActive(false);
        }

        public void ToggleRiwayat()
        {
            if (panelRiwayat == null) return;
            bool aktifBaru = !panelRiwayat.activeSelf;
            panelRiwayat.SetActive(aktifBaru);
            if (aktifBaru) SegarkanRiwayat();
        }

        // ---------------------------------------------------------------------
        // UTILITAS UI
        // ---------------------------------------------------------------------

        private void AturNama(BarisDialog baris)
        {
            bool tampilkanNama = baris.tipe == TipeBaris.Dialog && !string.IsNullOrEmpty(baris.namaKarakter);
            if (wadahNama != null) wadahNama.SetActive(tampilkanNama);

            // Narasi memakai gaya miring agar mudah dibedakan dari ucapan karakter.
            if (teksIsi != null)
                teksIsi.fontStyle = baris.tipe == TipeBaris.Narasi ? FontStyles.Italic : FontStyles.Normal;

            if (!tampilkanNama || teksNama == null) return;

            KarakterData profil = databaseKarakter != null ? databaseKarakter.Cari(baris.namaKarakter) : null;
            teksNama.text = profil != null && !string.IsNullOrEmpty(profil.namaTampil)
                ? profil.namaTampil
                : baris.namaKarakter;
            teksNama.color = profil != null ? profil.warnaNama : Color.white;
        }

        private void TampilkanPanelDialog()
        {
            if (panelDialog == null) return;
            panelDialog.alpha = 1f;
            panelDialog.blocksRaycasts = true;
        }

        private void SembunyikanPanelDialog()
        {
            if (panelDialog == null) return;
            panelDialog.alpha = 0f;
            panelDialog.blocksRaycasts = false;
            if (indikatorLanjut != null) indikatorLanjut.SetActive(false);
        }

        private IEnumerator TampilkanJudulBab(string judul, string subJudul)
        {
            if (panelJudulBab == null) yield break;

            if (teksJudulBab != null) teksJudulBab.text = judul;
            if (teksSubJudul != null)
            {
                teksSubJudul.text = subJudul;
                teksSubJudul.gameObject.SetActive(!string.IsNullOrEmpty(subJudul));
            }

            yield return StartCoroutine(FadeCanvasGroup(panelJudulBab, 0f, 1f, 0.6f));
            yield return new WaitForSecondsRealtime(durasiJudulBab);
            yield return StartCoroutine(FadeCanvasGroup(panelJudulBab, 1f, 0f, 0.6f));
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float dari, float ke, float durasi)
        {
            float t = 0f;
            cg.alpha = dari;
            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(dari, ke, t / durasi);
                yield return null;
            }
            cg.alpha = ke;
        }

        // ---------------------------------------------------------------------
        // AUDIO
        // ---------------------------------------------------------------------

        private void GantiBgm(AudioClip klip)
        {
            if (sumberBgm == null || klip == null) return;
            if (sumberBgm.clip == klip && sumberBgm.isPlaying) return;

            // Hentikan fade sebelumnya agar volume tidak diperebutkan dua coroutine.
            if (rutinFadeBgm != null) StopCoroutine(rutinFadeBgm);

            sumberBgm.clip = klip;
            sumberBgm.loop = true;
            sumberBgm.volume = 0f;
            sumberBgm.Play();
            rutinFadeBgm = StartCoroutine(FadeVolumeBgm(1f, 1.2f));
        }

        private void HentikanBgm()
        {
            if (sumberBgm == null) return;
            if (rutinFadeBgm != null) StopCoroutine(rutinFadeBgm);
            rutinFadeBgm = StartCoroutine(FadeVolumeBgm(0f, 0.8f));
        }

        private IEnumerator FadeVolumeBgm(float target, float durasi)
        {
            if (sumberBgm == null) yield break;
            float awal = sumberBgm.volume;
            float t = 0f;
            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                sumberBgm.volume = Mathf.Lerp(awal, target, t / durasi);
                yield return null;
            }
            sumberBgm.volume = target;
            if (Mathf.Approximately(target, 0f)) sumberBgm.Stop();
        }

        // ---------------------------------------------------------------------
        // RIWAYAT
        // ---------------------------------------------------------------------

        private void CatatRiwayat(BarisDialog baris)
        {
            string entri = baris.tipe == TipeBaris.Dialog
                ? $"<b>{baris.namaKarakter}</b>\n{baris.isiTeks}"
                : $"<i>{baris.isiTeks}</i>";

            riwayat.Add(entri);
            if (riwayat.Count > maksBarisRiwayat) riwayat.RemoveAt(0);
        }

        private void SegarkanRiwayat()
        {
            if (teksRiwayat == null) return;
            StringBuilder sb = new StringBuilder();
            foreach (var entri in riwayat)
            {
                sb.AppendLine(entri);
                sb.AppendLine();
            }
            teksRiwayat.text = sb.ToString();
        }
    }
}