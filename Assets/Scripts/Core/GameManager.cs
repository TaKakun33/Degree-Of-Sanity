using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Parameter Status Kelangsungan Hidup")]
    public int waktu = 30;
    public int uang = 5000000;
    [Range(0f, 100f)] public float progresSkripsi = 0f;
    [Range(0f, 100f)] public float lapar = 100f;
    [Range(0f, 100f)] public float sanity = 100f;

    [Header("Siklus Siang & Malam")]
    public float jamMulai = 6f;
    public float jamSaatIni = 6f;
    public float batasTidur = 24f;
    public float kecepatanWaktuNormal = 0.5f;
    private bool waktuBerjalan = true;

    [Header("Sistem Tick Waktu (Event-Driven)")]
    [Tooltip("Interval real-time (detik) antar tick waktu; UI & event hanya diproses tiap tick ini, bukan tiap frame")]
    public float intervalTick = 0.2f;
    private float akumulatorTick = 0f;
    private float akumulatorJamSejakTick = 0f;
    private float pengaliKecepatanWaktu = 1f; // diubah minigame/sistem lain lewat SetPengaliKecepatanWaktu()
    private bool prosesTidurAktif = false;
    private bool kopiDigunakanHariIni = false; // Buff Kopi Espresso: mundurkan batas tidur ke 02.00

    // --- EVENT: sistem lain subscribe di sini, GameManager TIDAK perlu tahu siapa yang dengar ---
    public event System.Action<float> OnTickWaktu;      // param: jumlah jam yang berlalu sejak tick terakhir
    public event System.Action<float> OnJamBerubah;     // param: jamSaatIni terbaru (buat UI)
    public event System.Action OnBatasWaktuTercapai;    // dipicu SEBELUM ProsesTidur mulai (buat interupsi minigame)
    public event System.Action OnPermainanBerakhir;     // dipicu saat Bad/Good Ending muncul (buat interupsi minigame aktif)

    [Header("Referensi UI")]
    public TextMeshProUGUI textWaktu;
    public TextMeshProUGUI textUang;
    public TextMeshProUGUI textJamHarian;
    public Slider sliderProgresSkripsi;
    public Slider sliderLapar;
    public Slider sliderSanity;
    [Tooltip("TAMBAHAN: teks Monolog Akhir Hari (opsional). Kalau field 'Monolog Akhir Hari Berikutnya' di bawah diisi sebelum tidur, teks ini bakal nampilinnya sesaat, lalu otomatis dikosongkan lagi.")]
    public TextMeshProUGUI textMonologAkhirHari;

    [Tooltip("Isi manual (atau lewat sistem cerita nanti) SEBELUM pemain tidur, buat nampilin 1 baris Monolog Akhir Hari. Kosongkan string ini kalau gak mau nampilin apa-apa.")]
    public string monologAkhirHariBerikutnya = "";

    [Tooltip("Batas progres skripsi maksimal SAAT INI - default 100 (bebas penuh). Sistem cerita nanti bisa nurunin ini sementara buat nge-cap progres sampai event tertentu terjadi (Naskah Alur: 'progres terkunci sampai event berjalan').")]
    public float batasProgresMaksimalSaatIni = 100f;

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
    [Tooltip("Jumlah lapar yang berkurang tiap kali tidur/ganti hari (proposal 3.3.7: Lapar memburuk tiap hari berganti)")]
    public float penguranganLaparSaatTidur = 20f;
    [Tooltip("Jumlah sanity yang DIPULIHKAN tiap kali tidur (proposal 3.6.3: 'Tidur lebih awal' adalah cara memulihkan Sanity, bukan menguras)")]
    public float pemulihanSanitySaatTidur = 15f;

    [Header("Ambang Batas Parameter (sesuai Proposal 3.3.7 & 3.6.3)")]
    [Tooltip("Sanity di bawah angka ini akan memicu efek Distorsi Visual (proposal: di bawah 50%)")]
    public float ambangSanityDistorsi = 50f;
    [Tooltip("Lapar di bawah angka ini dianggap kondisi kritis/kelaparan")]
    public float ambangLaparKritis = 20f;
    [Tooltip("Pengali kecepatan pengurasan Sanity saat kondisi lapar kritis (proposal: dua kali lipat lebih cepat)")]
    public float pengaliSanitySaatLaparKritis = 2f;

    [Header("Kondisi Akhir Permainan (Proposal 3.6.4)")]
    [Tooltip("Panel yang otomatis muncul saat Sanity mencapai 0% ATAU Waktu habis sebelum skripsi 100%")]
    public GameObject panelBadEnding;
    [Tooltip("Panel yang otomatis muncul saat Progres Skripsi mencapai 100%")]
    public GameObject panelGoodEnding;
    [Tooltip("TAMBAHAN: Panel True Ending - dipakai kalau nanti nambah sistem cerita/Anna, biarkan kosong (None) kalau belum ada")]
    public GameObject panelTrueEnding;
    [Tooltip("TAMBAHAN: Panel Bad Ending versi 'tertunda' (misal dari pilihan cerita tertentu) - biarkan kosong kalau belum ada")]
    public GameObject panelBadEndingTertunda;
    private bool endingSudahDipicu = false;

    [Header("Syarat Ending Tambahan (opsional, siap dipakai nanti kalau ada sistem cerita/Anna)")]
    [Tooltip("Hari ke berapa ending final dievaluasi kalau progres masih di bawah 100% (0 = fitur ini nonaktif, pakai sistem 'waktu habis' lama aja)")]
    public int hariEpilog = 0;
    public float sanityMinimalTrueEnding = 40f;
    public float sanityMinimalHappyEnding = 20f;
    [Tooltip("Placeholder tracking interaksi 'Ngobrol sama Anna' - panggil TambahInteraksiAnna() dari sistem Anna nanti")]
    public int totalInteraksiAnna = 0;
    public int minimalInteraksiAnnaTrueEnding = 15;
    [Tooltip("Isi manual dari sistem cerita nanti: 'A', 'B', atau 'C' (Main Event 6 'Pilihan Dilematis'). Kosongkan (' ') kalau belum dipakai.")]
    public char pilihanEvent6 = ' ';

    [Header("Batasan Minigame Skripsi")]
    [Tooltip("Skripsi cuma bisa dikerjakan 1x per hari; direset otomatis tiap ganti hari")]
    private bool skripsiSudahDikerjakanHariIni = false;

    [Header("Batasan Kerja Part Time")]
    [Tooltip("Kerja part time (Kasir/Ojol/Tutor) cuma bisa 1x per hari; direset otomatis tiap ganti hari")]
    private bool kerjaPartTimeSudahDilakukanHariIni = false;

    [Header("Sistem Hari & Cerita (siap dipakai nanti kalau nambah CeritaManager - gak wajib diisi sekarang)")]
    [Tooltip("Hari ke berapa dari awal permainan, NAIK terus - beda dari 'waktu' yang turun (sisa masa studi)")]
    public int hariKe = 1;
    [Tooltip("Dipicu tiap kali hari berganti - sistem cerita/Anna nanti tinggal subscribe ke event ini")]
    public event System.Action OnHariBerganti;

    [Header("Tombol HUD (disembunyikan saat tidur ATAU minigame aktif)")]
    [Tooltip("Tombol untuk buka Toko di HUD, akan otomatis disembunyikan selama proses tidur/minigame")]
    public GameObject tombolBukaToko;
    [Tooltip("Tombol untuk buka Inventory di HUD, akan otomatis disembunyikan selama proses tidur/minigame")]
    public GameObject tombolBukaInventory;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject); 
    }

    void Start() 
    { 
        if (SaveManager.Instance != null) SaveManager.Instance.MuatGame(SaveManager.slotUntukDiload);
        TerapkanHasilKerjaPartTimeJikaAda(); // --- TAMBAHAN: terapkan hasil kerja Kasir/Ojek/Tutor kalau ada ---
        UpdateUI();
    }

    // --- TAMBAHAN: dipanggil sekali tiap GameManager baru dibuat, cek titipan dari HasilKerjaPartTime ---
    void TerapkanHasilKerjaPartTimeJikaAda()
    {
        if (!HasilKerjaPartTime.adaHasilPending) return;

        TambahUang(HasilKerjaPartTime.uangDidapat);
        KurangiLapar(HasilKerjaPartTime.laparBerkurang);
        KurangiSanity(HasilKerjaPartTime.sanityBerkurang); // otomatis kena pengali lapar-kritis kalau relevan
        jamSaatIni += HasilKerjaPartTime.jamYangDilewati;  // skip waktu; kalau lewat batas tidur, Update() otomatis proses tidur

        Debug.Log("Hasil kerja part time diterapkan: +Rp " + HasilKerjaPartTime.uangDidapat +
                   ", lewat " + HasilKerjaPartTime.jamYangDilewati + " jam.");

        HasilKerjaPartTime.Bersihkan();
    }

    void Update()
    {
        if (!waktuBerjalan) return;

        // Jam tetap nambah tiap frame biar gerakannya smooth, tapi TIDAK langsung broadcast event/UI tiap frame
        float deltaJam = kecepatanWaktuNormal * pengaliKecepatanWaktu * Time.deltaTime;
        jamSaatIni += deltaJam;
        akumulatorJamSejakTick += deltaJam;

        // --- TICK SYSTEM: event & UI cuma diproses tiap intervalTick, bukan tiap frame ---
        akumulatorTick += Time.deltaTime;
        if (akumulatorTick >= intervalTick) {
            akumulatorTick = 0f;
            OnTickWaktu?.Invoke(akumulatorJamSejakTick);
            OnJamBerubah?.Invoke(jamSaatIni);
            UpdateUI();
            akumulatorJamSejakTick = 0f;
        }

        // --- Cek batas tidur efektif (mundur ke 02.00 kalau kopi dipakai) ---
        if (jamSaatIni >= DapatkanBatasTidurEfektif() && !prosesTidurAktif) {
            prosesTidurAktif = true;
            OnBatasWaktuTercapai?.Invoke(); // beri kesempatan minigame aktif buat simpan progres & berhenti dulu
            StartCoroutine(ProsesTidur(true));
        }
    }

    // --- TAMBAHAN: batas tidur efektif hari ini, mundur ke 02.00 kalau buff Kopi Espresso dipakai ---
    public float DapatkanBatasTidurEfektif()
    {
        return kopiDigunakanHariIni ? batasTidur + 2f : batasTidur;
    }

    // --- TAMBAHAN: dipanggil sistem Toko/Inventory saat item Kopi Espresso dipakai ---
    public void GunakanBuffKopiEspresso()
    {
        kopiDigunakanHariIni = true;
    }

    // --- TAMBAHAN: titik terpusat untuk minigame/sistem lain mengubah laju waktu ---
    // Contoh: minigame skripsi manggil SetPengaliKecepatanWaktu(6f) saat mulai, ResetPengaliKecepatanWaktu() saat selesai.
    public void SetPengaliKecepatanWaktu(float pengali)
    {
        pengaliKecepatanWaktu = pengali;
    }

    public void ResetPengaliKecepatanWaktu()
    {
        pengaliKecepatanWaktu = 1f;
    }

    void UpdateUI()
    {
        if (textWaktu) textWaktu.text = waktu + " Hari";
        if (textUang) textUang.text = "Rp " + uang.ToString("N0");
        if (textJamHarian) textJamHarian.text = string.Format("{0:00}:{1:00}", (int)jamSaatIni % 24, (int)((jamSaatIni % 1) * 60));
        if (sliderProgresSkripsi) sliderProgresSkripsi.value = progresSkripsi;
        if (sliderLapar) sliderLapar.value = lapar;
        if (sliderSanity) sliderSanity.value = sanity;
    }

    // --- FUNGSI TAMBAHAN UNTUK MEMPERBAIKI ERROR ---
    public void SetJedaWaktu(bool jeda) 
    { 
        waktuBerjalan = !jeda; 
    }

    // --- TAMBAHAN: Status kondisi (dipakai UI lain, sistem distorsi, atau minigame) ---
    public bool SedangDistorsi => sanity < ambangSanityDistorsi;
    public bool SedangKelaparan => lapar < ambangLaparKritis;

    // --- TAMBAHAN: Skripsi cuma boleh dikerjakan 1x per hari ---
    public bool BisaKerjakanSkripsiHariIni => !skripsiSudahDikerjakanHariIni;

    // --- TAMBAHAN: Dipanggil minigame skripsi begitu sesi DIMULAI (bukan saat selesai) ---
    // supaya jatah harian tetap terpakai walau sesi berakhir cepat (force quit/gagal typo/keluar manual).
    public void TandaiSkripsiSudahDikerjakan()
    {
        skripsiSudahDikerjakanHariIni = true;
    }

    // --- TAMBAHAN: dipakai SaveManager buat nyimpen/muat balik flag ini - PENTING karena KasirScene/
    // OjolScene/TutorScene di-load SINGLE, jadi GameManager beneran hancur & dibuat ulang. Tanpa ini,
    // flag "sudah dikerjakan hari ini" bakal balik ke false lagi tiap kali GameManager baru dibuat. ---
    public bool SkripsiSudahDikerjakanHariIni {
        get => skripsiSudahDikerjakanHariIni;
        set => skripsiSudahDikerjakanHariIni = value;
    }

    // --- TAMBAHAN: sama pola persis kayak Skripsi, tapi buat kerja part time (Kasir/Ojol/Tutor) ---
    public bool BisaKerjaPartTimeHariIni => !kerjaPartTimeSudahDilakukanHariIni;

    public void TandaiKerjaPartTimeSudahDilakukan()
    {
        kerjaPartTimeSudahDilakukanHariIni = true;
    }

    // --- TAMBAHAN: sama alasannya kayak di atas - SaveManager perlu ini biar flag-nya gak reset
    // sendiri tiap kali balik dari KasirScene/OjolScene/TutorScene (Single load) ---
    public bool KerjaPartTimeSudahDilakukanHariIni {
        get => kerjaPartTimeSudahDilakukanHariIni;
        set => kerjaPartTimeSudahDilakukanHariIni = value;
    }

    // --- TAMBAHAN: Tombol Toko & Inventory di HUD - dipakai baik saat tidur maupun minigame aktif ---
    public void SetTombolHUDAktif(bool aktif)
    {
        if (tombolBukaToko) tombolBukaToko.SetActive(aktif);
        if (tombolBukaInventory) tombolBukaInventory.SetActive(aktif);
    }

    // --- TAMBAHAN: Titik terpusat untuk mengubah Sanity ---
    // Semua minigame/aktivitas (skripsi, kerja part time, masak, dsb) sebaiknya lewat sini,
    // supaya penalti "lapar kritis -> sanity terkuras 2x lebih cepat" (proposal 3.6.3) otomatis berlaku.
    public void KurangiSanity(float jumlah)
    {
        float pengali = SedangKelaparan ? pengaliSanitySaatLaparKritis : 1f;
        sanity = Mathf.Clamp(sanity - (jumlah * pengali), 0f, 100f);
        UpdateUI();
        CekKondisiGameOver();
    }

    public void TambahSanity(float jumlah)
    {
        sanity = Mathf.Clamp(sanity + jumlah, 0f, 100f);
        UpdateUI();
    }

    // --- TAMBAHAN: Titik terpusat untuk mengubah Lapar ---
    public void KurangiLapar(float jumlah)
    {
        lapar = Mathf.Clamp(lapar - jumlah, 0f, 100f);
        UpdateUI();
    }

    public void TambahLapar(float jumlah)
    {
        lapar = Mathf.Clamp(lapar + jumlah, 0f, 100f);
        UpdateUI();
    }

    // --- TAMBAHAN: Dipanggil sistem Masak/Makan (Proposal 3.3.3) saat pemain makan sesuatu ---
    // Memasak sendiri secara proposal memulihkan Lapar signifikan; makan bareng/masakan enak juga ikut menenangkan Sanity dikit.
    public void Makan(float jumlahLaparDipulihkan, float jumlahSanityDipulihkan = 0f)
    {
        TambahLapar(jumlahLaparDipulihkan);
        if (jumlahSanityDipulihkan > 0f) TambahSanity(jumlahSanityDipulihkan);
    }

    // --- TAMBAHAN: Titik terpusat untuk mengubah Uang ---
    public void KurangiUang(int jumlah)
    {
        uang = Mathf.Max(0, uang - jumlah);
        UpdateUI();
    }

    public void TambahUang(int jumlah)
    {
        // --- Mathf.Max di sini penting: jumlah BISA negatif (misal gaji shift Kasir yang minus
        // karena kebanyakan penalti), tapi total uang pemain tetap gak boleh sampai di bawah 0 ---
        uang = Mathf.Max(0, uang + jumlah);
        UpdateUI();
    }

    // --- TAMBAHAN: Titik terpusat untuk menambah Progres Skripsi ---
    // TIDAK LAGI langsung memicu Good Ending di sini - dicek terpisah lewat CekEvaluasiEndingFinal()
    // (dipanggil dari GantiHari()), biar konsisten kalau nanti dipakai bareng sistem cerita.
    public void TambahProgresSkripsi(float jumlah)
    {
        progresSkripsi = Mathf.Clamp(progresSkripsi + jumlah, 0f, batasProgresMaksimalSaatIni);
        UpdateUI();
        CekEvaluasiEndingFinal();
    }

    // --- TAMBAHAN: Dipanggil sistem "Ngobrol sama Anna" nanti (placeholder, gak wajib dipakai sekarang) ---
    public void TambahInteraksiAnna()
    {
        totalInteraksiAnna++;
    }

    // --- TAMBAHAN: Cek kondisi Bad Ending (Proposal 3.6.4): Sanity 0% ---
    void CekKondisiGameOver()
    {
        if (endingSudahDipicu) return;
        if (sanity <= 0f) TampilkanBadEnding();
    }

    // --- TAMBAHAN: Evaluasi ending final. Kalau "Hari Epilog" belum diisi (masih 0), sistem ini
    // otomatis nonaktif - progres 100% cukup buat langsung munculin Happy Ending kayak sebelumnya
    // (biar tetap jalan normal walau kamu belum punya sistem cerita/hari). Begitu "Hari Epilog"
    // diisi manual di Inspector (misal 61), baru ending final ditunda sampai hari itu tercapai. ---
    void CekEvaluasiEndingFinal()
    {
        if (endingSudahDipicu) return;

        bool modeCeritaAktif = hariEpilog > 0;

        if (!modeCeritaAktif) {
            // --- Mode lama (tanpa sistem cerita): progres 100% langsung Happy Ending ---
            if (progresSkripsi >= 100f) TampilkanGoodEnding();
            return;
        }

        // --- Mode cerita aktif: tunda evaluasi sampai Hari Epilog tercapai ---
        if (hariKe < hariEpilog) return;

        bool syaratTrueEnding = progresSkripsi >= 100f
            && sanity >= sanityMinimalTrueEnding
            && pilihanEvent6 == 'C'
            && totalInteraksiAnna >= minimalInteraksiAnnaTrueEnding;

        if (syaratTrueEnding) { TampilkanTrueEnding(); return; }

        bool syaratHappyEnding = progresSkripsi >= 100f && sanity > sanityMinimalHappyEnding;
        if (syaratHappyEnding) { TampilkanGoodEnding(); return; }

        TampilkanBadEnding();
    }

    // --- TAMBAHAN: dipanggil manual (misal tombol pilihan cerita nanti) buat Bad Ending versi "tertunda" ---
    public void PicuBadEndingTertunda()
    {
        pilihanEvent6 = 'B';
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;
        Debug.Log("Bad Ending 'Tertunda' dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelBadEndingTertunda) panelBadEndingTertunda.SetActive(true);
    }

    // --- TAMBAHAN: Tampilkan panel Bad Ending & hentikan permainan ---
    void TampilkanBadEnding()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;
        Debug.Log("Bad Ending dipicu.");
        OnPermainanBerakhir?.Invoke(); // --- TAMBAHAN: paksa tutup minigame aktif (skripsi, dsb) kalau ada ---
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelBadEnding) panelBadEnding.SetActive(true);
    }

    // --- TAMBAHAN: Tampilkan panel Good Ending & hentikan permainan ---
    void TampilkanGoodEnding()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;
        Debug.Log("Good Ending dipicu.");
        OnPermainanBerakhir?.Invoke(); // --- TAMBAHAN: paksa tutup minigame aktif (skripsi, dsb) kalau ada ---
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelGoodEnding) panelGoodEnding.SetActive(true);
    }

    // --- TAMBAHAN: Tampilkan panel True Ending & hentikan permainan (siap dipakai kalau ada sistem cerita) ---
    void TampilkanTrueEnding()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;
        Debug.Log("True Ending dipicu.");
        OnPermainanBerakhir?.Invoke();
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelTrueEnding) panelTrueEnding.SetActive(true);
    }

    // --- TAMBAHAN: Restart permainan dari awal (dipanggil tombol "Restart" di panel Bad/Good Ending) ---
    // Reload scene yang sama, mulai sebagai Game Baru (bukan load save lama).
    public void RestartGame()
    {
        Time.timeScale = 1;
        SaveManager.slotUntukDiload = -1; // -1 = Game Baru, sesuai konvensi SaveManager
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- FUNGSI PANEL & PENGUNCIAN GERAKAN ---
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
        if (panelToko) {
            panelToko.SetActive(true);
            PlayerController p = Object.FindFirstObjectByType<PlayerController>();
            if (p) p.SetMenuStatus(true);
        }
    }

    public void BukaInventoryAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelInventory) {
            panelInventory.SetActive(true);
            PlayerController p = Object.FindFirstObjectByType<PlayerController>();
            if (p) p.SetMenuStatus(true);
        }
    }

    public void BukaMasakAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelMasak) {
            panelMasak.SetActive(true);
            PlayerController p = Object.FindFirstObjectByType<PlayerController>();
            if (p) p.SetMenuStatus(true);
        }
    }

    public void BukaKerjaAman()
    {
        if (ApakahAdaPanelAktif()) return;
        if (panelMenuKerja) {
            panelMenuKerja.SetActive(true);
            PlayerController p = Object.FindFirstObjectByType<PlayerController>();
            if (p) p.SetMenuStatus(true);
        }
    }

    // --- FUNGSI SISTEM LAINNYA ---
    public void GantiHari()
    {
        waktu -= 1;
        hariKe += 1; // --- TAMBAHAN: penanda hari-ke, naik terus (beda dari 'waktu' yang turun) ---
        jamSaatIni = jamMulai;
        kopiDigunakanHariIni = false; // --- TAMBAHAN: buff Kopi Espresso cuma berlaku 1 hari ---
        skripsiSudahDikerjakanHariIni = false; // --- TAMBAHAN: jatah skripsi harian direset tiap hari baru ---
        kerjaPartTimeSudahDilakukanHariIni = false; // --- TAMBAHAN: jatah kerja part time direset tiap hari baru ---

        // --- Lapar tetap memburuk tiap ganti hari, tapi Sanity DIPULIHKAN dari tidur (Proposal 3.6.3) ---
        KurangiLapar(penguranganLaparSaatTidur);
        TambahSanity(pemulihanSanitySaatTidur);

        // --- TAMBAHAN: kasih tau sistem lain (nanti CeritaManager/AnnaNPC/CicilanManager) hari udah berganti ---
        OnHariBerganti?.Invoke();

        // --- TAMBAHAN: evaluasi ending final (mode cerita, kalau Hari Epilog udah diisi) ---
        CekEvaluasiEndingFinal();

        // --- TAMBAHAN: Waktu (sisa masa studi) habis sebelum skripsi 100% -> Bad Ending (Proposal 3.3.7 & 3.6.4) ---
        if (waktu <= 0 && progresSkripsi < 100f) {
            TampilkanBadEnding();
        }

        if (SaveManager.Instance != null) SaveManager.Instance.SimpanGame(0);
    }

    public IEnumerator ProsesTidur(bool pingsan = false)
    {
        waktuBerjalan = false;
        TutupSemuaPanelGame();

        // --- Sembunyikan tombol Toko & Inventory di HUD selama proses tidur ---
        SetTombolHUDAktif(false);

        if (playerObj) {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc) pc.SetMenuStatus(false);
            if (posisiDepanKasur) playerObj.transform.position = posisiDepanKasur.position;
        }

        float alpha = 0;
        while (alpha < 1) { alpha += Time.deltaTime * 1.5f; if (layarGelap) layarGelap.color = new Color(0, 0, 0, alpha); yield return null; }
        GantiHari();

        // --- TAMBAHAN: tampilkan Monolog Akhir Hari kalau ada yang dititipkan (opsional, siap dipakai sistem cerita nanti) ---
        if (!string.IsNullOrEmpty(monologAkhirHariBerikutnya) && textMonologAkhirHari != null) {
            textMonologAkhirHari.text = monologAkhirHariBerikutnya;
            textMonologAkhirHari.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (textMonologAkhirHari != null) textMonologAkhirHari.gameObject.SetActive(false);
        monologAkhirHariBerikutnya = ""; // reset, cuma tampil sekali per titipan

        while (alpha > 0) { alpha -= Time.deltaTime * 1.5f; if (layarGelap) layarGelap.color = new Color(0, 0, 0, alpha); yield return null; }
        waktuBerjalan = true;
        prosesTidurAktif = false; // --- TAMBAHAN: izinkan trigger tidur lagi di hari berikutnya ---

        // --- Tampilkan kembali tombol Toko & Inventory setelah bangun ---
        SetTombolHUDAktif(true);
    }
}