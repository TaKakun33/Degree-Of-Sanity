using System.Collections;
using UnityEngine;

// --- TAMBAHAN: Manager Musik BGM - SATU sumber kebenaran tunggal buat semua transisi musik
// (Musik Utama / Main Event / Ending). Taruh di GameObject di MAIN SCENE (sama kayak GameManager,
// CeritaManager, CutsceneUI dkk) - SENGAJA BUKAN DontDestroyOnLoad, karena semua momen yang
// butuh musik (Prolog, Main Event, Ending) kejadian DI DALAM MainScene itu sendiri, gak pernah
// lintas-scene. Begitu MainScene di-reload (New Game/Load Game/RestartGame), instance baru
// otomatis kebuat lagi dan mulai dari nol - itu SENGAJA, biar "audio muncul perlahan begitu
// New Game/Load Game dimulai" selalu berlaku tiap kali MainScene ini mulai. ---
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sumber Audio")]
    [Tooltip("SATU AudioSource buat semua BGM - klipnya diganti-ganti sesuai kondisi (Utama/Main Event/Ending), BUKAN banyak source dobelan. Kosongkan buat auto-ambil AudioSource di GameObject yang sama.")]
    public AudioSource sumberMusik;

    [Header("Musik Utama (Prolog & Main Game - klipnya SENGAJA SAMA)")]
    [Tooltip("Diputar otomatis begitu MainScene mulai (New Game MAUPUN Load Game - dua-duanya lewat GameManager.Start()). Prolog gak perlu musik terpisah - biarin Musik Utama ini yang jalan terus selama Prolog.")]
    public AudioClip musikUtama;
    [Range(0f, 1f)] public float volumeMusikUtama = 0.7f;
    [Tooltip("Berapa detik Musik Utama fade-in pas MainScene PERTAMA KALI mulai (New Game/Load Game)")]
    public float durasiFadeMasukAwal = 2f;
    [Tooltip("TAMBAHAN: jeda HENING (detik) sebelum Musik Utama diulang dari awal begitu klipnya abis diputar sekali - biar gak loop mulus/nyambung langsung kayak biasa. KHUSUS Musik Utama - Main Event/Ending tetap loop mulus seperti biasa (isi 0 buat balik ke loop mulus juga).")]
    public float jedaLoopMusikUtama = 10f;

    [Header("Musik Main Event")]
    [Tooltip("Berapa detik Musik Utama meredup SEBELUM Musik Main Event mulai")]
    public float durasiFadeKeluarKeEvent = 1f;
    [Tooltip("Berapa detik Musik Main Event muncul perlahan SETELAH Musik Utama meredup")]
    public float durasiFadeMasukEvent = 1.5f;
    [Range(0f, 1f)] public float volumeMusikEvent = 0.7f;
    [Tooltip("Berapa detik Musik Main Event meredup begitu cutscene-nya BENERAN kelar (chain selesai)")]
    public float durasiFadeKeluarDariEvent = 1f;
    [Tooltip("Berapa detik Musik Utama muncul lagi perlahan begitu balik dari Main Event")]
    public float durasiFadeMasukKembaliUtama = 1.5f;

    [Header("Musik Ending (Good/Bad beda klip - diatur per-adegan lewat CutsceneSceneSO.musikKhusus)")]
    [Tooltip("Berapa detik Musik Utama meredup SEBELUM Musik Ending mulai")]
    public float durasiFadeKeluarKeEnding = 1f;
    [Tooltip("Berapa detik Musik Ending muncul perlahan SETELAH Musik Utama meredup")]
    public float durasiFadeMasukEnding = 1.5f;
    [Range(0f, 1f)] public float volumeMusikEnding = 0.7f;
    [Tooltip("Berapa detik Musik Ending meredup begitu player MEMILIH salah satu tombol di panel Layar Akhir (Restart/Kembali ke Menu) - musik Ending TETAP BUNYI sampai titik ini, gak otomatis berhenti pas panel muncul")]
    public float durasiFadeKeluarSetelahPilihEnding = 1f;

    private Coroutine coroutineAktif;
    // --- TAMBAHAN: coroutine TERPISAH khusus buat siklus "tunggu klip abis -> jeda hening ->
    // ulang dari awal" milik Musik Utama - sengaja dipisah dari coroutineAktif (yang ngurusin
    // fade) biar bisa dimatiin independen begitu musik pindah ke Main Event/Ending ---
    private Coroutine coroutineLoopUtama;
    // --- klip yang lagi/baru aja diminta diputar - dipakai buat CEGAH restart dobel kalau
    // dipanggil berkali-kali dengan klip yang sama (misal tiap baris dalam SATU Main Event
    // yang gak ganti musik lagi) ---
    private AudioClip klipSaatIni;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (sumberMusik == null) sumberMusik = GetComponent<AudioSource>();
        if (sumberMusik != null) {
            // --- TAMBAHAN: 'loop' SEKARANG diatur per-klip di GantiMusikCoroutine() (Musik Utama
            // loop manual pakai jeda, Main Event/Ending tetap loop mulus biasa) - bukan statis di sini lagi ---
            sumberMusik.playOnAwake = false;
            sumberMusik.volume = 0f;
        } else {
            Debug.LogError("[AudioManager] Gak ada AudioSource - drag salah satu ke field 'Sumber Musik', atau tempel AudioSource di GameObject yang sama.");
        }
    }

    // --- TAMBAHAN: dipanggil GameManager.Start() - jalan BAIK pas GAME BARU maupun LOAD GAME
    // (dua-duanya lewat titik Start() yang sama), soalnya Prolog & Main Game sengaja pakai
    // klip yang SAMA (musikUtama). ---
    public void MainkanMusikUtama()
    {
        GantiMusik(musikUtama, 0f, durasiFadeMasukAwal, volumeMusikUtama);
    }

    // --- TAMBAHAN: dipanggil CutsceneUI begitu ketemu adegan yang punya musikKhusus SELAGI
    // adegan itu BUKAN bagian dari chain Ending - dipakai buat Main Event. Musik yang lagi
    // kedengeran (biasanya Musik Utama) meredup dulu, BARU musik Main Event ini muncul. ---
    public void PindahKeMusikEvent(AudioClip klipEvent)
    {
        if (klipEvent == null) return;
        GantiMusik(klipEvent, durasiFadeKeluarKeEvent, durasiFadeMasukEvent, volumeMusikEvent);
    }

    // --- TAMBAHAN: dipanggil CutsceneUI begitu chain Main Event (BUKAN Ending) BENERAN kelar -
    // balik ke Musik Utama. Aman dipanggil walau musiknya emang udah Musik Utama dari awal
    // (misal chain Prolog) - GantiMusik() di bawah bakal no-op kalau klipnya udah sama. ---
    public void KembaliKeMusikUtama()
    {
        GantiMusik(musikUtama, durasiFadeKeluarDariEvent, durasiFadeMasukKembaliUtama, volumeMusikUtama);
    }

    // --- TAMBAHAN: dipanggil CutsceneUI begitu ketemu adegan bermusik dalam chain ENDING
    // (Happy ATAUPUN Bad - beda klipnya ditentukan lewat musikKhusus yang di-drag di Inspector
    // pada CutsceneSceneSO adegan pertama tiap Ending, BUKAN dibedain lewat kode). Musik Utama
    // meredup dulu, baru Musik Ending ini muncul - dan TIDAK otomatis balik ke Musik Utama
    // begitu chain-nya kelar (beda dari Main Event) - tetep bunyi sampai HentikanMusik()
    // dipanggil manual (lihat GameManager.RestartGame()). ---
    public void PindahKeMusikEnding(AudioClip klipEnding)
    {
        if (klipEnding == null) return;
        GantiMusik(klipEnding, durasiFadeKeluarKeEnding, durasiFadeMasukEnding, volumeMusikEnding);
    }

    // --- TAMBAHAN: hentiin musik SEKARANG JUGA (fade out lalu stop), gak nunggu apa-apa.
    // Dipakai kalau kamu gak butuh nunggu fade-nya kelar sebelum lanjut aksi lain. ---
    public void HentikanMusik()
    {
        klipSaatIni = null;
        if (coroutineLoopUtama != null) { StopCoroutine(coroutineLoopUtama); coroutineLoopUtama = null; }
        if (coroutineAktif != null) StopCoroutine(coroutineAktif);
        coroutineAktif = StartCoroutine(FadeOutLaluBerhenti(durasiFadeKeluarSetelahPilihEnding));
    }

    // --- TAMBAHAN: versi yang NUNGGU fade-out kelar dulu, BARU jalanin 'lanjutan' (misal pindah
    // scene/reload) - dipakai biar fade-nya BENERAN kedengeran sebelum scene keburu diganti
    // (kalau scene langsung diganti di frame yang sama, coroutine fade-nya gak sempet jalan sama
    // sekali karena GameObject ini ikut hancur). Panggil ini dari tombol "Main Lagi"/"Kembali ke
    // Menu" di panel Layar Akhir Ending, bukan langsung SceneManager.LoadScene(). ---
    public void HentikanMusikLaluJalankan(System.Action lanjutan)
    {
        klipSaatIni = null;
        if (coroutineLoopUtama != null) { StopCoroutine(coroutineLoopUtama); coroutineLoopUtama = null; }
        if (coroutineAktif != null) StopCoroutine(coroutineAktif);
        coroutineAktif = StartCoroutine(HentikanMusikLaluJalankanCoroutine(lanjutan));
    }

    IEnumerator HentikanMusikLaluJalankanCoroutine(System.Action lanjutan)
    {
        yield return StartCoroutine(FadeOutLaluBerhenti(durasiFadeKeluarSetelahPilihEnding));
        lanjutan?.Invoke();
    }

    // --- Inti dari semua transisi: FADE OUT klip lama (kalau ada & lagi kedengeran) -> ganti
    // klip -> FADE IN klip baru ke volumeTarget. SELALU sequential (redup dulu baru muncul),
    // sesuai permintaan - BUKAN crossfade dua source bersamaan. ---
    void GantiMusik(AudioClip klipBaru, float durasiFadeOut, float durasiFadeIn, float volumeTarget)
    {
        if (sumberMusik == null) return;
        // --- udah pas di klip yang sama & lagi kedengeran - gak usah ngapa-ngapain, biar gak
        // restart/fade dobel tiap kali dipanggil ulang (misal tiap baris dialog Main Event yang
        // gak ganti musik) ---
        if (klipSaatIni == klipBaru && sumberMusik.isPlaying) return;

        // --- TAMBAHAN: hentiin dulu siklus jeda-loop Musik Utama (kalau lagi jalan) - kita
        // mau pindah musik, jadi jangan sampai coroutine ini nyalain ulang Musik Utama
        // di tengah-tengah proses pindah ke musik lain ---
        if (coroutineLoopUtama != null) { StopCoroutine(coroutineLoopUtama); coroutineLoopUtama = null; }

        klipSaatIni = klipBaru;
        if (coroutineAktif != null) StopCoroutine(coroutineAktif);
        coroutineAktif = StartCoroutine(GantiMusikCoroutine(klipBaru, durasiFadeOut, durasiFadeIn, volumeTarget));
    }

    IEnumerator GantiMusikCoroutine(AudioClip klipBaru, float durasiFadeOut, float durasiFadeIn, float volumeTarget)
    {
        yield return StartCoroutine(FadeOutLaluBerhenti(durasiFadeOut));

        if (klipBaru == null) yield break;

        sumberMusik.clip = klipBaru;
        // --- TAMBAHAN: Musik Utama loop MANUAL (lewat LoopMusikUtamaDenganJeda di bawah, biar
        // ada jeda hening), klip lain (Main Event/Ending) tetap loop MULUS kayak biasa ---
        sumberMusik.loop = (klipBaru != musikUtama);
        sumberMusik.Play();

        if (durasiFadeIn > 0f) {
            float t = 0f;
            while (t < durasiFadeIn) {
                t += Time.unscaledDeltaTime; // --- unscaled: TETAP jalan walau Time.timeScale = 0 (panel Ending/Pause matiin timeScale) ---
                sumberMusik.volume = Mathf.Lerp(0f, volumeTarget, t / durasiFadeIn);
                yield return null;
            }
        }
        sumberMusik.volume = volumeTarget;

        // --- TAMBAHAN: begitu Musik Utama beneran mulai jalan, nyalain siklus jeda-loop-nya ---
        if (klipBaru == musikUtama) {
            coroutineLoopUtama = StartCoroutine(LoopMusikUtamaDenganJeda());
        }
    }

    // --- TAMBAHAN: siklus "tunggu klip Musik Utama abis diputar sekali (loop-nya sengaja
    // dimatiin di atas) -> tunggu jeda hening jedaLoopMusikUtama detik -> Play() lagi dari awal
    // -> ulang". Dipakai KHUSUS Musik Utama, biar ada jeda antar loop (beda dari Main Event/
    // Ending yang tetap loop mulus lewat AudioSource.loop = true biasa). ---
    IEnumerator LoopMusikUtamaDenganJeda()
    {
        while (true) {
            // --- nunggu klip beneran abis (isPlaying otomatis jadi false sendiri karena loop
            // udah dimatiin buat Musik Utama - TETAP nunggu dgn benar walau lagi di-AudioListener.pause,
            // soalnya isPlaying tetap true selama itu, gak keburu abis) ---
            yield return new WaitUntil(() => sumberMusik == null || !sumberMusik.isPlaying);
            if (sumberMusik == null) yield break;

            if (jedaLoopMusikUtama > 0f) {
                yield return new WaitForSecondsRealtime(jedaLoopMusikUtama); // --- realtime: jeda tetap jalan normal walau Time.timeScale lagi diubah ---
            }

            // --- jaga-jaga: kalau selama jeda musiknya sempet dipindah ke lagu lain, batalin -
            // GantiMusik() harusnya udah stop coroutine ini duluan, ini cuma pengaman tambahan ---
            if (sumberMusik == null || sumberMusik.clip != musikUtama) yield break;

            sumberMusik.Play();
        }
    }

    IEnumerator FadeOutLaluBerhenti(float durasi)
    {
        if (sumberMusik == null) yield break;

        if (sumberMusik.isPlaying && durasi > 0f) {
            float volumeAwal = sumberMusik.volume;
            float t = 0f;
            while (t < durasi) {
                t += Time.unscaledDeltaTime; // --- unscaled: sama alasannya - fade Ending harus tetap jalan walau timeScale 0 ---
                sumberMusik.volume = Mathf.Lerp(volumeAwal, 0f, t / durasi);
                yield return null;
            }
        }
        sumberMusik.volume = 0f;
        sumberMusik.Stop();
    }
}