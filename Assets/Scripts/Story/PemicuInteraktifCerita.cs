using UnityEngine;

// --- Objek dunia (misal Amplop di depan pintu) yang MUNCUL begitu syarat tanggal/jam/ruangan
// sebuah Peristiwa Terjadwal terpenuhi - BUKAN langsung mulai cutscene kayak Prolog. Player
// harus KLIK objek ini. (v2, pola sama kayak BedController/MandiController): objek ini PASIF,
// gak punya deteksi klik sendiri lagi - PlayerController yang urus jalan ke sini (lewatin
// tangga kalau beda lantai, dll), baru begitu BENERAN NYAMPE, method Sampai() dipanggil dari
// luar dan cutscene-nya mulai. Notifikasi (NotifikasiPopup) tetap muncul bareng aktivasinya. ---
public class PemicuInteraktifCerita : MonoBehaviour
{
    [Tooltip("Pesan notifikasi yang muncul begitu objek ini aktif (pakai NotifikasiPopup kalau ada di scene)")]
    public string pesanNotifikasi = "Ada amplop di depan pintu.";

    private CutsceneSceneSO adeganUntukDimulai;

    void Awake()
    {
        if (GetComponent<Collider2D>() == null) Debug.LogError($"[PemicuInteraktifCerita:{name}] TIDAK ADA Collider2D di object ini!");
        gameObject.SetActive(false); // pastiin mati dari awal, jaga-jaga
    }

    // --- Dipanggil CeritaManager begitu syarat Peristiwa Terjadwal ini terpenuhi ---
    public void Aktifkan(CutsceneSceneSO adegan)
    {
        adeganUntukDimulai = adegan;
        gameObject.SetActive(true);

        if (NotifikasiPopup.Instance != null && !string.IsNullOrEmpty(pesanNotifikasi)) {
            NotifikasiPopup.Instance.Tampilkan(pesanNotifikasi);
        }
    }

    // --- Dipanggil PlayerController.MovePlayer() saat karakter sudah tiba di objek ini ---
    public void Sampai()
    {
        gameObject.SetActive(false);

        if (CeritaManager.Instance != null) {
            CeritaManager.Instance.MulaiAdeganLangsung(adeganUntukDimulai);
        }
    }
}