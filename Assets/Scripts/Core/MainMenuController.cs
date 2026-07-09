using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuController : MonoBehaviour
{
    [Header("Referensi Panel UI")]
    public GameObject panelUtama;
    public GameObject panelLoadGame;
    public GameObject panelPengaturan;

    [Header("Pengaturan Nama Scene")]
    [Tooltip("Ketik nama scene game utama Anda di sini persis seperti huruf besar/kecilnya")]
    public string namaSceneGame = "SampleScene"; // Pastikan nama ini cocok dengan Scene Gameplay Anda!

    void Start()
    {
        KembaliKeMenuUtama();
    }

    // --- 1. MULAI GAME BARU ---
    public void MulaiGameBaru()
    {
        Debug.Log("Memulai Game Baru! Mengabaikan save data lama...");
        
        // Memberi tahu SaveManager bahwa ini New Game (jangan load apapun)
        if (SaveManager.Instance != null) SaveManager.slotUntukDiload = -1; 
        
        SceneManager.LoadScene(namaSceneGame);
    }

    // --- 2. LANJUTKAN (CONTINUE) ---
    public void LanjutkanGame()
    {
        Debug.Log("Melanjutkan Game! Memuat autosave terakhir...");
        
        // Memberi tahu SaveManager untuk memuat Slot 0 (Autosave ganti hari)
        if (SaveManager.Instance != null) SaveManager.slotUntukDiload = 0; 
        
        SceneManager.LoadScene(namaSceneGame);
    }

    // --- 3. BUKA MENU LOAD GAME ---
    public void BukaMenuLoadGame()
    {
        panelUtama.SetActive(false);
        panelLoadGame.SetActive(true);
        panelPengaturan.SetActive(false);
    }

    // Fungsi tambahan untuk tombol "Save Data 1", "Save Data 2" di dalam Panel Load Game
    public void LoadSaveDataSpesifik(int nomorSave)
    {
        Debug.Log("Memuat Save Data nomor: " + nomorSave);
        
        // Memberi tahu SaveManager untuk memuat slot sesuai angka yang diklik di menu
        if (SaveManager.Instance != null) SaveManager.slotUntukDiload = nomorSave; 
        
        SceneManager.LoadScene(namaSceneGame);
    }

    // --- 4. BUKA MENU PENGATURAN ---
    public void BukaMenuPengaturan()
    {
        panelUtama.SetActive(false);
        panelLoadGame.SetActive(false);
        panelPengaturan.SetActive(true);
    }

    // --- FUNGSI KEMBALI ---
    public void KembaliKeMenuUtama()
    {
        panelUtama.SetActive(true);
        panelLoadGame.SetActive(false);
        panelPengaturan.SetActive(false);
    }

    // --- 5. KELUAR GAME ---
    public void KeluarGame()
    {
        Debug.Log("Keluar dari Game (Aplikasi tertutup).");
        Application.Quit();
    }
}