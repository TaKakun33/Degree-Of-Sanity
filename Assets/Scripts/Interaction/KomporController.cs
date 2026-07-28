using UnityEngine;

public class KomporController : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Tarik UI Panel Masak ke sini")]
    public GameObject panelMasak;

    // Fungsi ini kini dipanggil otomatis oleh PlayerController saat karakter sampai
    public void BukaMenuMasak()
    {
        // Cek apakah ada panel lain yang terbuka
        if (GameManager.Instance != null && GameManager.Instance.ApakahAdaPanelAktif()) 
        {
            return; 
        }

        if (panelMasak != null)
        {
            panelMasak.SetActive(true); 
            
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null) player.SetMenuStatus(true); // Kunci pergerakan
        }
    }
}