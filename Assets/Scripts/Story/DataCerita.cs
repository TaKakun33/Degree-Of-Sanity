// =============================================================================
// File        : DataCerita.cs
// Deskripsi   : Kumpulan struktur data untuk sistem cerita (cutscene) game
//               "Degree of Sanity". Berisi enum, kelas baris dialog, data
//               karakter, dan ScriptableObject Adegan & Cerita.
// Tim         : Gethuk Pisang
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DegreeOfSanity.Cerita
{
    // -------------------------------------------------------------------------
    // ENUM PENDUKUNG
    // -------------------------------------------------------------------------

    /// <summary>Jenis baris yang akan ditampilkan pada panel dialog.</summary>
    public enum TipeBaris
    {
        Narasi,     // teks bercerita, kotak nama disembunyikan
        Dialog,     // ucapan karakter, kotak nama ditampilkan
        Perintah    // tidak menampilkan teks, hanya menjalankan efek (fade, sfx, jeda)
    }

    /// <summary>Posisi sprite karakter di layar.</summary>
    public enum PosisiPortrait
    {
        TanpaPortrait,
        Kiri,
        Tengah,
        Kanan
    }

    /// <summary>Jenis transisi ketika latar belakang diganti.</summary>
    public enum TransisiLatar
    {
        Langsung,   // ganti seketika
        Fade,       // crossfade halus antar latar
        FadeHitam,  // gelap dulu, baru muncul latar baru
        FadePutih   // kilat putih, baru muncul latar baru
    }

    /// <summary>Efek dramatis pada layar saat baris ditampilkan.</summary>
    public enum EfekKamera
    {
        TidakAda,
        Getar,       // getaran halus (mis. ketukan pintu)
        GetarKuat,   // getaran keras (mis. kaget, helm jatuh)
        Kilat,       // flash putih
        ZoomMasuk,
        ZoomKeluar
    }

    // -------------------------------------------------------------------------
    // BARIS DIALOG
    // -------------------------------------------------------------------------

    /// <summary>
    /// Satu baris dalam adegan cerita. Bisa berupa narasi, dialog karakter,
    /// atau perintah efek murni tanpa teks.
    /// </summary>
    [Serializable]
    public class BarisDialog
    {
        [Header("Isi Baris")]
        public TipeBaris tipe = TipeBaris.Narasi;

        [Tooltip("Nama karakter. Dicocokkan dengan DatabaseKarakter untuk warna & sprite.")]
        public string namaKarakter;

        [Tooltip("Ekspresi karakter, mis. 'sedih', 'berbisik'. Opsional.")]
        public string ekspresi;

        [TextArea(2, 6)]
        public string isiTeks;

        [Header("Visual Karakter")]
        [Tooltip("Kosongkan agar otomatis diambil dari DatabaseKarakter.")]
        public Sprite portraitOverride;
        public PosisiPortrait posisiPortrait = PosisiPortrait.TanpaPortrait;
        [Tooltip("Sembunyikan semua portrait sebelum baris ini ditampilkan.")]
        public bool keluarkanSemuaKarakter;

        [Header("Latar & Efek")]
        public Sprite latarBaru;
        public TransisiLatar transisiLatar = TransisiLatar.Fade;
        public float durasiTransisi = 0.8f;
        public EfekKamera efekKamera = EfekKamera.TidakAda;
        [Tooltip("Kekuatan getaran teks (0 = tidak bergetar). Cocok untuk momen panik.")]
        public float goyangTeks = 0f;

        [Header("Audio")]
        public AudioClip sfx;
        public AudioClip bgmBaru;
        public bool hentikanBgm;

        [Header("Timing")]
        [Tooltip("Jeda sebelum baris ini ditampilkan (detik).")]
        public float jedaSebelum = 0f;
        [Tooltip("Jeda setelah teks selesai diketik (detik).")]
        public float jedaSesudah = 0f;
        [Tooltip("0 = pakai kecepatan default dari CeritaManager.")]
        public float kecepatanKetikOverride = 0f;
        [Tooltip("Lanjut sendiri tanpa menunggu klik pemain.")]
        public bool lanjutOtomatis;
    }

    // -------------------------------------------------------------------------
    // DATA KARAKTER
    // -------------------------------------------------------------------------

    [Serializable]
    public class VarianEkspresi
    {
        public string namaEkspresi;
        public Sprite sprite;
    }

    /// <summary>Profil satu karakter: warna nama, posisi default, dan sprite ekspresi.</summary>
    [CreateAssetMenu(fileName = "Karakter_", menuName = "Degree of Sanity/Data Karakter")]
    public class KarakterData : ScriptableObject
    {
        [Tooltip("Nama yang dipakai di naskah, mis. 'Andrew'. Tidak case-sensitive.")]
        public string idKarakter;
        public string namaTampil;
        public Color warnaNama = Color.white;
        public PosisiPortrait posisiDefault = PosisiPortrait.Kanan;
        public Sprite spriteDefault;
        public List<VarianEkspresi> daftarEkspresi = new List<VarianEkspresi>();

        /// <summary>Mengambil sprite sesuai ekspresi, fallback ke sprite default.</summary>
        public Sprite AmbilSprite(string ekspresi)
        {
            if (!string.IsNullOrEmpty(ekspresi) && daftarEkspresi != null)
            {
                foreach (var varian in daftarEkspresi)
                {
                    if (varian != null && !string.IsNullOrEmpty(varian.namaEkspresi) &&
                        string.Equals(varian.namaEkspresi, ekspresi, StringComparison.OrdinalIgnoreCase))
                    {
                        return varian.sprite;
                    }
                }
            }
            return spriteDefault;
        }
    }

    /// <summary>Kumpulan seluruh karakter yang muncul di cerita.</summary>
    [CreateAssetMenu(fileName = "DatabaseKarakter", menuName = "Degree of Sanity/Database Karakter")]
    public class DatabaseKarakter : ScriptableObject
    {
        public List<KarakterData> daftarKarakter = new List<KarakterData>();

        public KarakterData Cari(string nama)
        {
            if (string.IsNullOrEmpty(nama)) return null;
            foreach (var karakter in daftarKarakter)
            {
                if (karakter == null) continue;
                if (string.Equals(karakter.idKarakter, nama, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(karakter.namaTampil, nama, StringComparison.OrdinalIgnoreCase))
                {
                    return karakter;
                }
            }
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // ADEGAN & CERITA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Satu adegan / bab. Berisi urutan baris dialog beserta latar dan BGM awal.
    /// </summary>
    [CreateAssetMenu(fileName = "Adegan_", menuName = "Degree of Sanity/Adegan Cerita")]
    public class AdeganData : ScriptableObject
    {
        public string idAdegan;
        public string judulBab;
        [Tooltip("Tampilkan kartu judul bab sebelum adegan dimulai.")]
        public bool tampilkanJudulBab = true;
        [Tooltip("Subjudul kecil di bawah judul, mis. lokasi atau waktu.")]
        public string subJudul;

        public Sprite latarAwal;
        public AudioClip bgmAwal;

        public List<BarisDialog> barisDialog = new List<BarisDialog>();
    }

    /// <summary>
    /// Kumpulan adegan yang dimainkan berurutan sebagai satu event cerita
    /// (Prologue, Main Event 1, dst).
    /// </summary>
    [CreateAssetMenu(fileName = "Cerita_", menuName = "Degree of Sanity/Data Cerita")]
    public class CeritaData : ScriptableObject
    {
        [Tooltip("ID unik, mis. PROLOG / MAIN_EVENT_1. Dipakai untuk penanda progres.")]
        public string idCerita;
        public string judulCerita;

        public List<AdeganData> daftarAdegan = new List<AdeganData>();

        [Header("Syarat Kemunculan")]
        [Tooltip("Hari in-game minimal agar cerita ini bisa dipicu (1 = 1 Maret).")]
        public int hariMinimal = 1;
        [Tooltip("ID cerita yang wajib selesai lebih dulu. Kosongkan bila tidak ada.")]
        public string idCeritaPrasyarat;

        [Header("Setelah Selesai")]
        [Tooltip("Nama scene tujuan setelah cerita habis. Wajib ada di Build Settings.")]
        public string sceneSetelahSelesai = "GameScene";
        [Tooltip("Tandai cerita ini selesai agar Threshold skripsi terbuka.")]
        public bool tandaiSelesai = true;

        /// <summary>Menghitung total baris dari seluruh adegan (untuk keperluan debug).</summary>
        public int TotalBaris()
        {
            int total = 0;
            foreach (var adegan in daftarAdegan)
            {
                if (adegan != null && adegan.barisDialog != null)
                    total += adegan.barisDialog.Count;
            }
            return total;
        }
    }
}