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

    [Header("Referensi UI")]
    public TextMeshProUGUI textWaktu;
    public TextMeshProUGUI textUang;
    public TextMeshProUGUI textJamHarian;
    public Slider sliderProgresSkripsi;
    public Slider sliderLapar;
    public Slider sliderSanity;

    [Header("Transisi Layar")]
    public Image layarGelap;

    [Header("Panel Game")]
    public GameObject panelToko;
    public GameObject panelInventory;
    public GameObject panelMenuKerja;
    public GameObject panelMasak;
    public GameObject playerObj;
    public Transform posisiDepanKasur;

    [Header("Pengurangan Status Saat Tidur")]
    [Tooltip("Jumlah lapar yang berkurang tiap kali tidur/ganti hari")]
    public float penguranganLaparSaatTidur = 20f;
    [Tooltip("Jumlah sanity yang berkurang tiap kali tidur/ganti hari")]
    public float penguranganSanitySaatTidur = 10f;

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
    private bool endingSudahDipicu = false;

    [Header("Tombol HUD (disembunyikan saat tidur)")]
    [Tooltip("Tombol untuk buka Toko di HUD, akan otomatis disembunyikan selama proses tidur")]
    public GameObject tombolBukaToko;
    [Tooltip("Tombol untuk buka Inventory di HUD, akan otomatis disembunyikan selama proses tidur")]
    public GameObject tombolBukaInventory;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject); 
    }

    void Start() 
    { 
        if (SaveManager.Instance != null) SaveManager.Instance.MuatGame(SaveManager.slotUntukDiload);
        UpdateUI();
    }

    void Update()
    {
        if (waktuBerjalan)
        {
            jamSaatIni += kecepatanWaktuNormal * Time.deltaTime;
            if (jamSaatIni >= batasTidur) StartCoroutine(ProsesTidur(true));
            UpdateUI();
        }
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

    // --- TAMBAHAN: Titik terpusat untuk mengubah Uang ---
    public void KurangiUang(int jumlah)
    {
        uang = Mathf.Max(0, uang - jumlah);
        UpdateUI();
    }

    public void TambahUang(int jumlah)
    {
        uang += jumlah;
        UpdateUI();
    }

    // --- TAMBAHAN: Titik terpusat untuk menambah Progres Skripsi, sekaligus cek Good Ending ---
    public void TambahProgresSkripsi(float jumlah)
    {
        progresSkripsi = Mathf.Clamp(progresSkripsi + jumlah, 0f, 100f);
        UpdateUI();
        CekKondisiKelulusan();
    }

    // --- TAMBAHAN: Cek kondisi Bad Ending (Proposal 3.6.4): Sanity 0% ---
    void CekKondisiGameOver()
    {
        if (endingSudahDipicu) return;
        if (sanity <= 0f) TampilkanBadEnding();
    }

    // --- TAMBAHAN: Cek kondisi Good/Happy Ending (Proposal 3.6.4): Progres Skripsi 100% ---
    void CekKondisiKelulusan()
    {
        if (endingSudahDipicu) return;
        if (progresSkripsi >= 100f) TampilkanGoodEnding();
    }

    // --- TAMBAHAN: Tampilkan panel Bad Ending & hentikan permainan ---
    void TampilkanBadEnding()
    {
        if (endingSudahDipicu) return;
        endingSudahDipicu = true;
        Debug.Log("Bad Ending dipicu.");
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
        Time.timeScale = 0;
        TutupSemuaPanelGame();
        if (panelGoodEnding) panelGoodEnding.SetActive(true);
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
        jamSaatIni = jamMulai;

        // --- TAMBAHAN: lapar & sanity ikut berkurang tiap kali tidur/ganti hari (lewat fungsi terpusat) ---
        KurangiLapar(penguranganLaparSaatTidur);
        KurangiSanity(penguranganSanitySaatTidur);

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

        // --- TAMBAHAN: sembunyikan tombol Toko & Inventory di HUD selama proses tidur ---
        if (tombolBukaToko) tombolBukaToko.SetActive(false);
        if (tombolBukaInventory) tombolBukaInventory.SetActive(false);

        if (playerObj) {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc) pc.SetMenuStatus(false);
            if (posisiDepanKasur) playerObj.transform.position = posisiDepanKasur.position;
        }

        float alpha = 0;
        while (alpha < 1) { alpha += Time.deltaTime * 1.5f; if (layarGelap) layarGelap.color = new Color(0, 0, 0, alpha); yield return null; }
        GantiHari();
        yield return new WaitForSeconds(1.5f);
        while (alpha > 0) { alpha -= Time.deltaTime * 1.5f; if (layarGelap) layarGelap.color = new Color(0, 0, 0, alpha); yield return null; }
        waktuBerjalan = true;

        // --- TAMBAHAN: tampilkan kembali tombol Toko & Inventory setelah bangun ---
        if (tombolBukaToko) tombolBukaToko.SetActive(true);
        if (tombolBukaInventory) tombolBukaInventory.SetActive(true);
    }
}