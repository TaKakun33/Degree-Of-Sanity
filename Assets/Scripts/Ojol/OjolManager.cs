using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// --- Satu baris di dalam pola: lane mana yang ada rintangan (minimal 1 HARUS tetap kosong biar ada jalur aman) ---
[System.Serializable]
public class BarisPola
{
    public bool kiri;
    public bool tengah;
    public bool kanan;
}

// --- Satu pola/template lengkap: kumpulan baris yang sudah didesain AMAN & bisa diselesaikan ---
[System.Serializable]
public class PolaRintangan
{
    public string namaPola = "Pola";
    public BarisPola[] baris;
}

// --- Minigame Kerja Part Time: Ojek Online (Proposal 3.3.4.2) ---
// Berjalan di SCENE TERPISAH beneran (Single load, sama persis arsitekturnya kayak KasirManager),
// karena itu GameManager TIDAK bisa diakses langsung selama minigame ini berjalan.
// Hasil shift (gaji, efek lapar/sanity, skip jam) dititipkan ke HasilKerjaPartTime,
// baru diterapkan lagi oleh GameManager begitu MainScene dimuat ulang.
public class OjolManager : MonoBehaviour
{
    public static OjolManager Instance;

    [Header("Referensi UI Umum")]
    public TextMeshProUGUI textGajiTerkumpul;
    public TextMeshProUGUI textPesananKe;

    [Header("Transisi Layar")]
    public Image layarTransisi;
    public float durasiFade = 0.5f;

    [Header("Fase Menunggu Pesanan")]
    public GameObject panelMenunggu;
    public TextMeshProUGUI textStatusMenunggu;
    public GameObject panelPesananMasuk;
    public TextMeshProUGUI textInfoPesanan;
    [Tooltip("Lama waktu tunggu pesanan itu ACAK di antara rentang ini (proposal: lama waktu tunggu acak)")]
    public float waktuTungguMin = 3f;
    public float waktuTungguMax = 10f;
    [Tooltip("Tombol geser buat terima pesanan (drag ke kanan)")]
    public TombolGeserTerimaPesanan tombolGeserTerima;
    public TextMeshProUGUI textWaktuTerimaPesanan;
    [Tooltip("Batas waktu (detik) buat geser tombol terima sebelum pesanan dianggap TERLEWAT (tetap kepakai 1 jatah pesanan, tapi gak dapat gaji)")]
    public float batasWaktuTerimaPesanan = 8f;

    [Header("Fase Mengantar Pesanan (3 Jalur)")]
    public GameObject panelMengantar;
    public RectTransform karakterPemain;
    [Tooltip("HARUS diisi tepat 3 elemen: posisi X jalur Kiri, Tengah, Kanan (urut)")]
    public RectTransform[] posisiLane;
    public float kecepatanPindahLane = 10f;
    [Tooltip("TAMBAHAN: isi 6 prefab rintangan yang BEDA-BEDA (sprite/tampilan beda) - tiap kali spawn, sistem pilih SATU secara ACAK dari array ini")]
    public GameObject[] prefabRintanganArray = new GameObject[6];
    [Tooltip("Wadah/parent untuk rintangan yang di-spawn - Canvas langsung atau child kosong di bawahnya")]
    public Transform wadahRintangan;
    public float yMulaiSpawnRintangan = 400f;
    public float yBatasBawahRintangan = -350f;
    [Tooltip("Kalau dicentang, Y Mulai Spawn & Y Batas Bawah di atas DIABAIKAN, dihitung otomatis dari tinggi Wadah Rintangan - jadi gak perlu tebak angka manual dan otomatis benar di resolusi/Canvas Scaler apapun")]
    public bool otomatisHitungBatasVertikal = true;
    public float kecepatanRintangan = 200f;

