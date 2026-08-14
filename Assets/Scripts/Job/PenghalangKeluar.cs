using UnityEngine;

// --- Penghalang keluar rumah VERSI COLLIDER - AKTIF OTOMATIS kalau jatah kerja part time hari
// ini udah kepakai. Pakai Collider2D (Trigger) yang nutupin area yang gak boleh dilewatin -
// begitu Player nyentuh situ selagi aktif, langsung didorong balik ke sisi "dalam rumah" dan
// gerakannya dihentikan paksa. ---
public class PenghalangKeluar : MonoBehaviour
{
    [Tooltip("Centang kalau LUAR RUMAH ada di sisi X LEBIH BESAR dari collider ini. Biarkan kosong kalau luar rumah ada di sisi X LEBIH KECIL.")]
    public bool luarRumahDiSisiKanan = true;

    private Collider2D colliderSaya;

    void Awake()
    {
        colliderSaya = GetComponent<Collider2D>();

        if (colliderSaya == null) {
            Debug.LogError($"[PenghalangKeluar:{name}] TIDAK ADA Collider2D di object ini!");
        } else if (!colliderSaya.isTrigger) {
            Debug.LogWarning($"[PenghalangKeluar:{name}] Collider2D ada tapi 'Is Trigger' belum dicentang - centang dulu.");
        }
    }

    bool SedangAktif()
    {
        return GameManager.Instance != null && !GameManager.Instance.BisaKerjaPartTimeHariIni;
    }

    // --- OnTriggerStay2D (bukan Enter) - dicek TIAP FRAME selagi Player masih nyentuh collider
    // ini, biar tetap kedorong balik walau dia terus-terusan nyoba maju lewat klik ---
    void OnTriggerStay2D(Collider2D lain)
    {
        if (!SedangAktif()) return;
        if (!lain.CompareTag("Player")) return;
        if (colliderSaya == null) return;

        Bounds batas = colliderSaya.bounds;
        Vector3 posisi = lain.transform.position;

        if (luarRumahDiSisiKanan) {
            posisi.x = Mathf.Min(posisi.x, batas.min.x);
        } else {
            posisi.x = Mathf.Max(posisi.x, batas.max.x);
        }

        lain.transform.position = posisi;

        PlayerController player = lain.GetComponent<PlayerController>();
        if (player != null) player.BerhentiPaksa();
    }
}