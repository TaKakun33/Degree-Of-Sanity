using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text;
using TMPro;

// --- Minigame Skripsi: gaya typing-test (kata berjalan terus, tanpa tekanan waktu) ---
// Berjalan di SCENE TERPISAH, di-load ADDITIVE di atas scene utama.
// GameManager TIDAK punya referensi apapun ke script ini (event-driven, lihat OnEnable/OnDisable).
public class MinigameSkripsiManager : MonoBehaviour
{
    [Header("Referensi UI Minigame")]
    public TextMeshProUGUI textKalimatTarget; // menampilkan beberapa kata berjalan + highlight karakter
    public TMP_InputField inputKetikan;
    public TextMeshProUGUI textJumlahTypo;
    public TextMeshProUGUI textProgresSesi;

    [Header("Bank Kata (dirangkai ULANG jadi ratusan/ribuan kata per sesi, gaya typing-test)")]
    public string[] bankKata = new string[] {
        "sekarang","yaitu","selama","kalau","tidak","sudah","baru","pula","paling","terhadap",
        "di","mulai","malam","pusat","serta","sesuai","dengan","waktu","sedang","ketika",
        "skripsi","progres","dosen","kampus","penelitian","data","metode","hasil","analisis","bab",
        "revisi","sidang","deadline","semester","tugas","catatan","laptop","referensi","jurnal","kutipan",
        "adik","rumah","uang","kerja","malam","pagi","lelah","semangat","fokus","istirahat",
        "makan","tidur","mimpi","harapan","masa","depan","keluarga","teman","dukungan","usaha",
        "belajar","paham","bingung","yakin","ragu","coba","lagi","hampir","selesai","lulus",
    };

    [Header("Pengaturan Tampilan (gaya typing-test)")]
    [Tooltip("Berapa kata ditampilkan sekaligus di layar (kata sekarang + kata-kata berikutnya)")]
    public int jumlahKataTampil = 12;
    [Tooltip("Total kata yang dirangkai untuk satu sesi (proposal: ~1000 kata acak)")]
    public int jumlahKataSesi = 1000;
    [Tooltip("Berapa kata yang perlu diketik benar untuk mendapat progres skripsi PENUH (progresMaksimalPerSesi)")]
    public int jumlahKataUntukProgresPenuh = 150;

    [Header("Pengaturan Minigame")]
    [Tooltip("Waktu in-game berjalan berapa kali lebih cepat saat minigame ini aktif")]
    public float pengaliKecepatanWaktuSaatMinigame = 6f;
    [Tooltip("Sanity berkurang segini tiap tick waktu selama minigame berjalan (KECIL SENGAJA, biar gak over-drop; otomatis x2 kalau lapar kritis)")]
    public float sanityBerkurangPerTick = 0.15f;
    [Tooltip("TAMBAHAN: Lapar berkurang segini tiap tick waktu selama minigame berjalan - makin lama ngerjain, makin banyak berkurang, TAPI dibatasi total maksimal per sesi (lihat field di bawah)")]
    public float laparBerkurangPerTick = 0.15f;
    [Tooltip("TAMBAHAN: Lapar MAKSIMAL yang bisa berkurang dalam SATU sesi skripsi, seberapa lama pun dikerjakan")]
    public float laparBerkurangMaksimalPerSesi = 20f;
    [Tooltip("Batas toleransi kesalahan ketik sebelum sesi dianggap gagal")]
    public int maxTypo = 3;
    [Tooltip("Progres Skripsi maksimum (%) yang bisa didapat dari satu sesi minigame")]
    public float progresMaksimalPerSesi = 10f;

    [Header("Scene")]
    [Tooltip("Nama scene minigame ini sendiri, HARUS sama persis dengan nama file & yang didaftarkan di Build Settings")]
    public string namaSceneMinigame = "MinigameSkripsi";

