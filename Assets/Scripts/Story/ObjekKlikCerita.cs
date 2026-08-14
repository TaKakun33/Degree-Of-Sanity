using UnityEngine;

public enum JenisKlikCerita { KlikAnna, KlikLaptop }

// --- Tempel di object Anna atau Laptop (Collider2D, boleh IsTrigger) - klik di sini
// dicek ke CeritaManager dulu (buat Main Event yang trigger-nya "klik Anna"/"klik Laptop"). ---
public class ObjekKlikCerita : MonoBehaviour
{
    public JenisKlikCerita jenis = JenisKlikCerita.KlikAnna;

    void OnMouseDown()
    {
        if (CeritaManager.Instance == null) return;

        if (jenis == JenisKlikCerita.KlikAnna) CeritaManager.Instance.CobaMulaiKlikAnna();
        else CeritaManager.Instance.CobaMulaiKlikLaptop();
    }
}