using UnityEngine;

public class ExitDoorController : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Tarik UI Panel Pop-up Menu Kerja ke sini")]
    public GameObject jobMenuPopUp;

    // Fungsi ini dipanggil oleh PlayerController saat karakter tiba di pintu keluar
    public void BukaMenuKerja()
    {
        // PENCEGAHAN: Cek dulu ke GameManager, kalau ada panel yang buka, batalkan!
        if (GameManager.Instance != null && GameManager.Instance.ApakahAdaPanelAktif()) 
        {
            return; 
        }

        if (jobMenuPopUp != null)
        {
            jobMenuPopUp.SetActive(true);
            // Menggunakan metode terbaru agar tidak muncul warning
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null) player.SetMenuStatus(true);

            // --- JEDA WAKTU HARIAN ---
            if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(true);
        }
    }
}