using UnityEngine;

// --- Rintangan di jalur Ojek Online: jatuh sendiri ke bawah, tabrakan dideteksi lewat Collider2D ---
// Komponen yang WAJIB ada di prefab ini: BoxCollider2D (Is Trigger = true)
[RequireComponent(typeof(BoxCollider2D))]
public class RintanganOjol : MonoBehaviour
{
    [HideInInspector] public int laneIndex;

    private RectTransform rect;
    private float kecepatan;
    private bool sudahDiproses = false;

    void Awake()
    {
        // --- TAMBAHAN: Box Collider 2D di UI TIDAK otomatis nyamain ukuran ke RectTransform,
        // jadi kita paksa sinkron di sini biar collider selalu pas sama ukuran visual sprite-nya. ---
        rect = GetComponent<RectTransform>();
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (rect != null && collider != null) {
            collider.size = rect.rect.size;
        }
    }

    public void Setup(int lane, float posisiX, float posisiYAwal, float kecepatanJatuh)
    {
        laneIndex = lane;
        kecepatan = kecepatanJatuh;
        rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posisiX, posisiYAwal);
    }

    void Update()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        rect.anchoredPosition += Vector2.down * kecepatan * Time.deltaTime;

        if (OjolManager.Instance == null || sudahDiproses) return;

        // --- Cek "lolos" (dihindari) berdasar posisi Y; tabrakannya sendiri ditangani OnTriggerEnter2D di bawah ---
        if (rect.anchoredPosition.y < OjolManager.Instance.BatasBawahHapus) {
            sudahDiproses = true;
            OjolManager.Instance.RintanganDihindari();
            Destroy(gameObject);
        }
    }

    // --- Deteksi tabrakan pakai Collider2D beneran - jauh lebih presisi/jelas daripada hitung jarak manual ---
    void OnTriggerEnter2D(Collider2D lawan)
    {
        if (sudahDiproses) return;
        if (lawan.GetComponent<KarakterPemainOjolMarker>() == null) return; // pastikan yang overlap itu emang karakter pemain

        sudahDiproses = true;
        OjolManager.Instance.TabrakRintangan();
        Destroy(gameObject);
    }

    // --- TAMBAHAN: lapor balik ke OjolManager begitu rintangan ini hilang (apapun sebabnya),
    // biar OjolManager tau lane mana yang udah "kosong" lagi buat jamin selalu ada jalur aman ---
    void OnDestroy()
    {
        if (OjolManager.Instance != null) OjolManager.Instance.LaneDibersihkan(laneIndex);
    }
}