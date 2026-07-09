using UnityEngine;
using UnityEngine.UI;
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

        // --- TAMBAHAN: lapar & sanity ikut berkurang tiap kali tidur/ganti hari ---
        lapar = Mathf.Clamp(lapar - penguranganLaparSaatTidur, 0f, 100f);
        sanity = Mathf.Clamp(sanity - penguranganSanitySaatTidur, 0f, 100f);
        UpdateUI(); // refresh slider/teks segera, karena Update() tidak jalan saat waktuBerjalan = false

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