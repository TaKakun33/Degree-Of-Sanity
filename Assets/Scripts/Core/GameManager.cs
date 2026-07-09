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

    [Header("Referensi UI (Antarmuka Pemain)")]
    public TextMeshProUGUI textWaktu;
    public TextMeshProUGUI textUang;
    public TextMeshProUGUI textJamHarian;
    public Slider sliderProgresSkripsi;
    public Slider sliderLapar;
    public Slider sliderSanity;

    [Header("Transisi Layar (Tidur)")]
    public Image layarGelap;

    [Header("Referensi UI & Objek (Tidur Paksa)")]
    public GameObject panelToko;
    public GameObject panelInventory;
    public GameObject panelMenuKerja;
    public GameObject panelMasak; 
    public GameObject playerObj; 
    public Transform posisiDepanKasur; 

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // --- MEMUAT DATA SAVE SAAT GAME BARU DIMULAI ---
        if (SaveManager.Instance != null) 
        {
            SaveManager.Instance.MuatGame();
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
        if (textUang != null) textUang.text = "Rp " + uang;

        if (textJamHarian != null)
        {
            int jam = Mathf.FloorToInt(jamSaatIni);
            int menit = Mathf.FloorToInt((jamSaatIni - jam) * 60f);
            jam = jam % 24; 
            textJamHarian.text = string.Format("{0:00}:{1:00}", jam, menit);
        }

        if (sliderProgresSkripsi != null) sliderProgresSkripsi.value = progresSkripsi;
        if (sliderLapar != null) sliderLapar.value = lapar;
        if (sliderSanity != null) sliderSanity.value = sanity;
    }

    public void SelesaikanMinigameSkripsi(float tambahanProgres)
    {
        progresSkripsi += tambahanProgres;
        progresSkripsi = Mathf.Clamp(progresSkripsi, 0f, 100f);
        sanity -= 20f;
        lapar -= 15f;
        UpdateUI();
    }

    private void CekKondisiGame()
    {
        if (sanity <= 30f && sanity > 0f) AktifkanDistorsiVisual();
        if (sanity <= 0f) TriggerBadEnding("Karakter mengalami depresi berat.");
        if (lapar <= 0f) PenaltiLaparKritis();
        if (waktu <= 0 && progresSkripsi < 100f) TriggerBadEnding("Waktu habis, terkena DO.");
    }

    public void GantiHari()
    {
        waktu -= 1;
        lapar -= 30f;
        jamSaatIni = jamMulai;
        batasTidur = 24f;
        Debug.Log("Hari berganti! Sisa waktu: " + waktu + " hari.");

        // --- SISTEM AUTOSAVE (MENYIMPAN KE SLOT 0 SETIAP GANTI HARI) ---
        if (SaveManager.Instance != null) 
        {
            SaveManager.Instance.SimpanGame(0);
        }
    }

    public IEnumerator ProsesTidur(bool pingsan = false)
    {
        waktuBerjalan = false;

        // Tutup semua panel UI
        if (panelToko != null) panelToko.SetActive(false);
        if (panelInventory != null) panelInventory.SetActive(false);
        if (panelMenuKerja != null) panelMenuKerja.SetActive(false);
        if (panelMasak != null) panelMasak.SetActive(false); 

        if (playerObj != null)
        {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc != null) pc.SetMenuStatus(false); 

            if (posisiDepanKasur != null)
            {
                playerObj.transform.position = posisiDepanKasur.position; 
            }
        }

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * 1.5f;
            if (layarGelap != null) layarGelap.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (pingsan)
        {
            Debug.Log("Karakter pingsan karena begadang maksimal!");
            sanity -= 15f;
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

    private void AktifkanDistorsiVisual() { Debug.Log("Efek distorsi visual aktif (Sanity rendah)."); }
    private void PenaltiLaparKritis() { Debug.Log("Pemain kelaparan."); }
    private void TriggerBadEnding(string alasan)
    {
        Debug.Log("GAME OVER: " + alasan);
        waktuBerjalan = false;
    }

    public void SetJedaWaktu(bool jeda) { waktuBerjalan = !jeda; }

    // --- FITUR PENCEGAH BUKA PANEL BERSAMAAN ---
    public bool ApakahAdaPanelAktif()
    {
        bool tokoBuka = panelToko != null && panelToko.activeSelf;
        bool invBuka = panelInventory != null && panelInventory.activeSelf;
        bool kerjaBuka = panelMenuKerja != null && panelMenuKerja.activeSelf;
        bool masakBuka = panelMasak != null && panelMasak.activeSelf; 

        return tokoBuka || invBuka || kerjaBuka || masakBuka;
    }

    public void BukaTokoAman()
    {
        if (ApakahAdaPanelAktif()) return; 
        if (panelToko != null) panelToko.SetActive(true);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(true);
    }

    public void BukaInventoryAman()
    {
        if (ApakahAdaPanelAktif()) return; 
        if (panelInventory != null) panelInventory.SetActive(true);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(true);
    }

    public void BukaMasakAman()
    {
        if (ApakahAdaPanelAktif()) return; 
        if (panelMasak != null) panelMasak.SetActive(true);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(true);
    }
}