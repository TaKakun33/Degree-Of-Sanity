using UnityEngine;

public class ExitDoorController : MonoBehaviour
{
    [Header("Batasan Harian")]
    [Tooltip("Pesan yang ditampilkan di popup kalau kerja part time udah dilakukan hari ini")]
    public string pesanPeringatanPartTime = "Kerja part time cuma bisa 1x per hari. Coba lagi besok!";

    void Awake()
    {
        // --- TAMBAHAN: pasang syarat ke PintuRuangan di object yang sama, biar klik LANGSUNG
        // ke pintu (bukan cuma lewat zona/job menu) juga gak bisa buka pintu ini kalau jatah
        // kerja part time hari ini udah kepakai ---
        PintuRuangan pintu = GetComponent<PintuRuangan>();
        if (pintu != null) {
            pintu.syaratBolehBuka = () => GameManager.Instance != null && GameManager.Instance.BisaKerjaPartTimeHariIni;
        }
    }

    // Fungsi ini dipanggil oleh PlayerController saat karakter tiba di pintu keluar
    public void BukaMenuKerja()
    {
        if (GameManager.Instance == null) return;

        // --- Cek jatah harian DI SINI, SEBELUM panel pilihan kerja dibuka sama sekali ---
        if (!GameManager.Instance.BisaKerjaPartTimeHariIni)
        {
            if (NotifikasiPopup.Instance != null) NotifikasiPopup.Instance.Tampilkan(pesanPeringatanPartTime);
            else Debug.Log(pesanPeringatanPartTime);
            return;
        }

        // --- FIX: pakai GameManager.BukaKerjaAman() - SATU sumber kebenaran tunggal buat
        // panelMenuKerja (dulu ada field LOKAL "jobMenuPopUp" terpisah dari GameManager.panelMenuKerja,
        // rawan gak sinkron - itu penyebab kontrol pemain kadang gak ke-kunci dengan benar) ---
        if (GameManager.Instance.ApakahAdaPanelAktif()) return;
        GameManager.Instance.BukaKerjaAman();

        // --- JEDA WAKTU HARIAN ---
        GameManager.Instance.SetJedaWaktu(true);
    }
}