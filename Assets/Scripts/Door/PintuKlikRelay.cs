using UnityEngine;
using UnityEngine.InputSystem;

// --- Klik PRESISI di pintu (lewat Layer, sama kayak sebelumnya) - TAPI sekarang klik di pintu
// yang TERTUTUP gak langsung buka, cuma NYURUH KARAKTER JALAN KE SITU DULU. Yang beneran
// buka pintunya: ZonaDeteksiPintu (buat pintu ruangan biasa) atau alur job menu (buat pintu
// exit kerja) - keduanya udah ada, gak perlu diubah. Klik di pintu yang UDAH TERBUKA tetap
// langsung nutup instan (gak perlu jalan dulu buat nutup). ---
public class PintuKlikRelay : MonoBehaviour
{
    [Tooltip("Drag object PintuRuangan (parent dari kedua object pintu ini) ke sini")]
    public PintuRuangan pintu;

    [Tooltip("WAJIB diisi: set ke Layer yang SAMA kayak Layer object ini sendiri (misal 'PintuKlik'), BUKAN Default, BUKAN sama kayak Layer ZonaDeteksi")]
    public LayerMask layerKlikPintu;

    private Collider2D colliderSaya;
    private PlayerController player;

    void Awake()
    {
        colliderSaya = GetComponent<Collider2D>();
        player = Object.FindFirstObjectByType<PlayerController>();

        if (colliderSaya == null) Debug.LogError($"[PintuKlikRelay:{name}] TIDAK ADA Collider2D di object ini!");
        if (player == null) Debug.LogError($"[PintuKlikRelay:{name}] Gak nemu PlayerController di scene!");
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (Mouse.current == null || Camera.main == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 posisiMouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D kenaKlik = Physics2D.OverlapPoint(posisiMouseWorld, layerKlikPintu);

            if (kenaKlik == colliderSaya) {
                TanganiKlik();
            }
        }
    }

    void TanganiKlik()
    {
        if (pintu == null) return;

        if (pintu.SedangTerbuka) {
            // --- Udah terbuka: tutup langsung, gak perlu jalan dulu ---
            pintu.Toggle();
        } else {
            // --- TAMBAHAN: masih tertutup -> jalanin karakter ke posisi pintu ini dulu,
            // JANGAN langsung dibuka. ZonaDeteksiPintu (kalau ada) yang bakal buka otomatis
            // begitu karakter beneran nyampe/nyentuh. ---
            if (player != null) player.JalanKeTitik(transform.position);
        }
    }
}