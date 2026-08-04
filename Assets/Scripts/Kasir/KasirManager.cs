using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// --- Data satu jenis barang di katalog toko (nama + harga) ---
[System.Serializable]
public class ItemBelanjaan
{
    public string namaItem;
    public int harga;
}

// --- Minigame Kerja Part Time: Kasir Supermarket (Proposal 3.3.4.1) ---
// Berjalan di SCENE TERPISAH beneran (Single load, BUKAN Additive seperti minigame Skripsi),
// karena itu GameManager TIDAK bisa diakses langsung selama minigame ini berjalan.
// Hasil shift (gaji, efek lapar/sanity, skip jam) dititipkan ke HasilKerjaPartTime,
// baru diterapkan lagi oleh GameManager begitu MainScene dimuat ulang.
public class KasirManager : MonoBehaviour
{
    public static KasirManager Instance;

    [Header("Katalog Barang")]
    public ItemBelanjaan[] katalogItem = new ItemBelanjaan[] {
        new ItemBelanjaan { namaItem = "Indomie Goreng",     harga = 3500  },
        new ItemBelanjaan { namaItem = "Teh Botol",          harga = 4500  },
        new ItemBelanjaan { namaItem = "Sabun Mandi",        harga = 5500  },
        new ItemBelanjaan { namaItem = "Susu Kotak",         harga = 6000  },
        new ItemBelanjaan { namaItem = "Telur 1/4kg",        harga = 7500  },
        new ItemBelanjaan { namaItem = "Snack Ringan",       harga = 8000  },
        new ItemBelanjaan { namaItem = "Pasta Gigi",         harga = 9500  },
        new ItemBelanjaan { namaItem = "Roti Tawar",         harga = 12000 },
        new ItemBelanjaan { namaItem = "Beras 1kg",          harga = 14000 },
        new ItemBelanjaan { namaItem = "Gula Pasir 1kg",     harga = 16000 },
        new ItemBelanjaan { namaItem = "Minyak Goreng 1L",   harga = 18000 },
        new ItemBelanjaan { namaItem = "Sampo Sachet",       harga = 1500  },
    };

    [Header("Pengaturan Shift")]
    public int jumlahPelangganPerShift = 4;
    public int minItemPerPelanggan = 2;
    public int maxItemPerPelanggan = 5;

    [Header("Referensi Conveyor Belt")]
    [Tooltip("Titik di ujung kanan belt, tempat barang baru muncul")]
    public RectTransform titikSpawnConveyor;
    [Tooltip("WAJIB diisi manual: wadah/parent untuk barang yang di-spawn. Harus Canvas langsung atau child KOSONG di bawah Canvas - JANGAN Panel_Pembayaran/Panel_Uang.")]
    public Transform wadahConveyor;
    public GameObject prefabItemBelanjaan;
    public float kecepatanConveyor = 60f;
    [Tooltip("OPSIONAL: kalau diisi, 'X Batas Kiri' & 'X Mulai Conveyor' di bawah OTOMATIS dihitung dari lebar object ini - gak perlu ketik angka manual. Bikin 1 Image/RectTransform kosong sepanjang track conveyor kamu, drag ke sini.")]
    public RectTransform trackConveyor;
    [Tooltip("Diabaikan kalau 'Track Conveyor' di atas diisi. Posisi X lokal di ujung kiri belt (manual fallback)")]
    public float xBatasKiriConveyor = -400f;
    [Tooltip("Diabaikan kalau 'Track Conveyor' di atas diisi. Posisi X lokal awal/spawn di ujung kanan belt (manual fallback)")]
    public float xMulaiConveyor = 400f;
    [Tooltip("Jarak minimum antar barang di conveyor - biar gak numpuk/tabrakan visual pas ngantre")]
    public float jarakMinimalAntarItem = 70f;

    [Header("Referensi UI Umum")]
    public TextMeshProUGUI textGajiTerkumpul;
    public TextMeshProUGUI textPelangganKe;

    [Header("Referensi UI Pembayaran")]
    public GameObject panelPembayaran;
    [Tooltip("Opsional: isi HANYA kalau tombol pecahan uang ada di panel TERPISAH dari Panel Pembayaran (bukan child-nya)")]
    public GameObject panelUang;
    public TextMeshProUGUI textTotalHarga;
    public TextMeshProUGUI textUangDibayarkan;
    public TextMeshProUGUI textKembalianDiberikan;
    public TextMeshProUGUI textWaktuTersisa;
    public TextMeshProUGUI textStatusTransaksi;

    [Header("Pengaturan Pembayaran")]
    [Tooltip("Berapa detik kesabaran pelanggan sebelum timeout")]
    public float batasWaktuKesabaran = 15f;
    [Range(0f, 1f)]
    [Tooltip("Persentase dari harga belanja yang jadi komisi/gaji per transaksi BENAR")]
    public float komisiPerTransaksi = 0.05f;
    [Tooltip("Potongan gaji shift kalau kembalian salah ATAU waktu habis (proposal 3.3.4.1)")]
    public int penaltiPerKesalahan = 20000;

