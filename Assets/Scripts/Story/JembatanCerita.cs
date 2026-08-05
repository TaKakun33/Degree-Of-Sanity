// =============================================================================
// File        : JembatanCerita.cs
// Deskripsi   : Kelas statis penghubung antar scene. Menyimpan cerita mana yang
//               harus dimainkan di CutsceneScene dan ke mana pemain kembali
//               setelah cerita selesai. Mengikuti pola static bridge yang sudah
//               dipakai untuk hasil minigame part-time.
// Tim         : Gethuk Pisang
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace DegreeOfSanity.Cerita
{
    public static class JembatanCerita
    {
        /// <summary>Cerita yang akan dimainkan oleh CeritaManager di CutsceneScene.</summary>
        public static CeritaData ceritaYangDimainkan;

        /// <summary>Scene tujuan setelah cerita selesai (diisi otomatis oleh PemicuCerita).</summary>
        public static string sceneKembali = "GameScene";

        /// <summary>ID cerita yang baru saja selesai. Dibaca oleh GameManager saat kembali.</summary>
        public static string idCeritaBaruSelesai;

        /// <summary>True bila ada cerita yang baru saja tuntas dan belum diproses GameManager.</summary>
        public static bool adaCeritaBaruSelesai;

        /// <summary>Menyiapkan data sebelum memuat CutsceneScene.</summary>
        public static void Siapkan(CeritaData cerita, string tujuanKembali)
        {
            ceritaYangDimainkan = cerita;
            if (!string.IsNullOrEmpty(tujuanKembali))
                sceneKembali = tujuanKembali;
        }

        /// <summary>Dipanggil CeritaManager ketika seluruh adegan sudah tuntas.</summary>
        public static void TandaiSelesai(string idCerita)
        {
            if (string.IsNullOrEmpty(idCerita)) return;
            idCeritaBaruSelesai = idCerita;
            adaCeritaBaruSelesai = true;
            ProgresCerita.Tandai(idCerita);
        }

        /// <summary>Dipanggil GameManager setelah hasil cerita selesai diproses.</summary>
        public static void Bersihkan()
        {
            ceritaYangDimainkan = null;
            idCeritaBaruSelesai = null;
            adaCeritaBaruSelesai = false;
        }
    }

    /// <summary>
    /// Pencatat cerita mana saja yang sudah pernah diselesaikan pemain.
    /// CATATAN INTEGRASI: secara default memakai PlayerPrefs supaya sistem cerita
    /// bisa langsung diuji tanpa menyentuh save/load yang sudah ada. Kalau save
    /// system kalian sudah siap, cukup ganti isi Tandai() dan Sudah() untuk
    /// membaca/menulis ke SaveData milik kalian (lihat bagian TODO).
    /// </summary>
    public static class ProgresCerita
    {
        private const string PREFIX = "DOS_CERITA_";

        // Cache dalam memori supaya pengecekan tiap frame tidak menyentuh disk.
        private static readonly HashSet<string> ceritaSelesai = new HashSet<string>();

        public static void Tandai(string idCerita)
        {
            if (string.IsNullOrEmpty(idCerita)) return;
            ceritaSelesai.Add(idCerita);

            // TODO INTEGRASI: ganti baris di bawah dengan
            // GameManager.Instance.dataSimpanan.ceritaSelesai.Add(idCerita);
            PlayerPrefs.SetInt(PREFIX + idCerita, 1);
            PlayerPrefs.Save();
        }

        public static bool Sudah(string idCerita)
        {
            if (string.IsNullOrEmpty(idCerita)) return false;
            if (ceritaSelesai.Contains(idCerita)) return true;

            // TODO INTEGRASI: ganti dengan pembacaan dari SaveData kalian.
            bool hasil = PlayerPrefs.GetInt(PREFIX + idCerita, 0) == 1;
            if (hasil) ceritaSelesai.Add(idCerita);
            return hasil;
        }

        /// <summary>Menghapus seluruh progres cerita (dipakai saat New Game).</summary>
        public static void Reset(IEnumerable<string> daftarId)
        {
            ceritaSelesai.Clear();
            if (daftarId == null) return;
            foreach (var id in daftarId)
            {
                if (!string.IsNullOrEmpty(id))
                    PlayerPrefs.DeleteKey(PREFIX + id);
            }
            PlayerPrefs.Save();
        }
    }
}