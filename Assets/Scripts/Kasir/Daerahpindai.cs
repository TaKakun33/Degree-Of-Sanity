using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// --- Zona Scanner: barang OTOMATIS terdeteksi begitu masuk area ini selagi diseret, TANPA perlu di-drop ---
public class DaerahPindai : MonoBehaviour
{
    public static DaerahPindai Instance;

    [Header("Visual Mode Scanner")]
    [Tooltip("Komponen Image di object scanner ini, tempat sprite mode-nya diganti")]
    public Image gambarScanner;
    [Tooltip("Sprite saat scanner idle/nunggu barang (mode 'no scan')")]
    public Sprite spriteModeSiap;
    [Tooltip("Sprite saat scanner lagi membaca barang (mode 'scan')")]
    public Sprite spriteModeScan;
    [Tooltip("Berapa detik sprite 'mode scan' ditampilkan sebelum balik lagi ke 'mode siap'")]
    public float durasiTampilModeScan = 0.5f;

    [Header("Audio Efek Scan")]
    [Tooltip("Drag komponen AudioSource yang ada di GameObject ini")]
    public AudioSource audioSourceScan;
    [Tooltip("Drag file suara/sound effect beep scanner ke sini")]
    public AudioClip klipSuaraScan;
    [Range(0f, 1f)]
    public float volumeScan = 0.8f;

    private RectTransform rectTransformSendiri;
    private Coroutine coroutineModeScanAktif;

    void Awake()
    {
        Instance = this;
        rectTransformSendiri = GetComponent<RectTransform>();
        TerapkanModeSiap();
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

        // --- TAMBAHAN: Mainkan sound effect scan beep di sini ---
        if (audioSourceScan != null && klipSuaraScan != null) {
            audioSourceScan.PlayOneShot(klipSuaraScan, volumeScan);
        }

        // --- TAMBAHAN: kedipkan sprite scanner ke "mode scan" sebentar sebagai feedback visual ---
        TampilkanModeScanSementara();
    }

    void TampilkanModeScanSementara()
    {
        // Kalau lagi nampilin mode scan dari barang sebelumnya, restart timernya (bukan numpuk coroutine)
        if (coroutineModeScanAktif != null) StopCoroutine(coroutineModeScanAktif);
        coroutineModeScanAktif = StartCoroutine(ModeScanSementaraCoroutine());
    }

    IEnumerator ModeScanSementaraCoroutine()
    {
        TerapkanModeScan();
        yield return new WaitForSeconds(durasiTampilModeScan);
        TerapkanModeSiap();
        coroutineModeScanAktif = null;
    }

    void TerapkanModeSiap()
    {
        if (gambarScanner != null && spriteModeSiap != null) gambarScanner.sprite = spriteModeSiap;
    }

    void TerapkanModeScan()
    {
        if (gambarScanner != null && spriteModeScan != null) gambarScanner.sprite = spriteModeScan;
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