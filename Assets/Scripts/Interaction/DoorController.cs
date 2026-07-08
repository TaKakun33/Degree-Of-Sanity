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
            // PERBAIKAN: Memindahkan pemain ke koordinat X dan Y pintu tujuan secara utuh!
            // Menggunakan Vector3 agar posisi depan-belakang (Z) dari layer karakter tidak rusak.
            player.transform.position = new Vector3(destination.position.x, destination.position.y, player.transform.position.z);
            
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