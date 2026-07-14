using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// --- Minigame Skripsi: berjalan di SCENE TERPISAH, di-load ADDITIVE di atas scene utama ---
// GameManager TIDAK punya referensi apapun ke script ini. Semua komunikasi
// terjadi lewat: (1) subscribe ke event publik GameManager, (2) manggil method publik GameManager.
public class MinigameSkripsiManager : MonoBehaviour
{
    [Header("Referensi UI Minigame")]
    public TextMeshProUGUI textKalimatTarget;
    public TMP_InputField inputKetikan;
    public TextMeshProUGUI textJumlahTypo;
    public TextMeshProUGUI textProgresSesi;

    [Header("Bank Kalimat (istilah akademis, diambil acak)")]
    public string[] daftarKalimat = new string[] {
        "Metodologi penelitian kualitatif",
        "Analisis regresi linear berganda",
        "Studi literatur sistematis",
        "Uji validitas dan reliabilitas",
    };

    [Header("Pengaturan Minigame")]
    [Tooltip("Waktu in-game berjalan berapa kali lebih cepat saat minigame ini aktif")]
    public float pengaliKecepatanWaktuSaatMinigame = 6f;
    [Tooltip("Sanity berkurang segini tiap tick waktu selama minigame berjalan (otomatis x2 kalau lapar kritis)")]
    public float sanityBerkurangPerTick = 0.5f;
    [Tooltip("Batas toleransi kesalahan ketik sebelum sesi dianggap gagal")]
    public int maxTypo = 3;
    [Tooltip("Progres Skripsi maksimum (%) yang bisa didapat dari satu sesi minigame berhasil")]
    public float progresMaksimalPerSesi = 10f;

    [Header("Scene")]
    [Tooltip("Nama scene minigame ini sendiri, HARUS sama persis dengan nama file & yang didaftarkan di Build Settings")]
    public string namaSceneMinigame = "MinigameSkripsi";

    private string kalimatSaatIni = "";
    private int indexKarakterBenar = 0;
    private int jumlahTypoSaatIni = 0;
    private float progresSesiIni = 0f; // 0 - progresMaksimalPerSesi, diakumulasi selama sesi berjalan
    private bool minigameAktif = false;

