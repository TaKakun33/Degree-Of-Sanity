using System.Collections.Generic;
using UnityEngine;

// --- Jenis baris dalam satu CutsceneScene ---
public enum JenisBarisCutscene { Narasi, Dialog, Bisikan }

[System.Serializable]
public class BarisCutscene
{
    public JenisBarisCutscene jenis = JenisBarisCutscene.Dialog;
    [Tooltip("Kosongkan buat Narasi/Bisikan. Isi 'ANDREW', 'ANNA', 'DOSEN', dll buat Dialog")]
    public string namaTokoh;
    [TextArea(2, 5)]
    public string teks;
    [Tooltip("TAMBAHAN: kosongkan biasanya. Isi 'Lapar'/'ProgresSkripsi'/'Tanggal'/'Sanity'/'Uang'/'Inventory' kalau baris INI yang mesti nampilin parameter itu (Prolog: reveal satu-satu)")]
    public string parameterUntukDitampilkan;
    [Tooltip("TAMBAHAN: baris ini CUMA muncul kalau Sanity pemain saat itu di bawah angka ini (0 = selalu muncul, gak ada syarat). Buat varian bisikan sesuai Sanity (naskah: <25% dst)")]
    [Range(0f, 100f)]
    public float munculKalauSanityDiBawah = 0f;
    [Tooltip("TAMBAHAN: prop di scene (misal Amplop/Laci) yang DIMUNCULKAN pas baris ini tampil. Kosongkan (None) kalau gak ada.")]
    public GameObject objekTampilkan;
    [Tooltip("TAMBAHAN: prop di scene yang DISEMBUNYIKAN pas baris ini tampil. Kosongkan (None) kalau gak ada.")]
    public GameObject objekSembunyikan;
}

[System.Serializable]
public class EfekParameterCutscene
{
    [Tooltip("Boleh negatif")]
    public float sanityDelta = 0f;
    public float laparDelta = 0f;
    public int uangDelta = 0;
    public float progresSkripsiDelta = 0f;
    public int tambahRoti = 0;
    [Tooltip("Centang kalau adegan ini yang mengaktifkan mekanik Cicilan Mingguan (Main Event 1)")]
    public bool aktifkanHutang = false;
}

[System.Serializable]
public class PilihanCabang
{
    public string labelTombol;
    public CutsceneSceneSO adeganLanjutan;
    [Tooltip("Opsional: nama flag yang di-set true kalau cabang ini dipilih (misal 'JANJI_ANNA', 'TEKAD_KUAT')")]
    public string setFlag;
}

// --- Satu adegan/scene cerita - diisi lewat Inspector, BUKAN lewat kode.
// Assets -> Create -> Degree of Sanity -> Cutscene Scene ---
[CreateAssetMenu(fileName = "Adegan_Baru", menuName = "Degree of Sanity/Cutscene Scene")]
public class CutsceneSceneSO : ScriptableObject
{
    [Header("Identitas")]
    [Tooltip("ID dari naskah, misal 'P_01', 'ME1_02'")]
    public string id;

    [Header("Latar")]
    [Tooltip("'LORONG' / 'DAPUR' / 'KAMAR_ANDREW' / 'KAMAR_ANNA' / 'LAYAR_HITAM'")]
    public string ruangId;
    public bool karakterAnnaHadir;

    [Header("Baris-baris (urut dari atas ke bawah)")]
    public List<BarisCutscene> baris;

    [Header("Efek Parameter (diterapkan SETELAH semua baris tampil)")]
    public EfekParameterCutscene efek;

    [Header("Pilihan (JembatanCerita) - opsional")]
    public bool adaPilihan;
    public List<PilihanCabang> pilihanCabang;

    [Header("Rantai Adegan")]
    [Tooltip("Adegan berikutnya OTOMATIS setelah ini selesai. Kosongkan (None) kalau ini akhir chain ATAU adaPilihan dicentang (pilihan yang nentuin lanjutannya, bukan field ini)")]
    public CutsceneSceneSO adeganBerikutnya;

    [Header("Monolog Akhir Hari (opsional)")]
    [TextArea(2, 4)]
    public string monologAkhirHari;
}