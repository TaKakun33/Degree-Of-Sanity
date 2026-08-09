using System.Collections.Generic;
using UnityEngine;

public enum JenisPemicuCerita { MasukRuangan, KlikAnna, KlikLaptop, Otomatis }

[System.Serializable]
public class PeristiwaTerjadwal
{
    [Tooltip("Label doang, buat gampang ngenalin di Inspector")]
    public string namaPeristiwa;
    public CutsceneSceneSO adeganPertama;
    [Tooltip("Tanggal syarat (1-31)")]
    public int tanggalPemicu = 1;
    [Tooltip("Bulan syarat: 3=Maret, 4=April, 5=Mei")]
    public int bulanPemicu = 3;
    [Tooltip("Jam minimal (format 24 jam desimal, misal 20 = jam 8 malam). Isi 0 kalau gak ada syarat jam")]
    public float jamMinimal = 0f;
    public JenisPemicuCerita jenisPemicu = JenisPemicuCerita.MasukRuangan;
    [Tooltip("Cuma dipakai kalau jenisPemicu = MasukRuangan. Isi ID ruang: 'LORONG'/'DAPUR'/'KAMAR_ANDREW'/'KAMAR_ANNA'. Kalau jenisPemicu = Otomatis, field ini diabaikan - peristiwa kepicu murni dari tanggal+jam, di ruangan manapun pemain berada.")]
    public string ruangSyarat;
    [Tooltip("OPSIONAL: kalau diisi, begitu syarat terpenuhi, adegan TIDAK langsung mulai - objek ini (misal Amplop) yang diaktifkan dulu. Player harus klik & jalan ke situ, baru cutscene mulai. Kosongkan buat perilaku langsung (dipakai Prolog & event lain yang emang harus auto-mulai).")]
    public PemicuInteraktifCerita objekPemicu;
    [Tooltip("TAMBAHAN: centang kalau peristiwa ini WAJIB kejadian di TANGGAL pemicunya, walau jamnya belum kecapai - kalau pemain nyoba tidur duluan (skip lewat hari itu) SEBELUM peristiwa ini kejadian, tidurnya diblokir & cutscene ini dipaksa jalan dulu.")]
    public bool wajibSebelumTidur = false;
}

// --- Manager utama sistem cerita v3. Ngecek daftar Peristiwa Terjadwal terhadap tanggal+jam
// saat ini dan jenis pemicu (masuk ruangan/klik Anna/klik Laptop), lalu manggil CutsceneUI. ---
public class CeritaManager : MonoBehaviour
{
    public static CeritaManager Instance;

    [Header("Prolog (dimainkan sekali di Game Baru)")]
    public CutsceneSceneSO adeganPrologPertama;

    [Header("Daftar Peristiwa Terjadwal (Main Event 1-3, dst)")]
    public List<PeristiwaTerjadwal> semuaPeristiwa;

    [Header("Referensi UI")]
    public CutsceneUI cutsceneUI;

    // --- FIX: pakai HashSet<string> (nama peristiwa), bukan HashSet<PeristiwaTerjadwal> - biar
    // bisa disimpan ke save data (SaveManager). Objek C# biasa gak bisa disimpan langsung. ---
    private readonly HashSet<string> sudahTerjadi = new HashSet<string>();
    private bool sedangMemutarAdegan = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (SaveManager.slotUntukDiload == -1 && adeganPrologPertama != null) {
            // --- reset jamSaatIni biar gak numpuk dari sesi testing sebelumnya -
            // kalau angkanya udah nyampe batas tidur SEBELUM Prolog mulai, begitu waktu jalan
            // lagi di akhir Prolog, sistem tidur bakal langsung kepicu gak sengaja ---
            if (GameManager.Instance != null) GameManager.Instance.jamSaatIni = GameManager.Instance.jamMulai;

            // --- langsung hitam dari frame PERTAMA, gak nunggu animasi fade -
            // biar gak ada jeda "keliatan sebentar" pas Game Baru dimulai ---
            if (cutsceneUI != null) cutsceneUI.PaksaHitamLangsung();

            MulaiProlog();
        }

