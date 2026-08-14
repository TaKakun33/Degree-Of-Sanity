using UnityEngine;

// --- Sistem SFX Tombol UI - TERPISAH TOTAL dari AudioManager (yang ngurus musik BGM/adegan).
// Singleton sendiri, AudioSource sendiri, tugasnya cuma SATU: mainin 1 suara klik pendek tiap
// kali tombol UI diklik (Buka Inventory/Toko/Utang, Tutup/Kembali, Pilih Item, Jual, Beli,
// Tambah, Kurang, Bayar Cicilan, dst).
//
// Tombol Memasak (CookingController.btnMasak / MulaiMasak) SENGAJA TIDAK dipasangin sistem
// ini - dia udah punya audio sendiri (klipSuaraMemasak). Jangan tempel TombolSfx.cs di situ.
//
// CARA PAKAI:
// 1. Taruh script ini di 1 GameObject kosong di scene awal (misal "UISfxManager"), sekali aja.
// 2. Isi field klipKlikTombol dengan 1 file suara klik pendek di Inspector.
// 3. Tempel TombolSfx.cs (lihat file satunya) ke tiap GameObject Button yang mau bunyi klik.
// --- 
public class UISfxManager : MonoBehaviour
{
    public static UISfxManager Instance;

    [Header("Sumber Suara Klik Tombol (Terpisah dari AudioManager)")]
    [Tooltip("AudioSource khusus buat SFX tombol - JANGAN pakai/isi dengan AudioSource milik AudioManager. Kosongin aja, otomatis dibikin sendiri.")]
    public AudioSource audioSourceKlik;

    [Tooltip("1 klip suara klik pendek - dipakai untuk SEMUA tombol yang ditempelin TombolSfx.cs")]
    public AudioClip klipKlikTombol;

    [Range(0f, 1f)]
    public float volumeKlik = 0.7f;

    void Awake()
    {
        // --- Singleton standar, sama pola kayak GameManager/AudioManager di project ini,
        // tapi instance-nya BEDA SENDIRI - gak nyambung ke AudioManager.Instance sama sekali ---
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // --- FIX: DontDestroyOnLoad CUMA jalan kalau GameObject ini ROOT (gak punya parent).
        // Kalau script ini ditaruh sebagai child (misal di dalam Canvas bareng UI lain),
        // DontDestroyOnLoad bakal gagal diam-diam - makanya pas ganti scene (Main Menu -> New
        // Game) objek ini ikut kehapus dan suara tombol hilang. SetParent(null) dulu di sini
        // biar aman walau lupa naruh di root pas di Editor. ---
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (audioSourceKlik == null) audioSourceKlik = gameObject.AddComponent<AudioSource>();
        audioSourceKlik.playOnAwake = false;
    }

    public void MainkanKlik()
    {
        if (audioSourceKlik == null || klipKlikTombol == null) return;
        // PlayOneShot - biar kalau tombol diklik cepet berturut-turut, suaranya numpuk/overlap
        // secara natural, bukan motong suara sebelumnya.
        audioSourceKlik.PlayOneShot(klipKlikTombol, volumeKlik);
    }
}