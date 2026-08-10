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

    [Header("TAMBAHAN: Fade Putih (opsional, misal momen pingsan)")]
    [Tooltip("Image full-screen TERPISAH, warna PUTIH, alpha 0 dari awal - dipakai KHUSUS pas 'Fade Ke Putih Di Akhir' dicentang")]
    public Image layarPutih;

    [Header("TAMBAHAN: Efek Goyang Kamera (pakai SanityDistortionEffect yang udah ada)")]
    [Tooltip("Drag object yang punya SanityDistortionEffect.cs (biasanya di Canvas HUD)")]
    public SanityDistortionEffect distorsiSanity;
    [Range(0f, 1f)]
    public float intensitasGeterCutscene = 1f;

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
    private bool gantiHariPendingSetelahChain = false; // --- TAMBAHAN ---

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

        // --- TAMBAHAN: sama alasannya kayak layarTransisi - pastiin gak nge-block klik dari awal ---
        if (layarPutih != null) layarPutih.raycastTarget = false;

        if (gambarProp) gambarProp.gameObject.SetActive(false);
    }

    public bool ApakahFlagAktif(string nama) => !string.IsNullOrEmpty(nama) && flagCerita.Contains(nama);

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

        LanjutkanBaris();
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

        if (portrait) portrait.SetActive(!iniNarasi && !iniBisikan);
        if (textNamaTokoh) textNamaTokoh.text = iniNarasi ? "" : (iniBisikan ? "" : b.namaTokoh);

        if (textDialog) {
            string teksFinal = b.teks ?? "";
            if (GameManager.Instance != null) teksFinal = teksFinal.Replace("{SKRIPSI}", Mathf.RoundToInt(GameManager.Instance.progresSkripsi).ToString());
            teksFinal = teksFinal.Replace("{MAKANAN}", (InventoryManager.Instance != null ? InventoryManager.Instance.TotalMakananDiTas() : 0).ToString());
            textDialog.text = teksFinal;
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
        if (panelPilihan == null || wadahTombolPilihan == null || prefabTombolPilihan == null) {
            Debug.LogError($"[CutsceneUI] TampilkanPilihan() GAGAL - ada field kosong: Panel Pilihan={(panelPilihan == null ? "NULL" : "OK")}, Wadah Tombol Pilihan={(wadahTombolPilihan == null ? "NULL" : "OK")}, Prefab Tombol Pilihan={(prefabTombolPilihan == null ? "NULL" : "OK")}"); // --- SEMENTARA ---
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