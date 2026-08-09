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
    [Tooltip("TAMBAHAN: baris ini CUMA muncul kalau flag ini SUDAH aktif (dari pilihan sebelumnya). Kosongkan kalau gak ada syarat.")]
    public string munculKalauFlagAktif;
    [Tooltip("TAMBAHAN: baris ini CUMA muncul kalau flag ini BELUM aktif. Kosongkan kalau gak ada syarat.")]
    public string munculKalauFlagTidakAktif;
    [Tooltip("TAMBAHAN: prop di scene (misal Amplop/Laci) yang DIMUNCULKAN pas baris ini tampil. Kosongkan (None) kalau gak ada.")]
    public GameObject objekTampilkan;
    [Tooltip("TAMBAHAN: prop di scene yang DISEMBUNYIKAN pas baris ini tampil. Kosongkan (None) kalau gak ada.")]
    public GameObject objekSembunyikan;
    [Tooltip("TAMBAHAN: gambar/sprite yang muncul DI DEPAN LAYAR (kayak ilustrasi VN, bukan di posisi ruangan) pas baris ini tampil - misal close-up Laci/Amplop. Kosongkan kalau gak ada.")]
    public Sprite gambarPropUntukDitampilkan;
    [Tooltip("Centang buat SEMBUNYIKAN gambar prop yang lagi tampil di depan layar (biasanya diisi di baris SETELAH prop-nya gak perlu kelihatan lagi)")]
    public bool sembunyikanGambarProp;
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
    [Tooltip("TAMBAHAN: nambah Utang Bank sejumlah ini (misal ME1: pinjaman 2000000) - TERPISAH dari Uang Delta, gak bikin Uang minus")]
    public float tambahUtang = 0f;
    [Tooltip("Centang kalau adegan ini yang mengaktifkan mekanik Cicilan Mingguan (Main Event 1)")]
    public bool aktifkanHutang = false;
    [Tooltip("TAMBAHAN: kalau diisi (bukan -1), jam in-game LANGSUNG diset ke angka ini begitu adegan ini kelar (format 24 jam, misal 11 = jam 11 siang). Biarkan -1 kalau gak mau ubah jam sama sekali.")]
    public float jamBaruSetelahAdegan = -1f;
    [Tooltip("TAMBAHAN: kalau diisi (bukan -1), Sanity gak akan dibiarkan jatuh di bawah angka ini akibat efek 'Sanity Delta' negatif di ADEGAN INI (naskah ME2: jangan sampai di bawah 15%). Cuma jepit ke ATAS kalau kurang, gak narik turun kalau udah lebih tinggi.")]
    public float sanityMinimalSetelahEfek = -1f;
    [Tooltip("TAMBAHAN: paksa buka Threshold ke-berapa (1/2/3) begitu adegan ini kelar, TERLEPAS dari progres skripsi saat ini. Isi -1 kalau gak perlu.")]
    public int paksaBukaThresholdKe = -1;
    [Tooltip("TAMBAHAN: centang buat ngasih bonus 'TEKAD_KUAT' (ME2_03) - distorsi Sanity dimatikan paksa 1 hari + Sanity gak akan jatuh di bawah 10 selama 3 hari")]
    public bool aktifkanBonusTekadKuat = false;
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