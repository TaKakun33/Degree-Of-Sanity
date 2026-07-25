using UnityEngine;

// --- Garis Finish tujuan pengantaran Ojol: turun kayak rintangan, TAPI begitu nyentuh pemain = SELESAI (bukan tabrakan) ---
// Prefab ini biasanya full-width (nutupin ketiga lane sekaligus), jadi kesentuh dari lane manapun.
[RequireComponent(typeof(BoxCollider2D))]
public class GarisFinishOjol : MonoBehaviour
{
    private RectTransform rect;
    private float kecepatan;
    private bool sudahDiproses = false;

    void Awake()
    {
        // --- Sama kayak RintanganOjol: paksa sinkron ukuran Box Collider 2D ke RectTransform ---
        rect = GetComponent<RectTransform>();
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (rect != null && collider != null) {
            collider.size = rect.rect.size;
        }
    }

    public void Setup(float posisiYAwal, float kecepatanJatuh)
    {
        kecepatan = kecepatanJatuh;
        rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, posisiYAwal);
    }

    void Update()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        rect.anchoredPosition += Vector2.down * kecepatan * Time.deltaTime;

        // --- Lapor posisi Y sendiri tiap frame, dipakai OjolManager buat hitung "Sisa Jarak" beneran ---
        if (OjolManager.Instance != null) {
            OjolManager.Instance.UpdateSisaJarakFinish(rect.anchoredPosition.y);
        }
    }

    void OnTriggerEnter2D(Collider2D lawan)
    {
        if (sudahDiproses) return;
        if (lawan.GetComponent<KarakterPemainOjolMarker>() == null) return; // pastikan yang overlap itu emang karakter pemain

        sudahDiproses = true;
        if (OjolManager.Instance != null) OjolManager.Instance.SelesaikanPengantaran();
        Destroy(gameObject);
    }
}