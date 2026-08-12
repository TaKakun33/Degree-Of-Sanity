using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Tujuan Pintu")]
    [Tooltip("Tarik objek/pintu di lantai tujuan ke kolom ini")]
    public Transform destination;
    
    [Tooltip("Pintu ini akan memindahkan pemain ke lantai berapa?")]
    public int lantaiTujuan = 2; 

    public void UseDoor(GameObject player)
    {
        if (destination != null)
        {
            // --- TAMBAHAN: Y sekarang dari KonfigurasiLantai (1 sumber tunggal per lantai),
            // BUKAN dari destination.position.y lagi - biar gak rawan salah presisi manual
            // per-pintu kayak sebelumnya. X tetap dari destination (posisi horizontal tangga). ---
            float yTujuan = (KonfigurasiLantai.Instance != null)
                ? KonfigurasiLantai.Instance.DapatkanPosisiY(lantaiTujuan)
                : destination.position.y; // fallback kalau KonfigurasiLantai belum ke-setup di scene

            Vector2 posisiBaru = new Vector2(destination.position.x, yTujuan);

            // --- FIX (dipertahankan): pakai rb.position (physics-aware), BUKAN transform.position
            // langsung - kalau cuma transform.position yang di-set, Rigidbody2D (Gravity Scale=0,
            // gak ada gravitasi buat "nyettle" otomatis) bisa desync dari posisi fisik internalnya. ---
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.position = posisiBaru;
            } else {
                player.transform.position = new Vector3(posisiBaru.x, posisiBaru.y, player.transform.position.z);
            }

            // Beri tahu sistem bahwa pemain sudah pindah lantai
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.lantaiSaatIni = lantaiTujuan;
        }
        else
        {
            Debug.LogWarning("Tujuan pintu belum diatur di Inspector!");
        }
    }
}