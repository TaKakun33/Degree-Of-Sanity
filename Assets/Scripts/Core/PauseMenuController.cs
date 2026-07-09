using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referensi Panel UI")]
    public GameObject panelPause, panelSave, panelLoad, panelSettings;
    
    [Header("Sistem UI Dinamis")]
    public GameObject prefabTombol; 
    public Transform contentSave, contentLoad; 

    [Header("Pengaturan")]
    public int maxSaveSlots = 10;
    public string namaSceneMainMenu = "MainMenu";

    void Update() {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) {
            if (Time.timeScale == 0) LanjutkanGame(); else BukaPause();
        }
    }

    // --- Navigasi ---
    public void BukaPause() { Time.timeScale = 0; TutupSemuaPanel(); panelPause.SetActive(true); }
    public void LanjutkanGame() { Time.timeScale = 1; TutupSemuaPanel(); }
    public void BukaSettings() { TutupSemuaPanel(); panelSettings.SetActive(true); }
    public void KembaliKePause() { TutupSemuaPanel(); panelPause.SetActive(true); }

    public void BukaSaveAs() {
        TutupSemuaPanel(); panelSave.SetActive(true);
        foreach (Transform child in contentSave) Destroy(child.gameObject);
        
        // 1. Tampilkan List Save yang ada
        for(int i = 1; i <= maxSaveSlots; i++) {
            if (SaveManager.Instance.CekSaveAda(i)) {
                int slot = i;
                GameObject btn = Instantiate(prefabTombol, contentSave);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = SaveManager.Instance.DapatkanInfoSave(slot);
                btn.GetComponent<Button>().onClick.AddListener(() => {
                    SaveManager.Instance.SimpanGame(slot);
                    BukaSaveAs(); 
                });
            }
        }
        // 2. Tombol New Save
        GameObject btnNew = Instantiate(prefabTombol, contentSave);
        btnNew.GetComponentInChildren<TextMeshProUGUI>().text = "+ New Save";
        btnNew.GetComponent<Button>().onClick.AddListener(() => {
            int slotBaru = SaveManager.Instance.GetNextAvailableSlot();
            SaveManager.Instance.SimpanGame(slotBaru);
            BukaSaveAs();
        });
    }

    public void BukaLoadGame() {
        TutupSemuaPanel(); panelLoad.SetActive(true);
        foreach (Transform child in contentLoad) Destroy(child.gameObject);
        
        if (SaveManager.Instance.CekSaveAda(0)) BuatTombolLoad(0, "[AUTOSAVE]");
        for(int i = 1; i <= maxSaveSlots; i++) {
            if (SaveManager.Instance.CekSaveAda(i)) BuatTombolLoad(i, SaveManager.Instance.DapatkanInfoSave(i));
        }
    }

    void BuatTombolLoad(int slot, string teks) {
        GameObject btn = Instantiate(prefabTombol, contentLoad);
        btn.GetComponentInChildren<TextMeshProUGUI>().text = teks;
        btn.GetComponent<Button>().onClick.AddListener(() => {
            SaveManager.Instance.UpdateSlotTerakhir(slot);
            SaveManager.slotUntukDiload = slot;
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    public void TutupSemuaPanel() {
        panelPause.SetActive(false); panelSave.SetActive(false); 
        panelLoad.SetActive(false); panelSettings.SetActive(false);
    }

    public void KembaliKeMainMenu() { Time.timeScale = 1; SceneManager.LoadScene(namaSceneMainMenu); }
    public void KeluarKeDesktop() { Application.Quit(); }
}