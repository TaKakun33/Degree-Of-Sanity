// =============================================================================
// File        : PemicuCerita.cs
// Deskripsi   : Dipasang di scene gameplay (Rumah). Mengecek apakah ada cerita
//               yang layak dipicu pada hari in-game saat ini, lalu memuat
//               CutsceneScene. Juga memproses hasil cerita ketika pemain
//               kembali dari cutscene (membuka Threshold skripsi).
// Tim         : Gethuk Pisang
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DegreeOfSanity.Cerita
{
    public class PemicuCerita : MonoBehaviour
    {
        [Header("Daftar Cerita")]
        [Tooltip("Urutkan dari yang paling awal: Prolog, Main Event 1, 2, 3, Epilogue.")]
        [SerializeField] private List<CeritaData> daftarCerita = new List<CeritaData>();

        [Header("Scene")]
        [SerializeField] private string namaSceneCutscene = "CutsceneScene";
        [SerializeField] private string namaSceneGameplay = "GameScene";

        [Header("Pengaturan")]
        [Tooltip("Cek otomatis saat scene gameplay dimuat.")]
        [SerializeField] private bool cekOtomatisSaatMulai = true;

        // Event agar sistem lain (GameManager, UI Threshold) bisa ikut bereaksi.
        public static event System.Action<string> OnCeritaSelesaiDiproses;

        private void Start()
        {
            // 1. Proses hasil cutscene yang barusan selesai (kalau ada).
            ProsesHasilCerita();

            // 2. Cek apakah ada cerita baru yang perlu dimainkan hari ini.
            if (cekOtomatisSaatMulai) CekPemicuHariIni();
        }

        // ---------------------------------------------------------------------
        // PEMICUAN
        // ---------------------------------------------------------------------

        /// <summary>
        /// Mengecek seluruh daftar cerita dan memainkan yang pertama memenuhi syarat.
        /// Panggil ulang setiap kali hari in-game berganti.
        /// </summary>
        public void CekPemicuHariIni()
        {
            int hariSekarang = AmbilHariInGame();

            foreach (var cerita in daftarCerita)
            {
                if (cerita == null) continue;
                if (ProgresCerita.Sudah(cerita.idCerita)) continue;
                if (hariSekarang < cerita.hariMinimal) continue;

                if (!string.IsNullOrEmpty(cerita.idCeritaPrasyarat) &&
                    !ProgresCerita.Sudah(cerita.idCeritaPrasyarat))
                {
                    continue;
                }

                MainkanCerita(cerita);
                return; // satu cerita per pengecekan
            }
        }

        /// <summary>Memaksa memainkan satu cerita (dipakai untuk tombol debug / trigger manual).</summary>
        public void MainkanCerita(CeritaData cerita)
        {
            if (cerita == null)
            {
                Debug.LogWarning("[PemicuCerita] CeritaData kosong, pemicuan dibatalkan.");
                return;
            }

            string tujuan = !string.IsNullOrEmpty(cerita.sceneSetelahSelesai)
                ? cerita.sceneSetelahSelesai
                : namaSceneGameplay;

            JembatanCerita.Siapkan(cerita, tujuan);
            SceneManager.LoadScene(namaSceneCutscene);
        }

        /// <summary>Memainkan cerita berdasarkan ID, mis. dipanggil dari GameManager.</summary>
        public void MainkanCeritaDenganId(string idCerita)
        {
            foreach (var cerita in daftarCerita)
            {
                if (cerita != null && cerita.idCerita == idCerita)
                {
                    MainkanCerita(cerita);
                    return;
                }
            }
            Debug.LogWarning($"[PemicuCerita] Cerita dengan id '{idCerita}' tidak ditemukan di daftar.");
        }

        // ---------------------------------------------------------------------
        // PEMROSESAN HASIL
        // ---------------------------------------------------------------------

        private void ProsesHasilCerita()
        {
            if (!JembatanCerita.adaCeritaBaruSelesai) return;

            string id = JembatanCerita.idCeritaBaruSelesai;
            Debug.Log($"[PemicuCerita] Cerita '{id}' selesai, memproses efeknya.");

            BukaThresholdSkripsi(id);
            OnCeritaSelesaiDiproses?.Invoke(id);

            JembatanCerita.Bersihkan();
        }

        /// <summary>
        /// Menghubungkan penyelesaian Main Event dengan pembukaan Threshold skripsi
        /// sesuai proposal (Threshold 1 = 10%, 2 = 50%, 3 = 90%).
        /// </summary>
        private void BukaThresholdSkripsi(string idCerita)
        {
            switch (idCerita)
            {
                case "MAIN_EVENT_1":
                    // TODO INTEGRASI: GameManager.Instance.BukaThreshold(1);
                    Debug.Log("[PemicuCerita] Threshold 1 (batas 10%) dibuka.");
                    break;
                case "MAIN_EVENT_2":
                    // TODO INTEGRASI: GameManager.Instance.BukaThreshold(2);
                    Debug.Log("[PemicuCerita] Threshold 2 (batas 50%) dibuka.");
                    break;
                case "MAIN_EVENT_3":
                    // TODO INTEGRASI: GameManager.Instance.BukaThreshold(3);
                    Debug.Log("[PemicuCerita] Threshold 3 (batas 90%) dibuka.");
                    break;
            }
        }

        // ---------------------------------------------------------------------
        // ADAPTER KE GAMEMANAGER
        // ---------------------------------------------------------------------

        /// <summary>
        /// Mengambil hari in-game saat ini (1 = 1 Maret, 61 = 1 Mei).
        /// TODO INTEGRASI: ganti isi method ini dengan pembacaan dari GameManager
        /// milik kalian, contohnya: return GameManager.Instance.hariKe;
        /// </summary>
        private int AmbilHariInGame()
        {
            return PlayerPrefs.GetInt("DOS_HARI_KE", 1);
        }
    }
}