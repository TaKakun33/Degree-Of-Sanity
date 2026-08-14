using UnityEngine;
using System.Collections;

// --- Audio Manager KHUSUS untuk Scene Minigame (Kasir, Ojol, Tutor) ---
// Berdiri sendiri di tiap scene minigame. Otomatis Fade-In saat scene dimulai, 
// dan Fade-Out dipanggil manual sebelum pindah kembali ke Main Scene.
public class MinigameAudioManager : MonoBehaviour
{
    public static MinigameAudioManager Instance;

    [Header("Pengaturan Audio")]
    [Tooltip("Komponen AudioSource untuk memutar BGM Minigame")]
    public AudioSource sumberMusik;
    [Tooltip("Klip musik latar (BGM) untuk minigame ini")]
    public AudioClip musikMinigame;
    [Range(0f, 1f)] 
    [Tooltip("Volume maksimal saat musik sudah selesai fade-in")]
    public float volumeMaksimal = 0.7f;
    
    [Header("Pengaturan Durasi Fade")]
    [Tooltip("Berapa detik musik membesar perlahan saat scene dimulai")]
    public float durasiFadeMasuk = 1.5f;
    [Tooltip("Berapa detik musik meredup perlahan saat scene mau berakhir")]
    public float durasiFadeKeluar = 1.0f;

    private Coroutine coroutineFade;

    void Awake()
    {
        // Setup Singleton khusus di scene ini
        if (Instance == null) Instance = this;
        
        if (sumberMusik == null) sumberMusik = GetComponent<AudioSource>();
        
        if (sumberMusik != null) {
            sumberMusik.playOnAwake = false;
            sumberMusik.loop = true; // BGM Minigame selalu di-loop
            sumberMusik.volume = 0f; // Mulai dari 0 untuk persiapan Fade-In
        }
    }

    void Start()
    {
        // Otomatis putar dan fade-in musik saat scene minigame terbuka
        MainkanMusik();
    }

    public void MainkanMusik()
    {
        if (sumberMusik == null || musikMinigame == null) return;

        sumberMusik.clip = musikMinigame;
        sumberMusik.Play();

        if (coroutineFade != null) StopCoroutine(coroutineFade);
        coroutineFade = StartCoroutine(FadeAudio(0f, volumeMaksimal, durasiFadeMasuk));
    }

    // Dipanggil oleh KasirManager / OjolManager / GradingGameManager sebelum pindah scene
    public void HentikanMusik()
    {
        if (sumberMusik == null) return;

        if (coroutineFade != null) StopCoroutine(coroutineFade);
        coroutineFade = StartCoroutine(FadeAudio(sumberMusik.volume, 0f, durasiFadeKeluar));
    }

    // Coroutine untuk transisi volume yang halus
    private IEnumerator FadeAudio(float targetAwal, float targetAkhir, float durasi)
    {
        float t = 0f;
        // Menggunakan unscaledDeltaTime agar fade tetap jalan walau game sedang di-pause
        while (t < durasi)
        {
            t += Time.unscaledDeltaTime; 
            sumberMusik.volume = Mathf.Lerp(targetAwal, targetAkhir, t / durasi);
            yield return null;
        }
        
        sumberMusik.volume = targetAkhir;

        // Jika fade-out selesai (volume 0), matikan AudioSource
        if (targetAkhir <= 0f)
        {
            sumberMusik.Stop();
        }
    }
}