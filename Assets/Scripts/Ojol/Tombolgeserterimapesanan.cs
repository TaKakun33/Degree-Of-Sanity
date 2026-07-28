using UnityEngine;
using UnityEngine.EventSystems;

// --- Tombol geser buat terima pesanan Ojol - drag handle ke kanan sampai batas, baru dianggap "diterima" ---
// Tempel di GameObject "handle" (yang bisa digeser), yang harus jadi CHILD dari "track"-nya.
public class TombolGeserTerimaPesanan : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Tooltip("RectTransform handle/tombol yang digeser (biasanya diri sendiri)")]
    public RectTransform handle;
    [Tooltip("RectTransform track/jalur tempat handle bergerak - HARUS parent dari handle")]
    public RectTransform track;
    [Range(0f, 1f)]
    [Tooltip("Seberapa persen (0-1) perjalanan geser yang harus dicapai biar dianggap 'diterima'")]
    public float ambangBatasTerima = 0.85f;

    private Vector2 posisiAwalHandle;
    private float batasKiri;
    private float batasKanan;
    private bool sudahDiterima = false;

    void Start()
    {
        if (handle == null) handle = GetComponent<RectTransform>();

        // --- FIX: hitung batas kiri/kanan dari LEBAR ASLI Track & Handle, bukan dari posisi manual di Editor.
        // Asumsi: Handle adalah child dari Track, keduanya pakai pivot (0.5, 0.5) - default Unity untuk UI baru. ---
        if (track != null) {
            float setengahLebarTrack = track.rect.width / 2f;
            float setengahLebarHandle = handle.rect.width / 2f;
            batasKiri = -setengahLebarTrack + setengahLebarHandle;
            batasKanan = setengahLebarTrack - setengahLebarHandle;
        } else {
            batasKiri = handle.anchoredPosition.x;
            batasKanan = batasKiri + 200f;
        }

        // --- FIX: paksa handle MULAI DARI KIRI, apapun posisi manualnya waktu di-drag ke Editor ---
        posisiAwalHandle = new Vector2(batasKiri, handle.anchoredPosition.y);
        handle.anchoredPosition = posisiAwalHandle;
    }

    // --- Dipanggil OjolManager tiap kali pesanan baru masuk, biar handle balik ke posisi kiri & siap dipakai lagi ---
    public void ResetHandle()
    {
        sudahDiterima = false;
        if (handle != null) handle.anchoredPosition = posisiAwalHandle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (sudahDiterima || handle == null) return;

        Canvas canvasIndukUtama = handle.GetComponentInParent<Canvas>();
        float faktorSkala = canvasIndukUtama != null ? canvasIndukUtama.scaleFactor : 1f;

        // --- Clamp ini yang mastiin handle GAK PERNAH bisa keluar dari batasKiri/batasKanan ---
        float posisiBaruX = Mathf.Clamp(handle.anchoredPosition.x + eventData.delta.x / faktorSkala, batasKiri, batasKanan);
        handle.anchoredPosition = new Vector2(posisiBaruX, handle.anchoredPosition.y);

        float progres = (batasKanan - batasKiri) > 0f ? (posisiBaruX - batasKiri) / (batasKanan - batasKiri) : 0f;
        if (progres >= ambangBatasTerima) {
            sudahDiterima = true;
            if (OjolManager.Instance != null) OjolManager.Instance.TerimaPesanan();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (sudahDiterima) return;
        // Belum sampai ambang batas geser -> balik ke posisi awal (kiri lagi, belum diterima)
        handle.anchoredPosition = posisiAwalHandle;
    }
}