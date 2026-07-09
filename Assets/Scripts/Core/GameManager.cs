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

    [Header("Siklus Siang & Malam (Waktu Harian)")]
    public float jamMulai = 6f;
    public float jamSaatIni = 6f;
    public float batasTidur = 24f;

    [Tooltip("Kecepatan waktu berjalan. Misal: 1 = 1 jam in-game per detik realita")]
    public float kecepatanWaktuNormal = 0.5f;
    private float kecepatanWaktuAktif;
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

    [Header("Referensi UI & Objek")]
    public GameObject panelToko;
    public GameObject panelInventory;
    public GameObject panelMenuKerja;
    public GameObject panelMasak; 
    public GameObject playerObj; 
    public Transform posisiDepanKasur; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // INTEGRASI SAVE/LOAD: Memuat data saat game pertama kali dimulai
        // Memanggil MuatGame(slot) sesuai slot yang dipilih di menu
        if (SaveManager.Instance != null) 
        {
            SaveManager.Instance.MuatGame(SaveManager.slotUntukDiload);
        }

        kecepatanWaktuAktif = kecepatanWaktuNormal;
        UpdateUI();
    }

    void Update()
    {
        if (waktuBerjalan)
        {
            float deltaJam = kecepatanWaktuAktif * Time.deltaTime;
            jamSaatIni += deltaJam;

            if (jamSaatIni >= batasTidur)
            {
                StartCoroutine(ProsesTidur(true));
            }
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (textWaktu != null) textWaktu.text = waktu + " Hari";
        if (textUang != null) textUang.text = "Rp " + uang.ToString("N0");

        if (textJamHarian != null)
        {
            int jam = Mathf.FloorToInt(jamSaatIni);
            int menit = Mathf.FloorToInt((jamSaatIni - jam) * 60f);
            textJamHarian.text = string.Format("{0:00}:{1:00}", jam % 24, menit);
        }

        if (sliderProgresSkripsi != null) sliderProgresSkripsi.value = progresSkripsi;
        if (sliderLapar != null) sliderLapar.value = lapar;
        if (sliderSanity != null) sliderSanity.value = sanity;
    }

    public void GantiHari()
    {
        waktu -= 1;
        lapar -= 30f;
        jamSaatIni = jamMulai;
        batasTidur = 24f;
        Debug.Log("Hari berganti! Sisa waktu: " + waktu + " hari.");

        // INTEGRASI AUTOSAVE: Menyimpan otomatis ke Slot 0 saat ganti hari
        if (SaveManager.Instance != null) 
        {
            SaveManager.Instance.SimpanGame(0);
        }
    }

    public IEnumerator ProsesTidur(bool pingsan = false)
    {
        waktuBerjalan = false;

        // Tutup semua panel aktif
        if (panelToko != null) panelToko.SetActive(false);
        if (panelInventory != null) panelInventory.SetActive(false);
        if (panelMenuKerja != null) panelMenuKerja.SetActive(false);
        if (panelMasak != null) panelMasak.SetActive(false); 

        if (playerObj != null)
        {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc != null) pc.SetMenuStatus(false); 
            if (posisiDepanKasur != null) playerObj.transform.position = posisiDepanKasur.position; 
        }

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * 1.5f;
            if (layarGelap != null) layarGelap.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        GantiHari();
        yield return new WaitForSeconds(1.5f);

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 1.5f;
            if (layarGelap != null) layarGelap.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        waktuBerjalan = true; 
    }

    // --- FITUR PENCEGAH BUKA PANEL BERSAMAAN ---
    public bool ApakahAdaPanelAktif()
    {
        return (panelToko != null && panelToko.activeSelf) || 
               (panelInventory != null && panelInventory.activeSelf) || 
               (panelMenuKerja != null && panelMenuKerja.activeSelf) || 
               (panelMasak != null && panelMasak.activeSelf);
    }

    public void BukaTokoAman() { if (!ApakahAdaPanelAktif() && panelToko != null) panelToko.SetActive(true); }
    public void BukaInventoryAman() { if (!ApakahAdaPanelAktif() && panelInventory != null) panelInventory.SetActive(true); }
    public void BukaMasakAman() { if (!ApakahAdaPanelAktif() && panelMasak != null) panelMasak.SetActive(true); }
    
    public void SetJedaWaktu(bool jeda) { waktuBerjalan = !jeda; }
}