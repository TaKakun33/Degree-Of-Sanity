using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// --- Penanda TEKS (World Space, bukan lingkaran/UI) yang muncul di atas objek interaktif pas
// mouse HOVER di atasnya - gaya "This War of Mine". Pakai deteksi hover MANUAL lewat
// Physics2D.OverlapPoint + Layer khusus (BUKAN OnMouseEnter/Exit bawaan Unity) - soalnya kalau
// ada collider lantai/objek lain yang numpuk di posisi yang sama, OnMouseEnter/Exit cuma ngecek
// 1 collider TERDEPAN dan bisa salah nangkep collider lain, bukan objek ini sendiri. ---
public class PenandaObjekInteraktif : MonoBehaviour
{
    [Tooltip("Object TEKS (TextMeshPro World Space) yang muncul pas hover - taruh sebagai CHILD di posisi yang kamu mau, drag GameObject-nya ke sini. Biarin NONAKTIF dari awal di Editor.")]
    public GameObject penanda;

    [Tooltip("OPSIONAL: isi buat otomatis ganti teks di dalam Penanda jadi ini (misal 'Tidur', 'Masak', 'Mandi') - kosongkan kalau teksnya udah kamu tulis manual langsung di Editor")]
    public string teksAksi = "";

    [Tooltip("WAJIB diisi: Layer objek INI SENDIRI - dipakai buat deteksi hover manual, biar gak ketiban collider lantai/objek lain yang numpuk di posisi sama")]
    public LayerMask layerObjekIni;

    private Collider2D colliderSaya;
    private bool sedangHover = false;

    void Awake()
    {
        colliderSaya = GetComponent<Collider2D>();
        if (colliderSaya == null) Debug.LogError($"[PenandaObjekInteraktif:{name}] TIDAK ADA Collider2D di object ini!");

        if (penanda != null) {
            if (!string.IsNullOrEmpty(teksAksi)) {
                // --- Pakai TMP_Text (base class) - cocok buat World Space TextMeshPro (3D) MAUPUN TextMeshProUGUI ---
                TMP_Text tmp = penanda.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = teksAksi;
            }
            penanda.SetActive(false);
        }
    }

    void Update()
    {
        if (Mouse.current == null || Camera.main == null || colliderSaya == null) return;

        // --- TAMBAHAN: jangan pernah muncul selama Pause (Time.timeScale=0, dipakai
        // PauseMenuController/MinigamePauseController) ATAU cutscene aktif ---
        if (Time.timeScale == 0f || (GameManager.Instance != null && GameManager.Instance.sedangDalamCutscene)) {
            if (sedangHover) {
                sedangHover = false;
                if (penanda != null) penanda.SetActive(false);
            }
            return;
        }

        Vector2 posisiMouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D kenaHover = Physics2D.OverlapPoint(posisiMouseWorld, layerObjekIni);

        bool hoverSekarang = (kenaHover == colliderSaya);

        if (hoverSekarang && !sedangHover) {
            sedangHover = true;
            if (penanda != null) penanda.SetActive(true);
        } else if (!hoverSekarang && sedangHover) {
            sedangHover = false;
            if (penanda != null) penanda.SetActive(false);
        }
    }

    // --- Jaga-jaga penanda kesangkut nyala kalau object ini tiba-tiba dinonaktifkan (misal
    // disembunyikan cutscene) SELAGI mouse masih di atasnya ---
    void OnDisable()
    {
        sedangHover = false;
        if (penanda != null) penanda.SetActive(false);
    }
}