using UnityEngine;

// --- Efek goyang halus buat teks Bisikan - tempel di object yang sama kayak TMP Text dialog ---
public class GoyangTeks : MonoBehaviour
{
    public float amplitudo = 2f;
    public float kecepatan = 8f;

    private RectTransform rt;
    private Vector2 posisiAsli;
    private bool aktif = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        posisiAsli = rt.anchoredPosition;
    }

    void Update()
    {
        if (!aktif) return;
        float x = Mathf.Sin(Time.time * kecepatan) * amplitudo;
        rt.anchoredPosition = posisiAsli + new Vector2(x, 0f);
    }

    public void Aktifkan()
    {
        aktif = true;
    }

    public void Matikan()
    {
        aktif = false;
        if (rt) rt.anchoredPosition = posisiAsli;
    }
}