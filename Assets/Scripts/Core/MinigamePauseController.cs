using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// --- Fitur Pause SEDERHANA, dipasang di SEMUA scene minigame (Skripsi/Kasir/Ojol/Tutor).
// TIDAK ADA Save/Load di sini - kalau pemain mau berhenti main di tengah minigame, tinggal
// pencet "Main Menu" (kembali ke title screen). Nanti kalau pilih "Continue"/"Load" dari Main
// Menu, otomatis balik ke state SEBELUM minigame ini dimulai (autosave slot 0 buat Kasir/Ojol/
// Tutor yang udah otomatis kejadian SEBELUM masuk minigame - progres di dalam minigame yang
// belum selesai ini SIMPLY DIBUANG, dianggap belum pernah dimulai). ---
public class MinigamePauseController : MonoBehaviour
{
    [Header("WAJIB diisi per-scene")]
    [Tooltip("Nama scene Main Menu (title screen)")]
    public string namaSceneMainMenu = "MainMenu";

    [Header("KHUSUS scene Kerja Part Time (Kasir/Ojol/Tutor) - kosongkan/uncheck kalau ini scene Skripsi")]
    [Tooltip("Centang KHUSUS di scene Kasir/Ojol/Tutor - biar tombol Main Menu otomatis BATALKAN jatah kerja hari ini + kembalikan posisi ke Titik Spawn, biar Continue nanti gak nyangkut di luar rumah dengan jatah kerja abis")]
    public bool iniMinigameKerjaPartTime = false;
    [Tooltip("WAJIB diisi kalau di atas dicentang: koordinat X/Y PERSIS Titik_SpawnPlayer di scene utama (copy dari Transform Position-nya di Inspector scene utama - TIDAK BISA drag langsung soalnya beda scene)")]
    public Vector2 posisiSpawnUntukBatal;
    [Tooltip("Lantai tempat Titik Spawn berada (biasanya 1)")]
    public int lantaiSpawnUntukBatal = 1;

    [Header("UI Pause")]
    public GameObject panelPause;

    private bool sedangPause = false;

    void Awake()
    {
        if (panelPause) panelPause.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) {
            if (sedangPause) LanjutkanGame(); else BukaPause();
        }
    }

    public void BukaPause()
    {
        sedangPause = true;
        Time.timeScale = 0f;
        AudioListener.pause = true; // --- TAMBAHAN: matiin SEMUA audio (BGM/SFX) sementara - otomatis nginget posisi playback-nya ---
        if (panelPause) {
            panelPause.SetActive(true);
            panelPause.transform.SetAsLastSibling(); // --- biar gak ketiban item yang di-spawn dinamis (misal conveyor Kasir) ---
        }
    }

    public void LanjutkanGame()
    {
        sedangPause = false;
        Time.timeScale = 1f;
        AudioListener.pause = false; // --- TAMBAHAN: nyalain lagi audio - lanjut dari posisi terakhir sebelum di-pause ---
        if (panelPause) panelPause.SetActive(false);
    }

    // --- Keluar minigame TANPA save apapun - progres sesi ini dibuang, kembali ke Main Menu.
    // "Continue" dari Main Menu nanti otomatis balik ke state SEBELUM minigame ini dimulai. ---
    public void KembaliKeMainMenu()
    {
        // --- TAMBAHAN: KHUSUS scene Kerja Part Time - autosave slot 0 yang kejadian pas
        // berangkat kerja udah kejebak (jatah kerja=true, posisi=luar rumah). Koreksi dulu
        // SEBELUM pindah ke Main Menu, biar Continue nanti balik normal (jatah kerja masih
        // ada, posisi di Titik Spawn) - bukan nyangkut di luar rumah dengan jatah abis. ---
        if (iniMinigameKerjaPartTime && SaveManager.Instance != null) {
            SaveManager.Instance.BatalkanPartTimeHariIni(posisiSpawnUntukBatal, lantaiSpawnUntukBatal);
        }

        Time.timeScale = 1f;
        AudioListener.pause = false; // --- TAMBAHAN: jaga-jaga - AudioListener.pause itu setting GLOBAL, gak otomatis kereset pas ganti scene, jadi Main Menu bisa nyangkut ke-mute kalau gak direset di sini ---
        SceneManager.LoadScene(namaSceneMainMenu, LoadSceneMode.Single);
    }

    public void KeluarKeDesktop()
    {
        Application.Quit();
    }
}