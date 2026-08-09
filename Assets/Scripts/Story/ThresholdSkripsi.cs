using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Threshold
{
    [Tooltip("Label doang buat Inspector, misal 'TH1 - Bab Satu'")]
    public string nama;
    [Tooltip("TUNABLE: persen Progres Skripsi buat threshold ini")]
    [Range(0f, 100f)] public float persenProgres = 33f;
    [TextArea(1, 3)]
    [Tooltip("Bark yang muncul begitu progres nyampe angka ini tapi BELUM kebuka")]
    public string barkTerkunci;
    [Tooltip("Centang kalau threshold ini BUTUH syarat tambahan (misal TH1: cicilan pertama lunas) sebelum bisa 'terbuka'")]
    public bool butuhSyaratTambahan;

    [HideInInspector] public bool sudahDitampilkanTerkunci;
    [HideInInspector] public bool sudahTerbuka;
    [HideInInspector] public bool syaratTambahanTerpenuhi;
}

// --- Bagian 1 naskah: TH1/TH2/TH3. Progres skripsi di-cap di threshold TERDEKAT yang belum
// terbuka - begitu kebuka, cap naik ke threshold berikutnya. TH1 butuh syarat tambahan
// (cicilan pertama lunas, dipanggil CicilanManager.cs). ---
public class ThresholdSkripsi : MonoBehaviour
{
    public static ThresholdSkripsi Instance;

    [Header("TUNABLE: Daftar Threshold (urutkan dari persen kecil ke besar)")]
    public List<Threshold> daftarThreshold;

    [Tooltip("TUNABLE: Sanity yang didapat begitu sebuah threshold TERBUKA")]
    public float sanityDapatDariThresholdTerbuka = 5f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameManager.Instance == null || daftarThreshold == null) return;

        // --- Terapkan cap progres ke threshold terdekat yang belum kebuka ---
        float cap = 100f;
        foreach (var th in daftarThreshold) {
            if (!th.sudahTerbuka) { cap = th.persenProgres; break; }
        }
        GameManager.Instance.batasProgresMaksimalSaatIni = cap;

        foreach (var th in daftarThreshold) {
            if (th.sudahTerbuka) continue;

            bool progresNyampe = GameManager.Instance.progresSkripsi >= th.persenProgres;

            if (progresNyampe && !th.sudahDitampilkanTerkunci) {
                th.sudahDitampilkanTerkunci = true;
                if (!string.IsNullOrEmpty(th.barkTerkunci) && PenampilBark.Instance != null) {
                    PenampilBark.Instance.Tampilkan(th.barkTerkunci);
                }
            }

            bool bolehTerbuka = progresNyampe && (!th.butuhSyaratTambahan || th.syaratTambahanTerpenuhi);
            if (bolehTerbuka) {
                BukaThreshold(th);
            }
        }
    }

    void BukaThreshold(Threshold th)
    {
        th.sudahTerbuka = true;
        GameManager.Instance.TambahSanity(sanityDapatDariThresholdTerbuka);
        if (PenampilBark.Instance != null) PenampilBark.Instance.Tampilkan("Lanjutkan. ...oke.");
    }

    // --- Dipanggil CicilanManager begitu cicilan PERTAMA lunas (buka syarat tambahan TH1) ---
    public void TandaiSyaratTambahanTerpenuhi()
    {
        foreach (var th in daftarThreshold) {
            if (th.butuhSyaratTambahan) th.syaratTambahanTerpenuhi = true;
        }
    }

    // --- TAMBAHAN: paksa buka Threshold ke-N (1/2/3), TERLEPAS dari progres skripsi saat ini.
    // Dipakai adegan Main Event yang emang narasinya nentuin threshold kebuka (ME2_03, ME3_02). ---
    public void PaksaBukaThresholdKe(int nomorKe1)
    {
        if (daftarThreshold == null || nomorKe1 < 1 || nomorKe1 > daftarThreshold.Count) return;
        var th = daftarThreshold[nomorKe1 - 1];
        if (th.sudahTerbuka) return;
        BukaThreshold(th);
    }
}