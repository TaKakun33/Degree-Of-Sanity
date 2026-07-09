using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Referensi Panel UI")]
    public GameObject panelUtama;
    public GameObject panelLoadGame;
    public GameObject panelPengaturan;

    [Header("Pengaturan Nama Scene")]
    public string namaSceneGame = "SampleScene"; 

    [Header("Sistem Load Dinamis (Scroll View)")]
    public GameObject prefabTombolSlot; 
    public Transform wadahContentLoad;  
    
    void Start()
    {
        // Pastikan saat game mulai, panel utama yang aktif
        KembaliKeMenuUtama();
    }

    public void MulaiGameBaru() 
    { 
        SaveManager.slotUntukDiload = -1; 
        SceneManager.LoadScene(namaSceneGame); 
    }

    public void LanjutkanGame()
    {
        int slotTerakhir = SaveManager.Instance.DapatkanSlotTerakhir();
        if (slotTerakhir != -1 && SaveManager.Instance.CekSaveAda(slotTerakhir)) {
            SaveManager.slotUntukDiload = slotTerakhir;
            SceneManager.LoadScene(namaSceneGame);
        } else {
            MulaiGameBaru();
        }
    }

    // --- FUNGSI NAVIGASI ---

    public void BukaMenuLoadGame() 
    { 
        panelUtama.SetActive(false); 
        panelLoadGame.SetActive(true);
        panelPengaturan.SetActive(false);
        
        // Refresh daftar tombol
        foreach (Transform anak in wadahContentLoad) Destroy(anak.gameObject);
        if (SaveManager.Instance.CekSaveAda(0)) BuatTombol(0, "[AUTOSAVE] " + SaveManager.Instance.DapatkanInfoSave(0));
        for(int i=1; i<=10; i++) if (SaveManager.Instance.CekSaveAda(i)) BuatTombol(i, SaveManager.Instance.DapatkanInfoSave(i));
    }

    public void BukaMenuPengaturan()
    {
        panelUtama.SetActive(false);
        panelLoadGame.SetActive(false);
        panelPengaturan.SetActive(true);
    }

    // --- FUNGSI KEMBALI (BACK) ---
    public void KembaliKeMenuUtama()
    {
        panelUtama.SetActive(true);
        panelLoadGame.SetActive(false);
        panelPengaturan.SetActive(false);
    }

    // --- FUNGSI LAINNYA ---
    private void BuatTombol(int slot, string teks) 
    {
        GameObject btnObj = Instantiate(prefabTombolSlot, wadahContentLoad);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = teks;
        btnObj.GetComponent<Button>().onClick.AddListener(() => {
            SaveManager.slotUntukDiload = slot;
            SceneManager.LoadScene(namaSceneGame);
        });
    }

    public void KeluarGame() 
    { 
        Application.Quit(); 
    }
}