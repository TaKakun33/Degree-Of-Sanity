using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk berpindah Scene

public class DeskController : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Ketikkan nama Scene minigame skripsi Anda di sini (harus sama persis)")]
    public string namaSceneMinigame = "SkripsiScene"; 

    [Tooltip("Pesan yang ditampilkan di popup kalau skripsi udah dikerjakan hari ini")]
    public string pesanPeringatan = "Skripsi cuma bisa dikerjakan 1x per hari. Coba lagi besok!";
    [Tooltip("TAMBAHAN: pesan yang ditampilkan kalau progres udah mentok di plafon Threshold saat ini (syarat tambahan belum kepenuhan)")]
    public string pesanMentokThreshold = "Belum bisa lanjut nulis. Ada yang perlu diselesaikan dulu.";

    // Fungsi ini dipanggil oleh PlayerController saat karakter sudah tiba di meja
    public void MulaiSkripsi()
    {
        Debug.Log($"[DeskController] MulaiSkripsi() TERPANGGIL. progresSkripsi={(GameManager.Instance != null ? GameManager.Instance.progresSkripsi.ToString("F1") : "GameManager NULL")}, batasProgresMaksimalSaatIni={(GameManager.Instance != null ? GameManager.Instance.batasProgresMaksimalSaatIni.ToString("F1") : "N/A")}"); // --- SEMENTARA ---

        // Cegah dobel-load kalau tombol/interaksi kepencet 2x sebelum scene selesai load
        if (SceneManager.GetSceneByName(namaSceneMinigame).isLoaded) return;
 
        // --- Skripsi cuma boleh dikerjakan 1x per hari ---
        if (GameManager.Instance != null && !GameManager.Instance.BisaKerjakanSkripsiHariIni) {
            // --- TAMBAHAN: pakai popup bareng (NotifikasiPopup), bukan panel sendiri lagi ---
            if (NotifikasiPopup.Instance != null) NotifikasiPopup.Instance.Tampilkan(pesanPeringatan);
            else Debug.Log(pesanPeringatan);
            return;
        }

        // --- TAMBAHAN: kalau progres udah mentok di plafon Threshold saat ini (syarat tambahan
        // belum kepenuhan, misal cicilan pertama belum lunas), JANGAN izinin mulai sesi baru -
        // toh progresnya bakal langsung ke-clamp balik, sesi jadi sia-sia ---
        if (GameManager.Instance != null && GameManager.Instance.progresSkripsi >= GameManager.Instance.batasProgresMaksimalSaatIni) {
            Debug.Log("[DeskController] DIBLOKIR - progres udah mentok plafon Threshold."); // --- SEMENTARA ---
            if (NotifikasiPopup.Instance != null) NotifikasiPopup.Instance.Tampilkan(pesanMentokThreshold);
            else Debug.Log(pesanMentokThreshold);
            return;
        }

        Debug.Log("[DeskController] Lolos semua pengecekan - buka scene minigame."); // --- SEMENTARA ---
        SceneManager.LoadScene(namaSceneMinigame, LoadSceneMode.Additive);
    }
}