using UnityEngine;

// --- Pintu Ruangan versi 2-OBJEK: 1 GameObject buat "Pintu Terbuka", 1 lagi buat "Pintu
// Tertutup" - tinggal SetActive() gantian, gak perlu ganti sprite manual/khawatir collider
// gak pas lagi. Posisi/bentuk pintu terbuka & tertutup bisa BEDA & diatur independen (misal
// pintu kebuka geser ke samping) - drag masing-masing ke posisi yang kamu mau di Scene view. ---
public class PintuRuangan : MonoBehaviour
{
    [Header("Dua Objek Pintu")]
    [Tooltip("GameObject yang AKTIF pas pintu TERBUKA (punya sprite + Collider2D sendiri)")]
    public GameObject objekPintuTerbuka;
    [Tooltip("GameObject yang AKTIF pas pintu TERTUTUP (punya sprite + Collider2D sendiri)")]
    public GameObject objekPintuTertutup;

    [Header("Status Awal")]
    public bool mulaiTerbuka = false;

    // --- TAMBAHAN: syarat opsional buat pintu ini. Diisi lewat kode (bukan Inspector) oleh script
    // lain, misal ExitDoorController, buat mem-blok KLIK MANUAL (Toggle()) kalau syarat gak
    // terpenuhi - CUMA ngaruh ke Toggle(), gak ngaruh ke BukaOtomatis()/TutupOtomatis() yang
    // dipanggil sistem lain secara terprogram (itu dianggap udah "sah", gak perlu dicek ulang). ---
    public System.Func<bool> syaratBolehBuka;

    private bool sedangTerbuka;

    void Start()
    {
        sedangTerbuka = mulaiTerbuka;

        if (objekPintuTerbuka == null) Debug.LogError($"[PintuRuangan:{name}] Objek Pintu Terbuka belum diisi!");
        if (objekPintuTertutup == null) Debug.LogError($"[PintuRuangan:{name}] Objek Pintu Tertutup belum diisi!");

        TerapkanStatus();
    }

    // --- Dipanggil dari PintuKlikRelay.cs, yang nempel di objekPintuTerbuka MAUPUN objekPintuTertutup ---
    public void Toggle()
    {
        // --- TAMBAHAN: kalau lagi mau BUKA (bukan tutup) dan ada syarat yang gak terpenuhi, batalkan ---
        if (!sedangTerbuka && syaratBolehBuka != null && !syaratBolehBuka())
        {
            Debug.Log($"[PintuRuangan:{name}] Toggle() dibatalkan - syarat buka belum terpenuhi."); // --- SEMENTARA ---
            return;
        }

        sedangTerbuka = !sedangTerbuka;
        Debug.Log($"[PintuRuangan:{name}] Toggle() - status sekarang: {(sedangTerbuka ? "TERBUKA" : "TERTUTUP")}"); // --- SEMENTARA ---
        TerapkanStatus();
    }

    // --- Opsional: dipanggil ZonaDeteksiPintu.cs pas pemain lewat dekat ---
    public void BukaOtomatis()
    {
        if (sedangTerbuka) return;
        sedangTerbuka = true;
        TerapkanStatus();
    }

    // --- TAMBAHAN: dipanggil ZonaDeteksiPintu.cs pas pemain MENJAUH dari area pintu ---
    public void TutupOtomatis()
    {
        if (!sedangTerbuka) return;
        Debug.Log($"[PintuRuangan:{name}] TutupOtomatis() - pemain menjauh."); // --- SEMENTARA ---
        sedangTerbuka = false;
        TerapkanStatus();
    }

    void TerapkanStatus()
    {
        if (objekPintuTerbuka != null) objekPintuTerbuka.SetActive(sedangTerbuka);
        if (objekPintuTertutup != null) objekPintuTertutup.SetActive(!sedangTerbuka);
    }

    public bool SedangTerbuka => sedangTerbuka;
}