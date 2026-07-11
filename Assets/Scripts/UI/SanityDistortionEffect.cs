using UnityEngine;
using UnityEngine.UI;

// --- Sistem Distorsi Visual (Proposal 3.3.7 poin 5 & 3.6.3) ---
// Aktif otomatis saat Sanity < ambangSanityDistorsi (default 50%) di GameManager.
// Taruh script ini di GameObject terpisah di Canvas HUD (misal "SanityDistortionEffect"),
// lalu assign referensi-referensi di bawah lewat Inspector.
public class SanityDistortionEffect : MonoBehaviour
{
    [Header("Referensi Overlay Distorsi (UI Image full screen, di atas semua elemen HUD lain)")]
    [Tooltip("Buat Image baru full screen (stretch semua sisi), warna bebas, alpha awal akan diatur otomatis ke 0")]
    public Image overlayDistorsi;

    [Header("Referensi Kamera (opsional, untuk efek shake ringan saat Sanity sangat rendah)")]
    public Transform kameraUtama;

    [Header("Audio Bisikan (opsional, dimainkan acak saat distorsi cukup parah)")]
    public AudioSource audioBisikan;
    public AudioClip[] klipBisikan;
    public float jedaMinBisikan = 8f;
    public float jedaMaxBisikan = 20f;

    [Header("Pengaturan Efek")]
    [Tooltip("Warna overlay saat distorsi PENUH (saat Sanity mendekati 0)")]
    public Color warnaDistorsiPenuh = new Color(0.35f, 0f, 0.15f, 0.55f);
    [Tooltip("Kekuatan maksimum shake kamera saat Sanity mendekati 0")]
    public float kekuatanShakeMaksimum = 0.08f;
    [Tooltip("Seberapa cepat efek transisi masuk/keluar mengikuti perubahan Sanity")]
    public float kecepatanTransisi = 2f;
    [Range(0f, 1f)]
    [Tooltip("Ambang intensitas (0-1) minimum sebelum bisikan mulai diputar")]
    public float ambangIntensitasBisikan = 0.3f;

    // 0 = normal (tidak ada distorsi), 1 = distorsi penuh
    private float intensitasSaatIni = 0f;
    private Vector3 posisiAsliKamera;
    private float timerBisikan;

    void Start()
    {
        if (kameraUtama) posisiAsliKamera = kameraUtama.localPosition;
        AturTimerBisikanBaru();

        if (overlayDistorsi) {
            Color c = overlayDistorsi.color;
            c.a = 0f;
            overlayDistorsi.color = c;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float sanity = GameManager.Instance.sanity;
        float ambang = GameManager.Instance.ambangSanityDistorsi;

        // Target intensitas: 0 saat Sanity >= ambang, naik linear menuju 1 saat Sanity mendekati 0
        float targetIntensitas = 0f;
        if (sanity < ambang) {
            targetIntensitas = Mathf.InverseLerp(ambang, 0f, sanity);
        }

        intensitasSaatIni = Mathf.MoveTowards(intensitasSaatIni, targetIntensitas, Time.deltaTime * kecepatanTransisi);

        TerapkanOverlay();
        TerapkanShakeKamera();
        TerapkanBisikan();
    }

    void TerapkanOverlay()
    {
        if (!overlayDistorsi) return;
        Color transparan = new Color(warnaDistorsiPenuh.r, warnaDistorsiPenuh.g, warnaDistorsiPenuh.b, 0f);
        overlayDistorsi.color = Color.Lerp(transparan, warnaDistorsiPenuh, intensitasSaatIni);
    }

    void TerapkanShakeKamera()
    {
        if (!kameraUtama) return;

        if (intensitasSaatIni <= 0.01f) {
            kameraUtama.localPosition = posisiAsliKamera;
            return;
        }

        float kekuatan = kekuatanShakeMaksimum * intensitasSaatIni;
        Vector3 offset = new Vector3(
            (Mathf.PerlinNoise(Time.time * 5f, 0f) - 0.5f) * kekuatan,
            (Mathf.PerlinNoise(0f, Time.time * 5f) - 0.5f) * kekuatan,
            0f
        );
        kameraUtama.localPosition = posisiAsliKamera + offset;
    }

    void TerapkanBisikan()
    {
        if (!audioBisikan || klipBisikan == null || klipBisikan.Length == 0) return;
        if (intensitasSaatIni < ambangIntensitasBisikan) return;

        timerBisikan -= Time.deltaTime;
        if (timerBisikan <= 0f) {
            AudioClip klip = klipBisikan[Random.Range(0, klipBisikan.Length)];
            audioBisikan.PlayOneShot(klip, intensitasSaatIni);
            AturTimerBisikanBaru();
        }
    }

    void AturTimerBisikanBaru()
    {
        timerBisikan = Random.Range(jedaMinBisikan, jedaMaxBisikan);
    }

    // --- TAMBAHAN: dipanggil sistem minigame lain kalau mau menyesuaikan tingkat kesulitan ---
    // sesuai proposal 3.3.7 poin 5: "peningkatan tingkat kesulitan dalam setiap minigame" saat distorsi aktif.
    public float DapatkanIntensitasDistorsi() => intensitasSaatIni;
}