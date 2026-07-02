using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Tujuan Pintu")]
    [Tooltip("Tarik objek/pintu di lantai 2 ke kolom ini di Inspector")]
    public Transform destination;

    // Fungsi ini akan dipanggil oleh Player saat sudah sampai di depan pintu
    public void UseDoor(GameObject player)
    {
        if (destination != null)
        {
            // Memindahkan pemain secara instan ke posisi tujuan (Lantai 2)
            player.transform.position = destination.position;
        }
        else
        {
            Debug.LogWarning("Tujuan pintu belum diatur di Inspector!");
        }
    }
}