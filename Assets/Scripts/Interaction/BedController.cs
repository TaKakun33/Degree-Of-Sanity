using UnityEngine;

public class BedController : MonoBehaviour
{
    // Fungsi ini dipanggil oleh PlayerController saat karakter sudah tiba di kasur
    public void Tidur()
    {
        // --- FIX: lewat gerbang CobaMulaiTidur() dulu, BUKAN langsung ProsesTidur() -
        // biar peristiwa cerita yang "Wajib Sebelum Tidur" (misal Main Event 2) sempet
        // ngeblokir & dipaksa jalan duluan kalau syaratnya kena. ---
        GameManager.Instance.CobaMulaiTidur(false);
    }
}