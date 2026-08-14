using UnityEngine;

// --- Konfigurasi TUNGGAL posisi Y tiap lantai - dipakai SEMUA DoorController biar konsisten.
// Sebelumnya tiap pintu butuh "destination" yang Y-nya harus presisi manual satu-satu (rawan
// salah/beda-beda antar pintu). Sekarang cukup atur SEKALI di sini, semua pintu yang menuju
// lantai yang sama otomatis pakai Y yang sama persis. ---
public class KonfigurasiLantai : MonoBehaviour
{
    public static KonfigurasiLantai Instance;

    [Header("TUNABLE: Posisi Y pemain per lantai")]
    [Tooltip("Posisi Y pemain kalau tujuannya Lantai 1")]
    public float posisiYLantai1 = -3.922f;
    [Tooltip("Posisi Y pemain kalau tujuannya Lantai 2")]
    public float posisiYLantai2 = -0.04983255f;

    void Awake()
    {
        Instance = this;
    }

    // --- Tambah lantai baru di sini kalau nanti rumahnya punya lebih dari 2 lantai ---
    public float DapatkanPosisiY(int lantai)
    {
        if (lantai == 1) return posisiYLantai1;
        if (lantai == 2) return posisiYLantai2;

        Debug.LogWarning($"[KonfigurasiLantai] Gak ada Posisi Y buat Lantai {lantai} - fallback ke 0. Tambahin field baru kalau emang ada lantai ini.");
        return 0f;
    }
}