using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    [Header("Tombol Toggle Mode Hapus (Panel Load Game)")]
    [Tooltip("Opsional: assign Text/TMP di tombol toggle mode hapus, biar teksnya otomatis berubah 'Hapus Save' <-> 'Batal Hapus'.")]
    public TextMeshProUGUI textTombolModeHapus;

    // --- TAMBAHAN: status mode di panel Load Game, false = mode Load biasa, true = mode Hapus ---
    private bool modeHapusAktif = false;
    
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
        ResetModeHapus(); // --- TAMBAHAN: tiap kali panel Load Game dibuka dari Main Menu, mulai dari mode Load biasa ---
        RefreshDaftarLoad();
    }

    // --- TAMBAHAN: Refresh isi scroll view sesuai daftar slot yang ada & mode saat ini ---
    void RefreshDaftarLoad()
    {
        foreach (Transform anak in wadahContentLoad) Destroy(anak.gameObject);
        if (SaveManager.Instance.CekSaveAda(0)) BuatTombol(0, "[AUTOSAVE] " + SaveManager.Instance.DapatkanInfoSave(0));

        // Dinamis, tidak dibatasi angka 10 lagi
        List<int> daftarSlot = SaveManager.Instance.DapatkanDaftarSlotTersimpan();
        foreach (int i in daftarSlot) {
            BuatTombol(i, SaveManager.Instance.DapatkanInfoSave(i));
        }
    }

    // --- TAMBAHAN: Tombol untuk toggle antara mode Load dan mode Hapus di panel yang sama ---
    // Hubungkan tombol "Hapus Save" di panelLoadGame ke fungsi ini lewat OnClick di Inspector
    public void ToggleModeHapusSave() {
        modeHapusAktif = !modeHapusAktif;
        UpdateTeksTombolModeHapus();
        RefreshDaftarLoad(); // refresh isi scroll view sesuai mode baru
    }

    // --- TAMBAHAN: reset mode Hapus ke false ---
    void ResetModeHapus() {
        modeHapusAktif = false;
        UpdateTeksTombolModeHapus();
    }

    void UpdateTeksTombolModeHapus() {
        if (textTombolModeHapus != null) {
            textTombolModeHapus.text = modeHapusAktif ? "Cancel" : "Delete";
        }
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
        ResetModeHapus(); // --- TAMBAHAN: pastikan lain kali dibuka, mulai dari mode Load biasa ---
    }

    // --- FUNGSI LAINNYA ---
    private void BuatTombol(int slot, string teks) 
    {
        GameObject btnObj = Instantiate(prefabTombolSlot, wadahContentLoad);
        TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();

        if (modeHapusAktif) {
            // --- MODE HAPUS: klik tombol slot ini akan menghapus save-nya ---
            label.text = "[HAPUS] " + teks;
            btnObj.GetComponent<Button>().onClick.AddListener(() => {
                SaveManager.Instance.HapusSave(slot);
                RefreshDaftarLoad(); // refresh daftar setelah dihapus
            });
        } else {
            // --- MODE LOAD (default): klik tombol slot ini akan load game ---
            label.text = teks;
            btnObj.GetComponent<Button>().onClick.AddListener(() => {
                SaveManager.slotUntukDiload = slot;
                SceneManager.LoadScene(namaSceneGame);
            });
        }
    }

    public void KeluarGame() 
    { 
        Application.Quit(); 
    }
}