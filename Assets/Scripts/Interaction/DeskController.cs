using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk berpindah Scene

public class DeskController : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Ketikkan nama Scene minigame skripsi Anda di sini (harus sama persis)")]
    public string namaSceneMinigame = "SkripsiScene"; 

    [Tooltip("Pesan yang ditampilkan di popup kalau skripsi udah dikerjakan hari ini")]
    public string pesanPeringatan = "Skripsi cuma bisa dikerjakan 1x per hari. Coba lagi besok!";

    // Fungsi ini dipanggil oleh PlayerController saat karakter sudah tiba di meja
    public void MulaiSkripsi()
    {
        // Cegah dobel-load kalau tombol/interaksi kepencet 2x sebelum scene selesai load
        if (SceneManager.GetSceneByName(namaSceneMinigame).isLoaded) return;
 
        // --- Skripsi cuma boleh dikerjakan 1x per hari ---
        if (GameManager.Instance != null && !GameManager.Instance.BisaKerjakanSkripsiHariIni) {
            // --- TAMBAHAN: pakai popup bareng (NotifikasiPopup), bukan panel sendiri lagi ---
            if (NotifikasiPopup.Instance != null) NotifikasiPopup.Instance.Tampilkan(pesanPeringatan);
            else Debug.Log(pesanPeringatan);
            return;
        }

        SceneManager.LoadScene(namaSceneMinigame, LoadSceneMode.Additive);
    }
}