    // --- Subscribe saat object aktif, unsubscribe saat nonaktif (WAJIB, hindari memory leak/NullReference) ---
    void OnEnable()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTickWaktu += TanganiTickWaktu;
            GameManager.Instance.OnBatasWaktuTercapai += TanganiInterupsiPaksa;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTickWaktu -= TanganiTickWaktu;
            GameManager.Instance.OnBatasWaktuTercapai -= TanganiInterupsiPaksa;
        }
    }

    // --- Scene ini di-load = minigame otomatis mulai, gak perlu dipicu manual dari scene lain ---
    void Start()
    {
        MulaiMinigame();
    }

    public void MulaiMinigame()
    {
        if (minigameAktif) return;
        if (GameManager.Instance == null) {
            Debug.LogError("GameManager.Instance null! Pastikan scene minigame di-load ADDITIVE, bukan Single.");
            return;
        }

        minigameAktif = true;
        jumlahTypoSaatIni = 0;
        progresSesiIni = 0f;
        UpdateTeksTypo();
        UpdateTeksProgres();
        PasangKalimatBaru();

        if (inputKetikan) {
            inputKetikan.onValueChanged.AddListener(TanganiPerubahanInput);
            inputKetikan.text = "";
            inputKetikan.ActivateInputField();
        }

        KunciPemainDiSceneUtama(true);

        // --- Percepat waktu in-game selama minigame berlangsung ---
        GameManager.Instance.SetPengaliKecepatanWaktu(pengaliKecepatanWaktuSaatMinigame);
    }

    void PasangKalimatBaru()
    {
        if (daftarKalimat == null || daftarKalimat.Length == 0) return;

        kalimatSaatIni = daftarKalimat[Random.Range(0, daftarKalimat.Length)];
        indexKarakterBenar = 0;

        if (textKalimatTarget) textKalimatTarget.text = kalimatSaatIni;
        if (inputKetikan) inputKetikan.SetTextWithoutNotify("");
    }

    // --- Dipanggil tiap kali isi TMP_InputField berubah (tiap 1 karakter diketik) ---
    void TanganiPerubahanInput(string teksBaru)
    {
        if (!minigameAktif || string.IsNullOrEmpty(kalimatSaatIni)) return;
        if (teksBaru.Length <= indexKarakterBenar) return; // penghapusan (backspace), abaikan

        char karakterDiketik = teksBaru[teksBaru.Length - 1];
        char karakterSeharusnya = kalimatSaatIni[indexKarakterBenar];

        if (karakterDiketik == karakterSeharusnya) {
            indexKarakterBenar++;
            float progresPerKarakter = progresMaksimalPerSesi / kalimatSaatIni.Length;
            TanganiKetikBenar(progresPerKarakter);

            if (indexKarakterBenar >= kalimatSaatIni.Length) {
                PasangKalimatBaru(); // kalimat ini selesai, lanjut ke kalimat berikutnya
            }
        } else {
            TanganiTypo();
            // Hapus karakter yang salah biar pemain gak numpuk ketikan di atas kesalahan
            if (inputKetikan) inputKetikan.SetTextWithoutNotify(teksBaru.Substring(0, teksBaru.Length - 1));
        }
    }

    // --- Setiap kali GameManager tick (bukan tiap frame), kurangi Sanity berbasis durasi bermain ---
    // Pengali "lapar kritis -> sanity 2x lebih cepat" sudah otomatis ditangani di dalam KurangiSanity().
    void TanganiTickWaktu(float deltaJam)
    {
        if (!minigameAktif) return;
        GameManager.Instance.KurangiSanity(sanityBerkurangPerTick);
    }

    public void TanganiKetikBenar(float tambahProgres)
    {
        if (!minigameAktif) return;
        progresSesiIni = Mathf.Clamp(progresSesiIni + tambahProgres, 0f, progresMaksimalPerSesi);
        UpdateTeksProgres();
    }

    public void TanganiTypo()
    {
        if (!minigameAktif) return;

        jumlahTypoSaatIni++;
        UpdateTeksTypo();

        if (jumlahTypoSaatIni >= maxTypo) {
            SelesaikanSesi(); // gagal karena typo melebihi batas toleransi
        }
    }

    void UpdateTeksTypo()
    {
        if (textJumlahTypo) textJumlahTypo.text = jumlahTypoSaatIni + " / " + maxTypo + " Typo";
    }

    void UpdateTeksProgres()
    {
        if (textProgresSesi) textProgresSesi.text = "+" + progresSesiIni.ToString("F1") + "% Skripsi";
    }

    // --- FORCE QUIT: dipanggil otomatis oleh GameManager LEWAT EVENT, SEBELUM ProsesTidur() mulai ---
    void TanganiInterupsiPaksa()
    {
        if (!minigameAktif) return;
        Debug.Log("Waktu habis - minigame skripsi dihentikan paksa (Force Quit).");
        SelesaikanSesi();
    }

    // --- Titik keluar tunggal buat sesi minigame: selesai normal, gagal typo, ATAU force quit ---
    void SelesaikanSesi()
    {
        minigameAktif = false;

        if (GameManager.Instance != null) {
            GameManager.Instance.ResetPengaliKecepatanWaktu();
            GameManager.Instance.TambahProgresSkripsi(progresSesiIni);
        }

        KunciPemainDiSceneUtama(false);
        progresSesiIni = 0f;

        // --- Scene minigame ini di-unload, scene utama TIDAK ikut ke-unload (additive) ---
        SceneManager.UnloadSceneAsync(namaSceneMinigame);
    }

    void KunciPemainDiSceneUtama(bool kunci)
    {
        // FindFirstObjectByType tetap bisa nemu object di scene utama walau kita ada di scene additive lain
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(kunci);
    }
}