    [Header("Pattern-Based Spawning (kumpulan pola aman)")]
    [Tooltip("Jarak vertikal (pixel) antar BARIS di dalam satu pola")]
    public float jarakAntarBarisDalamPola = 300f;
    [Tooltip("Jarak vertikal dasar (pixel) SETELAH satu pola selesai, sebelum pola berikutnya mulai")]
    public float jarakAntarPola = 600f;
    [Tooltip("Variasi acak tambahan (pixel) di jarak antar pola, biar ritmenya gak monoton/ketebak")]
    public float variasiJarakAntarPola = 200f;
    [Tooltip("Kumpulan pola/template yang sudah didesain AMAN - sistem pilih ACAK dari sini tiap kali spawn")]
    public PolaRintangan[] kumpulanPola = new PolaRintangan[] {
        new PolaRintangan { namaPola = "Geser Kiri-Kanan", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = true,  tengah = false, kanan = false },
        }},
        new PolaRintangan { namaPola = "Zigzag Tengah", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = true,  kanan = false },
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = true,  kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
        }},
        new PolaRintangan { namaPola = "Dua Sisi Bergantian", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = true,  kanan = false },
            new BarisPola { kiri = false, tengah = true,  kanan = true  },
        }},
        new PolaRintangan { namaPola = "Lurus Tengah", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = true, kanan = false },
            new BarisPola { kiri = false, tengah = true, kanan = false },
        }},
        new PolaRintangan { namaPola = "Satu Sisi Saja", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = false, kanan = true },
        }},
        new PolaRintangan { namaPola = "Istirahat", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = false, kanan = false },
        }},
        new PolaRintangan { namaPola = "Lurus Kiri", baris = new BarisPola[] {
            new BarisPola { kiri = true, tengah = false, kanan = false },
            new BarisPola { kiri = true, tengah = false, kanan = false },
        }},
        new PolaRintangan { namaPola = "Lurus Kanan", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = false, kanan = true },
            new BarisPola { kiri = false, tengah = false, kanan = true },
        }},
        new PolaRintangan { namaPola = "Tengah Aman (Sisi Keblok)", baris = new BarisPola[] {
            new BarisPola { kiri = true, tengah = false, kanan = true },
            new BarisPola { kiri = true, tengah = false, kanan = true },
        }},
        new PolaRintangan { namaPola = "Spiral Panjang", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = true,  kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = false, tengah = true,  kanan = false },
            new BarisPola { kiri = true,  tengah = false, kanan = false },
        }},
        new PolaRintangan { namaPola = "Kejutan Setelah Jeda", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = true,  kanan = true  },
        }},
        new PolaRintangan { namaPola = "Zigzag Cepat", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
        }},

        // === TAMBAHAN: pola-pola LEBIH RUMIT - dominan pakai DOUBLE-BLOCK (2 lane keblok sekaligus,
        // cuma 1 jalur sempit yang aman), jadi maksa pemain pindah SAMPAI UJUNG tiap baris, bukan cuma geser dikit ===

        new PolaRintangan { namaPola = "Gelombang Tiga Fase", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // jalur aman: kanan
            new BarisPola { kiri = false, tengah = true,  kanan = true  }, // jalur aman: kiri
            new BarisPola { kiri = true,  tengah = false, kanan = true  }, // jalur aman: tengah
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // jalur aman: kanan
            new BarisPola { kiri = false, tengah = true,  kanan = true  }, // jalur aman: kiri
            new BarisPola { kiri = true,  tengah = false, kanan = true  }, // jalur aman: tengah
        }},

        new PolaRintangan { namaPola = "Labirin Berliku", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = false, kanan = false }, // single
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // double, jalur aman kanan
            new BarisPola { kiri = false, tengah = false, kanan = true  }, // single
            new BarisPola { kiri = false, tengah = true,  kanan = true  }, // double, jalur aman kiri
            new BarisPola { kiri = false, tengah = true,  kanan = false }, // single
            new BarisPola { kiri = true,  tengah = false, kanan = true  }, // double, jalur aman tengah
            new BarisPola { kiri = false, tengah = false, kanan = true  }, // single
            new BarisPola { kiri = true,  tengah = false, kanan = false }, // single
        }},

        new PolaRintangan { namaPola = "Jebakan Kejutan", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = false, kanan = true  }, // bikin pemain terbiasa di kiri/tengah
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // TIBA-TIBA flip - maksa lompat jauh ke kanan
            new BarisPola { kiri = true,  tengah = true,  kanan = false },
        }},

        new PolaRintangan { namaPola = "Ular Panjang", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = false, tengah = true,  kanan = false },
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = false, tengah = true,  kanan = false },
            new BarisPola { kiri = false, tengah = false, kanan = true  },
            new BarisPola { kiri = true,  tengah = false, kanan = false },
            new BarisPola { kiri = false, tengah = true,  kanan = false },
        }},

        new PolaRintangan { namaPola = "Sempit Bergantian Cepat", baris = new BarisPola[] {
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // aman kanan
            new BarisPola { kiri = false, tengah = true,  kanan = true  }, // aman kiri
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // aman kanan
            new BarisPola { kiri = false, tengah = true,  kanan = true  }, // aman kiri
            new BarisPola { kiri = true,  tengah = false, kanan = true  }, // aman tengah (napas sebentar)
        }},

        new PolaRintangan { namaPola = "Maraton Kombinasi", baris = new BarisPola[] {
            new BarisPola { kiri = false, tengah = true,  kanan = false }, // single tengah
            new BarisPola { kiri = true,  tengah = true,  kanan = false }, // double, aman kanan
            new BarisPola { kiri = true,  tengah = false, kanan = false }, // single kiri
            new BarisPola { kiri = false, tengah = false, kanan = false }, // jeda kosong (istirahat sejenak)
            new BarisPola { kiri = false, tengah = true,  kanan = true  }, // double, aman kiri
            new BarisPola { kiri = false, tengah = false, kanan = true  }, // single kanan
            new BarisPola { kiri = true,  tengah = false, kanan = true  }, // double, aman tengah
            new BarisPola { kiri = false, tengah = true,  kanan = false }, // single tengah
        }},
    };
    public TextMeshProUGUI textJumlahTabrakan;
    public TextMeshProUGUI textWaktuPengantaran;
    public TextMeshProUGUI textStatusPesanan;

    [Header("Garis Finish (jarak tempuh - GANTI sistem durasi waktu)")]
    [Tooltip("Prefab garis finish, biasanya full-width nutupin ketiga lane, dengan script GarisFinishOjol + BoxCollider2D (Is Trigger)")]
    public GameObject prefabGarisFinish;
    [Tooltip("Total jarak (dalam pixel) dari titik mulai pengantaran sampai garis finish")]
    public float jarakTujuanPengantaran = 4000f;
    [Tooltip("Kalau sisa jarak beneran udah di bawah angka ini (misal karena collider udah nyentuh duluan sebelum jarak pas 0), tampilan dipaksa jadi '0m' biar kelihatan lebih masuk akal/pas 'sampai'")]
    public float ambangJarakDekat = 100f;
    [Tooltip("TAMBAHAN: padding kiri-kanan (pixel) di luar lane paling kiri/kanan, biar garis finish keliatan lebih 'penuh' / gak mepet pas-pasan sama lane")]
    public float paddingLebarGarisFinish = 150f;

    [Header("Panel Hasil Pesanan (muncul begitu sampai tujuan)")]
    public GameObject panelHasilPesanan;
    public TextMeshProUGUI textHasilPesanan;
    [Tooltip("Berapa detik panel hasil ditampilkan sebelum otomatis lanjut ke pesanan berikutnya")]
    public float durasiTampilHasil = 2.5f;

    [Header("Pengaturan Shift & Pendapatan")]
    public int jumlahPesananPerShift = 4;
    [Tooltip("Pendapatan penuh kalau 0 tabrakan")]
    public int pendapatanDasarPerPesanan = 25000;
    [Tooltip("Potongan pendapatan per 1x nabrak rintangan (proposal: makin banyak nabrak, makin kecil pendapatan)")]
    public int penguranganPerTabrakan = 5000;

    [Header("Efek ke Parameter (diterapkan lewat HasilKerjaPartTime setelah shift SELESAI)")]
    public float laparBerkurangPerShift = 50f;
    public float sanityBerkurangPerShift = 8f;
    [Tooltip("Berapa jam in-game yang dilewati sepulang shift")]
    public float jamDilewatiShift = 6f;

    [Header("Audio Efek Ojol")]
    [Tooltip("Komponen AudioSource untuk memutar sound effect ojol")]
    public AudioSource audioSourceOjol;
    [Tooltip("Sound effect saat pesanan baru masuk")]
    public AudioClip klipPesananMasuk;
    [Range(0f, 1f)]
    public float volumePesananMasuk = 0.8f;

    // --- TAMBAHAN: Variabel untuk suara nabrak ---
    [Tooltip("Sound effect saat menabrak rintangan")]
    public AudioClip klipTabrak;
    [Range(0f, 1f)]
    public float volumeTabrak = 0.8f;

    [Header("Audio Efek Mengantar (Loop)")]
    [Tooltip("Komponen AudioSource KHUSUS untuk suara mesin/jalan (biar gak nabrak suara lain)")]
    public AudioSource audioSourceMesin;
    [Tooltip("Sound effect saat mengantar pesanan (seperti suara motor/angin)")]
    public AudioClip klipSuaraJalan;
    [Range(0f, 1f)]
    public float volumeJalan = 0.8f;

    [Header("Scene")]
    public string namaSceneUtama = "MainScene";

    // --- State internal shift ---
    private int pesananSaatIni = 0;
    private int laneSaatIni = 1; // 0 = kiri, 1 = tengah, 2 = kanan
    private int jumlahTabrakanSaatIni = 0;
    private int jumlahDihindariSaatIni = 0;
    private int gajiTerkumpul = 0;
    private float posisiYGarisFinishSaatIni = float.MaxValue; // --- TAMBAHAN: posisi Y garis finish, buat cegah rintangan spawn di area finish/di belakangnya ---
    private bool sedangMengantar = false;
    private bool pesananDiterima = false; // --- TAMBAHAN: flag buat cegah race antara swipe manual & timeout ---
    private Coroutine coroutineBatasTerima;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {   
        if (layarTransisi != null) StartCoroutine(FadeMasuk());
        
        if (posisiLane == null || posisiLane.Length != 3) {
            Debug.LogError("[OjolManager] Posisi Lane harus diisi TEPAT 3 elemen (kiri, tengah, kanan)!");
        }

        // --- TAMBAHAN: hitung otomatis batas spawn atas/bawah dari tinggi Wadah Rintangan,
        // biar rintangan selalu muncul dari LUAR layar atas, apapun resolusi/Canvas Scaler-nya ---
        if (otomatisHitungBatasVertikal && wadahRintangan != null) {
            RectTransform wadahRect = wadahRintangan as RectTransform;
            if (wadahRect != null) {
                float setengahTinggi = wadahRect.rect.height / 2f;
                yMulaiSpawnRintangan = setengahTinggi + 100f;   // 100px di luar batas atas, biar spawn-nya gak kelihatan mendadak
                yBatasBawahRintangan = -setengahTinggi - 100f;

                // --- TAMBAHAN: log biar ketauan angka SEBENARNYA yang dipakai runtime (Inspector suka reset pas Stop Play) ---
                Debug.Log("[OjolManager] Wadah Rintangan tinggi = " + wadahRect.rect.height +
                          " -> Y Mulai Spawn = " + yMulaiSpawnRintangan +
                          ", Y Batas Bawah = " + yBatasBawahRintangan);
            } else {
                Debug.LogWarning("[OjolManager] Wadah Rintangan bukan RectTransform, gak bisa hitung otomatis - pakai nilai manual.");
            }
        }

        if (panelPesananMasuk) panelPesananMasuk.SetActive(false);
        if (panelMengantar) panelMengantar.SetActive(false);
        if (panelHasilPesanan) panelHasilPesanan.SetActive(false);

        MulaiShift();
    }

    void Update()
    {
        if (!sedangMengantar) return;

        // --- Input pindah jalur, gaya sama kayak PauseMenuController (Keyboard baru/InputSystem) ---
        if (Keyboard.current != null) {
            if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) PindahLane(-1);
            if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) PindahLane(1);
        }

        // --- TAMBAHAN: pindah lane pakai KLIK/TAP KURSOR - klik di lane manapun (kiri/tengah/kanan),
        // karakter langsung lompat ke lane yang diklik itu (bukan geser satu-satu kayak WASD) ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            DeteksiKlikLane(Mouse.current.position.ReadValue());
        }

        if (karakterPemain != null && posisiLane != null && posisiLane.Length == 3) {
            Vector2 posisiTarget = new Vector2(posisiLane[laneSaatIni].anchoredPosition.x, karakterPemain.anchoredPosition.y);
            karakterPemain.anchoredPosition = Vector2.Lerp(karakterPemain.anchoredPosition, posisiTarget, Time.deltaTime * kecepatanPindahLane);
        }
    }

    void PindahLane(int arah)
    {
        laneSaatIni = Mathf.Clamp(laneSaatIni + arah, 0, 2);
    }

    void DeteksiKlikLane(Vector2 posisiKlikLayar)
    {
        if (EventSystem.current != null) {
            PointerEventData dataPointer = new PointerEventData(EventSystem.current);
            dataPointer.position = posisiKlikLayar;
            
            // Lakukan raycast khusus untuk elemen UI Overlay
            List<RaycastResult> hasilRaycastUI = new List<RaycastResult>();
            EventSystem.current.RaycastAll(dataPointer, hasilRaycastUI);

            foreach (RaycastResult hasil in hasilRaycastUI) {
                // Cari apakah objek UI yang diklik memiliki script ZonaLaneOjol
                ZonaLaneOjol zona = hasil.gameObject.GetComponent<ZonaLaneOjol>();
                
                if (zona != null) {
                    // Jika kena zona, pindahkan karakter dan langsung keluar dari fungsi
                    laneSaatIni = Mathf.Clamp(zona.laneIndex, 0, 2);
                    return; 
                }

                // Opsional: Jika klik mengenai UI interaktif lain (misal tombol Pause), 
                // abaikan klik agar tidak tembus ke zona di bawahnya.
                if (hasil.gameObject.GetComponent<Selectable>() != null) {
                    return;
                }
            }
        }
    }

    private IEnumerator FadeMasuk()
    {
        if (layarTransisi != null) {
            layarTransisi.gameObject.SetActive(true);
            layarTransisi.raycastTarget = true;
            float t = 0f;
            while (t < durasiFade) {
                t += Time.deltaTime;
                Color c = layarTransisi.color;
                c.a = Mathf.Lerp(1f, 0f, t / durasiFade);
                layarTransisi.color = c;
                yield return null;
            }
            layarTransisi.raycastTarget = false;
        }
    }

    private IEnumerator FadeKeluar(string namaScene)
    {
        // --- TAMBAHAN: Panggil MinigameAudioManager untuk fade-out musik BGM ---
        if (MinigameAudioManager.Instance != null) MinigameAudioManager.Instance.HentikanMusik();

        if (layarTransisi != null) {
            layarTransisi.gameObject.SetActive(true);
            layarTransisi.raycastTarget = true;
            float t = 0f;
            while (t < durasiFade) {
                t += Time.deltaTime;
                Color c = layarTransisi.color;
                c.a = Mathf.Lerp(0f, 1f, t / durasiFade);
                layarTransisi.color = c;
                yield return null;
            }
        }
        SceneManager.LoadScene(namaScene, LoadSceneMode.Single);
    }

    public void MulaiShift()
    {
        pesananSaatIni = 0;
        gajiTerkumpul = 0;
        UpdateTeksGaji();
        MulaiTungguPesanan();
    }

    void MulaiTungguPesanan()
    {
        pesananSaatIni++;
        UpdateTeksPesanan();

        if (panelMengantar) panelMengantar.SetActive(false);
        if (panelPesananMasuk) panelPesananMasuk.SetActive(false);
        if (panelMenunggu) panelMenunggu.SetActive(true);

        StartCoroutine(TungguPesananCoroutine());
    }

    IEnumerator TungguPesananCoroutine()
    {
        float sisaWaktu = Random.Range(waktuTungguMin, waktuTungguMax);
        while (sisaWaktu > 0f) {
            sisaWaktu -= Time.deltaTime;
            if (textStatusMenunggu) textStatusMenunggu.text = "Menunggu pesanan... (" + Mathf.CeilToInt(sisaWaktu) + " dtk)";
            yield return null;
        }

        if (panelMenunggu) panelMenunggu.SetActive(false);
        if (panelPesananMasuk) panelPesananMasuk.SetActive(true);
        if (textInfoPesanan) textInfoPesanan.text = "Pesanan masuk! Geser buat terima.";

        // --- TAMBAHAN: Mainkan sound effect saat pesanan masuk ---
        if (audioSourceOjol != null && klipPesananMasuk != null) {
            audioSourceOjol.PlayOneShot(klipPesananMasuk, volumePesananMasuk);
        }

        // --- TAMBAHAN: reset tombol geser & mulai batas waktu terima pesanan ---
        pesananDiterima = false;
        if (tombolGeserTerima != null) tombolGeserTerima.ResetHandle();
        coroutineBatasTerima = StartCoroutine(BatasWaktuTerimaPesananCoroutine());
    }

    // --- TAMBAHAN: kalau pemain gak geser tombol dalam batas waktu, pesanan dianggap TERLEWAT ---
    IEnumerator BatasWaktuTerimaPesananCoroutine()
    {
        float sisaWaktu = batasWaktuTerimaPesanan;
        while (sisaWaktu > 0f && !pesananDiterima) {
            sisaWaktu -= Time.deltaTime;
            if (textWaktuTerimaPesanan) textWaktuTerimaPesanan.text = "Geser sebelum: " + Mathf.CeilToInt(sisaWaktu) + " dtk";
            yield return null;
        }

        if (!pesananDiterima) {
            PesananTerlewat();
        }
    }

    // --- Pesanan terlewat: TETAP kepakai 1 jatah dari jumlahPesananPerShift, tapi gak dapat gaji sama sekali ---
    void PesananTerlewat()
    {
        if (panelPesananMasuk) panelPesananMasuk.SetActive(false);

        if (pesananSaatIni < jumlahPesananPerShift) {
            MulaiTungguPesanan();
        } else {
            SelesaikanShift();
        }
    }

    // --- Dipanggil TombolGeserTerimaPesanan begitu geser sampai ambang batas ---
    public void TerimaPesanan()
    {
        if (pesananDiterima) return; // guard: cegah dobel-trigger (misal swipe & timeout kebetulan barengan)
        pesananDiterima = true;
        if (coroutineBatasTerima != null) StopCoroutine(coroutineBatasTerima);

        if (panelPesananMasuk) panelPesananMasuk.SetActive(false);
        if (panelMengantar) panelMengantar.SetActive(true);

        // --- TAMBAHAN: Mulai mainkan sound effect jalan (looping) ---
        if (audioSourceMesin != null && klipSuaraJalan != null) {
            audioSourceMesin.clip = klipSuaraJalan;
            audioSourceMesin.loop = true;
            audioSourceMesin.volume = volumeJalan;
            audioSourceMesin.Play();
        }

        laneSaatIni = 1;
        jumlahTabrakanSaatIni = 0;
        jumlahDihindariSaatIni = 0;
        sedangMengantar = true;
        posisiYGarisFinishSaatIni = float.MaxValue; // --- TAMBAHAN: reset juga, biar spawn rintangan diizinkan lagi di awal pesanan baru ---

        if (karakterPemain != null && posisiLane != null && posisiLane.Length == 3) {
            karakterPemain.anchoredPosition = new Vector2(posisiLane[1].anchoredPosition.x, karakterPemain.anchoredPosition.y);
        }

        UpdateTeksTabrakan();
        if (textStatusPesanan) textStatusPesanan.text = "";

        SpawnGarisFinish();
        StartCoroutine(SpawnRintanganBerulang());
    }

    // --- TAMBAHAN: spawn garis finish di titik yang jaraknya = jarakTujuanPengantaran dari posisi pemain ---
    void SpawnGarisFinish()
    {
        if (prefabGarisFinish == null || wadahRintangan == null) {
            Debug.LogError("[OjolManager] Prefab Garis Finish atau Wadah Rintangan belum diisi!");
            return;
        }

        float posisiYPemain = karakterPemain != null ? karakterPemain.anchoredPosition.y : 0f;
        float posisiAwalGarisFinish = posisiYPemain + jarakTujuanPengantaran;

        GameObject objek = Instantiate(prefabGarisFinish, wadahRintangan, false); // --- FIX: worldPositionStays = false, biar sizeDelta/scale prefab TIDAK ikut direcalculate/kekecilan gara-gara skala wadahRintangan ---

        // --- FIX: paksa lebar garis finish selalu PAS nutupin ketiga lane (kiri sampai kanan) + padding,
        // gak gantung ke sizeDelta prefab yang gampang keliatan kekecilan tergantung anchor/parent ---
        RectTransform rectFinish = objek.GetComponent<RectTransform>();
        if (rectFinish != null && posisiLane != null && posisiLane.Length == 3) {
            float xKiri = posisiLane[0].anchoredPosition.x;
            float xKanan = posisiLane[2].anchoredPosition.x;
            float lebarTarget = Mathf.Abs(xKanan - xKiri) + paddingLebarGarisFinish * 2f;

            rectFinish.localScale = Vector3.one; // --- FIX: pastikan scale gak ikut ke-inherit aneh dari prefab/parent ---
            Vector2 ukuran = rectFinish.sizeDelta;
            ukuran.x = lebarTarget;
            rectFinish.sizeDelta = ukuran;
            // --- CATATAN: posisi X sengaja gak diatur di sini, karena GarisFinishOjol.Setup() di bawah
            // bakal override anchoredPosition.x jadi 0 - itu udah benar SELAMA posisiLane[0] & posisiLane[2]
            // simetris di kiri-kanan angka 0. Kalau lane kamu TIDAK simetris, kasih tau aku biar disesuaikan. ---
        }

        GarisFinishOjol garis = objek.GetComponent<GarisFinishOjol>();
        if (garis != null) {
            garis.Setup(posisiAwalGarisFinish, kecepatanRintangan);
        }
    }

    // --- TAMBAHAN: dipanggil GarisFinishOjol.Update() tiap frame, hitung & tampilkan SISA JARAK beneran ---
    public void UpdateSisaJarakFinish(float posisiYGarisFinishSaatIniParam)
    {
        posisiYGarisFinishSaatIni = posisiYGarisFinishSaatIniParam; // --- TAMBAHAN: simpan posisi Y-nya, dipakai SpawnSatuRintangan() ---

        if (!sedangMengantar) return;

        float posisiYPemain = karakterPemain != null ? karakterPemain.anchoredPosition.y : 0f;
        float sisaJarak = Mathf.Max(0f, posisiYGarisFinishSaatIniParam - posisiYPemain);
        if (sisaJarak < ambangJarakDekat) sisaJarak = 0f; // --- TAMBAHAN: biar keliatan "sampai" begitu udah cukup deket, gak numpuk angka kecil ganjil ---

        if (textWaktuPengantaran) textWaktuPengantaran.text = "Sisa jarak: " + Mathf.CeilToInt(sisaJarak) + "m";
    }

    IEnumerator SpawnRintanganBerulang()
    {
        while (sedangMengantar) {
            if (kumpulanPola == null || kumpulanPola.Length == 0) {
                yield return null;
                continue;
            }

            PolaRintangan pola = kumpulanPola[Random.Range(0, kumpulanPola.Length)];
            float totalTinggiPola = (pola.baris.Length - 1) * jarakAntarBarisDalamPola;

            // --- FIX: cek SELURUH tinggi pola (termasuk baris paling atas/jauh), bukan cuma titik dasar spawn.
            // Kalau garis finish udah lebih dekat dari baris TERTINGGI pola ini, JANGAN spawn sama sekali -
            // biar bener-bener gak ada satupun rintangan yang nongol di area finish/di belakangnya. ---
            if (posisiYGarisFinishSaatIni <= yMulaiSpawnRintangan + totalTinggiPola) {
                yield return null;
                continue;
            }

            SpawnSatuPola(pola);

            // --- Tunggu sampai waktunya pola berikutnya, dihitung dari total "tinggi" pola ini + jarak antar pola ---
            float totalJarakSampaiPolaBerikutnya = totalTinggiPola + jarakAntarPola + Random.Range(0f, variasiJarakAntarPola);
            float waktuTunggu = kecepatanRintangan > 0f ? totalJarakSampaiPolaBerikutnya / kecepatanRintangan : 1f;

            yield return new WaitForSeconds(waktuTunggu);
        }
    }

    // --- Spawn SATU POLA UTUH sekaligus: tiap baris di dalam pola langsung dipasang di posisi Y masing-masing
    // (bukan nunggu delay tiap baris), biar spacing antar barisnya PERSIS sesuai desain pola, bukan hasil timing acak ---
    void SpawnSatuPola(PolaRintangan pola)
    {
        if (prefabRintanganArray == null || prefabRintanganArray.Length == 0 || wadahRintangan == null || posisiLane == null || posisiLane.Length != 3) return;
        if (pola.baris == null) return;

        for (int i = 0; i < pola.baris.Length; i++) {
            float posisiYBarisIni = yMulaiSpawnRintangan + (i * jarakAntarBarisDalamPola);
            BarisPola baris = pola.baris[i];

            if (baris.kiri) SpawnSatuRintanganDiLane(0, posisiYBarisIni);
            if (baris.tengah) SpawnSatuRintanganDiLane(1, posisiYBarisIni);
            if (baris.kanan) SpawnSatuRintanganDiLane(2, posisiYBarisIni);
        }
    }

    // --- TAMBAHAN: pilih SATU prefab rintangan secara acak dari 6 tipe yang ada di prefabRintanganArray,
    // biar tiap rintangan yang muncul di layar tampilannya bervariasi (gak monoton itu-itu aja) ---
    GameObject PilihPrefabRintanganAcak()
    {
        // --- Filter slot kosong di Inspector dulu, biar gak error kalau ternyata belum keisi 6-6nya ---
        List<GameObject> validPrefab = new List<GameObject>();
        for (int i = 0; i < prefabRintanganArray.Length; i++) {
            if (prefabRintanganArray[i] != null) validPrefab.Add(prefabRintanganArray[i]);
        }
        if (validPrefab.Count == 0) return null;
        return validPrefab[Random.Range(0, validPrefab.Count)];
    }

    void SpawnSatuRintanganDiLane(int lane, float posisiY)
    {
        GameObject prefabTerpilih = PilihPrefabRintanganAcak();
        if (prefabTerpilih == null) return;

        GameObject objek = Instantiate(prefabTerpilih, wadahRintangan, false);
        RintanganOjol rintangan = objek.GetComponent<RintanganOjol>();
        if (rintangan != null) {
            rintangan.Setup(lane, posisiLane[lane].anchoredPosition.x, posisiY, kecepatanRintangan);
        }
    }

    // --- Dipanggil RintanganOjol.OnDestroy() - gak dipakai lagi (sistem pola gak butuh tracking lane real-time),
    // dibiarkan kosong biar RintanganOjol.cs yang udah ada gak perlu diubah/error ---
    public void LaneDibersihkan(int lane) { }

    // --- Dipanggil RintanganOjol.Update() buat cek kapan rintangan udah lewat batas bawah (dihindari) ---
    public float BatasBawahHapus => yBatasBawahRintangan;

    public void TabrakRintangan()
    {
        jumlahTabrakanSaatIni++;
        UpdateTeksTabrakan();

        // --- TAMBAHAN: Mainkan sound effect tabrakan di sini ---
        if (audioSourceOjol != null && klipTabrak != null) {
            audioSourceOjol.PlayOneShot(klipTabrak, volumeTabrak);
        }
    }

    public void RintanganDihindari()
    {
        jumlahDihindariSaatIni++;
    }

    // --- Dipanggil GarisFinishOjol begitu garis finish nyentuh pemain (sampai tujuan) ---
    public void SelesaikanPengantaran()
    {
        if (!sedangMengantar) return; // guard: cegah dobel-trigger
        sedangMengantar = false;

        // --- TAMBAHAN: Hentikan suara jalan karena sudah sampai ---
        if (audioSourceMesin != null) audioSourceMesin.Stop();

        int pendapatan = Mathf.Max(0, pendapatanDasarPerPesanan - (jumlahTabrakanSaatIni * penguranganPerTabrakan));
        gajiTerkumpul += pendapatan;
        UpdateTeksGaji();

        // --- TAMBAHAN: tampilkan Panel Hasil Pesanan TERPISAH, gantiin Panel Mengantar sepenuhnya ---
        if (panelMengantar) panelMengantar.SetActive(false);
        if (panelHasilPesanan) panelHasilPesanan.SetActive(true);
        if (textHasilPesanan) {
            textHasilPesanan.text = "Sampai tujuan!\n+Rp " + pendapatan.ToString("N0") +
                "\nKena rintangan: " + jumlahTabrakanSaatIni + "x" +
                "\nBerhasil dihindari: " + jumlahDihindariSaatIni + "x";
        }

        StartCoroutine(BersihkanRintanganDanLanjut());
    }

    IEnumerator BersihkanRintanganDanLanjut()
    {
        yield return new WaitForSeconds(durasiTampilHasil); // beri waktu pemain baca Panel Hasil Pesanan dulu

        if (panelHasilPesanan) panelHasilPesanan.SetActive(false);

        if (wadahRintangan != null) {
            foreach (Transform anak in wadahRintangan) Destroy(anak.gameObject);
        }

        if (pesananSaatIni < jumlahPesananPerShift) {
            MulaiTungguPesanan();
        } else {
            SelesaikanShift();
        }
    }

    // --- Dipanggil tombol "Pulang" kalau pemain mau sudahi shift lebih awal ---
    public void PulangLebihAwal()
    {
        sedangMengantar = false;
        StopAllCoroutines();

        // --- TAMBAHAN: Hentikan suara jalan jika dibatalkan paksa ---
        if (audioSourceMesin != null) audioSourceMesin.Stop();

        // --- TAMBAHAN: bersihin sisa rintangan & garis finish yang masih ada di layar (mereka jalan lewat Update(),
        // bukan Coroutine, jadi StopAllCoroutines() di atas gak otomatis nyetop mereka) ---
        if (wadahRintangan != null) {
            foreach (Transform anak in wadahRintangan) Destroy(anak.gameObject);
        }

        SelesaikanShift();
    }

    void SelesaikanShift()
    {
        // Gunakan gajiTerkumpul, bukan gajiBersih
        HasilKerjaPartTime.SimpanHasil(gajiTerkumpul, laparBerkurangPerShift, sanityBerkurangPerShift, jamDilewatiShift);
        
        // Panggil Coroutine untuk transisi ke Main Scene
        StartCoroutine(FadeKeluar(namaSceneUtama));
    }

    void UpdateTeksGaji()
    {
        if (textGajiTerkumpul) textGajiTerkumpul.text = "Gaji shift ini: Rp " + gajiTerkumpul.ToString("N0");
    }

    void UpdateTeksPesanan()
    {
        if (textPesananKe) textPesananKe.text = "Pesanan " + pesananSaatIni + " / " + jumlahPesananPerShift;
    }

    void UpdateTeksTabrakan()
    {
        if (textJumlahTabrakan) textJumlahTabrakan.text = "Tabrakan: " + jumlahTabrakanSaatIni + "x";
    }
}