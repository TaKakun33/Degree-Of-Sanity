using UnityEngine;

// --- Zona Scanner: barang OTOMATIS terdeteksi begitu masuk area ini selagi diseret, TANPA perlu di-drop ---
public class DaerahPindai : MonoBehaviour
{
    public static DaerahPindai Instance;

    private RectTransform rectTransformSendiri;

    void Awake()
    {
        Instance = this;
        rectTransformSendiri = GetComponent<RectTransform>();
    }

    // --- Dipanggil BarangBelanjaan.OnDrag tiap frame selagi diseret, cek udah masuk area scanner atau belum ---
    public bool CekApakahBarangMasukArea(RectTransform rectBarang)
    {
        return RectOverlap(rectTransformSendiri, rectBarang);
    }

    // --- Dipanggil BarangBelanjaan begitu overlap kedeteksi, "membaca" barang tanpa perlu drop ---
    public void PindaiBarang(BarangBelanjaan barang)
    {
        if (barang.sudahDipindai) return;

        barang.sudahDipindai = true;
        if (KasirManager.Instance != null) KasirManager.Instance.ItemDipindai(barang);
    }

    bool RectOverlap(RectTransform a, RectTransform b)
    {
        return GetWorldRect(a).Overlaps(GetWorldRect(b));
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] sudut = new Vector3[4];
        rt.GetWorldCorners(sudut);
        return new Rect(sudut[0].x, sudut[0].y, sudut[2].x - sudut[0].x, sudut[2].y - sudut[0].y);
    }
}