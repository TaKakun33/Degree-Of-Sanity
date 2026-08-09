using System.Collections.Generic;
using UnityEngine;

public enum JenisPemicuCerita { MasukRuangan, KlikAnna, KlikLaptop }

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
    [Tooltip("Cuma dipakai kalau jenisPemicu = MasukRuangan. Isi ID ruang: 'LORONG'/'DAPUR'/'KAMAR_ANDREW'/'KAMAR_ANNA'")]
    public string ruangSyarat;
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

    private readonly HashSet<PeristiwaTerjadwal> sudahTerjadi = new HashSet<PeristiwaTerjadwal>();
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
            if (p == null || sudahTerjadi.Contains(p)) continue;
            if (p.jenisPemicu != jenis) continue;
            if (jenis == JenisPemicuCerita.MasukRuangan && p.ruangSyarat != ruangId) continue;

            bool tanggalOk = GameManager.Instance.ApakahSudahLewatTanggal(p.tanggalPemicu, p.bulanPemicu);
            bool jamOk = p.jamMinimal <= 0f || GameManager.Instance.jamSaatIni >= p.jamMinimal;

            if (tanggalOk && jamOk) {
                sudahTerjadi.Add(p);
                MulaiAdegan(p.adeganPertama);
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
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(true);

        cutsceneUI.MainkanAdegan(adeganPrologPertama, () => {
            sedangMemutarAdegan = false;

            if (GameManager.Instance != null) {
                GameManager.Instance.SetJedaWaktu(false);
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
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(true);

        cutsceneUI.MainkanAdegan(adegan, () => {
            Debug.Log("[CeritaManager] Adegan/chain SELESAI - buka kunci waktu & kontrol player."); // --- SEMENTARA ---

            sedangMemutarAdegan = false;
            if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);

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
        sudahTerjadi.Remove(p);
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