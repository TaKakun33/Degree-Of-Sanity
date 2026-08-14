using UnityEngine;

public class PintuRuangan : MonoBehaviour
{
    [Header("Dua Objek Pintu")]
    [Tooltip("GameObject yang AKTIF pas pintu TERBUKA (punya sprite + Collider2D sendiri)")]
    public GameObject objekPintuTerbuka;
    [Tooltip("GameObject yang AKTIF pas pintu TERTUTUP (punya sprite + Collider2D sendiri)")]
    public GameObject objekPintuTertutup;

    [Header("Status Awal")]
    public bool mulaiTerbuka = false;

    [Header("Audio Efek Pintu")]
    [Tooltip("Komponen AudioSource untuk suara pintu (bisa ditaruh di GameObject ini)")]
    public AudioSource audioSourcePintu;
    [Tooltip("Sound effect saat pintu terbuka")]
    public AudioClip klipPintuTerbuka;
    [Tooltip("Sound effect saat pintu tertutup")]
    public AudioClip klipPintuTertutup;
    [Range(0f, 1f)]
    public float volumePintu = 0.8f;

    public System.Func<bool> syaratBolehBuka;

    private bool sedangTerbuka;
    private bool statusSebelumnya; // --- TAMBAHAN: Untuk mendeteksi perubahan status buka/tutup ---

    void Start()
    {
        sedangTerbuka = mulaiTerbuka;
        statusSebelumnya = sedangTerbuka; // Inisialisasi awal

        if (objekPintuTerbuka == null) Debug.LogError($"[PintuRuangan:{name}] Objek Pintu Terbuka belum diisi!");
        if (objekPintuTertutup == null) Debug.LogError($"[PintuRuangan:{name}] Objek Pintu Tertutup belum diisi!");

        TerapkanStatus();
    }

    public void Toggle()
    {
        if (!sedangTerbuka && syaratBolehBuka != null && !syaratBolehBuka())
        {
            Debug.Log($"[PintuRuangan:{name}] Toggle() dibatalkan - syarat buka belum terpenuhi.");
            return;
        }

        sedangTerbuka = !sedangTerbuka;
        Debug.Log($"[PintuRuangan:{name}] Toggle() - status sekarang: {(sedangTerbuka ? "TERBUKA" : "TERTUTUP")}");
        TerapkanStatus();
    }

    public void BukaOtomatis()
    {
        if (sedangTerbuka) return;
        sedangTerbuka = true;
        TerapkanStatus();
    }

    public void TutupOtomatis()
    {
        if (!sedangTerbuka) return;
        Debug.Log($"[PintuRuangan:{name}] TutupOtomatis() - pemain menjauh.");
        sedangTerbuka = false;
        TerapkanStatus();
    }

    void TerapkanStatus()
    {
        if (objekPintuTerbuka != null) objekPintuTerbuka.SetActive(sedangTerbuka);
        if (objekPintuTertutup != null) objekPintuTertutup.SetActive(!sedangTerbuka);

        // --- TAMBAHAN: Mainkan suara berdasarkan perubahan status pintu ---
        if (sedangTerbuka != statusSebelumnya)
        {
            if (audioSourcePintu != null)
            {
                if (sedangTerbuka && klipPintuTerbuka != null)
                {
                    audioSourcePintu.PlayOneShot(klipPintuTerbuka, volumePintu);
                }
                else if (!sedangTerbuka && klipPintuTertutup != null)
                {
                    audioSourcePintu.PlayOneShot(klipPintuTertutup, volumePintu);
                }
            }
            statusSebelumnya = sedangTerbuka;
        }
    }

    public bool SedangTerbuka => sedangTerbuka;
}