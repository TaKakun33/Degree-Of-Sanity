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
        // Cegah dobel-load kalau tombol/interaksi kepencet 2x sebelum scene selesai load
        if (SceneManager.GetSceneByName(namaSceneMinigame).isLoaded) return;
 
        // --- TAMBAHAN: Skripsi cuma boleh dikerjakan 1x per hari ---
        if (GameManager.Instance != null && !GameManager.Instance.BisaKerjakanSkripsiHariIni) {
            Debug.Log("Skripsi sudah dikerjakan hari ini. Coba lagi besok.");
            return;
        }

        SceneManager.LoadScene(namaSceneMinigame, LoadSceneMode.Additive);
    }
}