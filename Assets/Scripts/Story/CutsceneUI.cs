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
// --- TAMBAHAN: 1 pasangan nama tokoh -> sprite potrait ---
[System.Serializable]
public class PotraitTokoh
{
    [Tooltip("Nama tokoh - HARUS SAMA PERSIS (gak case-sensitive) sama 'Nama Tokoh' di baris dialog, misal 'Andrew', 'Anna', 'Dosen'")]
    public string namaTokoh;
    public Sprite sprite;
}

public class CutsceneUI : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelCutscene;
    [Tooltip("TAMBAHAN: wadah khusus Dialog (potrait+nama+teks) - nyala CUMA pas baris Dialog")]
    public GameObject panelDialog;
    [Tooltip("TAMBAHAN: wadah khusus Narasi & Bisikan (teks doang, gak ada nama/potrait) - nyala CUMA pas baris Narasi/Bisikan")]
    public GameObject panelNarasiBisikan;
    [Tooltip("TAMBAHAN: TextMeshProUGUI di dalam Panel Narasi Bisikan")]
    public TextMeshProUGUI textNarasiBisikan;
    public TextMeshProUGUI textNamaTokoh;
    public TextMeshProUGUI textDialog;
    public GoyangTeks goyangTeks;
    public GameObject portrait;
    [Tooltip("TAMBAHAN: Image di dalam 'Portrait' yang nampilin sprite tokoh - diganti otomatis sesuai Nama Tokoh baris yang lagi tampil")]
    public Image gambarPotrait;
    public Button tombolLanjut;

    [Header("TAMBAHAN: Potrait per Tokoh")]
    [Tooltip("Daftar pasangan Nama Tokoh -> Sprite Potrait. Tambah entri baru buat tiap tokoh yang ada (Andrew, Anna, Dosen, dll)")]
    public List<PotraitTokoh> daftarPotrait = new List<PotraitTokoh>();

    [Header("Transisi Layar Hitam")]
    [Tooltip("Image full-screen, warna hitam, dipakai KHUSUS buat transisi cutscene (beda dari 'Layar Gelap' punya GameManager buat tidur)")]
    public Image layarTransisi;
    public float durasiFade = 0.5f;

    [Header("TAMBAHAN: Fade Putih (opsional, misal momen pingsan)")]
    [Tooltip("Image full-screen TERPISAH, warna PUTIH, alpha 0 dari awal - dipakai KHUSUS pas 'Fade Ke Putih Di Akhir' dicentang")]
    public Image layarPutih;

    [Header("TAMBAHAN: Efek Goyang Kamera (pakai SanityDistortionEffect yang udah ada)")]
    [Tooltip("Drag object yang punya SanityDistortionEffect.cs (biasanya di Canvas HUD)")]
    public SanityDistortionEffect distorsiSanity;
    [Range(0f, 1f)]
    public float intensitasGeterCutscene = 1f;

    [Header("Karakter")]
    [Tooltip("Drag GameObject Anna INTERAKSI (NPC yang bisa diklik pemain sehari-hari, ObjekKlikCerita dkk) - OTOMATIS disembunyikan begitu ada cutscene yang butuh Anna hadir")]
    public Transform annaInteraksiTransform;

    [Header("Gambar Prop (ilustrasi close-up di depan layar - misal Laci/Amplop)")]
    public Image gambarProp;

    [Header("Panel Pilihan 1 (misal ME3 - 2 opsi) - dipisah total dari Panel Pilihan 2")]
    public GameObject panelPilihan1;
    public List<Button> tombolPilihan1;

    [Header("Panel Pilihan 2 (misal ME2 - 3 opsi) - dipisah total dari Panel Pilihan 1")]
    public GameObject panelPilihan2;
    public List<Button> tombolPilihan2;

    private CutsceneSceneSO adeganAktif;
    private int indexBaris;
    private Action selesaiCallback;
    private readonly HashSet<string> flagCerita = new HashSet<string>();
    private bool sedangHitam = false;
    private GameObject annaCeritaAktifSaatIni; // --- TAMBAHAN: sprite Anna Cerita ruangan yang lagi nyala, biar bisa dimatiin pas pindah/kelar ---
    private GameObject andrewCeritaAktifSaatIni; // --- TAMBAHAN: sama, buat sprite Andrew Cerita ---
    // --- TAMBAHAN: nyimpen kondisi flipX ASLI tiap sprite ruangan, dicatat SEKALI pertama kali
    // ketemu - biar "Balik Arah Hadap" selalu flip dari kondisi asli yang KONSISTEN, gak numpuk
    // dari sisa flip adegan sebelumnya ---
    private readonly Dictionary<GameObject, bool> flipXAsliSprite = new Dictionary<GameObject, bool>();
    private RuangTrigger ruangTerakhirDipakai; // --- TAMBAHAN: dipakai buat teleport Andrew asli ke ruangan terakhir begitu chain kelar ---
    private bool gantiHariPendingSetelahChain = false; // --- TAMBAHAN ---

    void Awake()
    {
        if (tombolLanjut) tombolLanjut.onClick.AddListener(LanjutkanBaris);
        if (panelCutscene) panelCutscene.SetActive(false);
        if (panelDialog) panelDialog.SetActive(false); // --- TAMBAHAN ---
        if (panelPilihan1) panelPilihan1.SetActive(false); // --- TAMBAHAN ---
        if (panelPilihan2) panelPilihan2.SetActive(false); // --- TAMBAHAN ---
        if (panelNarasiBisikan) panelNarasiBisikan.SetActive(false); // --- TAMBAHAN ---

        // --- TAMBAHAN: pastiin layar transisi GAK nge-block klik dari awal, apapun kondisinya -
        // sebelumnya ini cuma "kebetulan" ke-matiin lewat fade coroutine begitu Prolog jalan.
        // Kalau Prolog gak jalan (Load Game/balik kerja), gak ada yang pernah matiin ini,
        // dan kalau Raycast Target default-nya nyala di Editor, klik ke lantai/objek keblokir. ---
        if (layarTransisi != null) layarTransisi.raycastTarget = false;

        // --- TAMBAHAN: sama alasannya kayak layarTransisi - pastiin gak nge-block klik dari awal ---
        if (layarPutih != null) layarPutih.raycastTarget = false;

        if (gambarProp) gambarProp.gameObject.SetActive(false);
    }

    public bool ApakahFlagAktif(string nama) => !string.IsNullOrEmpty(nama) && flagCerita.Contains(nama);

    // --- TAMBAHAN: cari sprite potrait yang cocok sama nama tokoh (gak case-sensitive) ---
    Sprite CariPotrait(string namaTokoh)
    {
        if (string.IsNullOrEmpty(namaTokoh) || daftarPotrait == null) return null;

        foreach (var p in daftarPotrait) {
            if (p != null && string.Equals(p.namaTokoh, namaTokoh, System.StringComparison.OrdinalIgnoreCase)) {
                return p.sprite;
            }
        }
        return null;
    }

    // --- TAMBAHAN: dipakai CeritaManager/SaveManager buat simpan/muat flag cerita (misal
    // JANJI_ANNA, AMBIL_TABUNGAN, TEKAD_KUAT) ke save data - biar gak ke-reset begitu Load Game ---
    public List<string> DapatkanFlagCerita() => new List<string>(flagCerita);

    public void MuatFlagCerita(List<string> daftar)
    {
        flagCerita.Clear();
        if (daftar != null) {
            foreach (var f in daftar) flagCerita.Add(f);
        }
    }

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

        // --- Kalau "Lewati Transisi Awal" dicentang: SKIP fade hitamnya doang, tapi posisi/
        // kemunculan karakter (Andrew/Anna) TETAP diterapkan - cuma instan, gak ada animasi
        // yang keliatan. Biar Anna tetap sinkron ke ruangan yang bener walau gak ada fade. ---
        if (adegan.lewatiTransisiAwal) {
            TerapkanPosisiKarakterInstan(adegan);
        } else {
            yield return StartCoroutine(TransisiKeRuangan(adegan));
        }

        if (panelCutscene) panelCutscene.SetActive(true);
        if (goyangTeks) goyangTeks.Matikan();
        if (gambarProp) gambarProp.gameObject.SetActive(false);

        // --- TAMBAHAN: goyang KAMERA (bukan panel) sepanjang adegan kalau dicentang, lewat
        // SanityDistortionEffect yang udah ada - pakai sistem shake yang sama kayak Sanity rendah ---
        if (distorsiSanity != null) {
            distorsiSanity.PaksaAktifSementara(adegan.geterSepanjangAdegan, intensitasGeterCutscene);
        }

        // --- TAMBAHAN: paksa arah hadap Andrew/Anna sesuai adegan ini, kalau diisi ---
        TerapkanArahHadap(adegan);

        LanjutkanBaris();
    }

    // --- TAMBAHAN: flip sprite Andrew/Anna secara RELATIF dari kondisi asli sprite ruangan -
    // "Balik Arah Hadap" dicentang = di-flip, gak dicentang = biarin apa adanya ---
    // "Tidak Diubah" dilewatin (biarin arah apa adanya) ---
    void TerapkanArahHadap(CutsceneSceneSO adegan)
    {
        if (andrewCeritaAktifSaatIni != null) {
            SpriteRenderer sr = andrewCeritaAktifSaatIni.GetComponent<SpriteRenderer>();
            if (sr != null) {
                bool asli = DapatkanFlipXAsli(andrewCeritaAktifSaatIni, sr);
                sr.flipX = adegan.balikArahAndrew ? !asli : asli;
            }
        }

        if (annaCeritaAktifSaatIni != null) {
            SpriteRenderer sr = annaCeritaAktifSaatIni.GetComponent<SpriteRenderer>();
            if (sr != null) {
                bool asli = DapatkanFlipXAsli(annaCeritaAktifSaatIni, sr);
                sr.flipX = adegan.balikArahAnna ? !asli : asli;
            }
        }
    }

    // --- TAMBAHAN: catat flipX ASLI sprite ini SEKALI (pertama kali ketemu), biar toggle
    // "Balik Arah Hadap" selalu konsisten flip dari kondisi awal, gak numpuk ---
    bool DapatkanFlipXAsli(GameObject obj, SpriteRenderer sr)
    {
        if (!flipXAsliSprite.TryGetValue(obj, out bool asli)) {
            asli = sr.flipX;
            flipXAsliSprite[obj] = asli;
        }
        return asli;
    }

    // --- TAMBAHAN: versi TerapkanPosisiKarakter TANPA fade sama sekali - dipanggil kalau
    // "Lewati Transisi Awal" dicentang, biar Anna/Andrew tetap kepasang ke Ruang Id yang bener. ---
    void TerapkanPosisiKarakterInstan(CutsceneSceneSO adegan)
    {
        if (adegan == null || string.IsNullOrEmpty(adegan.ruangId) || adegan.ruangId == "LAYAR_HITAM") return;

        if (RuangTrigger.semuaRuang.TryGetValue(adegan.ruangId, out RuangTrigger ruang)) {
            TeleportKarakter(ruang, adegan.karakterAnnaHadir);
        } else {
            Debug.LogWarning($"[CutsceneUI] Ruang Id '{adegan.ruangId}' gak ketemu di registry RuangTrigger! (Lewati Transisi Awal)");
        }
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
        ruangTerakhirDipakai = ruang; // --- TAMBAHAN: dicatat, dipakai SelesaikanChain() buat teleport Andrew asli ---

        // --- TAMBAHAN: Andrew ASLI (PlayerController) - POSISINYA GAK DISENTUH LAGI SAMA
        // SEKALI, cuma render-nya disembunyikan. Ini nyegah resiko posisi/lantai kacau kalau
        // ruangan cutscene ada di lantai beda dari posisi asli pemain. ---
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player != null) {
            SpriteRenderer srPlayer = player.GetComponent<SpriteRenderer>();
            if (srPlayer != null) srPlayer.enabled = false;
        }

        // --- matiin sprite Andrew Cerita RUANGAN SEBELUMNYA (kalau ada) ---
        if (andrewCeritaAktifSaatIni != null) {
            andrewCeritaAktifSaatIni.SetActive(false);
            andrewCeritaAktifSaatIni = null;
        }

        // --- Andrew Cerita: objek FIXED per-ruangan (pola SAMA PERSIS kayak Anna) - posisinya
        // udah diatur manual di Editor lewat RuangTrigger.spriteAndrewCutscene ---
        if (ruang.spriteAndrewCutscene != null) {
            ruang.spriteAndrewCutscene.SetActive(true);
            andrewCeritaAktifSaatIni = ruang.spriteAndrewCutscene;
        }

        // --- Anna Interaksi (NPC sehari-hari) SELALU disembunyikan begitu ada cutscene aktif ---
        if (annaInteraksiTransform != null) annaInteraksiTransform.gameObject.SetActive(false);

        // --- matiin sprite Anna Cerita RUANGAN SEBELUMNYA (kalau ada), biar gak
        // ada 2 Anna Cerita nyala bareng dari ruangan berbeda ---
        if (annaCeritaAktifSaatIni != null) {
            annaCeritaAktifSaatIni.SetActive(false);
            annaCeritaAktifSaatIni = null;
        }

        // --- Anna Cerita: objek FIXED per-ruangan - posisinya udah diatur manual di Editor
        // lewat RuangTrigger.spriteAnnaCutscene, GAK ADA lagi perhitungan posisi di kode. ---
        Debug.Log($"[CutsceneUI] TeleportKarakter ke ruang '{ruang.ruangId}': annaHadir={annaHadir}, spriteAnnaCutscene={(ruang.spriteAnnaCutscene != null ? ruang.spriteAnnaCutscene.name : "NULL/BELUM DIISI")}"); // --- SEMENTARA ---

        if (annaHadir && ruang.spriteAnnaCutscene != null) {
            ruang.spriteAnnaCutscene.SetActive(true);
            annaCeritaAktifSaatIni = ruang.spriteAnnaCutscene;
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

            int totalMakanan = InventoryManager.Instance != null ? InventoryManager.Instance.TotalMakananDiTas() : 0;
            bool lewatiKarenaAdaMakanan = baris.munculKalauAdaMakananDiTas && totalMakanan <= 0;
            bool lewatiKarenaTidakAdaMakanan = baris.munculKalauTidakAdaMakananDiTas && totalMakanan > 0;

            if (lewatiKarenaSanity || lewatiKarenaFlagAktif || lewatiKarenaFlagTidakAktif || lewatiKarenaAdaMakanan || lewatiKarenaTidakAdaMakanan) {
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

        // --- TAMBAHAN: toggle 2 panel - Dialog buat karakter ngomong, Narasi/Bisikan buat sisanya ---
        bool iniDialog = !iniNarasi && !iniBisikan;
        if (panelDialog) panelDialog.SetActive(iniDialog);
        if (panelNarasiBisikan) panelNarasiBisikan.SetActive(!iniDialog);

        if (portrait) portrait.SetActive(!iniNarasi && !iniBisikan);
        if (textNamaTokoh) textNamaTokoh.text = iniNarasi ? "" : (iniBisikan ? "" : b.namaTokoh);

        // --- TAMBAHAN: ganti sprite potrait sesuai Nama Tokoh baris ini, cuma pas Dialog ---
        if (gambarPotrait != null) {
            if (!iniNarasi && !iniBisikan) {
                Sprite spriteDipakai = CariPotrait(b.namaTokoh);
                if (spriteDipakai != null) {
                    gambarPotrait.sprite = spriteDipakai;
                    gambarPotrait.gameObject.SetActive(true);
                } else {
                    gambarPotrait.gameObject.SetActive(false); // gak ketemu potrait yang cocok
                }
            } else {
                gambarPotrait.gameObject.SetActive(false);
            }
        }

        string teksFinal = b.teks ?? "";
        if (GameManager.Instance != null) teksFinal = teksFinal.Replace("{SKRIPSI}", Mathf.RoundToInt(GameManager.Instance.progresSkripsi).ToString());
        teksFinal = teksFinal.Replace("{MAKANAN}", (InventoryManager.Instance != null ? InventoryManager.Instance.TotalMakananDiTas() : 0).ToString());

        // --- TAMBAHAN: teks ditulis ke text box yang SESUAI jenis barisnya ---
        if (iniDialog) {
            if (textDialog) {
                textDialog.text = teksFinal;
                textDialog.fontStyle = FontStyles.Normal;
            }
        } else {
            if (textNarasiBisikan) {
                textNarasiBisikan.text = teksFinal;
                textNarasiBisikan.fontStyle = iniBisikan ? FontStyles.Italic : FontStyles.Normal;
            }
        }

        if (iniBisikan && goyangTeks) goyangTeks.Aktifkan();

        if (b.objekTampilkan != null) b.objekTampilkan.SetActive(true);
        if (b.objekSembunyikan != null) b.objekSembunyikan.SetActive(false);

        // --- TAMBAHAN: tampilkan/sembunyikan gambar prop DI DEPAN LAYAR (bukan di world) ---
        if (gambarProp != null) {
            if (b.gambarPropUntukDitampilkan != null) {
                Debug.Log($"[CutsceneUI] Gambar Prop diterapkan: '{b.gambarPropUntukDitampilkan.name}' (baris ini)"); // --- SEMENTARA ---
                gambarProp.sprite = b.gambarPropUntukDitampilkan;
                gambarProp.gameObject.SetActive(true);
            } else if (b.sembunyikanGambarProp) {
                gambarProp.gameObject.SetActive(false);
            } else if (gambarProp.gameObject.activeSelf) {
                // --- SEMENTARA: peringatan - baris ini gak isi Gambar Prop DAN gak centang
                // Sembunyikan, tapi Gambar Prop LAGI KELIATAN dari baris sebelumnya - sprite yang
                // nongol sekarang itu SISA baris lama, bukan buat baris ini ---
                Debug.LogWarning($"[CutsceneUI] Baris ini ('{(b.teks?.Length > 30 ? b.teks.Substring(0, 30) : b.teks)}...') gak isi Gambar Prop Untuk Ditampilkan DAN gak centang Sembunyikan Gambar Prop - sprite '{gambarProp.sprite?.name}' yang keliatan sekarang itu SISA dari baris sebelumnya!");
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

            // --- TAMBAHAN: nambah Utang Bank (terpisah dari Uang) ---
            if (e.tambahUtang > 0f) {
                GameManager.Instance.TambahUtang(e.tambahUtang);
            }

            // --- (aktifkanHutang field sengaja gak dipakai lagi - Utang Bank sekarang otomatis
            // aktif begitu Tambah Utang > 0, lihat GameManager.TambahUtang()) ---

            // --- TAMBAHAN: paksa buka Threshold ke-N, terlepas dari progres skripsi ---
            if (e.paksaBukaThresholdKe > 0 && ThresholdSkripsi.Instance != null) {
                ThresholdSkripsi.Instance.PaksaBukaThresholdKe(e.paksaBukaThresholdKe);
            }

            // --- TAMBAHAN: bonus TEKAD_KUAT (ME2_03) ---
            if (e.aktifkanBonusTekadKuat) {
                GameManager.Instance.AktifkanBonusTekadKuat();
            }

            // --- TAMBAHAN: simpen niat ganti hari dulu, JANGAN langsung GantiHari() di sini -
            // biar bisa dieksekusi lewat animasi tidur (ProsesTidur) di SelesaikanChain(), bukan
            // lompat instan. Kalau ini dicentang, "Jam Baru Setelah Adegan" DIABAIKAN - biarin
            // ProsesTidur() yang nentuin jam bangun (jamMulai), biar gak tabrakan urutan. ---
            if (e.gantiHariSetelahAdegan) {
                gantiHariPendingSetelahChain = true;
            } else if (e.jamBaruSetelahAdegan >= 0f) {
                GameManager.Instance.jamSaatIni = e.jamBaruSetelahAdegan;
            }

            if (!string.IsNullOrEmpty(adeganAktif.monologAkhirHari)) {
                GameManager.Instance.monologAkhirHariBerikutnya = adeganAktif.monologAkhirHari;
            }
        }

        if (panelCutscene) panelCutscene.SetActive(false);

        if (adeganAktif.adaPilihan) {
            Debug.Log($"[CutsceneUI] Adegan '{adeganAktif.id}' Ada Pilihan = true, jumlah cabang: {adeganAktif.pilihanCabang?.Count ?? 0}"); // --- SEMENTARA ---
            TampilkanPilihan();
            return;
        }

        // --- TAMBAHAN: matiin goyang kamera begitu adegan ini kelar (scene berikutnya nyalain sendiri kalau perlu) ---
        if (distorsiSanity != null) distorsiSanity.PaksaAktifSementara(false);

        // --- TAMBAHAN: fade ke PUTIH dulu (bukan hitam) sebelum lanjut, kalau dicentang ---
        if (adeganAktif.fadeKePutihDiAkhir) {
            StartCoroutine(FadeKePutihLaluLanjut(adeganAktif.adeganBerikutnya));
            return;
        }

        if (adeganAktif.adeganBerikutnya != null) {
            MulaiSatuAdegan(adeganAktif.adeganBerikutnya);
        } else {
            Debug.Log($"[CutsceneUI] Adegan '{adeganAktif.id}' selesai TANPA pilihan dan TANPA adeganBerikutnya - chain berakhir di sini."); // --- SEMENTARA ---
            SelesaikanChain();
        }
    }

    // --- TAMBAHAN: fade layarPutih 0->1, jeda sebentar, mulai adegan berikutnya (disaranin pakai
    // "Lewati Transisi Awal" biar gak dobel sama fade hitam), lalu fade layarPutih 1->0 nampilinnya ---
    IEnumerator FadeKePutihLaluLanjut(CutsceneSceneSO adeganBerikutnyaLokal)
    {
        if (layarPutih != null) {
            layarPutih.raycastTarget = true;
            float t = 0f;
            while (t < durasiFade) {
                t += Time.deltaTime;
                Color c = layarPutih.color;
                c.a = Mathf.Lerp(0f, 1f, t / durasiFade);
                layarPutih.color = c;
                yield return null;
            }
            Color penuh = layarPutih.color; penuh.a = 1f; layarPutih.color = penuh;
        }

        yield return new WaitForSeconds(0.3f);

        if (adeganBerikutnyaLokal != null) {
            MulaiSatuAdegan(adeganBerikutnyaLokal);
        } else {
            SelesaikanChain();
        }

        if (layarPutih != null) {
            float t = 0f;
            while (t < durasiFade) {
                t += Time.deltaTime;
                Color c = layarPutih.color;
                c.a = Mathf.Lerp(1f, 0f, t / durasiFade);
                layarPutih.color = c;
                yield return null;
            }
            Color kosong = layarPutih.color; kosong.a = 0f; layarPutih.color = kosong;
            layarPutih.raycastTarget = false;
        }
    }

    // --- TAMBAHAN: titik tunggal buat nutup seluruh chain adegan - dipanggil dari 2 tempat
    // (abis baris terakhir tanpa pilihan, ATAU abis pilihan tanpa adeganLanjutan). Nampilin lagi
    // jam yang disembunyikan pas cutscene mulai. ---
    void SelesaikanChain()
    {
        adeganAktif = null;
        if (GameManager.Instance != null) GameManager.Instance.SetTampilanJamAktif(true);

        // --- TAMBAHAN: begitu chain BENERAN kelar, matiin sprite Andrew Cerita ruangan yang
        // lagi aktif, balikin render Andrew asli ke normal (nyala lagi) ---
        if (andrewCeritaAktifSaatIni != null) {
            andrewCeritaAktifSaatIni.SetActive(false);
            andrewCeritaAktifSaatIni = null;
        }
        PlayerController playerSelesai = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (playerSelesai != null) {
            // --- TAMBAHAN: teleport Andrew ASLI ke titikAndrew RUANGAN TERAKHIR yang dipakai
            // cutscene ini (pola sama kayak sistem lama) - biar begitu render-nya nyala lagi,
            // posisinya cocok sama cerita yang barusan ditampilin, bukan balik ke posisi
            // SEBELUM cutscene mulai (yang bisa aja beda ruangan sama sekali). ---
            if (ruangTerakhirDipakai != null && ruangTerakhirDipakai.titikAndrew != null) {
                Vector3 posisiAkhir = ruangTerakhirDipakai.titikAndrew.position;
                posisiAkhir.z = playerSelesai.transform.position.z;

                Rigidbody2D rbAkhir = playerSelesai.GetComponent<Rigidbody2D>();
                if (rbAkhir != null) rbAkhir.position = posisiAkhir;
                else playerSelesai.transform.position = posisiAkhir;
            }

            SpriteRenderer srPlayerSelesai = playerSelesai.GetComponent<SpriteRenderer>();
            if (srPlayerSelesai != null) srPlayerSelesai.enabled = true;
        }
        ruangTerakhirDipakai = null; // --- reset, siap buat chain cutscene berikutnya ---

        // --- begitu chain BENERAN kelar, matiin sprite Anna Cerita ruangan yang lagi
        // aktif dan balikin Anna Interaksi ke normal (nyala lagi) ---
        if (annaCeritaAktifSaatIni != null) {
            annaCeritaAktifSaatIni.SetActive(false);
            annaCeritaAktifSaatIni = null;
        }
        if (annaInteraksiTransform != null) annaInteraksiTransform.gameObject.SetActive(true);

        if (gantiHariPendingSetelahChain) {
            gantiHariPendingSetelahChain = false;
            StartCoroutine(TidurLaluLanjutkanChain());
        } else {
            selesaiCallback?.Invoke();
        }
    }

    // --- TAMBAHAN: mainin animasi tidur (ProsesTidur) yang udah ada di GameManager, BARU
    // setelah itu beneran kelar, lanjutkan callback penutup chain ---
    IEnumerator TidurLaluLanjutkanChain()
    {
        if (GameManager.Instance != null) {
            yield return StartCoroutine(GameManager.Instance.ProsesTidur());
        }
        selesaiCallback?.Invoke();
    }

    void TampilkanPilihan()
    {
        // --- TAMBAHAN: pilih SALAH SATU dari 2 panel yang beneran terpisah total, sesuai
        // yang dipilih di asset adegan ini (Panel Pilihan Dipakai) ---
        GameObject panelDipakai = (adeganAktif.panelPilihanDipakai == PilihanPanelMana.Panel1) ? panelPilihan1 : panelPilihan2;
        List<Button> tombolDipakai = (adeganAktif.panelPilihanDipakai == PilihanPanelMana.Panel1) ? tombolPilihan1 : tombolPilihan2;

        if (panelDipakai == null || tombolDipakai == null || tombolDipakai.Count == 0) {
            Debug.LogError($"[CutsceneUI] TampilkanPilihan() GAGAL - Panel Pilihan {adeganAktif.panelPilihanDipakai} atau tombol-tombolnya belum diisi di CutsceneUI."); // --- SEMENTARA ---
            SelesaikanChain();
            return;
        }

        if (adeganAktif.pilihanCabang.Count > tombolDipakai.Count) {
            Debug.LogError($"[CutsceneUI] Adegan '{adeganAktif.id}' punya {adeganAktif.pilihanCabang.Count} pilihan, tapi Panel Pilihan {adeganAktif.panelPilihanDipakai} cuma ada {tombolDipakai.Count} tombol - tambah tombol lagi di panel itu!"); // --- SEMENTARA ---
        }

        for (int i = 0; i < tombolDipakai.Count; i++) {
            Button tombol = tombolDipakai[i];
            if (tombol == null) continue;

            tombol.onClick.RemoveAllListeners(); // --- bersihin listener lama, biar gak numpuk tiap kali panel ini dibuka ulang ---

            if (i < adeganAktif.pilihanCabang.Count) {
                var cabang = adeganAktif.pilihanCabang[i];
                TextMeshProUGUI label = tombol.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = cabang.labelTombol;

                PilihanCabang cabangLokal = cabang;
                tombol.onClick.AddListener(() => PilihCabang(cabangLokal));
                tombol.gameObject.SetActive(true);
            } else {
                tombol.gameObject.SetActive(false); // --- jaga-jaga kalau Panel 2 kebetulan punya lebih banyak tombol dari yang dibutuhin adegan ini ---
            }
        }

        panelDipakai.SetActive(true);
    }

    void PilihCabang(PilihanCabang cabang)
    {
        if (!string.IsNullOrEmpty(cabang.setFlag)) flagCerita.Add(cabang.setFlag);

        // --- TAMBAHAN: tutup panel yang SESUAI (adeganAktif masih adegan pilihan ini, belum
        // pindah ke adegan lanjutan) ---
        if (adeganAktif != null) {
            GameObject panelDipakai = (adeganAktif.panelPilihanDipakai == PilihanPanelMana.Panel1) ? panelPilihan1 : panelPilihan2;
            if (panelDipakai) panelDipakai.SetActive(false);
        }

        if (cabang.adeganLanjutan != null) {
            MulaiSatuAdegan(cabang.adeganLanjutan);
        } else {
            SelesaikanChain();
        }
    }
}