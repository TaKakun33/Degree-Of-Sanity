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
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false); // Kembalikan waktu
        SceneManager.LoadScene(sceneKasir);
    }

    public void PilihOjekOnline()
    {
        player.SetMenuStatus(false);
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);
        SceneManager.LoadScene(sceneOjol);
    }

    public void PilihHometutor()
    {
        player.SetMenuStatus(false);
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);
        SceneManager.LoadScene(sceneTutor);
    }

    // Dipanggil saat tombol Batal/Tutup diklik
    public void TutupMenu() 
    { 
        gameObject.SetActive(false); 
        if (player != null)
        {
            player.SetMenuStatus(false);
            Debug.Log("Menu ditutup, status player dikembalikan ke: " + false);
        }
        else
        {
            Debug.LogError("Referensi player hilang di JobMenuController!");
        }

        // --- KEMBALIKAN WAKTU HARIAN ---
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);
    }
}