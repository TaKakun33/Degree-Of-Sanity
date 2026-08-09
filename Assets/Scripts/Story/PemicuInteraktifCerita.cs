using UnityEngine;
using UnityEngine.InputSystem;

// --- Objek dunia (misal Amplop di depan pintu) yang MUNCUL begitu syarat tanggal/jam/ruangan
// sebuah Peristiwa Terjadwal terpenuhi - BUKAN langsung mulai cutscene kayak Prolog. Player
// harus KLIK objek ini, karakter jalan ke situ (JalanKeTitik), baru begitu BENERAN NYAMPE
// cutscene-nya mulai. Notifikasi (NotifikasiPopup) muncul bareng aktivasinya. ---
public class PemicuInteraktifCerita : MonoBehaviour
{
    [Tooltip("Pesan notifikasi yang muncul begitu objek ini aktif (pakai NotifikasiPopup kalau ada di scene)")]
    public string pesanNotifikasi = "Ada amplop di depan pintu.";

    [Tooltip("WAJIB diisi: Layer klik KHUSUS (sama konsepnya kayak PintuKlikRelay) - biar klik gak numpuk sama collider lain")]
    public LayerMask layerKlik;

    private Collider2D colliderSaya;
    private CutsceneSceneSO adeganUntukDimulai;
    private bool sedangMenungguKlik = false;
    private bool sedangMenungguSampai = false;
    private PlayerController playerReferensi;

    void Awake()
    {
        colliderSaya = GetComponent<Collider2D>();
        if (colliderSaya == null) Debug.LogError($"[PemicuInteraktifCerita:{name}] TIDAK ADA Collider2D di object ini!");
        gameObject.SetActive(false); // pastiin mati dari awal, jaga-jaga
    }

    // --- Dipanggil CeritaManager begitu syarat Peristiwa Terjadwal ini terpenuhi ---
    public void Aktifkan(CutsceneSceneSO adegan)
    {
        adeganUntukDimulai = adegan;
        gameObject.SetActive(true);
        sedangMenungguKlik = true;

        if (NotifikasiPopup.Instance != null && !string.IsNullOrEmpty(pesanNotifikasi)) {
            NotifikasiPopup.Instance.Tampilkan(pesanNotifikasi);
        }
    }

    void Update()
    {
        if (sedangMenungguKlik && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Camera.main != null) {
            Vector2 posisiMouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D kenaKlik = Physics2D.OverlapPoint(posisiMouseWorld, layerKlik);

            if (kenaKlik == colliderSaya) {
                sedangMenungguKlik = false;
                playerReferensi = Object.FindFirstObjectByType<PlayerController>();
                if (playerReferensi != null) {
                    playerReferensi.KunciKontrol(true);
                    playerReferensi.JalanKeTitik(transform.position);
                    sedangMenungguSampai = true;
                }
            }
        }

        if (sedangMenungguSampai && playerReferensi != null && !playerReferensi.SedangBergerak) {
            sedangMenungguSampai = false;
            gameObject.SetActive(false);

            if (CeritaManager.Instance != null) {
                CeritaManager.Instance.MulaiAdeganLangsung(adeganUntukDimulai);
            }
        }
    }
}