    [Header("Upgrade Permanen: Keyboard Ergonomis (Item Toko 4)")]
    [Tooltip("Kalau pemain punya Keyboard Ergonomis, Max Typo ditambah segini (lebih toleran, 'lebih mudah mengetik dengan presisi')")]
    public int bonusMaxTypoKeyboard = 2;
    [Range(0f, 1f)]
    [Tooltip("Kalau pemain punya Keyboard Ergonomis, drain Sanity per tick dikali segini (lebih ringan/santai)")]
    public float pengaliSanityKeyboard = 0.5f;

    [Header("Upgrade Permanen: Buku Referensi (Item Toko 5)")]
    [Tooltip("Kalau pemain punya Buku Referensi, plafon Progres Skripsi per sesi dikali segini")]
    public float pengaliProgresBuku = 1.5f;

    // --- Nilai EFEKTIF yang dipakai selama sesi berjalan, dihitung sekali di MulaiMinigame()
    // berdasarkan upgrade permanen yang dipunyai pemain (InventoryManager) ---
    private int maxTypoEfektif;
    private float sanityBerkurangPerTickEfektif;
    private float progresMaksimalPerSesiEfektif;

    private List<string> urutanKataSesi;
    private int indexKataSesi = 0;       // posisi kata yang sedang dikerjakan di urutanKataSesi
    private string kataSaatIni = "";
    private int indexKarakterBenar = 0;  // posisi karakter yang sudah benar diketik di kataSaatIni

    private int jumlahTypoSaatIni = 0;
    private float progresSesiIni = 0f;
    private float totalLaparBerkurangSesiIni = 0f; // --- TAMBAHAN ---
    private bool minigameAktif = false;

    // --- Subscribe saat object aktif, unsubscribe saat nonaktif (WAJIB, hindari memory leak/NullReference) ---
    void OnEnable()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTickWaktu += TanganiTickWaktu;
            GameManager.Instance.OnBatasWaktuTercapai += TanganiInterupsiPaksa;
            GameManager.Instance.OnPermainanBerakhir += TanganiInterupsiPaksa; // Bad/Good Ending juga menutup minigame
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnTickWaktu -= TanganiTickWaktu;
            GameManager.Instance.OnBatasWaktuTercapai -= TanganiInterupsiPaksa;
            GameManager.Instance.OnPermainanBerakhir -= TanganiInterupsiPaksa;
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

        // --- Skripsi cuma boleh dikerjakan 1x per hari (safety net; PemicuMinigameSkripsi juga sudah cek ini) ---
        if (!GameManager.Instance.BisaKerjakanSkripsiHariIni) {
            Debug.Log("Skripsi sudah dikerjakan hari ini - minigame ditutup lagi.");
            SceneManager.UnloadSceneAsync(namaSceneMinigame);
            return;
        }
        GameManager.Instance.TandaiSkripsiSudahDikerjakan();

        // --- TAMBAHAN: terapkan upgrade permanen (Keyboard Ergonomis & Buku Referensi) kalau dipunyai pemain ---
        maxTypoEfektif = maxTypo;
        sanityBerkurangPerTickEfektif = sanityBerkurangPerTick;
        progresMaksimalPerSesiEfektif = progresMaksimalPerSesi;

        if (InventoryManager.Instance != null) {
            if (InventoryManager.Instance.punyaKeyboard) {
                maxTypoEfektif += bonusMaxTypoKeyboard;
                sanityBerkurangPerTickEfektif *= pengaliSanityKeyboard;
            }
            if (InventoryManager.Instance.punyaBuku) {
                progresMaksimalPerSesiEfektif *= pengaliProgresBuku;
            }
        }

        // --- TAMBAHAN: bonus toleransi typo HARIAN dari Kopi Espresso (terpisah dari Keyboard yang permanen) ---
        maxTypoEfektif += GameManager.Instance.BonusTypoDariKopiHariIni;

        minigameAktif = true;
        jumlahTypoSaatIni = 0;
        progresSesiIni = 0f;
        totalLaparBerkurangSesiIni = 0f; // --- TAMBAHAN ---
        UpdateTeksTypo();
        UpdateTeksProgres();

