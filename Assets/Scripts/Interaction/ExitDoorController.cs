using UnityEngine;

public class ExitDoorController : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Tarik UI Panel Pop-up Menu Kerja ke sini")]
    public GameObject jobMenuPopUp;

    [Header("Batasan Harian")]
    [Tooltip("Pesan yang ditampilkan di popup kalau kerja part time udah dilakukan hari ini")]
    public string pesanPeringatanPartTime = "Kerja part time cuma bisa 1x per hari. Coba lagi besok!";

    // Fungsi ini dipanggil oleh PlayerController saat karakter tiba di pintu keluar
    public void BukaMenuKerja()
    {
        // PENCEGAHAN: Cek dulu ke GameManager, kalau ada panel yang buka, batalkan!
        if (GameManager.Instance != null && GameManager.Instance.ApakahAdaPanelAktif()) 
        {
            return; 
        }

        // --- TAMBAHAN: cek jatah harian DI SINI, SEBELUM panel pilihan kerja dibuka sama sekali ---
        if (GameManager.Instance != null && !GameManager.Instance.BisaKerjaPartTimeHariIni)
        {
            if (NotifikasiPopup.Instance != null) NotifikasiPopup.Instance.Tampilkan(pesanPeringatanPartTime);
            else Debug.Log(pesanPeringatanPartTime);
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