using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Parameter Status Kelangsungan Hidup")]
    public int waktu = 30;
    public int uang = 5000000;

    [Range(0f, 100f)] public float progresSkripsi = 0f;
    [Range(0f, 100f)] public float lapar = 100f;
    [Range(0f, 100f)] public float sanity = 100f;

    [Header("Siklus Siang & Malam (Waktu Harian)")]
    public float jamMulai = 6f;
    public float jamSaatIni = 6f;
    public float batasTidur = 24f;

    [Tooltip("Kecepatan waktu berjalan. Misal: 1 = 1 jam in-game per detik realita")]
    public float kecepatanWaktuNormal = 0.5f;
    private float kecepatanWaktuAktif;
    private bool waktuBerjalan = true;

    [Header("Referensi UI (Antarmuka Pemain)")]
    public TextMeshProUGUI textWaktu;
    public TextMeshProUGUI textUang;
    public TextMeshProUGUI textJamHarian;
    public Slider sliderProgresSkripsi;
    public Slider sliderLapar;
    public Slider sliderSanity;

    [Header("Transisi Layar (Tidur)")]
    public Image layarGelap;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        kecepatanWaktuAktif = kecepatanWaktuNormal;
        UpdateUI();
        if (layarGelap != null) layarGelap.color = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        UpdateUI();
        CekKondisiKritis();
        SistemWaktuHarian();
    }

    void UpdateUI()
    {
        if(textWaktu != null) textWaktu.text = "Sisa Waktu: " + waktu + " Hari";
        if(textUang != null) textUang.text = "Uang: Rp " + uang;

        if(sliderProgresSkripsi != null) sliderProgresSkripsi.value = progresSkripsi;
        if(sliderLapar != null) sliderLapar.value = lapar;
        if(sliderSanity != null) sliderSanity.value = sanity;

        if(textJamHarian != null)
        {
            int jam = Mathf.FloorToInt(jamSaatIni) % 24;
            int menit = Mathf.FloorToInt((jamSaatIni % 1) * 60);
            textJamHarian.text = string.Format("{0:00}:{1:00}", jam, menit);
        }
    }

    void SistemWaktuHarian()
    {
        if (!waktuBerjalan) return;

        jamSaatIni += kecepatanWaktuAktif * Time.deltaTime;

        if (jamSaatIni >= batasTidur)
        {
            waktuBerjalan = false;
            StartCoroutine(ProsesTidur(true));
        }
    }

    public void PercepatWaktu(float multiplier)
    {
        kecepatanWaktuAktif = kecepatanWaktuNormal * multiplier;
    }

    public void KembalikanWaktuNormal()
    {
        kecepatanWaktuAktif = kecepatanWaktuNormal;
    }

    public void MinumEspresso()
    {
        batasTidur = 26f;
        Debug.Log("Espresso diminum! Batas waktu diperpanjang hingga pukul 02:00.");
    }

    void CekKondisiKritis()
    {
        sanity = Mathf.Clamp(sanity, 0f, 100f);
        lapar = Mathf.Clamp(lapar, 0f, 100f);
        progresSkripsi = Mathf.Clamp(progresSkripsi, 0f, 100f);

        if (sanity < 50f && sanity > 0f) AktifkanDistorsiVisual();
        if (sanity <= 0f) TriggerBadEnding("Karakter mengalami depresi berat.");
        if (lapar <= 0f) PenaltiLaparKritis();
        if (waktu <= 0 && progresSkripsi < 100f) TriggerBadEnding("Waktu habis, terkena DO.");
    }

    public void GantiHari()
    {
        waktu -= 1;
        lapar -= 30f;

        jamSaatIni = jamMulai;
        batasTidur = 24f;

        Debug.Log("Hari berganti! Sisa waktu: " + waktu + " hari.");
    }

    public IEnumerator ProsesTidur(bool pingsan = false)
    {
        waktuBerjalan = false;

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * 1.5f;
            if (layarGelap != null) layarGelap.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (pingsan)
        {
            Debug.Log("Karakter pingsan karena begadang maksimal!");
            sanity -= 15f;
        }

        GantiHari();

        yield return new WaitForSeconds(1.5f);

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 1.5f;
            if (layarGelap != null) layarGelap.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        waktuBerjalan = true;
    }

    private void AktifkanDistorsiVisual() { }

    private void PenaltiLaparKritis()
    {
        float baseSanityDrain = 2f;
        sanity -= (baseSanityDrain * 2) * Time.deltaTime;
    }

    private void TriggerBadEnding(string alasan) { }
}