        urutanKataSesi = BuatUrutanKataSesi(jumlahKataSesi);
        indexKataSesi = 0;
        kataSaatIni = urutanKataSesi[indexKataSesi];
        indexKarakterBenar = 0;
        TampilkanJendelaKata();

        if (inputKetikan) {
            inputKetikan.onValueChanged.AddListener(TanganiPerubahanInput);
            inputKetikan.SetTextWithoutNotify("");
            inputKetikan.ActivateInputField();
        }

        KunciPemainDiSceneUtama(true);
        GameManager.Instance.SetTombolHUDAktif(false); // --- TAMBAHAN: tombol Toko/Inventory hilang selama minigame ---

        // --- Percepat waktu in-game selama minigame berlangsung ---
        GameManager.Instance.SetPengaliKecepatanWaktu(pengaliKecepatanWaktuSaatMinigame);
    }

    // --- Rangkai urutan kata acak sepanjang "jumlah", kata berikutnya DIJAMIN beda dari kata sebelumnya ---
    List<string> BuatUrutanKataSesi(int jumlah)
    {
        List<string> hasil = new List<string>(jumlah);
        string kataSebelumnya = "";

        for (int i = 0; i < jumlah; i++) {
            string kataBaru;
            do {
                kataBaru = bankKata[Random.Range(0, bankKata.Length)];
            } while (kataBaru == kataSebelumnya && bankKata.Length > 1);

            hasil.Add(kataBaru);
            kataSebelumnya = kataBaru;
        }
        return hasil;
    }

    // --- Render kata sekarang (dengan highlight posisi ketik) + beberapa kata berikutnya, gaya typing-test ---
    void TampilkanJendelaKata()
    {
        if (!textKalimatTarget || urutanKataSesi == null) return;

        StringBuilder sb = new StringBuilder();

        // Kata yang sedang diketik: karakter benar jadi abu redup, posisi kursor di-highlight kuning
        for (int i = 0; i < kataSaatIni.Length; i++) {
            if (i < indexKarakterBenar) {
                sb.Append("<color=#888780>").Append(kataSaatIni[i]).Append("</color>");
            } else if (i == indexKarakterBenar) {
                sb.Append("<mark=#F5D66E80>").Append(kataSaatIni[i]).Append("</mark>");
            } else {
                sb.Append(kataSaatIni[i]);
            }
        }

        // Kata-kata berikutnya, warna normal (belum disentuh)
        for (int i = 1; i < jumlahKataTampil && (indexKataSesi + i) < urutanKataSesi.Count; i++) {
            sb.Append(" ").Append(urutanKataSesi[indexKataSesi + i]);
        }

        textKalimatTarget.text = sb.ToString();
    }

    // --- Dipanggil tiap kali isi TMP_InputField berubah (tiap 1 karakter diketik) ---
    void TanganiPerubahanInput(string teksBaru)
    {
        if (!minigameAktif || string.IsNullOrEmpty(kataSaatIni)) return;
        if (teksBaru.Length <= indexKarakterBenar) return; // backspace, abaikan

        char karakterDiketik = teksBaru[teksBaru.Length - 1];
        char karakterSeharusnya = kataSaatIni[indexKarakterBenar];

        if (karakterDiketik == karakterSeharusnya) {
            indexKarakterBenar++;
            TampilkanJendelaKata();

            if (indexKarakterBenar >= kataSaatIni.Length) {
                SelesaikanSatuKata();
            }
        } else {
            TanganiTypo();
            // Hapus karakter yang salah biar pemain gak numpuk ketikan di atas kesalahan
            if (inputKetikan) inputKetikan.SetTextWithoutNotify(teksBaru.Substring(0, teksBaru.Length - 1));
        }
    }

    // --- Satu kata selesai diketik benar: kasih progres, lanjut ke kata berikutnya di urutan ---
    void SelesaikanSatuKata()
    {
        float progresPerKata = progresMaksimalPerSesiEfektif / Mathf.Max(1, jumlahKataUntukProgresPenuh);
        TanganiKetikBenar(progresPerKata);

        indexKataSesi++;
        if (indexKataSesi >= urutanKataSesi.Count) {
            // Kata di sesi ini habis (jarang kejadian) -> rangkai ulang biar bisa lanjut terus
            urutanKataSesi = BuatUrutanKataSesi(jumlahKataSesi);
            indexKataSesi = 0;
        }

        kataSaatIni = urutanKataSesi[indexKataSesi];
        indexKarakterBenar = 0;
        if (inputKetikan) inputKetikan.SetTextWithoutNotify("");
        TampilkanJendelaKata();
    }

    // --- Setiap kali GameManager tick (bukan tiap frame), kurangi Sanity berbasis durasi bermain ---
    // Pengali "lapar kritis -> sanity 2x lebih cepat" sudah otomatis ditangani di dalam KurangiSanity().
    void TanganiTickWaktu(float deltaJam)
    {
        if (!minigameAktif) return;
        GameManager.Instance.KurangiSanity(sanityBerkurangPerTickEfektif);

        // --- TAMBAHAN: Lapar berkurang seiring lamanya sesi berjalan, TAPI dibatasi total
        // maksimal per sesi (laparBerkurangMaksimalPerSesi) - seberapa lama pun dikerjakan,
        // gak akan berkurang lebih dari itu dalam 1 sesi ---
        if (totalLaparBerkurangSesiIni < laparBerkurangMaksimalPerSesi) {
            float jumlahDikurangi = Mathf.Min(laparBerkurangPerTick, laparBerkurangMaksimalPerSesi - totalLaparBerkurangSesiIni);
            GameManager.Instance.KurangiLapar(jumlahDikurangi);
            totalLaparBerkurangSesiIni += jumlahDikurangi;
        }
    }

    public void TanganiKetikBenar(float tambahProgres)
    {
        if (!minigameAktif) return;
        progresSesiIni = Mathf.Clamp(progresSesiIni + tambahProgres, 0f, progresMaksimalPerSesiEfektif);
        UpdateTeksProgres();
    }

    public void TanganiTypo()
    {
        if (!minigameAktif) return;

        jumlahTypoSaatIni++;
        UpdateTeksTypo();

        if (jumlahTypoSaatIni >= maxTypoEfektif) {
            SelesaikanSesi(); // gagal karena typo melebihi batas toleransi
        }
    }

    void UpdateTeksTypo()
    {
        if (textJumlahTypo) textJumlahTypo.text = jumlahTypoSaatIni + " / " + maxTypoEfektif + " Typo";
    }

    void UpdateTeksProgres()
    {
        if (textProgresSesi) textProgresSesi.text = "+" + progresSesiIni.ToString("F1") + "% Skripsi";
    }

    // --- Dipanggil dari tombol "Selesai"/"Keluar" di UI - santai, gak perlu ngetik 1000 kata dulu ---
    public void SelesaikanManual()
    {
        if (!minigameAktif) return;
        SelesaikanSesi();
    }

    // --- INTERUPSI PAKSA: dipanggil GameManager LEWAT EVENT - baik karena waktu habis (force quit)
    // MAUPUN karena Bad/Good Ending muncul - keduanya harus langsung menutup minigame ini.
    void TanganiInterupsiPaksa()
    {
        if (!minigameAktif) return;
        Debug.Log("Minigame skripsi dihentikan paksa (waktu habis atau ending terpicu).");
        SelesaikanSesi();
    }

    // --- Titik keluar tunggal buat sesi minigame: selesai manual, gagal typo, ATAU force quit ---
    void SelesaikanSesi()
    {
        minigameAktif = false;

        if (GameManager.Instance != null) {
            GameManager.Instance.ResetPengaliKecepatanWaktu();
            GameManager.Instance.TambahProgresSkripsi(progresSesiIni);
        }

        KunciPemainDiSceneUtama(false);
        if (GameManager.Instance != null) GameManager.Instance.SetTombolHUDAktif(true); // --- TAMBAHAN: kembalikan tombol HUD ---
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