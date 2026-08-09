using UnityEngine;

// --- Bagian 1 & Main Event 1: Cicilan Mingguan. Diaktifkan lewat EfekParameterCutscene
// (aktifkanHutang) di adegan ME1_03. Cek otomatis tiap "Senin" (kelipatan 7 hari sejak
// diaktifkan). Klik Laptop buat bayar manual kalau ada uangnya; kalau gak dibayar sampai
// hari itu lewat, otomatis kena telat. ---
public class CicilanManager : MonoBehaviour
{
    public static CicilanManager Instance;

    [Header("TUNABLE")]
    public bool cicilanAktif = false;
    public int nominalCicilan = 200000;
    [Range(0f, 1f)] public float dendaKeterlambatan = 0.1f;
    public float sanityDendaTelat = 8f;
    public float sanityLunas = 3f;
    [Tooltip("TUNABLE: berapa kali gagal bayar BERTURUT-TURUT sebelum Bad Ending 3 terpicu")]
    public int batasGagalBerturutTurut = 3;

    private int hariKeCicilanAktif = -1;
    private int gagalBerturutTurut = 0;
    private bool sudahDicekMingguIni = false;
    private bool cicilanPertamaSudahLunas = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnHariBerganti += CekCicilanMingguan;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnHariBerganti -= CekCicilanMingguan;
    }

    // --- Dipanggil EfekParameterCutscene.aktifkanHutang (ME1_03) ---
    public void AktifkanCicilan()
    {
        cicilanAktif = true;
        hariKeCicilanAktif = GameManager.Instance != null ? GameManager.Instance.HariKeSaatIni : 0;
    }

    void CekCicilanMingguan()
    {
        if (!cicilanAktif || GameManager.Instance == null) return;
        if (!GameManager.Instance.ApakahKelipatan7HariDari(hariKeCicilanAktif)) { sudahDicekMingguIni = false; return; }
        if (sudahDicekMingguIni) return;

        sudahDicekMingguIni = true;

        if (GameManager.Instance.uang >= nominalCicilan) {
            BayarOtomatisKalauCukup();
        } else {
            GagalBayar();
        }
    }

    void BayarOtomatisKalauCukup()
    {
        GameManager.Instance.KurangiUang(nominalCicilan);
        GameManager.Instance.TambahSanity(sanityLunas);
        gagalBerturutTurut = 0;

        if (!cicilanPertamaSudahLunas) {
            cicilanPertamaSudahLunas = true;
            if (ThresholdSkripsi.Instance != null) ThresholdSkripsi.Instance.TandaiSyaratTambahanTerpenuhi();
        }

        if (PenampilBark.Instance != null) PenampilBark.Instance.Tampilkan("Minggu ini aman.");
    }

    void GagalBayar()
    {
        gagalBerturutTurut++;
        nominalCicilan = Mathf.RoundToInt(nominalCicilan * (1f + dendaKeterlambatan));
        GameManager.Instance.KurangiSanity(sanityDendaTelat);

        if (PenampilBark.Instance != null) PenampilBark.Instance.Tampilkan("Denda. Angkanya kecil. Rasanya nggak.");

        if (gagalBerturutTurut >= batasGagalBerturutTurut) {
            GameManager.Instance.PicuBadEndingUang();
        }
    }

    // --- Dipanggil ObjekKlikCerita di Laptop (lewat CeritaManager.CobaMulaiKlikLaptop, ATAU panggil
    // langsung dari UI Laptop kamu sendiri) - bark "belum bisa bayar" kalau uang kurang ---
    public void CobaBayarManual()
    {
        if (!cicilanAktif) return;

        if (GameManager.Instance.uang >= nominalCicilan) {
            GameManager.Instance.KurangiUang(nominalCicilan);
            GameManager.Instance.TambahSanity(sanityLunas);
            gagalBerturutTurut = 0;

            if (!cicilanPertamaSudahLunas) {
                cicilanPertamaSudahLunas = true;
                if (ThresholdSkripsi.Instance != null) ThresholdSkripsi.Instance.TandaiSyaratTambahanTerpenuhi();
            }

            if (PenampilBark.Instance != null) PenampilBark.Instance.Tampilkan("Minggu ini aman.");
        } else {
            if (PenampilBark.Instance != null) PenampilBark.Instance.Tampilkan("Nggak bisa. Kepalaku isinya angka.");
        }
    }
}