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

// --- Sistem pembayaran Utang Bank (v7 - SIMPEL): semua minggu (termasuk Minggu 1) diperlakukan
// SAMA - dicek tiap hari SENIN (ngevaluasi minggu yang barusan lewat). Belum dibayar = Telat,
// TITIK. Begitu status Telat ke-set, notifikasi (Adegan Denda) jalan dulu, terus LANGSUNG
// (gak perlu nunggu berapa kali) Bad Ending 3 kepicu setelah notifikasi itu kelar. ---
public class CicilanManager : MonoBehaviour
{
    public static CicilanManager Instance;

    [Header("TUNABLE")]
    [Tooltip("Nominal cicilan tiap minggu - hasil riset: SPP 2 juta, bunga 0,3%/hari (batas OJK 2025), lunas ~8 minggu -> ~292.000/minggu")]
    public int nominalCicilanMingguan = 292000;
    public float sanityDendaTelat = 8f;

    [Header("Cutscene (dimainkan otomatis begitu status Telat ke-set)")]
    public CutsceneSceneSO adeganDenda;

    [Header("Jadwal Cicilan (baca-saja saat runtime, ditampilkan PanelUtangController)")]
    public List<MingguCicilan> daftarMinggu = new List<MingguCicilan>();

    private int nomorMingguBerikutnya = 1;
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

        // --- Minggu PERTAMA muncul begitu utang ada, gak nunggu Senin ---
        if (daftarMinggu.Count == 0) {
            TambahMingguBaru();
            return;
        }

        // --- Hari SENIN: evaluasi minggu yang BARU AJA lewat (belum dibayar = Telat),
        // SAMA RATA buat semua minggu termasuk Minggu 1 - baru setelah itu minggu baru muncul ---
        if (GameManager.Instance.HariMingguSaatIni == 1) {
            CekMingguTerbaruTelat();
            TambahMingguBaru();
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

    // --- Dipanggil GameManager.TambahUtang() LANGSUNG pas utang pertama kali muncul ---
    public void PastikanMingguPertamaAda()
    {
        if (daftarMinggu.Count == 0 && GameManager.Instance != null && GameManager.Instance.utangBank > 0f) {
            TambahMingguBaru();
        }
    }

    void CekMingguTerbaruTelat()
    {
        if (daftarMinggu.Count == 0) return;

        var entriTerbaru = daftarMinggu[daftarMinggu.Count - 1];
        if (!entriTerbaru.sudahDibayar && !entriTerbaru.sudahTelat) {
            entriTerbaru.sudahTelat = true;
            GagalBayar();
        }
    }

    // --- Titik TUNGGAL pas status Telat ke-set - jalanin notifikasi DULU, begitu itu kelar
    // (kontrol balik normal), LANGSUNG trigger Bad Ending 3, gak perlu itung berapa kali lagi ---
    void GagalBayar()
    {
        GameManager.Instance.KurangiSanity(sanityDendaTelat);

        if (adeganDenda != null && CeritaManager.Instance != null) {
            CeritaManager.Instance.MulaiAdeganLangsung(adeganDenda, () => {
                if (GameManager.Instance != null) GameManager.Instance.PicuBadEndingUang();
            });
        } else {
            GameManager.Instance.PicuBadEndingUang();
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

        if (!cicilanPertamaSudahLunas) {
            cicilanPertamaSudahLunas = true;
            if (ThresholdSkripsi.Instance != null) ThresholdSkripsi.Instance.TandaiSyaratTambahanTerpenuhi();
        }

        return true;
    }

    // ================== TAMBAHAN: dipakai SaveManager.cs ==================
    public List<MingguCicilan> DapatkanDaftarMinggu() => daftarMinggu;
    public int DapatkanNomorMingguBerikutnya() => nomorMingguBerikutnya;
    public bool DapatkanCicilanPertamaSudahLunas() => cicilanPertamaSudahLunas;

    public void MuatDaftarMinggu(List<MingguCicilan> daftar, int nomorBerikutnya, bool pertamaLunas)
    {
        daftarMinggu = daftar ?? new List<MingguCicilan>();
        nomorMingguBerikutnya = nomorBerikutnya > 0 ? nomorBerikutnya : 1;
        cicilanPertamaSudahLunas = pertamaLunas;
    }
}