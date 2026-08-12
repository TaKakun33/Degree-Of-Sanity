using UnityEngine;

// --- Objek Anna INTERAKSI (NPC sehari-hari yang bisa diklik) - SELALU aktif/muncul dari awal,
// KECUALI pas ada cutscene (otomatis disembunyikan CutsceneUI.TeleportKarakter() - lihat field
// "Anna Interaksi" di situ). Pola sama kayak BedController/MandiController/PemicuInteraktifCerita:
// objek ini PASIF, PlayerController yang urus jalan ke sini (lewatin tangga kalau beda lantai),
// method Interaksi() dipanggil dari luar begitu karakter BENERAN NYAMPE. ---
public class AnnaInteraksiController : MonoBehaviour
{
    // --- Dipanggil PlayerController.MovePlayer() saat karakter sudah tiba di objek Anna ini ---
    public void Interaksi()
    {
        if (CeritaManager.Instance != null) {
            CeritaManager.Instance.CobaMulaiKlikAnna();
        }
    }
}