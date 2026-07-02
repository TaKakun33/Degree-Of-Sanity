using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk berpindah Scene

public class DeskController : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Ketikkan nama Scene minigame skripsi Anda di sini (harus sama persis)")]
    public string namaSceneMinigame = "SkripsiScene"; 

    // Fungsi ini dipanggil oleh PlayerController saat karakter sudah tiba di meja
    public void MulaiSkripsi()
    {
        Debug.Log("Pindah ke scene minigame skripsi...");
        
        // Memuat scene baru berdasarkan nama yang diketik di Inspector
        SceneManager.LoadScene(namaSceneMinigame);
    }
}