        // --- TAMBAHAN: langganan tick waktu, buat ngecek Peristiwa bertipe Otomatis terus-menerus
        // (murni tanggal+jam, gak nunggu event MasukRuangan/Klik apapun) ---
        if (GameManager.Instance != null) GameManager.Instance.OnTickWaktu += CekPeristiwaOtomatis;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnTickWaktu -= CekPeristiwaOtomatis;
    }

    void CekPeristiwaOtomatis(float deltaJam)
    {
        CekPeristiwa(JenisPemicuCerita.Otomatis, null);
    }

    // --- Dipanggil RuangTrigger.cs begitu Player masuk area sebuah ruangan ---
    public void CobaMulaiMasukRuangan(string ruangId)
    {
        CekPeristiwa(JenisPemicuCerita.MasukRuangan, ruangId);
    }

    // --- Dipanggil ObjekKlikCerita.cs di object Anna ---
    public void CobaMulaiKlikAnna()
    {
        CekPeristiwa(JenisPemicuCerita.KlikAnna, null);
    }

    // --- Dipanggil ObjekKlikCerita.cs di object Laptop (juga dipakai bark "Cicilan belum dibayar") ---
    public void CobaMulaiKlikLaptop()
    {
        CekPeristiwa(JenisPemicuCerita.KlikLaptop, null);
    }

    void CekPeristiwa(JenisPemicuCerita jenis, string ruangId)
    {
        if (sedangMemutarAdegan) return;
        if (GameManager.Instance == null || semuaPeristiwa == null) return;

        foreach (var p in semuaPeristiwa) {
            if (p == null || sudahTerjadi.Contains(p.namaPeristiwa)) continue;
            if (p.jenisPemicu != jenis) continue;
            if (jenis == JenisPemicuCerita.MasukRuangan && p.ruangSyarat != ruangId) continue;

            bool tanggalOk = GameManager.Instance.ApakahSudahLewatTanggal(p.tanggalPemicu, p.bulanPemicu);
            bool jamOk = p.jamMinimal <= 0f || GameManager.Instance.jamSaatIni >= p.jamMinimal;

            if (tanggalOk && jamOk) {
                sudahTerjadi.Add(p.namaPeristiwa);

                // --- TAMBAHAN: kalau ada Objek Pemicu, aktifin itu dulu (player harus klik+jalan
                // ke situ), JANGAN langsung mulai cutscene. Prolog/event lain gak diisi ini, jadi
                // tetap langsung mulai seperti biasa. ---
                if (p.objekPemicu != null) {
                    p.objekPemicu.Aktifkan(p.adeganPertama);
                } else {
                    MulaiAdegan(p.adeganPertama);
                }
                break;
            }
        }
    }

    // --- TAMBAHAN: pembungkus KHUSUS Prolog - beda dari MulaiAdegan() biasa (dipakai Main Event)
    // karena ini yang nandain prologSelesai=true DAN maksa semua parameter/tombol ke-reveal
    // begitu chain-nya (P_01...P_06) beneran kelar. ---
    void MulaiProlog()
    {
        if (adeganPrologPertama == null || cutsceneUI == null) return;

        sedangMemutarAdegan = true;
        if (GameManager.Instance != null) {
            GameManager.Instance.SetJedaWaktu(true);
            GameManager.Instance.MulaiCutscene(); // --- TAMBAHAN ---
        }

        // --- TAMBAHAN: kunci kontrol player SELAMA cutscene - sebelumnya cuma di-UNLOCK di
        // akhir, tapi gak pernah di-LOCK di awal, jadi player masih bisa klik/gerak bebas
        // walau lagi ada cutscene aktif ---
        PlayerController playerAwal = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (playerAwal != null) playerAwal.SetMenuStatus(true);

        cutsceneUI.MainkanAdegan(adeganPrologPertama, () => {
            sedangMemutarAdegan = false;

            if (GameManager.Instance != null) {
                GameManager.Instance.SetJedaWaktu(false);
                GameManager.Instance.SelesaiCutscene(); // --- TAMBAHAN ---
                GameManager.Instance.prologSelesai = true; // --- TAMBAHAN: tandai Prolog kelar ---
                GameManager.Instance.TampilkanSemuaParameter(); // --- TAMBAHAN: pastiin semua ke-reveal ---
            }

            PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (player != null) {
                player.SetMenuStatus(false);
                player.KunciKontrol(false);
            }
        });
    }

    void MulaiAdegan(CutsceneSceneSO adegan)
    {
        Debug.Log($"[CeritaManager] MulaiAdegan() TERPANGGIL. adegan={(adegan != null ? adegan.id : "NULL")}, cutsceneUI={(cutsceneUI != null ? "OK" : "NULL")}"); // --- SEMENTARA ---

        if (adegan == null || cutsceneUI == null) return;

        sedangMemutarAdegan = true;
        if (GameManager.Instance != null) {
            GameManager.Instance.SetJedaWaktu(true);
            GameManager.Instance.MulaiCutscene(); // --- TAMBAHAN ---
        }

        // --- TAMBAHAN: kunci kontrol player SELAMA cutscene ---
        PlayerController playerAwal = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (playerAwal != null) playerAwal.SetMenuStatus(true);

        cutsceneUI.MainkanAdegan(adegan, () => {
            Debug.Log("[CeritaManager] Adegan/chain SELESAI - buka kunci waktu & kontrol player."); // --- SEMENTARA ---

            sedangMemutarAdegan = false;
            if (GameManager.Instance != null) {
                GameManager.Instance.SetJedaWaktu(false);
                GameManager.Instance.SelesaiCutscene(); // --- TAMBAHAN ---
            }

            // --- TAMBAHAN: paksa buka kunci kontrol player, jaga-jaga ada yang kesangkut
            // kekunci (isMenuOpen ATAU kontrolDikunci) dari proses cutscene sebelumnya ---
            PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (player != null) {
                player.SetMenuStatus(false);
                player.KunciKontrol(false);
                Debug.Log("[CeritaManager] player.SetMenuStatus(false) & KunciKontrol(false) dipanggil."); // --- SEMENTARA ---
            } else {
                Debug.LogError("[CeritaManager] Gak nemu PlayerController pas mau buka kunci!"); // --- SEMENTARA ---
            }
        });
    }

    public bool ApakahFlagAktif(string nama) => cutsceneUI != null && cutsceneUI.ApakahFlagAktif(nama);

    // --- TAMBAHAN: dipanggil GameManager.CobaMulaiTidur() SEBELUM beneran mulai tidur - cek
    // apa ada peristiwa "Wajib Sebelum Tidur" yang tanggalnya udah kena tapi belum kejadian ---
    public bool ApakahAdaPeristiwaWajibSebelumTidurHariIni()
    {
        if (semuaPeristiwa == null || GameManager.Instance == null) return false;

        foreach (var p in semuaPeristiwa) {
            if (p == null) continue;

            bool sudahDicatatSelesai = sudahTerjadi.Contains(p.namaPeristiwa);
            bool tanggalOk = GameManager.Instance.ApakahSudahLewatTanggal(p.tanggalPemicu, p.bulanPemicu);
            Debug.Log($"[CeritaManager] Cek '{p.namaPeristiwa}': wajibSebelumTidur={p.wajibSebelumTidur}, sudahTerjadi={sudahDicatatSelesai}, tanggalOk={tanggalOk} (tanggal sekarang {GameManager.Instance.tanggal}/{GameManager.Instance.bulan})"); // --- SEMENTARA ---

            if (!p.wajibSebelumTidur || sudahDicatatSelesai) continue;
            if (tanggalOk) return true;
        }
        return false;
    }

    // --- Dipanggil GameManager.CobaMulaiTidur() begitu ApakahAdaPeristiwaWajibSebelumTidurHariIni()
    // return true - PAKSA jalanin peristiwa itu (abaikan syarat jamMinimal-nya) ---
    public void PaksaTriggerPeristiwaWajibSebelumTidur()
    {
        if (semuaPeristiwa == null || GameManager.Instance == null) return;

        foreach (var p in semuaPeristiwa) {
            if (p == null || !p.wajibSebelumTidur || sudahTerjadi.Contains(p.namaPeristiwa)) continue;
            if (!GameManager.Instance.ApakahSudahLewatTanggal(p.tanggalPemicu, p.bulanPemicu)) continue;

            sudahTerjadi.Add(p.namaPeristiwa);
            if (p.objekPemicu != null) {
                p.objekPemicu.Aktifkan(p.adeganPertama);
            } else {
                MulaiAdegan(p.adeganPertama);
            }
            return;
        }
    }

    // --- TAMBAHAN: dipanggil SaveManager.cs buat simpan/muat daftar peristiwa yang udah kejadian,
    // biar gak ke-reset dan ngulang lagi (misal Amplop ME1) begitu Load Game ---
    public List<string> DapatkanPeristiwaSudahTerjadi() => new List<string>(sudahTerjadi);

    public void MuatPeristiwaSudahTerjadi(List<string> daftar)
    {
        sudahTerjadi.Clear();
        if (daftar != null) {
            foreach (var nama in daftar) sudahTerjadi.Add(nama);
        }
    }

    // --- TAMBAHAN: dipanggil PemicuInteraktifCerita.cs begitu player beneran nyampe di objeknya ---
    public void MulaiAdeganLangsung(CutsceneSceneSO adegan)
    {
        MulaiAdegan(adegan);
    }

    // --- Testing: klik kanan komponen ini di Inspector pas Play, biar bisa paksa trigger tanpa nunggu tanggal asli ---
    [Header("Testing")]
    public int indexPeristiwaUntukTest = 0;

    [ContextMenu("TEST: Paksa Trigger Peristiwa Ini")]
    public void PaksaTriggerPeristiwaTest()
    {
        if (semuaPeristiwa == null || indexPeristiwaUntukTest < 0 || indexPeristiwaUntukTest >= semuaPeristiwa.Count) {
            Debug.LogError("[TEST] Index gak valid.");
            return;
        }

        var p = semuaPeristiwa[indexPeristiwaUntukTest];
        sudahTerjadi.Remove(p.namaPeristiwa);
        MulaiAdegan(p.adeganPertama);
    }

    [ContextMenu("TEST: Paksa Trigger Prolog")]
    public void PaksaTriggerPrologTest()
    {
        // --- reset jamSaatIni, sama alasannya kayak di Start() ---
        if (GameManager.Instance != null) GameManager.Instance.jamSaatIni = GameManager.Instance.jamMulai;
        if (cutsceneUI != null) cutsceneUI.PaksaHitamLangsung();
        MulaiProlog();
    }
}