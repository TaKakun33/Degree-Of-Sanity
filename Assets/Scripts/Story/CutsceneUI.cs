using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// --- Nampilin CutsceneScene: narasi/dialog/bisikan satu baris per klik "Lanjut", portrait mati
// buat Narasi, GoyangTeks buat Bisikan, panel pilihan (JembatanCerita) kalau ada.
// TAMBAHAN: transisi layar hitam + TELEPORT karakter (Andrew & Anna) ke titik yang sesuai
// tiap ganti Ruang Id - gaya visual novel, bukan jalan kelihatan (belum ada animasi jalan). ---
public class CutsceneUI : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelCutscene;
    public TextMeshProUGUI textNamaTokoh;
    public TextMeshProUGUI textDialog;
    public GoyangTeks goyangTeks;
    public GameObject portrait;
    public Button tombolLanjut;

    [Header("Transisi Layar Hitam")]
    [Tooltip("Image full-screen, warna hitam, dipakai KHUSUS buat transisi cutscene (beda dari 'Layar Gelap' punya GameManager buat tidur)")]
    public Image layarTransisi;
    public float durasiFade = 0.5f;

    [Header("Karakter")]
    [Tooltip("Drag GameObject Anna di scene ke sini (yang dipakai di seluruh game, bukan yang baru)")]
    public Transform annaTransform;

    [Header("Gambar Prop (ilustrasi close-up di depan layar - misal Laci/Amplop)")]
    public Image gambarProp;

    [Header("Panel Pilihan (JembatanCerita)")]
    public GameObject panelPilihan;
    public Transform wadahTombolPilihan;
    public GameObject prefabTombolPilihan;

    private CutsceneSceneSO adeganAktif;
    private int indexBaris;
    private Action selesaiCallback;
    private readonly HashSet<string> flagCerita = new HashSet<string>();
    private bool sedangHitam = false;

    void Awake()
    {
        if (tombolLanjut) tombolLanjut.onClick.AddListener(LanjutkanBaris);
        if (panelCutscene) panelCutscene.SetActive(false);
        if (panelPilihan) panelPilihan.SetActive(false);

        // --- TAMBAHAN: pastiin layar transisi GAK nge-block klik dari awal, apapun kondisinya -
        // sebelumnya ini cuma "kebetulan" ke-matiin lewat fade coroutine begitu Prolog jalan.
        // Kalau Prolog gak jalan (Load Game/balik kerja), gak ada yang pernah matiin ini,
        // dan kalau Raycast Target default-nya nyala di Editor, klik ke lantai/objek keblokir. ---
        if (layarTransisi != null) layarTransisi.raycastTarget = false;

        if (gambarProp) gambarProp.gameObject.SetActive(false);
    }

    public bool ApakahFlagAktif(string nama) => !string.IsNullOrEmpty(nama) && flagCerita.Contains(nama);

    // --- TAMBAHAN: paksa layar HITAM LANGSUNG (gak lewat animasi fade 0.5 detik) - dipakai
    // CeritaManager.Start() SEBELUM Prolog mulai, biar gak ada jeda "keliatan sebentar" pas
    // Game Baru dimulai. ---
    public void PaksaHitamLangsung()
    {
        if (layarTransisi == null) return;
        Color c = layarTransisi.color;
        c.a = 1f;
        layarTransisi.color = c;
        layarTransisi.raycastTarget = true;
        sedangHitam = true;
    }

    public void MainkanAdegan(CutsceneSceneSO adegan, Action onSemuaChainSelesai)
    {
        selesaiCallback = onSemuaChainSelesai;
        if (GameManager.Instance != null) GameManager.Instance.SetTampilanJamAktif(false); // TAMBAHAN: sembunyiin jam selama cutscene
        MulaiSatuAdegan(adegan);
    }

    void MulaiSatuAdegan(CutsceneSceneSO adegan)
    {
        StartCoroutine(MulaiSatuAdeganCoroutine(adegan));
    }

    IEnumerator MulaiSatuAdeganCoroutine(CutsceneSceneSO adegan)
    {
        adeganAktif = adegan;
        indexBaris = -1;

        // --- TAMBAHAN: transisi layar hitam + teleport karakter ke Ruang Id adegan ini ---
        yield return StartCoroutine(TransisiKeRuangan(adegan));

        if (panelCutscene) panelCutscene.SetActive(true);
        if (goyangTeks) goyangTeks.Matikan();
        if (gambarProp) gambarProp.gameObject.SetActive(false);

        LanjutkanBaris();
    }

    // --- Kalau Ruang Id = "LAYAR_HITAM": fade ke hitam SEKALI, TETAP hitam (gak fade balik).
    // Kalau Ruang Id ketemu di registry RuangTrigger: fade ke hitam (skip kalau udah hitam dari
    // adegan sebelumnya), teleport Andrew/Anna, baru fade balik nampilin ruangan itu. ---
    IEnumerator TransisiKeRuangan(CutsceneSceneSO adegan)
    {
        if (adegan == null || string.IsNullOrEmpty(adegan.ruangId)) yield break;

        bool iniLayarHitam = adegan.ruangId == "LAYAR_HITAM";

        if (!sedangHitam) {
            yield return StartCoroutine(FadeLayarTransisi(0f, 1f));
            sedangHitam = true;
        }

        if (iniLayarHitam) yield break; // tetap hitam, gak ada ruangan buat ditampilin

        if (RuangTrigger.semuaRuang.TryGetValue(adegan.ruangId, out RuangTrigger ruang)) {
            TeleportKarakter(ruang, adegan.karakterAnnaHadir);
        } else {
            Debug.LogWarning($"[CutsceneUI] Ruang Id '{adegan.ruangId}' gak ketemu di registry RuangTrigger!");
        }

        yield return StartCoroutine(FadeLayarTransisi(1f, 0f));
        sedangHitam = false;
    }

    void TeleportKarakter(RuangTrigger ruang, bool annaHadir)
    {
        // --- Andrew (Player) ---
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player != null && ruang.titikAndrew != null) {
            Vector3 posisi = ruang.titikAndrew.position;
            posisi.z = player.transform.position.z;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = posisi;
            else player.transform.position = posisi;
        }

        // --- Anna: cuma dimunculin & dipindah kalau adegan ini butuh dia ---
        if (annaTransform != null) {
            annaTransform.gameObject.SetActive(annaHadir);
            if (annaHadir && ruang.titikAnna != null) {
                Vector3 posisiAnna = ruang.titikAnna.position;
                posisiAnna.z = annaTransform.position.z;
                annaTransform.position = posisiAnna;
            }
        }
    }

    IEnumerator FadeLayarTransisi(float dari, float ke)
    {
        if (layarTransisi == null) yield break;

        // --- Selama transisi lagi berlangsung, blok klik (wajar, layar lagi hitam/nge-fade) ---
        layarTransisi.raycastTarget = true;

        float t = 0f;
        while (t < durasiFade) {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(dari, ke, t / durasiFade);
            Color c = layarTransisi.color;
            c.a = alpha;
            layarTransisi.color = c;
            yield return null;
        }

        Color akhir = layarTransisi.color;
        akhir.a = ke;
        layarTransisi.color = akhir;

        // --- FIX: kalau abis fade IN (transparan total), matiin Raycast Target - biar GameObject
        // yang tetap aktif ini gak terus-terusan nge-block klik pemain walau gak keliatan lagi ---
        layarTransisi.raycastTarget = ke > 0.01f;
    }

    void LanjutkanBaris()
    {
        if (goyangTeks) goyangTeks.Matikan();
        indexBaris++;

        while (adeganAktif.baris != null && indexBaris < adeganAktif.baris.Count) {
            var baris = adeganAktif.baris[indexBaris];

            bool lewatiKarenaSanity = baris.munculKalauSanityDiBawah > 0f && GameManager.Instance != null && GameManager.Instance.sanity >= baris.munculKalauSanityDiBawah;
            bool lewatiKarenaFlagAktif = !string.IsNullOrEmpty(baris.munculKalauFlagAktif) && !ApakahFlagAktif(baris.munculKalauFlagAktif);
            bool lewatiKarenaFlagTidakAktif = !string.IsNullOrEmpty(baris.munculKalauFlagTidakAktif) && ApakahFlagAktif(baris.munculKalauFlagTidakAktif);

            if (lewatiKarenaSanity || lewatiKarenaFlagAktif || lewatiKarenaFlagTidakAktif) {
                indexBaris++;
                continue;
            }
            break;
        }

        if (adeganAktif.baris == null || indexBaris >= adeganAktif.baris.Count) {
            SelesaikanAdeganSaatIni();
            return;
        }

        BarisCutscene b = adeganAktif.baris[indexBaris];
        bool iniNarasi = b.jenis == JenisBarisCutscene.Narasi;
        bool iniBisikan = b.jenis == JenisBarisCutscene.Bisikan;

        if (portrait) portrait.SetActive(!iniNarasi && !iniBisikan);
        if (textNamaTokoh) textNamaTokoh.text = iniNarasi ? "" : (iniBisikan ? "" : b.namaTokoh);

        if (textDialog) {
            textDialog.text = b.teks;
            textDialog.fontStyle = iniBisikan ? FontStyles.Italic : FontStyles.Normal;
        }

        if (iniBisikan && goyangTeks) goyangTeks.Aktifkan();

        if (b.objekTampilkan != null) b.objekTampilkan.SetActive(true);
        if (b.objekSembunyikan != null) b.objekSembunyikan.SetActive(false);

        // --- TAMBAHAN: tampilkan/sembunyikan gambar prop DI DEPAN LAYAR (bukan di world) ---
        if (gambarProp != null) {
            if (b.gambarPropUntukDitampilkan != null) {
                gambarProp.sprite = b.gambarPropUntukDitampilkan;
                gambarProp.gameObject.SetActive(true);
            } else if (b.sembunyikanGambarProp) {
                gambarProp.gameObject.SetActive(false);
            }
        }

        if (!string.IsNullOrEmpty(b.parameterUntukDitampilkan) && GameManager.Instance != null) {
            GameManager.Instance.TampilkanParameter(b.parameterUntukDitampilkan);
        }
    }

    void SelesaikanAdeganSaatIni()
    {
        if (GameManager.Instance != null) {
            var e = adeganAktif.efek;
            if (e.sanityDelta > 0) GameManager.Instance.TambahSanity(e.sanityDelta);
            else if (e.sanityDelta < 0) GameManager.Instance.KurangiSanity(-e.sanityDelta);

            // --- TAMBAHAN: jepit Sanity ke minimal tertentu SETELAH efek di atas (naskah ME2: gak boleh di bawah 15%) ---
            if (e.sanityMinimalSetelahEfek >= 0f) {
                GameManager.Instance.TetapkanSanityMinimal(e.sanityMinimalSetelahEfek);
            }

            if (e.laparDelta > 0) GameManager.Instance.TambahLapar(e.laparDelta);
            else if (e.laparDelta < 0) GameManager.Instance.KurangiLapar(-e.laparDelta);

            if (e.uangDelta > 0) GameManager.Instance.TambahUang(e.uangDelta);
            else if (e.uangDelta < 0) GameManager.Instance.KurangiUang(-e.uangDelta);

            if (e.progresSkripsiDelta != 0) GameManager.Instance.TambahProgresSkripsi(e.progresSkripsiDelta);

            if (e.tambahRoti > 0 && InventoryManager.Instance != null) InventoryManager.Instance.jumlahRoti += e.tambahRoti;

            if (e.aktifkanHutang && CicilanManager.Instance != null) CicilanManager.Instance.AktifkanCicilan();

            // --- TAMBAHAN: paksa buka Threshold ke-N, terlepas dari progres skripsi ---
            if (e.paksaBukaThresholdKe > 0 && ThresholdSkripsi.Instance != null) {
                ThresholdSkripsi.Instance.PaksaBukaThresholdKe(e.paksaBukaThresholdKe);
            }

            // --- TAMBAHAN: bonus TEKAD_KUAT (ME2_03) ---
            if (e.aktifkanBonusTekadKuat) {
                GameManager.Instance.AktifkanBonusTekadKuat();
            }

            // --- TAMBAHAN: paksa jam in-game ke angka tertentu begitu adegan ini kelar ---
            if (e.jamBaruSetelahAdegan >= 0f) {
                GameManager.Instance.jamSaatIni = e.jamBaruSetelahAdegan;
            }

            if (!string.IsNullOrEmpty(adeganAktif.monologAkhirHari)) {
                GameManager.Instance.monologAkhirHariBerikutnya = adeganAktif.monologAkhirHari;
            }
        }

        if (panelCutscene) panelCutscene.SetActive(false);

        if (adeganAktif.adaPilihan) {
            TampilkanPilihan();
            return;
        }

        if (adeganAktif.adeganBerikutnya != null) {
            MulaiSatuAdegan(adeganAktif.adeganBerikutnya);
        } else {
            SelesaikanChain();
        }
    }

    // --- TAMBAHAN: titik tunggal buat nutup seluruh chain adegan - dipanggil dari 2 tempat
    // (abis baris terakhir tanpa pilihan, ATAU abis pilihan tanpa adeganLanjutan). Nampilin lagi
    // jam yang disembunyikan pas cutscene mulai. ---
    void SelesaikanChain()
    {
        adeganAktif = null;
        if (GameManager.Instance != null) GameManager.Instance.SetTampilanJamAktif(true);
        selesaiCallback?.Invoke();
    }

    void TampilkanPilihan()
    {
        if (panelPilihan == null || wadahTombolPilihan == null || prefabTombolPilihan == null) {
            SelesaikanChain();
            return;
        }

        foreach (Transform anak in wadahTombolPilihan) Destroy(anak.gameObject);

        foreach (var cabang in adeganAktif.pilihanCabang) {
            GameObject tombolObj = Instantiate(prefabTombolPilihan, wadahTombolPilihan);
            TextMeshProUGUI label = tombolObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = cabang.labelTombol;

            Button tombol = tombolObj.GetComponent<Button>();
            PilihanCabang cabangLokal = cabang;
            tombol.onClick.AddListener(() => PilihCabang(cabangLokal));
        }

        panelPilihan.SetActive(true);
    }

    void PilihCabang(PilihanCabang cabang)
    {
        if (!string.IsNullOrEmpty(cabang.setFlag)) flagCerita.Add(cabang.setFlag);

        if (panelPilihan) panelPilihan.SetActive(false);

        if (cabang.adeganLanjutan != null) {
            MulaiSatuAdegan(cabang.adeganLanjutan);
        } else {
            SelesaikanChain();
        }
    }
}