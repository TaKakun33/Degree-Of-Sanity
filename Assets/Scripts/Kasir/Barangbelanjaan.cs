using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// --- Barang belanjaan di atas conveyor belt: bisa di-drag, bergerak sendiri kalau belum disentuh ---
public class BarangBelanjaan : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referensi UI (opsional, buat nampilin nama & harga di atas ikon)")]
    public TextMeshProUGUI textNamaItem;
    [Tooltip("TAMBAHAN: komponen Image yang nampilin gambar produk ini")]
    public Image gambarItem;

    [HideInInspector] public string namaItem;
    [HideInInspector] public int harga;
    [HideInInspector] public bool sudahDipindai = false;
    [HideInInspector] public bool sudahDibungkus = false;
    [HideInInspector] public bool berhasilDijatuhkanValid = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvasIndukUtama;
    private Vector2 posisiSebelumDrag;
    private bool sedangDiseret = false;

    private float kecepatanGerak = 60f;
    private float xBatasKiri = -400f;
    private float xMulaiUlang = 400f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasIndukUtama = GetComponentInParent<Canvas>();

        // --- TAMBAHAN: auto-cari komponen Image kalau belum di-assign manual di Inspector -
        // gak perlu edit prefab sama sekali, asal ada Image di object ini (root atau child manapun) ---
        if (gambarItem == null) {
            gambarItem = GetComponent<Image>();
            if (gambarItem == null) gambarItem = GetComponentInChildren<Image>();
        }
    }

    // --- TAMBAHAN: dipakai KasirManager buat ngecek posisi barang ini sebagai penghalang buat barang di belakangnya ---
    public float PosisiXSaatIni => rectTransform.anchoredPosition.x;

    // --- Dipanggil KasirManager begitu barang ini di-spawn ---
    // spriteItem & ukuranCustom OPSIONAL (default kosong), biar tetap kompatibel kalau ada pemanggilan lama
    public void Setup(string nama, int hargaItem, float kecepatan, float batasKiri, float mulaiUlang, Sprite spriteItem = null, Vector2 ukuranCustom = default)
    {
        namaItem = nama;
        harga = hargaItem;
        kecepatanGerak = kecepatan;
        xBatasKiri = batasKiri;
        xMulaiUlang = mulaiUlang;
        if (textNamaItem) textNamaItem.text = nama + "\nRp " + hargaItem.ToString("N0");

        // --- Pasang gambar spesifik produk ini ---
        if (gambarItem != null && spriteItem != null) {
            gambarItem.sprite = spriteItem;

            // --- TAMBAHAN: ukuran otomatis ngikutin ukuran ASLI gambarnya (native size), BUKAN
            // ukuran default dari prefab. Ukuran Custom di katalog tetap bisa dipakai buat OVERRIDE
            // manual kalau perlu (isi selain (0,0)) - kalau kosong, native size yang menang. ---
            if (ukuranCustom == Vector2.zero) {
                gambarItem.SetNativeSize(); // resize komponen Image ke ukuran asli sprite (px)

                // Kalau gambarItem ada di object CHILD (bukan root), samain juga ukuran RectTransform
                // ROOT (yang dipakai buat pergerakan & batas track), biar dua-duanya gak mismatch
                if (gambarItem.rectTransform != rectTransform) {
                    rectTransform.sizeDelta = gambarItem.rectTransform.sizeDelta;
                }
            } else {
                rectTransform.sizeDelta = ukuranCustom; // override manual kalau diisi
            }
        }
    }

    void Update()
    {
        // Berhenti bergerak begitu sudah dipindai (nunggu ditarik ke kantong), lagi diseret, atau sudah dibungkus
        if (sudahDibungkus || sedangDiseret || sudahDipindai) return;

        // --- TAMBAHAN: batas kiri EFEKTIF - ujung track kalau dia paling depan, atau posisi barang
        // lain yang masih ada di depannya (biar antre, gak numpuk/tembus). Ganti dari sistem recycle lama. ---
        float batasKiriEfektif = xBatasKiri;
        if (KasirManager.Instance != null) {
            batasKiriEfektif = KasirManager.Instance.DapatkanBatasKiriUntukItem(this);
        }

        // --- TAMBAHAN: kompensasi pivot - biar TEPI KIRI VISUAL barang yang berhenti pas di batas,
        // bukan titik pivot/tengahnya doang. Otomatis nyesuain ke rectTransform.rect.width SAAT INI,
        // jadi tetap bener walau tiap produk ukurannya beda-beda (lihat Setup() -> ukuranCustom). ---
        float jarakPivotKeTepiKiri = rectTransform.rect.width * rectTransform.pivot.x;
        batasKiriEfektif += jarakPivotKeTepiKiri;

        Vector2 posisi = rectTransform.anchoredPosition;
        float posisiBaruX = posisi.x - kecepatanGerak * Time.deltaTime;

        // Berhenti pas nyampe batas (ujung track ATAU nempel barang di depannya), jangan lewatin
        if (posisiBaruX < batasKiriEfektif) posisiBaruX = batasKiriEfektif;

        posisi.x = posisiBaruX;
        rectTransform.anchoredPosition = posisi;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (sudahDibungkus) return;
        sedangDiseret = true;
        berhasilDijatuhkanValid = false;
        posisiSebelumDrag = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false; // biar zona drop di bawahnya kedeteksi raycast
        transform.SetAsLastSibling(); // tampil di atas item lain selagi diseret
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (sudahDibungkus) return;
        float faktorSkala = canvasIndukUtama ? canvasIndukUtama.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / faktorSkala;

        // --- Scan otomatis: begitu barang lewat/masuk area DaerahPindai selagi diseret, langsung kebaca ---
        // Gak perlu drop terpisah - satu tarikan mouse dari conveyor bisa langsung nyambung ke Kantong.
        if (!sudahDipindai && DaerahPindai.Instance != null && DaerahPindai.Instance.CekApakahBarangMasukArea(rectTransform)) {
            DaerahPindai.Instance.PindaiBarang(this);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (sudahDibungkus) return;
        sedangDiseret = false;
        canvasGroup.blocksRaycasts = true;

        // Kalau gak di-drop ke zona valid (Scanner/Kantong), kembalikan ke posisi semula
        if (!berhasilDijatuhkanValid) {
            rectTransform.anchoredPosition = posisiSebelumDrag;
        }
    }
}