using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class PauseMenuController : MonoBehaviour
{
    [Header("Referensi Panel Pause (Options)")]
    public GameObject panelPauseUtama; 
    public GameObject panelSettings;
    public GameObject panelSaveAs;
    public GameObject panelLoadGame;

    [Header("Pengaturan Scene")]
    [Tooltip("Ketik nama scene Main Menu Anda di sini (Pastikan huruf besar/kecil sama persis)")]
    public string namaSceneMainMenu = "MainMenu"; // <-- Tambahan kolom baru

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) LanjutkanGame();
            else BukaMenuPause();
        }
    }

    public void BukaMenuPause()
    {
        // Cegah pause jika panel masak/toko sedang terbuka
        if (GameManager.Instance != null && GameManager.Instance.ApakahAdaPanelAktif() && !isPaused) return;

        isPaused = true;
        Time.timeScale = 0f; // Bekukan waktu dunia game
        TutupSemuaSubPanel();
        panelPauseUtama.SetActive(true);

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(true); // Kunci gerak pemain
    }

    public void LanjutkanGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Kembalikan jalannya waktu
        TutupSemuaSubPanel();

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }

    // --- NAVIGASI SUB-PANEL ---
    public void BukaSettings() { TutupSemuaSubPanel(); panelSettings.SetActive(true); }
    public void BukaSaveAs() { TutupSemuaSubPanel(); panelSaveAs.SetActive(true); }
    public void BukaLoadGame() { TutupSemuaSubPanel(); panelLoadGame.SetActive(true); }
    public void KembaliKePauseUtama() { TutupSemuaSubPanel(); panelPauseUtama.SetActive(true); }

    private void TutupSemuaSubPanel()
    {
        if (panelPauseUtama) panelPauseUtama.SetActive(false);
        if (panelSettings) panelSettings.SetActive(false);
        if (panelSaveAs) panelSaveAs.SetActive(false);
        if (panelLoadGame) panelLoadGame.SetActive(false);
    }

    // --- FUNGSI KLIK SLOT SAVE / LOAD ---
    public void EksekusiSaveManual(int slot)
    {
        if (SaveManager.Instance != null) {
            SaveManager.Instance.SimpanGame(slot);
        }
    }

    public void EksekusiLoadManual(int slot)
    {
        if (SaveManager.Instance != null) {
            SaveManager.slotUntukDiload = slot;
            Time.timeScale = 1f; // Cairkan waktu sebelum me-restart scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // --- KELUAR GAME ---
    public void KembaliKeMainMenu()
    {
        Time.timeScale = 1f; // Wajib mencairkan waktu
        SceneManager.LoadScene(namaSceneMainMenu); // <-- Memanggil scene berdasarkan variabel Inspector
    }

    public void KeluarKeDesktop()
    {
        Application.Quit();
        Debug.Log("Quit Game ditekan.");
    }
}