    [Header("Efek ke Parameter (diterapkan lewat HasilKerjaPartTime setelah shift SELESAI)")]
    public float laparBerkurangPerShift = 25f;
    public float sanityBerkurangPerShift = 10f;
    [Tooltip("Berapa jam in-game yang dilewati sepulang shift (proposal: skip waktu)")]
    public float jamDilewatiShift = 8f;

    [Header("Scene")]
    public string namaSceneUtama = "SampleScene";

    // --- State internal shift ---
    private int pelangganSaatIni = 0;
    private List<BarangBelanjaan> itemPelangganAktif = new List<BarangBelanjaan>();
    private int jumlahItemDibungkus = 0;
    private int totalHargaPelangganIni = 0;
    private int totalHargaTerpindaiSaatIni = 0; // running total, update tiap barang di-scan
    private int uangDibayarkanPelangganIni = 0;
    private int kembalianDiberikanSaatIni = 0;
    private int gajiTerkumpul = 0;
    private int penaltiTotal = 0;
    private bool sedangTahapPembayaran = false;
    private Coroutine coroutineTimerPembayaran;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // --- TAMBAHAN: kalau Track Conveyor diisi, otomatis hitung X Batas Kiri & X Mulai Conveyor
        // dari lebar object itu sendiri (dikonversi ke local space wadahConveyor, biar sinkron sama
        // anchoredPosition barang yang di-spawn nanti) - gak perlu ketik angka manual lagi. ---
        if (trackConveyor != null && wadahConveyor != null) {
            Vector3[] sudutTrack = new Vector3[4];
            trackConveyor.GetWorldCorners(sudutTrack); // [0]=kiri-bawah, [2]=kanan-atas (world space)

            Vector3 kiriLocal = wadahConveyor.InverseTransformPoint(sudutTrack[0]);
            Vector3 kananLocal = wadahConveyor.InverseTransformPoint(sudutTrack[2]);

            // --- TAMBAHAN: kompensasi setengah lebar barang itu sendiri - tanpa ini, yang "berhenti"
            // di ujung track cuma TITIK TENGAH (anchoredPosition) barang, jadi separuh lebar visualnya
            // masih nembus keluar track. Diambil otomatis dari lebar prefab (asumsi pivot di tengah). ---
            float setengahLebarItem = 0f;
            if (prefabItemBelanjaan != null) {
                RectTransform rectPrefab = prefabItemBelanjaan.GetComponent<RectTransform>();
                if (rectPrefab != null) setengahLebarItem = rectPrefab.rect.width / 2f;
            }

            xBatasKiriConveyor = kiriLocal.x + setengahLebarItem;
            xMulaiConveyor = kananLocal.x - setengahLebarItem;

            Debug.Log($"[KasirManager] Batas conveyor otomatis dari Track (dikompensasi lebar item {setengahLebarItem:F0}px): kiri={xBatasKiriConveyor:F0}, kanan={xMulaiConveyor:F0}");
        }

        // --- validasi wiring, biar ketauan LANGSUNG di Console kalau salah drag lagi ---
        if (wadahConveyor == null) {
            Debug.LogError("[KasirManager] Wadah Conveyor belum diisi! Barang gak akan ke-spawn dengan benar.");
        } else if (panelPembayaran != null && wadahConveyor == panelPembayaran.transform) {
            Debug.LogError("[KasirManager] Wadah Conveyor ke-set sama dengan Panel Pembayaran! Ganti ke Canvas/wadah lain.");
        } else if (panelUang != null && wadahConveyor == panelUang.transform) {
            Debug.LogError("[KasirManager] Wadah Conveyor ke-set sama dengan Panel Uang! Ganti ke Canvas/wadah lain.");
        }

