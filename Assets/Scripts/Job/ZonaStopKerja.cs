using UnityEngine;

// --- Zona "titik berhenti" di LUAR rumah - begitu Player (yang lagi jalan keluar buat kerja)
// nyampe sini, BARU scene minigame kerja dimuat. Taruh di posisi yang kamu mau player berhenti
// dulu sebelum masuk ke scene KasirScene/OjolScene/TutorScene. ---
public class ZonaStopKerja : MonoBehaviour
{
    [Tooltip("Drag object yang punya JobMenuController ke sini")]
    public JobMenuController jobMenu;

    void OnTriggerEnter2D(Collider2D lain)
    {
        if (!lain.CompareTag("Player")) return;
        if (jobMenu != null) jobMenu.LanjutkanKeSceneKerja();
    }
}