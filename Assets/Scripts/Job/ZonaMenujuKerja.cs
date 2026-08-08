using UnityEngine;

// --- Zona deteksi SEBELUM pintu keluar kerja - begitu Player masuk sini, TAMPILIN job menu
// dulu (pintu MASIH TERTUTUP di titik ini). Pintu baru kebuka kalau pemain beneran pilih
// salah satu kerja (lihat JobMenuController.cs) - kalau pencet Cancel, pintu tetap tertutup. ---
public class ZonaMenujuKerja : MonoBehaviour
{
    [Tooltip("Drag object yang punya ExitDoorController ke sini")]
    public ExitDoorController exitDoor;

    void OnTriggerEnter2D(Collider2D lain)
    {
        if (!lain.CompareTag("Player")) return;
        if (exitDoor != null) exitDoor.BukaMenuKerja();
    }
}