        if (panelUang) panelUang.SetActive(false);
        MulaiShift();
    }

    public void MulaiShift()
    {
        pelangganSaatIni = 0;
        gajiTerkumpul = 0;
        penaltiTotal = 0;
        UpdateTeksGaji();
        MulaiPelangganBaru();
    }

    void MulaiPelangganBaru()
    {
        pelangganSaatIni++;
        UpdateTeksPelanggan();

        itemPelangganAktif.Clear();
        jumlahItemDibungkus = 0;
        totalHargaPelangganIni = 0;
        totalHargaTerpindaiSaatIni = 0;

        // --- Reset tampilan Panel Pembayaran ke kondisi awal pelanggan baru (gaya layar kasir: selalu nunjukin angka, gak pernah kosong) ---
        if (textTotalHarga) textTotalHarga.text = "Total: Rp 0";
        if (textUangDibayarkan) textUangDibayarkan.text = "Dibayar: Rp 0";
        if (textKembalianDiberikan) textKembalianDiberikan.text = "Kembalian: Rp 0";
        if (textWaktuTersisa) textWaktuTersisa.text = "Sabar pelanggan: -";
        if (textStatusTransaksi) textStatusTransaksi.text = "";

        int jumlahItem = Random.Range(minItemPerPelanggan, maxItemPerPelanggan + 1);
        StartCoroutine(SpawnItemBelanjaan(jumlahItem));
    }

    IEnumerator SpawnItemBelanjaan(int jumlah)
    {
        for (int i = 0; i < jumlah; i++) {
            ItemBelanjaan dataItem = katalogItem[Random.Range(0, katalogItem.Length)];
            totalHargaPelangganIni += dataItem.harga;

            GameObject objekBaru = Instantiate(prefabItemBelanjaan, wadahConveyor);
            RectTransform rect = objekBaru.GetComponent<RectTransform>();
            rect.anchoredPosition = titikSpawnConveyor.anchoredPosition + new Vector2(i * 60f, 0f);

            BarangBelanjaan barang = objekBaru.GetComponent<BarangBelanjaan>();
            barang.Setup(dataItem.namaItem, dataItem.harga, kecepatanConveyor, xBatasKiriConveyor, xMulaiConveyor);

            itemPelangganAktif.Add(barang);
            yield return new WaitForSeconds(0.4f); // spawn satu-satu, gak numpuk di titik yang sama
        }
    }

    // --- Dipanggil DaerahPindai.OnDrop - update total di Panel Pembayaran secara real-time selagi masih scan ---
    public void ItemDipindai(BarangBelanjaan barang)
    {
        totalHargaTerpindaiSaatIni += barang.harga;
        if (textTotalHarga) textTotalHarga.text = "Total: Rp " + totalHargaTerpindaiSaatIni.ToString("N0");
    }

    // --- TAMBAHAN: dipanggil BarangBelanjaan tiap frame, cari batas kiri EFEKTIF buat item itu -
    // entah itu ujung track (kalau dia paling depan), atau posisi barang lain yang masih ada di depannya
    // (biar gak numpuk/tabrakan visual, kayak antrian di conveyor belt beneran). ---
    public float DapatkanBatasKiriUntukItem(BarangBelanjaan itemIni)
    {
        int indexItemIni = itemPelangganAktif.IndexOf(itemIni);
        if (indexItemIni < 0) return xBatasKiriConveyor; // jaga-jaga kalau somehow gak ketemu di list

        // Cari item TERDEKAT di depan (index lebih kecil = di-spawn lebih dulu = lebih ke kiri/depan)
        // yang MASIH ADA (belum ke-Destroy karena udah masuk kantong)
        for (int i = indexItemIni - 1; i >= 0; i--) {
            BarangBelanjaan itemDiDepan = itemPelangganAktif[i];
            if (itemDiDepan != null) {
                return itemDiDepan.PosisiXSaatIni + jarakMinimalAntarItem;
            }
        }

        return xBatasKiriConveyor; // gak ada barang lain yang masih ada di depan -> batasnya ujung track
    }

    // --- Dipanggil DaerahKantong.OnDrop ---
    public void ItemDimasukkanKantong(BarangBelanjaan barang)
    {
        jumlahItemDibungkus++;
        if (jumlahItemDibungkus >= itemPelangganAktif.Count) {
            MulaiTahapPembayaran();
        }
    }

    void MulaiTahapPembayaran()
    {
        sedangTahapPembayaran = true;

        // Pelanggan bayar pakai pecahan besar terdekat yang cukup (dibulatkan ke atas)
        uangDibayarkanPelangganIni = HitungUangBayarDibulatkan(totalHargaPelangganIni);
        kembalianDiberikanSaatIni = 0;

        if (textTotalHarga) textTotalHarga.text = "Total: Rp " + totalHargaPelangganIni.ToString("N0");
        if (textUangDibayarkan) textUangDibayarkan.text = "Dibayar: Rp " + uangDibayarkanPelangganIni.ToString("N0");
        if (textStatusTransaksi) textStatusTransaksi.text = "";
        UpdateTeksKembalian();

        if (panelUang) panelUang.SetActive(true); // Panel Pembayaran udah kelihatan dari tadi, tinggal munculin tombol uangnya
        coroutineTimerPembayaran = StartCoroutine(TimerKesabaranPelanggan());
    }

    int HitungUangBayarDibulatkan(int total)
    {
        // Diurut NAIK, cari pecahan TERKECIL yang cukup menutup total (bukan langsung yang terbesar)
        int[] pecahanUrutNaik = { 5000, 10000, 20000, 50000, 100000 };
        foreach (int pecahan in pecahanUrutNaik) {
            if (pecahan >= total) return pecahan;
        }
        return Mathf.CeilToInt(total / 100000f) * 100000;
    }

    IEnumerator TimerKesabaranPelanggan()
    {
        float sisaWaktu = batasWaktuKesabaran;
        while (sisaWaktu > 0f) {
            sisaWaktu -= Time.deltaTime;
            if (textWaktuTersisa) textWaktuTersisa.text = "Sabar pelanggan: " + Mathf.CeilToInt(sisaWaktu) + " dtk";
            yield return null;
        }
        SelesaikanTransaksi(waktuHabis: true);
    }

    // --- Dipanggil tiap tombol pecahan uang diklik ---
    public void TambahKembalian(int nilai)
    {
        if (!sedangTahapPembayaran) return;
        kembalianDiberikanSaatIni += nilai;
        UpdateTeksKembalian();
    }

    // --- Dipanggil tombol "Reset" kalau pemain salah pencet pecahan ---
    public void ResetKembalian()
    {
        if (!sedangTahapPembayaran) return;
        kembalianDiberikanSaatIni = 0;
        UpdateTeksKembalian();
    }

    // --- Dipanggil tombol "Berikan Kembalian" ---
    public void KonfirmasiKembalian()
    {
        if (!sedangTahapPembayaran) return;
        SelesaikanTransaksi(waktuHabis: false);
    }

    void SelesaikanTransaksi(bool waktuHabis)
    {
        sedangTahapPembayaran = false;
        if (coroutineTimerPembayaran != null) StopCoroutine(coroutineTimerPembayaran);

        int kembalianSeharusnya = uangDibayarkanPelangganIni - totalHargaPelangganIni;
        bool benar = !waktuHabis && kembalianDiberikanSaatIni == kembalianSeharusnya;

        if (benar) {
            int komisi = Mathf.RoundToInt(totalHargaPelangganIni * komisiPerTransaksi);
            gajiTerkumpul += komisi;
            if (textStatusTransaksi) textStatusTransaksi.text = "Benar! +Rp " + komisi.ToString("N0");
        } else {
            penaltiTotal += penaltiPerKesalahan;
            if (textStatusTransaksi) {
                textStatusTransaksi.text = waktuHabis
                    ? "Waktu habis! Penalti Rp " + penaltiPerKesalahan.ToString("N0")
                    : "Kembalian salah! Penalti Rp " + penaltiPerKesalahan.ToString("N0");
            }
        }

        UpdateTeksGaji();
        StartCoroutine(LanjutSetelahJeda());
    }

    IEnumerator LanjutSetelahJeda()
    {
        yield return new WaitForSeconds(1.5f); // beri waktu pemain baca status transaksi dulu
        if (panelUang) panelUang.SetActive(false); // sembunyikan tombol uang lagi sampai pelanggan berikutnya siap bayar

        foreach (BarangBelanjaan sisa in itemPelangganAktif) {
            if (sisa != null) Destroy(sisa.gameObject);
        }
        itemPelangganAktif.Clear();

        if (pelangganSaatIni < jumlahPelangganPerShift) {
            MulaiPelangganBaru();
        } else {
            SelesaikanShift();
        }
    }

    // --- Dipanggil tombol "Pulang" kalau pemain mau sudahi shift lebih awal ---
    public void PulangLebihAwal()
    {
        SelesaikanShift();
    }

    void SelesaikanShift()
    {
        // --- Gaji shift BOLEH MINUS kalau penalti kesalahan lebih besar dari komisi yang didapat -
        // ini akan MOTONG uang yang sudah ada di MainScene (bukan cuma "gaji jadi Rp 0" doang) ---
        int gajiBersih = gajiTerkumpul - penaltiTotal;
        HasilKerjaPartTime.SimpanHasil(gajiBersih, laparBerkurangPerShift, sanityBerkurangPerShift, jamDilewatiShift);
        SceneManager.LoadScene(namaSceneUtama, LoadSceneMode.Single);
    }

    void UpdateTeksGaji()
    {
        int gajiBersih = gajiTerkumpul - penaltiTotal;
        string tandaMinus = gajiBersih < 0 ? "-" : "";
        if (textGajiTerkumpul) textGajiTerkumpul.text = "Gaji shift ini: " + tandaMinus + "Rp " + Mathf.Abs(gajiBersih).ToString("N0");
    }

    void UpdateTeksPelanggan()
    {
        if (textPelangganKe) textPelangganKe.text = "Pelanggan " + pelangganSaatIni + " / " + jumlahPelangganPerShift;
    }

    void UpdateTeksKembalian()
    {
        if (textKembalianDiberikan) textKembalianDiberikan.text = "Kembalian: Rp " + kembalianDiberikanSaatIni.ToString("N0");
    }
}