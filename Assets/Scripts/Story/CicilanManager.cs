using System.Collections.Generic;
using UnityEngine;

// --- Satu entri jadwal cicilan mingguan ---
[System.Serializable]
public class MingguCicilan
{
    public int nomorMinggu;
    public int nominal;
    public bool sudahDibayar;
    public bool sudahTelat;
}

// --- Sistem pembayaran Utang Bank (v4): daftar MINGGU (bukan pilih rencana lagi). Minggu ke-1
// muncul begitu utang aktif (GAK ADA deadline, cuma nge-gate skripsi). Minggu ke-2 dst muncul
// tiap SENIN, harus dibayar sebelum hari MINGGU (maksimal Sabtu), telat kalau enggak. Semua
// minggu (lunas/belum/telat) tetap nangkring di daftar - ditampilkan lewat Scroll View. ---
public class CicilanManager : MonoBehaviour
{
    public static CicilanManager Instance;

    [Header("TUNABLE")]
    [Tooltip("Nominal cicilan tiap minggu - hasil riset: SPP 2 juta, bunga 0,3%/hari (batas OJK 2025), lunas ~8 minggu -> ~292.000/minggu")]
    public int nominalCicilanMingguan = 292000;
    public float sanityDendaTelat = 8f;
    public int batasGagalBerturutTurut = 3;

    [Header("Cutscene (opsional)")]
    public CutsceneSceneSO adeganDenda;

    [Header("Jadwal Cicilan (baca-saja saat runtime, ditampilkan PanelUtangController)")]
    public List<MingguCicilan> daftarMinggu = new List<MingguCicilan>();

    private int nomorMingguBerikutnya = 1;
    private bool sudahDicekJatuhTempoMingguIni = false;
    private int gagalBerturutTurut = 0;
    private bool cicilanPertamaSudahLunas = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnHariBerganti += CekHarian;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnHariBerganti -= CekHarian;
    }

    void CekHarian()
    {
        if (GameManager.Instance == null || GameManager.Instance.utangBank <= 0f) return;

        // --- Minggu PERTAMA muncul begitu utang ada, gak nunggu Senin, GAK PERNAH dicek telat ---
        if (daftarMinggu.Count == 0) {
            TambahMingguBaru();
            return;
        }

        int hariMinggu = GameManager.Instance.HariMingguSaatIni; // 0=Minggu, 1=Senin, ..., 6=Sabtu

        if (hariMinggu == 1) { // Senin - minggu baru muncul
            sudahDicekJatuhTempoMingguIni = false;
            TambahMingguBaru();
        }

        if (hariMinggu == 0 && !sudahDicekJatuhTempoMingguIni) { // Minggu - Sabtu udah lewat
            sudahDicekJatuhTempoMingguIni = true;
            CekMingguTerbaruTelat();
        }
    }

    void TambahMingguBaru()
    {
        daftarMinggu.Add(new MingguCicilan {
            nomorMinggu = nomorMingguBerikutnya,
            nominal = nominalCicilanMingguan,
            sudahDibayar = false,
            sudahTelat = false
        });
        nomorMingguBerikutnya++;
    }

    // --- TAMBAHAN: dipanggil GameManager.TambahUtang() LANGSUNG pas utang pertama kali muncul -
    // biar Minggu ke-1 gak nunggu OnHariBerganti (tidur) dulu buat ada di daftar ---
    public void PastikanMingguPertamaAda()
    {
        if (daftarMinggu.Count == 0 && GameManager.Instance != null && GameManager.Instance.utangBank > 0f) {
            TambahMingguBaru();
        }
    }

    void CekMingguTerbaruTelat()
    {
        // --- Cuma entri PALING BARU yang dicek - minggu ke-1 (index 0) gak pernah masuk sini
        // soalnya daftarMinggu.Count harus >= 2 dulu (minggu ke-1 + minggu berjalan) ---
        if (daftarMinggu.Count < 2) return;

        var entriTerbaru = daftarMinggu[daftarMinggu.Count - 1];
        if (!entriTerbaru.sudahDibayar && !entriTerbaru.sudahTelat) {
            entriTerbaru.sudahTelat = true;
            GagalBayar();
        }
    }

    // --- Dipanggil PanelUtangController pas pemain klik "Bayar" di salah satu baris minggu ---
    public bool BayarMinggu(int index)
    {
        if (index < 0 || index >= daftarMinggu.Count) return false;

        var entri = daftarMinggu[index];
        if (entri.sudahDibayar) return false;
        if (GameManager.Instance.uang < entri.nominal) return false;

        GameManager.Instance.KurangiUang(entri.nominal);
        GameManager.Instance.KurangiUtang(entri.nominal);
        entri.sudahDibayar = true;
        gagalBerturutTurut = 0;

        if (!cicilanPertamaSudahLunas) {
            cicilanPertamaSudahLunas = true;
            if (ThresholdSkripsi.Instance != null) ThresholdSkripsi.Instance.TandaiSyaratTambahanTerpenuhi();
        }

        return true;
    }

    void GagalBayar()
    {
        gagalBerturutTurut++;
        GameManager.Instance.KurangiSanity(sanityDendaTelat);

        if (adeganDenda != null && CeritaManager.Instance != null) {
            CeritaManager.Instance.MulaiAdeganLangsung(adeganDenda);
        }

        if (gagalBerturutTurut >= batasGagalBerturutTurut) {
            GameManager.Instance.PicuBadEndingUang();
        }
    }

    // ================== TAMBAHAN: dipakai SaveManager.cs ==================
    public List<MingguCicilan> DapatkanDaftarMinggu() => daftarMinggu;
    public int DapatkanNomorMingguBerikutnya() => nomorMingguBerikutnya;
    public int DapatkanGagalBerturutTurut() => gagalBerturutTurut;
    public bool DapatkanCicilanPertamaSudahLunas() => cicilanPertamaSudahLunas;

    public void MuatDaftarMinggu(List<MingguCicilan> daftar, int nomorBerikutnya, int gagalBerturut, bool pertamaLunas)
    {
        daftarMinggu = daftar ?? new List<MingguCicilan>();
        nomorMingguBerikutnya = nomorBerikutnya > 0 ? nomorBerikutnya : 1;
        gagalBerturutTurut = gagalBerturut;
        cicilanPertamaSudahLunas = pertamaLunas;
    }
}