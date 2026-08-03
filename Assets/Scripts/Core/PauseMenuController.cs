using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referensi Panel UI")]
    public GameObject panelPause, panelSave, panelLoad, panelSettings;
    
    [Header("Sistem UI Dinamis")]
    public GameObject prefabTombol; 
    public Transform contentSave, contentLoad;

    [Header("Tombol Toggle Mode Hapus (Panel Load)")]
    [Tooltip("Opsional: assign Text/TMP di tombol toggle mode hapus, biar teksnya otomatis berubah 'Hapus Save' <-> 'Batal Hapus'.")]
    public TextMeshProUGUI textTombolModeHapus;

    // --- TAMBAHAN: status mode di panel Load, false = mode Load biasa, true = mode Hapus ---
    private bool modeHapusAktif = false;

    [Header("Pengaturan")]
    public string namaSceneMainMenu = "MainMenu";

    void Update() 
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) 
        {
            if (Time.timeScale == 0) LanjutkanGame(); 
            else BukaPause();
        }
    }

    // --- NAVIGASI UTAMA ---
    public void BukaPause() 
    { 
        // --- TAMBAHAN: Tutup otomatis semua panel game sebelum pause ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TutupSemuaPanelGame();
        }

        Time.timeScale = 0; 
        TutupSemuaPanel(); 
        panelPause.SetActive(true); 
        ResetModeHapus(); // --- TAMBAHAN: pastikan panel Load selalu mulai dari mode Load biasa ---
    }
    
    public void LanjutkanGame() 
    { 
        Time.timeScale = 1; 
        TutupSemuaPanel(); 
    }

    public void BukaSettings() { TutupSemuaPanel(); panelSettings.SetActive(true); }
    public void KembaliKePause() { TutupSemuaPanel(); panelPause.SetActive(true); ResetModeHapus(); }

    // --- MENU SAVE (DINAMIS) ---
    public void BukaSaveAs() {
        TutupSemuaPanel(); panelSave.SetActive(true);
        foreach (Transform child in contentSave) Destroy(child.gameObject);
        
        // 1. Tampilkan List Save yang ada (dinamis, TIDAK dibatasi maxSaveSlots lagi)
        List<int> daftarSlot = SaveManager.Instance.DapatkanDaftarSlotTersimpan();
        foreach (int i in daftarSlot) {
            int slot = i; // hindari closure bug pada lambda di bawah
            GameObject btn = Instantiate(prefabTombol, contentSave);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = SaveManager.Instance.DapatkanInfoSave(slot);
            btn.GetComponent<Button>().onClick.AddListener(() => {
                SaveManager.Instance.SimpanGame(slot);
                BukaSaveAs(); 
            });
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

    // --- MENU LOAD (DINAMIS) ---
    public void BukaLoadGame() {
        TutupSemuaPanel(); panelLoad.SetActive(true);
        foreach (Transform child in contentLoad) Destroy(child.gameObject);
        
        if (SaveManager.Instance.CekSaveAda(0)) BuatTombolSlot(0, "[AUTOSAVE] " + SaveManager.Instance.DapatkanInfoSave(0));

        // Dinamis, TIDAK dibatasi maxSaveSlots lagi
        List<int> daftarSlot = SaveManager.Instance.DapatkanDaftarSlotTersimpan();
        foreach (int i in daftarSlot) {
            BuatTombolSlot(i, SaveManager.Instance.DapatkanInfoSave(i));
        }
    }

    // --- TAMBAHAN: Tombol untuk toggle antara mode Load dan mode Hapus di panel yang sama ---
    // Hubungkan tombol "Hapus Save" di panelLoad ke fungsi ini lewat OnClick di Inspector
    public void ToggleModeHapusSave() {
        modeHapusAktif = !modeHapusAktif;
        UpdateTeksTombolModeHapus();
        BukaLoadGame(); // refresh isi scroll view sesuai mode baru
    }

    // --- TAMBAHAN: reset mode Hapus ke false (dipanggil tiap keluar/masuk ulang ke menu Pause) ---
    void ResetModeHapus() {
        modeHapusAktif = false;
        UpdateTeksTombolModeHapus();
    }

    void UpdateTeksTombolModeHapus() {
        if (textTombolModeHapus != null) {
            textTombolModeHapus.text = modeHapusAktif ? "Cancel" : "Delete";
        }
    }

    // --- TAMBAHAN: hapus SEMUA save (termasuk autosave) - hubungkan tombol "Reset" di panelLoad ke sini ---
    public void ResetSemuaSave()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.HapusSemuaSave();
        BukaLoadGame(); // refresh isi scroll view (bakal kosong total setelah direset)
    }

    // --- Membuat satu tombol slot di scroll view Load. ---
    // Isi & aksinya berubah tergantung mode: Load (default) atau Hapus (kalau modeHapusAktif aktif)
    void BuatTombolSlot(int slot, string teks) {
        GameObject btn = Instantiate(prefabTombol, contentLoad);
        TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();

        if (modeHapusAktif) {
            // --- MODE HAPUS: klik tombol slot ini akan menghapus save-nya ---
            label.text = "[HAPUS] " + teks;
            btn.GetComponent<Button>().onClick.AddListener(() => {
                SaveManager.Instance.HapusSave(slot);
                BukaLoadGame(); // refresh daftar setelah dihapus
            });
        } else {
            // --- MODE LOAD (default): klik tombol slot ini akan load game ---
            label.text = teks;
            btn.GetComponent<Button>().onClick.AddListener(() => {
                SaveManager.Instance.UpdateSlotTerakhir(slot);
                SaveManager.slotUntukDiload = slot;
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
    }

    // --- FUNGSI PENTING ---
    public void TutupSemuaPanel() {
        panelPause.SetActive(false);
        panelSave.SetActive(false);
        panelLoad.SetActive(false);
        panelSettings.SetActive(false);
    }

    public void KembaliKeMainMenu() { Time.timeScale = 1; SceneManager.LoadScene(namaSceneMainMenu); }
    public void KeluarKeDesktop() { Application.Quit(); }
}