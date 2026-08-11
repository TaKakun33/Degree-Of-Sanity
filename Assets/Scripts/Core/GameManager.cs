using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// --- Degree of Sanity v3 (Simple) - GameManager dirombak total ngikutin naskah baru.
// SEMUA angka kecepatan/pengaruh di bawah ini TUNABLE lewat Inspector - gak ada yang di-hardcode. ---
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Parameter Status Kelangsungan Hidup")]
    public int uang = 150000;
    [Range(0f, 100f)] public float progresSkripsi = 0f;
    [Range(0f, 100f)] public float lapar = 100f;
    [Range(0f, 100f)] public float sanity = 100f;

    [Header("Sistem Tanggal (Kalender) - GANTI dari sistem 'sisa hari' lama")]
    [Tooltip("Tanggal saat ini, 1-31")]
    public int tanggal = 1;
    [Tooltip("Bulan saat ini: 3=Maret, 4=April, 5=Mei")]
    public int bulan = 3;
    [Tooltip("Tanggal deadline masa studi (naskah: 1 Mei)")]
    public int tanggalDeadline = 1;
    public int bulanDeadline = 5;

    [Header("Siklus Siang & Malam")]
    public float jamMulai = 6f;
    public float jamSaatIni = 6f;
    public float batasTidur = 24f;
    [Tooltip("TUNABLE: kecepatan waktu normal (jam in-game per detik real-time)")]
    public float kecepatanWaktuNormal = 0.5f;
    private bool waktuBerjalan = true;

    [Header("Sistem Tick Waktu (Event-Driven)")]
    public float intervalTick = 0.2f;
    private float akumulatorTick = 0f;
    private float akumulatorJamSejakTick = 0f;
    private float pengaliKecepatanWaktu = 1f;
    private bool prosesTidurAktif = false;
    private bool kopiDigunakanHariIni = false;

    public event System.Action<float> OnTickWaktu;
    public event System.Action<float> OnJamBerubah;
    public event System.Action OnBatasWaktuTercapai;
    public event System.Action OnPermainanBerakhir;
    [Tooltip("Dipicu tiap kali hari berganti - CeritaManager/ThresholdSkripsi/CicilanManager subscribe di sini")]
    public event System.Action OnHariBerganti;

    [Header("Referensi UI")]
    public TextMeshProUGUI textTanggal;
    public TextMeshProUGUI textUang;
    public TextMeshProUGUI textUtang; // --- TAMBAHAN ---
    public TextMeshProUGUI textJamHarian;
    public Slider sliderProgresSkripsi;
    public Slider sliderLapar;
    public Slider sliderSanity;
    public TextMeshProUGUI textMonologAkhirHari;
    public string monologAkhirHariBerikutnya = "";

    [Header("Reveal Parameter (Tutorial Prolog) - GameObject wadah UI tiap parameter, mati default")]
    [Tooltip("Wadah UI Lapar (slider+label) - diaktifkan CutsceneScene P_03")]
    public GameObject uiLapar;
    [Tooltip("Wadah UI Progres Skripsi - diaktifkan CutsceneScene P_04")]
    public GameObject uiProgresSkripsi;
    [Tooltip("Wadah UI Tanggal/Waktu - diaktifkan CutsceneScene P_04")]
    public GameObject uiTanggal;
    [Tooltip("Wadah UI Sanity - diaktifkan CutsceneScene P_04")]
    public GameObject uiSanity;
    [Tooltip("Wadah UI Uang - diaktifkan CutsceneScene P_05")]
    public GameObject uiUang;
    [Tooltip("Panel Inventory (tombol HUD-nya) - diaktifkan CutsceneScene P_03")]
    public GameObject uiTombolInventory;

    [Tooltip("Batas progres skripsi maksimal SAAT INI - ThresholdSkripsi.cs yang ngatur nilai ini")]
    public float batasProgresMaksimalSaatIni = 100f;

    [Tooltip("TAMBAHAN: true begitu Prolog udah pernah kelar (disimpan ke save) - dipakai buat mastiin semua parameter/tombol yang di-reveal Prolog TETAP aktif walau MainScene dimuat ulang (Load Game/balik kerja), gak nyandarin Prolog muter ulang")]
    public bool prologSelesai = false;

    [Header("Transisi Layar")]
    public Image layarGelap;

    [Header("Panel Game")]
    public GameObject panelToko;
    public GameObject panelInventory;
    public GameObject panelMenuKerja;
    public GameObject panelMasak;
    public GameObject playerObj;
    public Transform posisiDepanKasur;

    [Header("Status Saat Tidur/Ganti Hari")]
    [Tooltip("TUNABLE: lapar berkurang tiap ganti hari")]
    public float penguranganLaparSaatTidur = 30f;
    [Tooltip("TAMBAHAN: Sanity naik segini kalau tidur SEBELUM Jam Batas Tidur Awal (misal jam 20)")]
    public float sanityNaikTidurAwal = 5f;
    [Tooltip("TAMBAHAN: Sanity turun segini kalau tidur SETELAH/TEPAT Jam Batas Tidur Awal")]
    public float sanityTurunTidurTerlambat = 5f;
    [Tooltip("TAMBAHAN: jam batas buat nentuin 'tidur awal' vs 'tidur terlambat' (format 24 jam)")]
    public float jamBatasTidurAwal = 20f;
    [Tooltip("TAMBAHAN: Sanity turun tiap hari (di GantiHari()) kalau Progres Skripsi masih di bawah plafon Threshold aktif saat ini - tekanan belum mencapai target")]
    public float sanityTurunBelumCapaiTargetTH = 8f;

    [Header("Ambang Batas Parameter")]
    public float ambangSanityDistorsi = 50f;
    public float ambangLaparKritis = 20f;
    public float pengaliSanitySaatLaparKritis = 2f;

    [Header("4 Ending (naskah v3: Happy + 3 Bad)")]
    public GameObject panelHappyEnding;
    [Tooltip("TAMBAHAN: adegan pertama chain cutscene Happy Ending (misal END_HAPPY_01) - dimuter DULU sebelum Panel Happy Ending (layar akhir statis) ditampilkan. Kosongkan buat perilaku lama (langsung tampil panel, gak ada cutscene).")]
    public CutsceneSceneSO adeganHappyEndingPertama;
    [Tooltip("Bad Ending 1 'Hari Keenam Puluh Dua' - Sanity 0%")]
    public GameObject panelBadEndingSanity;
    [Tooltip("TAMBAHAN: adegan pertama chain cutscene Bad Ending 1 (END_BAD1_01)")]
    public CutsceneSceneSO adeganBadEnding1Pertama;
    [Tooltip("TAMBAHAN: teks LAYAR AKHIR di panelBadEndingSanity - diisi otomatis dengan {SKRIPSI} diganti persen skripsi saat ending terpicu")]
    public TextMeshProUGUI textLayarAkhirBadEnding1;

    [Tooltip("Bad Ending 2 'Nanti Kalau Kakak Inget' - lapar kritis berkepanjangan")]
    public GameObject panelBadEndingLapar;
    [Tooltip("TAMBAHAN: adegan pertama chain cutscene Bad Ending 2 (END_BAD2_01)")]
    public CutsceneSceneSO adeganBadEnding2Pertama;
    [Tooltip("TAMBAHAN: teks LAYAR AKHIR di panelBadEndingLapar")]
    public TextMeshProUGUI textLayarAkhirBadEnding2;

    [Tooltip("Bad Ending 3 'Lemari Bawah' - kehabisan biaya (gagal bayar utang)")]
    public GameObject panelBadEndingUang;
    [Tooltip("TAMBAHAN: adegan pertama Bad Ending 3 (END_BAD3_01)")]
    public CutsceneSceneSO adeganBadEnding3Pertama;
    [Tooltip("TAMBAHAN: teks LAYAR AKHIR di panelBadEndingUang")]
    public TextMeshProUGUI textLayarAkhirBadEnding3;

    [Tooltip("TAMBAHAN - Bad Ending 4 (dulu 'varian B' Bad Ending 3) - kehabisan waktu, sekarang ending berdiri sendiri")]
    public GameObject panelBadEnding4Waktu;
    [Tooltip("TAMBAHAN: adegan pertama Bad Ending 4 (END_BAD4_01)")]
    public CutsceneSceneSO adeganBadEnding4Pertama;
    [Tooltip("TAMBAHAN: teks LAYAR AKHIR di panelBadEnding4Waktu")]
    public TextMeshProUGUI textLayarAkhirBadEnding4;

    private bool endingSudahDipicu = false;

    [Header("TAMBAHAN: Status Cutscene & Bonus Sementara")]
    [Tooltip("True selagi CutsceneUI lagi muter apapun - dipakai buat NUNDA cek Bad Ending 1 sampai kontrol balik ke pemain (naskah ME2: 'Bad Ending 1 tidak boleh terpicu selama cutscene berlangsung')")]
    public bool sedangDalamCutscene = false;
    private int hariDistorsiDimatikanPaksaSisa = 0;
    private int hariSanityFloorSisa = 0;
    private float sanityFloorNilai = 0f;

    [Header("Bad Ending 2: Lapar Kritis Berkepanjangan")]
    [Tooltip("TUNABLE: berapa hari BERTURUT-TURUT lapar kritis sebelum Bad Ending 2 terpicu")]
    public int batasHariLaparKritisBerturutTurut = 3;
    private int hariLaparKritisBerturutTurut = 0;

    [Header("Batasan Minigame Skripsi & Kerja Part Time")]
    private bool skripsiSudahDikerjakanHariIni = false;
    private bool kerjaPartTimeSudahDilakukanHariIni = false;
    // --- TAMBAHAN: sama pola-nya - Mandi & Interaksi Anna cuma ngasih bonus Sanity SEKALI per
    // hari, TAPI aksinya sendiri tetap bisa dilakukan berkali-kali (gak diblokir kayak Skripsi/Kerja) ---
    private bool sudahMandiHariIni = false;
    private bool sudahInteraksiAnnaHariIni = false;

    [Header("Tombol HUD")]
    public GameObject tombolBukaToko;
    public GameObject tombolBukaInventory;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.MuatGame(SaveManager.slotUntukDiload);
        TerapkanHasilKerjaPartTimeJikaAda();

        // --- TAMBAHAN: kalau Prolog udah pernah kelar (dari save ATAU baru balik kerja),
        // pastiin SEMUA parameter/tombol yang di-reveal Prolog TETAP aktif - gak nyandarin
        // Prolog muter ulang (yang emang cuma muter sekali doang di Game Baru) ---
        if (prologSelesai) {
            TampilkanSemuaParameter();
        }

        UpdateUI();
    }

    // --- TAMBAHAN: nyalain SEMUA elemen UI yang biasanya di-reveal satu-satu selama Prolog,
    // sekaligus. Dipanggil di Start() (kalau prologSelesai true) DAN begitu Prolog beneran
    // kelar pertama kalinya (dari CeritaManager). ---
    public void TampilkanSemuaParameter()
    {
        if (uiLapar) uiLapar.SetActive(true);
        if (uiProgresSkripsi) uiProgresSkripsi.SetActive(true);
        if (uiTanggal) uiTanggal.SetActive(true);
        if (uiSanity) uiSanity.SetActive(true);
        if (uiUang) uiUang.SetActive(true);
        if (uiTombolInventory) uiTombolInventory.SetActive(true);
        if (tombolBukaToko) tombolBukaToko.SetActive(true);
    }

    // --- TAMBAHAN: sembunyikan/tampilkan teks jam - dipanggil CutsceneUI begitu cutscene mulai/kelar ---
    public void SetTampilanJamAktif(bool aktif)
    {
        if (textJamHarian) textJamHarian.gameObject.SetActive(aktif);
    }

    // --- TAMBAHAN: sembunyikan/tampilkan tombol Toko/Inventory/Utang - dipakai Main Event & Ending
    // (BUKAN Prolog, itu udah punya sistem reveal-nya sendiri). Pas ditampilkan lagi, Tombol Utang
    // dicek ulang lewat UpdateTombolUtang() biar statusnya bener (gak asal nyala walau utang 0). ---
    public void SembunyikanTombolSaatCutscene(bool sembunyikan)
    {
        if (sembunyikan) {
            if (tombolBukaToko) tombolBukaToko.SetActive(false);
            if (uiTombolInventory) uiTombolInventory.SetActive(false);
            if (tombolUtang) tombolUtang.SetActive(false);
        } else {
            if (tombolBukaToko) tombolBukaToko.SetActive(true);
            if (uiTombolInventory) uiTombolInventory.SetActive(true);
            UpdateTombolUtang();
        }
    }

    // --- TAMBAHAN: sembunyikan/tampilkan parameter Sanity/Lapar/Progres Skripsi - KHUSUS dipakai
    // pas Ending (bukan Main Event biasa) ---
    public void SembunyikanParameterSaatEnding(bool sembunyikan)
    {
        if (uiSanity) uiSanity.SetActive(!sembunyikan);
        if (uiLapar) uiLapar.SetActive(!sembunyikan);
        if (uiProgresSkripsi) uiProgresSkripsi.SetActive(!sembunyikan);
    }

    void TerapkanHasilKerjaPartTimeJikaAda()
    {
        if (!HasilKerjaPartTime.adaHasilPending) return;

        TambahUang(HasilKerjaPartTime.uangDidapat);
        KurangiLapar(HasilKerjaPartTime.laparBerkurang);
        KurangiSanity(HasilKerjaPartTime.sanityBerkurang);
        jamSaatIni += HasilKerjaPartTime.jamYangDilewati;

        HasilKerjaPartTime.Bersihkan();
    }

    void Update()
    {
        if (!waktuBerjalan) return;

        float deltaJam = kecepatanWaktuNormal * pengaliKecepatanWaktu * Time.deltaTime;
        jamSaatIni += deltaJam;
        akumulatorJamSejakTick += deltaJam;

        akumulatorTick += Time.deltaTime;
        if (akumulatorTick >= intervalTick) {
            akumulatorTick = 0f;
            OnTickWaktu?.Invoke(akumulatorJamSejakTick);
            OnJamBerubah?.Invoke(jamSaatIni);
            UpdateUI();
            akumulatorJamSejakTick = 0f;

            // --- TAMBAHAN: safety net - cek Bad Ending 1 (Sanity=0) SECARA INDEPENDEN tiap tick,
            // gak cuma nyandarin ke pengecekan yang nempel di KurangiSanity(). Ini nyegah kasus
            // Sanity nyampe 0% tapi "kesalip" sebelum sempet ke-trigger (misal race timing pas
            // deket-deket jam mau tidur otomatis) - dengan ini, paling telat 0.2 detik terdeteksi. ---
            CekBadEndingSanity();
            CekBadEndingLaparInstant(); // --- TAMBAHAN: safety net yang sama buat Lapar=0 ---
        }

        if (jamSaatIni >= DapatkanBatasTidurEfektif() && !prosesTidurAktif) {
            OnBatasWaktuTercapai?.Invoke();
            CobaMulaiTidur(true); // --- TAMBAHAN: lewat gerbang cek dulu; pingsan=true karena ini kemaleman otomatis ---
        }
    }

    // ================== SISTEM TANGGAL ==================

    int JumlahHariDiBulan(int b)
    {
        if (b == 3) return 31; // Maret
        if (b == 4) return 30; // April
        return 31;              // Mei (gak akan kepakai kalau deadline 1 Mei)
    }

    public string NamaBulan(int b)
    {
        if (b == 3) return "Maret";
        if (b == 4) return "April";
        if (b == 5) return "Mei";
        return "?";
    }

    // --- TAMBAHAN: nama hari dalam minggu, digabung ke depan tanggal ---
    public string NamaHari(int h)
    {
        switch (h) {
            case 0: return "Minggu";
            case 1: return "Senin";
            case 2: return "Selasa";
            case 3: return "Rabu";
            case 4: return "Kamis";
            case 5: return "Jumat";
            case 6: return "Sabtu";
            default: return "?";
        }
    }

    public string TanggalFormatted => $"{NamaHari(HariMingguSaatIni)}, {tanggal} {NamaBulan(bulan)}";

    // --- Hari ke berapa sejak 1 Maret (1 Maret = hari 1) - dipakai internal buat perbandingan tanggal ---
    public int HariKeDariTanggal(int t, int b)
    {
        int hari = t;
        if (b >= 4) hari += 31;
        if (b >= 5) hari += 30;
        return hari;
    }

    public int HariKeSaatIni => HariKeDariTanggal(tanggal, bulan);

    // --- Dipakai CeritaManager buat ngecek apakah tanggal pemicu sebuah peristiwa udah tercapai ---
    public bool ApakahSudahLewatTanggal(int t, int b) => HariKeSaatIni >= HariKeDariTanggal(t, b);
    public bool ApakahTanggalPersis(int t, int b) => tanggal == t && bulan == b;

    void MajukanTanggal()
    {
        tanggal++;
        if (tanggal > JumlahHariDiBulan(bulan)) {
            tanggal = 1;
            bulan++;
        }
    }

    // --- Dipakai CicilanManager versi lama: true kalau hari ini "Senin" relatif ke hari referensi yang dikasih ---
    public bool ApakahKelipatan7HariDari(int hariKeReferensi)
    {
        int selisih = HariKeSaatIni - hariKeReferensi;
        return selisih >= 0 && selisih % 7 == 0;
    }

    // ================== TAMBAHAN: HARI DALAM MINGGU (buat Cicilan versi baru) ==================

    [Header("Hari Dalam Minggu")]
    [Tooltip("Hari dalam minggu pas Hari 1 (1 Maret): 0=Minggu, 1=Senin, 2=Selasa, 3=Rabu, 4=Kamis, 5=Jumat, 6=Sabtu")]
    public int hariMingguSaatHariPertama = 1;

    // --- 0=Minggu, 1=Senin, ..., 6=Sabtu ---
    public int HariMingguSaatIni => ((hariMingguSaatHariPertama + HariKeSaatIni - 1) % 7 + 7) % 7;
    public bool ApakahHariIniSenin => HariMingguSaatIni == 1;
    public bool ApakahHariIniSabtu => HariMingguSaatIni == 6;
    public bool ApakahHariIniMinggu => HariMingguSaatIni == 0;

    // ================== REVEAL PARAMETER (TUTORIAL PROLOG) ==================

    // --- Dipanggil dari efek CutsceneScene - nama: "Lapar"/"ProgresSkripsi"/"Tanggal"/"Sanity"/"Uang"/"Inventory" ---
    public void TampilkanParameter(string nama)
    {
        switch (nama) {
            case "Lapar": if (uiLapar) uiLapar.SetActive(true); break;
            case "ProgresSkripsi": if (uiProgresSkripsi) uiProgresSkripsi.SetActive(true); break;
            case "Tanggal": if (uiTanggal) uiTanggal.SetActive(true); break;
            case "Sanity": if (uiSanity) uiSanity.SetActive(true); break;
            case "Uang": if (uiUang) uiUang.SetActive(true); break;
            case "Inventory": if (uiTombolInventory) uiTombolInventory.SetActive(true); break;
            case "Toko": if (tombolBukaToko) tombolBukaToko.SetActive(true); break;
        }
    }

    // ================== SISTEM WAKTU/TIDUR ==================

    public float DapatkanBatasTidurEfektif() => kopiDigunakanHariIni ? batasTidur + 2f : batasTidur;
    public void GunakanBuffKopiEspresso() { kopiDigunakanHariIni = true; }
    public void SetPengaliKecepatanWaktu(float pengali) { pengaliKecepatanWaktu = pengali; }
    public void ResetPengaliKecepatanWaktu() { pengaliKecepatanWaktu = 1f; }

    void UpdateUI()
    {
        if (textTanggal) textTanggal.text = TanggalFormatted;
        if (textUang) textUang.text = "Rp " + uang.ToString("N0");
        if (textUtang) textUtang.text = "Rp " + Mathf.RoundToInt(utangBank).ToString("N0"); // --- TAMBAHAN ---
        if (textJamHarian) textJamHarian.text = string.Format("{0:00}:{1:00}", (int)jamSaatIni % 24, (int)((jamSaatIni % 1) * 60));
        if (sliderProgresSkripsi) sliderProgresSkripsi.value = progresSkripsi;
        if (sliderLapar) sliderLapar.value = lapar;
        if (sliderSanity) sliderSanity.value = sanity;
    }

    public void SetJedaWaktu(bool jeda) { waktuBerjalan = !jeda; }

    public bool SedangDistorsi => hariDistorsiDimatikanPaksaSisa <= 0 && sanity < ambangSanityDistorsi;
    public bool SedangKelaparan => lapar < ambangLaparKritis;

    public bool BisaKerjakanSkripsiHariIni => !skripsiSudahDikerjakanHariIni;
    public void TandaiSkripsiSudahDikerjakan() { skripsiSudahDikerjakanHariIni = true; }
    public bool SkripsiSudahDikerjakanHariIni { get => skripsiSudahDikerjakanHariIni; set => skripsiSudahDikerjakanHariIni = value; }

    public bool BisaKerjaPartTimeHariIni => !kerjaPartTimeSudahDilakukanHariIni;
    public void TandaiKerjaPartTimeSudahDilakukan() { kerjaPartTimeSudahDilakukanHariIni = true; }
    public bool KerjaPartTimeSudahDilakukanHariIni { get => kerjaPartTimeSudahDilakukanHariIni; set => kerjaPartTimeSudahDilakukanHariIni = value; }

    // --- TAMBAHAN: accessor buat flag Mandi & Interaksi Anna ---
    public bool SudahMandiHariIni { get => sudahMandiHariIni; set => sudahMandiHariIni = value; }
    public void TandaiSudahMandiHariIni() { sudahMandiHariIni = true; }
    public bool SudahInteraksiAnnaHariIni { get => sudahInteraksiAnnaHariIni; set => sudahInteraksiAnnaHariIni = value; }
    public void TandaiSudahInteraksiAnnaHariIni() { sudahInteraksiAnnaHariIni = true; }

    public void SetTombolHUDAktif(bool aktif)
    {
        if (tombolBukaToko) tombolBukaToko.SetActive(aktif);
        if (tombolBukaInventory) tombolBukaInventory.SetActive(aktif);
    }

    // ================== PARAMETER: SANITY / LAPAR / UANG / SKRIPSI ==================

    public void KurangiSanity(float jumlah)
    {
        float pengali = SedangKelaparan ? pengaliSanitySaatLaparKritis : 1f;
        float floor = hariSanityFloorSisa > 0 ? sanityFloorNilai : 0f; // --- TAMBAHAN: floor sementara dari bonus TEKAD_KUAT ---
        sanity = Mathf.Clamp(sanity - (jumlah * pengali), floor, 100f);
        UpdateUI();
        CekBadEndingSanity();
    }

    // --- TAMBAHAN: paksa Sanity gak jatuh di bawah angka ini, TAPI cuma naikin (jepit ke atas) -
    // gak narik turun kalau Sanity udah lebih tinggi. Dipakai buat "penalti ME2 gak boleh di bawah 15%". ---
    public void TetapkanSanityMinimal(float minimal)
    {
        if (sanity < minimal) {
            sanity = minimal;
            UpdateUI();
        }
    }

    // --- TAMBAHAN: bonus TEKAD_KUAT (ME2_03, kalau pilih "Jujur") ---
    public void AktifkanBonusTekadKuat()
    {
        hariDistorsiDimatikanPaksaSisa = 1;
        hariSanityFloorSisa = 3;
        sanityFloorNilai = 10f;
    }

    public void TambahSanity(float jumlah)
    {
        sanity = Mathf.Clamp(sanity + jumlah, 0f, 100f);
        UpdateUI();
    }

    public void KurangiLapar(float jumlah)
    {
        lapar = Mathf.Clamp(lapar - jumlah, 0f, 100f);
        UpdateUI();
        CekBadEndingLaparInstant(); // --- TAMBAHAN: mirip CekBadEndingSanity(), trigger instan pas Lapar=0 ---
    }

    // --- TAMBAHAN: sama polanya kayak CekBadEndingSanity() - langsung trigger begitu Lapar
    // nyampe 0, gak perlu nunggu streak 3 hari berturut-turut lagi (itu tetap ada sebagai
    // jaring pengaman tambahan, gak saya hapus, tapi ini yang bakal kena duluan biasanya) ---
    void CekBadEndingLaparInstant()
    {
        if (endingSudahDipicu) return;
        if (sedangDalamCutscene) return;
        if (lapar <= 0f) TampilkanBadEndingLapar();
    }

    public void TambahLapar(float jumlah)
    {
        lapar = Mathf.Clamp(lapar + jumlah, 0f, 100f);
        UpdateUI();
    }

    public void Makan(float jumlahLaparDipulihkan, float jumlahSanityDipulihkan = 0f)
    {
        TambahLapar(jumlahLaparDipulihkan);
        if (jumlahSanityDipulihkan > 0f) TambahSanity(jumlahSanityDipulihkan);
    }

    [Header("TAMBAHAN: Parameter Utang Bank (terpisah dari Uang, gak pernah bikin Uang minus)")]
    [Tooltip("Total utang yang masih harus dibayar - naik dari efek pinjaman (CutsceneScene), berkurang dari pembayaran cicilan")]
    public float utangBank = 0f;
    [Tooltip("TUNABLE: bunga harian yang nambah ke Utang Bank tiap hari (0.003 = 0,3% - sesuai batas resmi OJK 2025 buat pinjol konsumtif tenor <6 bulan)")]
    [Range(0f, 0.02f)] public float bungaHarianUtang = 0.003f;
    [Tooltip("Tombol Utang di HUD - otomatis nyala kalau Utang Bank > 0, mati kalau lunas")]
    public GameObject tombolUtang;

    public void TambahUtang(float jumlah)
    {
        utangBank += jumlah;
        UpdateUI();
        UpdateTombolUtang();

        // --- TAMBAHAN: langsung munculin "Minggu ke-1" di jadwal cicilan, gak nunggu tidur dulu ---
        if (CicilanManager.Instance != null) CicilanManager.Instance.PastikanMingguPertamaAda();
    }

    public void KurangiUtang(float jumlah)
    {
        utangBank = Mathf.Max(0f, utangBank - jumlah);
        UpdateUI();
        UpdateTombolUtang();
    }

    public void UpdateTombolUtang()
    {
        if (tombolUtang) tombolUtang.SetActive(utangBank > 0f);
    }

    public void KurangiUang(int jumlah)
    {
        // --- REVISI: balik di-clamp ke 0 lagi - sekarang utang punya parameter sendiri
        // (Utang Bank), jadi Uang gak perlu lagi merangkap jadi representasi utang minus ---
        uang = Mathf.Max(0, uang - jumlah);
        UpdateUI();
    }

    public void TambahUang(int jumlah)
    {
        uang = Mathf.Max(0, uang + jumlah);
        UpdateUI();
    }

    public void TambahProgresSkripsi(float jumlah)
    {
        progresSkripsi = Mathf.Clamp(progresSkripsi + jumlah, 0f, batasProgresMaksimalSaatIni);
        UpdateUI();
    }

    // ================== ENDING (naskah v3: Happy + 3 Bad) ==================

    void CekBadEndingSanity()
    {
        if (endingSudahDipicu) return;
        if (sedangDalamCutscene) return; // --- TAMBAHAN: jangan cek selama cutscene, tunda dulu ---
        if (sanity <= 0f) TampilkanBadEndingSanity();
    }

    // --- TAMBAHAN: dipanggil CeritaManager pas cutscene MULAI ---
    public void MulaiCutscene()
    {
        sedangDalamCutscene = true;
    }

    // --- TAMBAHAN: dipanggil CeritaManager begitu cutscene BENERAN kelar - buka gerbang lagi
    // DAN re-cek kondisi ending yang mungkin sempet ketunda selama cutscene tadi ---
    public void SelesaiCutscene()
    {
        sedangDalamCutscene = false;
        CekBadEndingSanity();
    }

    // --- Bad Ending 3 "Lemari Bawah" - kehabisan biaya. Dipanggil CicilanManager begitu
    // uang habis / cicilan gagal berulang. BERDIRI SENDIRI, gak ada varian lain lagi. ---
    public void PicuBadEndingUang()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;

        if (CeritaManager.Instance != null && adeganBadEnding3Pertama != null) {
            CeritaManager.Instance.MulaiEndingChain(adeganBadEnding3Pertama, TampilkanLayarAkhirBadEnding3);
        } else {
            TampilkanLayarAkhirBadEnding3();
        }
    }

    // --- Dipanggil CeritaManager begitu chain END_BAD3_01->02 kelar ---
    public void TampilkanLayarAkhirBadEnding3()
    {
        Debug.Log("Bad Ending 3 'Lemari Bawah' (kehabisan biaya) dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelBadEndingUang) panelBadEndingUang.SetActive(true);

        if (textLayarAkhirBadEnding3) {
            textLayarAkhirBadEnding3.text = "Bukan angkanya yang berat.\nTapi minggu yang terus datang, nagih, tanpa pernah nunggu.";
        }
    }

    // --- TAMBAHAN - Bad Ending 4, BERDIRI SENDIRI (dulu "varian B") - kehabisan waktu.
    // Dipanggil GantiHari() begitu hari ke-61 lewat dengan Skripsi < 100%. ---
    public void PicuBadEnding4Waktu()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;

        if (CeritaManager.Instance != null && adeganBadEnding4Pertama != null) {
            CeritaManager.Instance.MulaiEndingChain(adeganBadEnding4Pertama, TampilkanLayarAkhirBadEnding4);
        } else {
            TampilkanLayarAkhirBadEnding4();
        }
    }

    // --- Dipanggil CeritaManager begitu chain END_BAD4_01->02 kelar ---
    public void TampilkanLayarAkhirBadEnding4()
    {
        Debug.Log("Bad Ending 4 (kehabisan waktu) dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelBadEnding4Waktu) panelBadEnding4Waktu.SetActive(true);

        if (textLayarAkhirBadEnding4) {
            textLayarAkhirBadEnding4.text = $"Skripsi Andrew berhenti di {Mathf.RoundToInt(progresSkripsi)}%.\nia nggak berhenti ngerjain melainkan waktunya aja yang berhenti duluan.";
        }
    }

    void TampilkanBadEndingSanity()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;

        if (CeritaManager.Instance != null && adeganBadEnding1Pertama != null) {
            CeritaManager.Instance.MulaiEndingChain(adeganBadEnding1Pertama, TampilkanLayarAkhirBadEnding1);
        } else {
            TampilkanLayarAkhirBadEnding1();
        }
    }

    // --- TAMBAHAN: dipanggil CeritaManager begitu chain END_BAD1_01->02 kelar ---
    void TampilkanLayarAkhirBadEnding1()
    {
        Debug.Log("Bad Ending 1 'Hari Keenam Puluh Dua' dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelBadEndingSanity) panelBadEndingSanity.SetActive(true);

        if (textLayarAkhirBadEnding1) {
            textLayarAkhirBadEnding1.text = $"Skripsi Andrew berhenti di {Mathf.RoundToInt(progresSkripsi)}%.\nBukan karena dia malas tetapi karena nggak ada yang nanya lebih awal.";
        }
    }

    void TampilkanBadEndingLapar()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;

        if (CeritaManager.Instance != null && adeganBadEnding2Pertama != null) {
            CeritaManager.Instance.MulaiEndingChain(adeganBadEnding2Pertama, TampilkanLayarAkhirBadEnding2);
        } else {
            TampilkanLayarAkhirBadEnding2();
        }
    }

    // --- TAMBAHAN: dipanggil CeritaManager begitu chain END_BAD2_01->02 kelar ---
    void TampilkanLayarAkhirBadEnding2()
    {
        Debug.Log("Bad Ending 2 'Nanti Kalau Kakak Inget' dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelBadEndingLapar) panelBadEndingLapar.SetActive(true);

        if (textLayarAkhirBadEnding2) {
            textLayarAkhirBadEnding2.text = "Badan nagih lebih sabar daripada bank.\nTapi tetep nagih.";
        }
    }

    // --- TAMBAHAN: dijadiin public + diganti nama, biar bisa dipanggil CeritaManager begitu
    // chain cutscene Happy Ending BENERAN kelar (dulu private, namanya TampilkanHappyEnding()) ---
    public void TampilkanLayarAkhirHappyEnding()
    {
        Debug.Log("Happy Ending 'Pulang' dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelHappyEnding) panelHappyEnding.SetActive(true);
    }

    // --- Evaluasi Happy Ending: skripsi 100% SEBELUM deadline, gak kena Bad Ending manapun ---
    void CekHappyEnding()
    {
        if (endingSudahDipicu) return;
        if (progresSkripsi < 100f) return;

        endingSudahDipicu = true; // --- ditandai DI SINI, sebelum cutscene mulai, biar gak ke-trigger dobel ---

        // --- TAMBAHAN: kalau Adegan Happy Ending Pertama udah diisi, muter cutscene-nya dulu -
        // baru munculin layar akhir begitu chain-nya kelar. Kalau belum diisi, fallback ke
        // perilaku lama (langsung tampil panel). ---
        if (CeritaManager.Instance != null && adeganHappyEndingPertama != null) {
            CeritaManager.Instance.MulaiHappyEndingChain(adeganHappyEndingPertama);
        } else {
            TampilkanLayarAkhirHappyEnding();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SaveManager.slotUntukDiload = -1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ================== PANEL & KUNCI GERAKAN ==================

    public void TutupSemuaPanelGame()
    {
        if (panelToko) panelToko.SetActive(false);
        if (panelInventory) panelInventory.SetActive(false);
        if (panelMenuKerja) panelMenuKerja.SetActive(false);
        if (panelMasak) panelMasak.SetActive(false);

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }

    public bool ApakahAdaPanelAktif()
    {
        return (panelToko && panelToko.activeSelf) ||
               (panelInventory && panelInventory.activeSelf) ||
               (panelMenuKerja && panelMenuKerja.activeSelf) ||
               (panelMasak && panelMasak.activeSelf);
    }

    public void BukaTokoAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelToko) { panelToko.SetActive(true); PlayerController p = Object.FindFirstObjectByType<PlayerController>(); if (p) p.SetMenuStatus(true); }
    }

    public void BukaInventoryAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelInventory) { panelInventory.SetActive(true); PlayerController p = Object.FindFirstObjectByType<PlayerController>(); if (p) p.SetMenuStatus(true); }
    }

    public void BukaMasakAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelMasak) { panelMasak.SetActive(true); PlayerController p = Object.FindFirstObjectByType<PlayerController>(); if (p) p.SetMenuStatus(true); }
    }

    public void BukaKerjaAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelMenuKerja) { panelMenuKerja.SetActive(true); PlayerController p = Object.FindFirstObjectByType<PlayerController>(); if (p) p.SetMenuStatus(true); }
    }

    // ================== GANTI HARI ==================

    public void GantiHari()
    {
        MajukanTanggal();
        jamSaatIni = jamMulai;
        kopiDigunakanHariIni = false;
        skripsiSudahDikerjakanHariIni = false;
        kerjaPartTimeSudahDilakukanHariIni = false;
        sudahMandiHariIni = false; // --- TAMBAHAN ---
        sudahInteraksiAnnaHariIni = false; // --- TAMBAHAN ---

        KurangiLapar(penguranganLaparSaatTidur);
        // --- pemulihanSanitySaatTidur (bonus flat) DIHAPUS - diganti logic kondisional jam
        // di ProsesTidur() (TambahSanity/KurangiSanity sesuai jamBatasTidurAwal) ---

        // --- TAMBAHAN: penalti Sanity harian kalau Progres Skripsi masih di bawah plafon
        // Threshold aktif saat ini (belum "mencapai target") - berhenti otomatis begitu
        // progres nyampe/lewatin plafon itu, gak peduli Threshold-nya udah beneran "terbuka" atau belum ---
        if (progresSkripsi < batasProgresMaksimalSaatIni) {
            KurangiSanity(sanityTurunBelumCapaiTargetTH);
        }

        // --- TAMBAHAN: hitung mundur durasi bonus TEKAD_KUAT (kalau lagi aktif) ---
        if (hariDistorsiDimatikanPaksaSisa > 0) hariDistorsiDimatikanPaksaSisa--;
        if (hariSanityFloorSisa > 0) hariSanityFloorSisa--;

        // --- TAMBAHAN: bunga harian nambah ke Utang Bank tiap hari, kalau masih ada utang ---
        if (utangBank > 0f) {
            utangBank += utangBank * bungaHarianUtang;
            UpdateUI();
            UpdateTombolUtang();
        }

        // --- Bad Ending 2: lapar kritis BERTURUT-TURUT sekian hari ---
        if (SedangKelaparan) {
            hariLaparKritisBerturutTurut++;
            if (hariLaparKritisBerturutTurut >= batasHariLaparKritisBerturutTurut) {
                TampilkanBadEndingLapar();
            }
        } else {
            hariLaparKritisBerturutTurut = 0;
        }

        OnHariBerganti?.Invoke();

        // --- FIX: urutan prioritas sesuai naskah ("URUTAN PENGECEKAN ENDING") - Sanity=0 udah
        // independen lewat KurangiSanity(), Lapar kritis udah dicek di atas. Sisanya: Hari ke-61
        // Skripsi<100% (Bad Ending 3 Varian B) HARUS dicek SEBELUM Happy Ending, bukan sesudah -
        // biar kalau dua-duanya kebetulan valid bareng di hari yang sama, yang menang Bad Ending. ---
        if (ApakahSudahLewatTanggal(tanggalDeadline, bulanDeadline) && progresSkripsi < 100f) {
            PicuBadEnding4Waktu();
        } else {
            CekHappyEnding();
        }

        if (SaveManager.Instance != null) SaveManager.Instance.SimpanGame(0);
    }

    // --- TAMBAHAN: gerbang TUNGGAL buat semua cara mulai tidur (otomatis kemaleman, ATAU klik
    // Kasur manual - Kasur/BedController.cs WAJIB manggil INI, bukan langsung ProsesTidur()).
    // Kalau ada peristiwa cerita yang wajib kejadian hari ini tapi belum, tidur DIBLOKIR,
    // cutscene-nya dipaksa jalan dulu - coba tidur lagi abis cutscene-nya kelar. ---
    public void CobaMulaiTidur(bool pingsan = false)
    {
        Debug.Log("[GameManager] CobaMulaiTidur() TERPANGGIL."); // --- SEMENTARA ---

        if (prosesTidurAktif) return;

        if (CeritaManager.Instance != null && CeritaManager.Instance.ApakahAdaPeristiwaWajibSebelumTidurHariIni()) {
            Debug.Log("[GameManager] Ada peristiwa wajib - tidur DIBLOKIR, paksa trigger cutscene."); // --- SEMENTARA ---
            CeritaManager.Instance.PaksaTriggerPeristiwaWajibSebelumTidur();
            return;
        }

        Debug.Log("[GameManager] Gak ada peristiwa wajib yang pending - lanjut tidur normal."); // --- SEMENTARA ---
        prosesTidurAktif = true;
        StartCoroutine(ProsesTidur(pingsan));
    }

    public IEnumerator ProsesTidur(bool pingsan = false)
    {
        waktuBerjalan = false;
        TutupSemuaPanelGame();
        SetTombolHUDAktif(false);

        if (playerObj) {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc) pc.SetMenuStatus(false);

            if (posisiDepanKasur) {
                // --- FIX: sama pola kayak DoorController - X dari posisiDepanKasur (posisi
                // horizontal spesifik kasur), Y dari KonfigurasiLantai (sumber tunggal per
                // lantai, asumsi Kasur ada di Lantai 2 - sesuaikan angkanya kalau ternyata beda).
                // Pakai rb.position (physics-aware), BUKAN transform.position langsung - hindari
                // desync Rigidbody2D (Gravity Scale=0, gak ada gravitasi buat "nyettle" otomatis). ---
                float yLantaiKasur = (KonfigurasiLantai.Instance != null)
                    ? KonfigurasiLantai.Instance.DapatkanPosisiY(2)
                    : posisiDepanKasur.position.y; // fallback kalau KonfigurasiLantai belum ke-setup

                Vector2 posisiBangun = new Vector2(posisiDepanKasur.position.x, yLantaiKasur);

                Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
                if (rb != null) {
                    rb.position = posisiBangun;
                } else {
                    playerObj.transform.position = new Vector3(posisiBangun.x, posisiBangun.y, playerObj.transform.position.z);
                }

                if (pc) pc.lantaiSaatIni = 2; // --- pastiin konsisten, sama kayak DoorController nge-set lantaiTujuan ---
            }
        }

        // --- TAMBAHAN: cek jam SEKARANG (SEBELUM GantiHari() reset ke jamMulai) - tidur
        // sebelum Jam Batas Tidur Awal dapet bonus Sanity, tidur pas/lewat jam itu kena penalti.
        // Ini juga otomatis nangkep kemaleman OTOMATIS (jam udah >= batasTidur, pasti lewat 20). ---
        if (jamSaatIni < jamBatasTidurAwal) {
            TambahSanity(sanityNaikTidurAwal);
        } else {
            KurangiSanity(sanityTurunTidurTerlambat);
        }

        float alpha = 0;
        while (alpha < 1) { alpha += Time.deltaTime * 1.5f; if (layarGelap) layarGelap.color = new Color(0, 0, 0, alpha); yield return null; }
        GantiHari();

        if (!string.IsNullOrEmpty(monologAkhirHariBerikutnya) && textMonologAkhirHari != null) {
            textMonologAkhirHari.text = monologAkhirHariBerikutnya;
            textMonologAkhirHari.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (textMonologAkhirHari != null) textMonologAkhirHari.gameObject.SetActive(false);
        monologAkhirHariBerikutnya = "";

        while (alpha > 0) { alpha -= Time.deltaTime * 1.5f; if (layarGelap) layarGelap.color = new Color(0, 0, 0, alpha); yield return null; }
        waktuBerjalan = true;
        prosesTidurAktif = false;

        SetTombolHUDAktif(true);
    }
}