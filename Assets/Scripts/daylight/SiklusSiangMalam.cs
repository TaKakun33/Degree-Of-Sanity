using System.Collections.Generic;
using UnityEngine;

// --- 1 titik warna kunci di jam tertentu. Warna DI ANTARA 2 titik akan di-interpolasi mulus. ---
[System.Serializable]
public class TitikWarnaWaktu
{
    [Tooltip("Jam (format 24 jam desimal, misal 6.5 = jam setengah 7 pagi)")]
    public float jam;
    [Tooltip("Warna tint di jam ini - putih (255,255,255) = warna asli gambar, gak ada tint sama sekali")]
    public Color warna = Color.white;
}

// --- Sistem siklus siang-malam (Opsi C - hybrid): background TETAP 1 gambar per ruangan, cuma
// di-tint warnanya berubah mulus sepanjang hari mengikuti Daftar Titik Warna yang diatur di
// Inspector. Dengerin GameManager.OnJamBerubah yang UDAH ADA, gak perlu event baru. Reusable -
// 1 script ini cukup buat SEMUA ruangan sekaligus, tinggal drag semua background ke 1 list. ---
public class SiklusSiangMalam : MonoBehaviour
{
    [Header("Titik Warna Kunci (URUTKAN dari jam kecil ke besar)")]
    [Tooltip("Contoh isian: jam 6=subuh keunguan lembut, jam 11=siang putih terang, jam 17=senja oranye, jam 22=malam biru gelap")]
    public List<TitikWarnaWaktu> titikWarna = new List<TitikWarnaWaktu>();

    [Header("Semua SpriteRenderer Background yang ikut berubah warna")]
    [Tooltip("Drag SEMUA background 4 ruangan ke sini - semuanya bakal berubah warna bareng, otomatis sinkron sama jam in-game")]
    public List<SpriteRenderer> daftarBackground = new List<SpriteRenderer>();

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnJamBerubah -= TerapkanWarnaJam;
    }

    void Start()
    {
        // --- FIX: langganan OnJamBerubah dipindah ke SINI (bukan OnEnable) - Start() DIJAMIN
        // jalan setelah SEMUA Awake() (termasuk GameManager.Awake() yang nge-set Instance)
        // selesai duluan. Kalau ini dilakuin di OnEnable(), ada resiko jalan LEBIH DULU dari
        // GameManager.Awake(), bikin GameManager.Instance masih null, langganan GAGAL DIAM-DIAM. ---
        if (GameManager.Instance == null) {
            Debug.LogError("[SiklusSiangMalam] GameManager.Instance NULL pas Start() - warna gak akan pernah update. Cek GameManager ada di scene ini.");
            return;
        }

        GameManager.Instance.OnJamBerubah += TerapkanWarnaJam;
        Debug.Log("[SiklusSiangMalam] Berhasil langganan ke OnJamBerubah."); // --- SEMENTARA ---

        // --- Terapkan warna LANGSUNG begitu scene mulai, jangan nunggu jam berubah dulu -
        // biar gak ada 1 frame keliatan warna asli (putih) sebelum tick pertama ---
        TerapkanWarnaJam(GameManager.Instance.jamSaatIni);
    }

    void TerapkanWarnaJam(float jamSaatIni)
    {
        if (titikWarna == null || titikWarna.Count == 0) {
            Debug.LogWarning("[SiklusSiangMalam] Daftar Titik Warna KOSONG - gak ada yang bisa diterapkan."); // --- SEMENTARA ---
            return;
        }

        Color warnaSekarang = HitungWarnaInterpolasi(jamSaatIni);
        Debug.Log($"[SiklusSiangMalam] TerapkanWarnaJam({jamSaatIni:F2}) -> warna={warnaSekarang}, jumlah background={daftarBackground.Count}"); // --- SEMENTARA ---

        foreach (var bg in daftarBackground) {
            if (bg != null) bg.color = warnaSekarang;
        }
    }

    Color HitungWarnaInterpolasi(float jam)
    {
        if (titikWarna.Count == 1) return titikWarna[0].warna;

        // --- Sebelum titik pertama atau setelah titik terakhir - clamp ke ujung terdekat, gak wrap ---
        if (jam <= titikWarna[0].jam) return titikWarna[0].warna;
        if (jam >= titikWarna[titikWarna.Count - 1].jam) return titikWarna[titikWarna.Count - 1].warna;

        // --- Cari 2 titik yang mengapit jam sekarang, interpolasi mulus di antaranya ---
        for (int i = 0; i < titikWarna.Count - 1; i++) {
            if (jam >= titikWarna[i].jam && jam <= titikWarna[i + 1].jam) {
                float t = Mathf.InverseLerp(titikWarna[i].jam, titikWarna[i + 1].jam, jam);
                return Color.Lerp(titikWarna[i].warna, titikWarna[i + 1].warna, t);
            }
        }

        return titikWarna[titikWarna.Count - 1].warna; // fallback, secara teori gak akan kesampe
    }
}