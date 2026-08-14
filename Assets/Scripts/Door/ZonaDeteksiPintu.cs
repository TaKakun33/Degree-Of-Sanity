using UnityEngine;

// --- Zona deteksi "pemain lewat dekat pintu" - TERPISAH dari collider klik di PintuRuangan.cs.
// Tempel di CHILD OBJECT baru di bawah object pintu, dengan Collider2D LEBIH BESAR (IsTrigger
// dicentang) yang nutupin area jalan masuk ke ruangan. Dipisah biar collider klik di pintu
// tetap kecil & presisi, gak pernah numpuk sama pemain yang lagi berdiri di area itu. ---
public class ZonaDeteksiPintu : MonoBehaviour
{
    [Tooltip("Drag object PintuRuangan (biasanya parent dari object ini) ke sini")]
    public PintuRuangan pintu;

    void OnTriggerEnter2D(Collider2D lain)
    {
        if (!lain.CompareTag("Player")) return;
        if (pintu != null) pintu.BukaOtomatis();
    }

    // --- TAMBAHAN: pintu otomatis nutup lagi begitu pemain keluar dari area zona ini ---
    void OnTriggerExit2D(Collider2D lain)
    {
        if (!lain.CompareTag("Player")) return;
        if (pintu != null) pintu.TutupOtomatis();
    }
}