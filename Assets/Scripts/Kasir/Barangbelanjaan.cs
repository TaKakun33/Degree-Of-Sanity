using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// --- Barang belanjaan di atas conveyor belt: bisa di-drag, bergerak sendiri kalau belum disentuh ---
public class BarangBelanjaan : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referensi UI (opsional, buat nampilin nama & harga di atas ikon)")]
    public TextMeshProUGUI textNamaItem;

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
    }

    // --- Dipanggil KasirManager begitu barang ini di-spawn ---
    public void Setup(string nama, int hargaItem, float kecepatan, float batasKiri, float mulaiUlang)
    {
        namaItem = nama;
        harga = hargaItem;
        kecepatanGerak = kecepatan;
        xBatasKiri = batasKiri;
        xMulaiUlang = mulaiUlang;
        if (textNamaItem) textNamaItem.text = nama + "\nRp " + hargaItem.ToString("N0");
    }

    void Update()
    {
        // Berhenti bergerak begitu sudah dipindai (nunggu ditarik ke kantong), lagi diseret, atau sudah dibungkus
        if (sudahDibungkus || sedangDiseret || sudahDipindai) return;

        Vector2 posisi = rectTransform.anchoredPosition;
        posisi.x -= kecepatanGerak * Time.deltaTime;

        // Gak ada tekanan waktu: kalau kelewat ujung belt, muncul lagi dari awal (recycle), bukan hilang/penalti
        if (posisi.x < xBatasKiri) posisi.x = xMulaiUlang;

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