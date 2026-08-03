using UnityEngine;
using UnityEngine.SceneManagement; 

public class JobMenuController : MonoBehaviour
{
    [Header("Pengaturan Scene Kerja Part Time")]
    public string sceneKasir = "KasirScene";
    public string sceneOjol = "OjolScene";
    public string sceneTutor = "TutorScene";

    [Header("Referensi Player")]
    [Tooltip("Tarik objek Player Anda ke sini")]
    public PlayerController player; 

    // Panggil fungsi ini saat tombol kerja diklik
    public void PilihKasir()
    {
        // Sebelum pindah scene, buka kunci pergerakan agar tidak bug saat kembali
        player.SetMenuStatus(false); 
        if (GameManager.Instance != null) {
            GameManager.Instance.SetJedaWaktu(false); // Kembalikan waktu
            GameManager.Instance.TandaiKerjaPartTimeSudahDilakukan(); // Tandai jatah harian terpakai
        }

        // --- PENTING: KasirScene dimuat SINGLE, GameManager di sini bakal HANCUR.
        // Autosave dulu ke slot 0, supaya balik nanti GameManager reload state SAAT INI. ---
        if (SaveManager.Instance != null) {
            SaveManager.Instance.SimpanGame(0);
            SaveManager.slotUntukDiload = 0;
        }

        SceneManager.LoadScene(sceneKasir);
    }

    public void PilihOjekOnline()
    {
        player.SetMenuStatus(false);
        if (GameManager.Instance != null) {
            GameManager.Instance.SetJedaWaktu(false);
            GameManager.Instance.TandaiKerjaPartTimeSudahDilakukan();
        }

        if (SaveManager.Instance != null) {
            SaveManager.Instance.SimpanGame(0);
            SaveManager.slotUntukDiload = 0;
        }

        SceneManager.LoadScene(sceneOjol);
    }

    public void PilihHometutor()
    {
        player.SetMenuStatus(false);
        if (GameManager.Instance != null) {
            GameManager.Instance.SetJedaWaktu(false);
            GameManager.Instance.TandaiKerjaPartTimeSudahDilakukan();
        }

        if (SaveManager.Instance != null) {
            SaveManager.Instance.SimpanGame(0);
            SaveManager.slotUntukDiload = 0;
        }

        SceneManager.LoadScene(sceneTutor);
    }

    // Dipanggil saat tombol Batal/Tutup diklik
    public void TutupMenu() 
    { 
        gameObject.SetActive(false); 
        if (player != null)
        {
            player.SetMenuStatus(false);
        }
        else
        {
            Debug.LogError("Referensi player hilang di JobMenuController!");
        }

        // --- KEMBALIKAN WAKTU HARIAN ---
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);
    }
}