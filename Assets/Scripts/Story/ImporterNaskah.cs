// =============================================================================
// File        : ImporterNaskah.cs
// Deskripsi   : Editor tool untuk mengubah file naskah .txt menjadi asset
//               AdeganData + CeritaData secara otomatis, sehingga tim tidak
//               perlu mengetik ratusan baris dialog manual di Inspector.
// Lokasi      : WAJIB berada di dalam folder bernama "Editor".
// Menu        : Degree of Sanity > Importer Naskah
// Tim         : Gethuk Pisang
// =============================================================================

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using DegreeOfSanity.Cerita;

namespace DegreeOfSanity.CeritaEditor
{
    public class ImporterNaskah : EditorWindow
    {
        // --- Pengaturan yang tampil di jendela editor
        private TextAsset fileNaskah;
        private string folderOutput = "Assets/Cerita/Adegan";
        private string folderSprite = "Assets/Art/Background";
        private string folderPortrait = "Assets/Art/Karakter";
        private string folderAudio = "Assets/Audio";
        private string idCerita = "PROLOG";
        private string judulCerita = "Prologue";
        private string sceneSetelahSelesai = "GameScene";
        private int hariMinimal = 1;
        private bool buatCeritaData = true;
        private bool hapusTandaKutip = true;

        private Vector2 scroll;
        private string pesanStatus = "";

        // Regex untuk mendeteksi baris dialog: "Nama (ekspresi): isi"
        private static readonly Regex REGEX_DIALOG =
            new Regex(@"^\s*([^:()\.\?!]{1,32}?)\s*(?:\(([^)]*)\))?\s*:\s*(.+)$");

        [MenuItem("Degree of Sanity/Importer Naskah")]
        public static void BukaJendela()
        {
            var jendela = GetWindow<ImporterNaskah>("Importer Naskah");
            jendela.minSize = new Vector2(430, 520);
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("Importer Naskah — Degree of Sanity", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Ubah file naskah .txt menjadi asset AdeganData otomatis.\n" +
                "Lihat SETUP_UNITY.md untuk daftar lengkap perintah @.",
                MessageType.Info);

            EditorGUILayout.Space();
            fileNaskah = (TextAsset)EditorGUILayout.ObjectField("File Naskah (.txt)", fileNaskah, typeof(TextAsset), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Folder", EditorStyles.boldLabel);
            folderOutput = EditorGUILayout.TextField("Output Adegan", folderOutput);
            folderSprite = EditorGUILayout.TextField("Folder Background", folderSprite);
            folderPortrait = EditorGUILayout.TextField("Folder Portrait", folderPortrait);
            folderAudio = EditorGUILayout.TextField("Folder Audio", folderAudio);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Data Cerita", EditorStyles.boldLabel);
            idCerita = EditorGUILayout.TextField("ID Cerita", idCerita);
            judulCerita = EditorGUILayout.TextField("Judul Cerita", judulCerita);
            hariMinimal = EditorGUILayout.IntField("Hari Minimal", hariMinimal);
            sceneSetelahSelesai = EditorGUILayout.TextField("Scene Setelah Selesai", sceneSetelahSelesai);
            buatCeritaData = EditorGUILayout.Toggle("Buat/Update CeritaData", buatCeritaData);
            hapusTandaKutip = EditorGUILayout.Toggle("Hapus Tanda Kutip", hapusTandaKutip);

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(fileNaskah == null);
            if (GUILayout.Button("IMPORT NASKAH", GUILayout.Height(38)))
            {
                Import();
            }
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(pesanStatus))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(pesanStatus, MessageType.None);
            }

            EditorGUILayout.EndScrollView();
        }

        // ---------------------------------------------------------------------
        // PROSES IMPORT
        // ---------------------------------------------------------------------

        private void Import()
        {
            PastikanFolderAda(folderOutput);

            string[] baris = fileNaskah.text.Replace("\r\n", "\n").Split('\n');

            List<AdeganData> hasilAdegan = new List<AdeganData>();
            AdeganData adeganSekarang = null;
            int nomorAdegan = 0;

            // Perintah yang menunggu untuk ditempelkan ke baris teks berikutnya.
            BarisDialog perintahTertunda = BarisBaru();
            bool adaPerintahTertunda = false;

            foreach (string barisMentah in baris)
            {
                string isi = barisMentah.Trim();

                if (string.IsNullOrEmpty(isi)) continue;
                if (isi.StartsWith("#")) continue; // komentar

                // ---- Adegan baru
                if (isi.StartsWith("=="))
                {
                    // Perintah yang belum terpakai dijadikan baris tipe Perintah.
                    if (adaPerintahTertunda && adeganSekarang != null)
                    {
                        perintahTertunda.tipe = TipeBaris.Perintah;
                        adeganSekarang.barisDialog.Add(perintahTertunda);
                    }
                    perintahTertunda = BarisBaru();
                    adaPerintahTertunda = false;

                    nomorAdegan++;
                    string judul = isi.Substring(2).Trim();
                    string subJudul = "";

                    // Format opsional: "== Bab 1 — Pendahuluan | Kontrakan Andrew, Sore"
                    int pipa = judul.IndexOf('|');
                    if (pipa >= 0)
                    {
                        subJudul = judul.Substring(pipa + 1).Trim();
                        judul = judul.Substring(0, pipa).Trim();
                    }

                    adeganSekarang = BuatAtauMuatAdegan(nomorAdegan, judul, subJudul);
                    hasilAdegan.Add(adeganSekarang);
                    continue;
                }

                // Pastikan ada adegan aktif; kalau naskah tidak diawali "==", buat default.
                if (adeganSekarang == null)
                {
                    nomorAdegan++;
                    adeganSekarang = BuatAtauMuatAdegan(nomorAdegan, judulCerita, "");
                    hasilAdegan.Add(adeganSekarang);
                }

                // ---- Perintah
                if (isi.StartsWith("@"))
                {
                    ProsesPerintah(isi.Substring(1), perintahTertunda, adeganSekarang);
                    adaPerintahTertunda = true;
                    continue;
                }

                // ---- Baris teks (dialog atau narasi)
                BarisDialog barisBaru = perintahTertunda;
                perintahTertunda = BarisBaru();
                adaPerintahTertunda = false;

                Match cocok = REGEX_DIALOG.Match(isi);
                if (cocok.Success)
                {
                    barisBaru.tipe = TipeBaris.Dialog;
                    barisBaru.namaKarakter = cocok.Groups[1].Value.Trim();
                    barisBaru.ekspresi = cocok.Groups[2].Success ? cocok.Groups[2].Value.Trim() : "";
                    barisBaru.isiTeks = BersihkanTeks(cocok.Groups[3].Value.Trim());
                }
                else
                {
                    barisBaru.tipe = TipeBaris.Narasi;
                    barisBaru.namaKarakter = "";
                    barisBaru.isiTeks = isi;
                }

                adeganSekarang.barisDialog.Add(barisBaru);
            }

            // Perintah sisa di akhir file
            if (adaPerintahTertunda && adeganSekarang != null)
            {
                perintahTertunda.tipe = TipeBaris.Perintah;
                adeganSekarang.barisDialog.Add(perintahTertunda);
            }

            // Simpan semua adegan
            int totalBaris = 0;
            foreach (var adegan in hasilAdegan)
            {
                totalBaris += adegan.barisDialog.Count;
                EditorUtility.SetDirty(adegan);
            }

            if (buatCeritaData) BuatAtauUpdateCeritaData(hasilAdegan);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            pesanStatus = $"Berhasil!\n{hasilAdegan.Count} adegan, {totalBaris} baris dialog.\n" +
                          $"Tersimpan di: {folderOutput}";
            Debug.Log("[ImporterNaskah] " + pesanStatus);
        }

        // ---------------------------------------------------------------------
        // PERINTAH @
        // ---------------------------------------------------------------------

        private void ProsesPerintah(string perintahPenuh, BarisDialog target, AdeganData adegan)
        {
            string[] bagian = perintahPenuh.Split(' ');
            string perintah = bagian[0].ToLowerInvariant();

            switch (perintah)
            {
                case "latar": // @latar nama_sprite [fade|langsung|hitam|putih] [durasi]
                    if (bagian.Length > 1)
                    {
                        target.latarBaru = CariAset<Sprite>(bagian[1], folderSprite);
                        if (target.latarBaru == null)
                            Debug.LogWarning($"[ImporterNaskah] Sprite latar '{bagian[1]}' tidak ditemukan di {folderSprite}.");
                    }
                    if (bagian.Length > 2) target.transisiLatar = ParseTransisi(bagian[2]);
                    if (bagian.Length > 3) target.durasiTransisi = ParseFloat(bagian[3], 0.8f);

                    // Baris pertama dalam adegan sekaligus jadi latar awal adegan.
                    if (adegan.barisDialog.Count == 0 && adegan.latarAwal == null)
                        adegan.latarAwal = target.latarBaru;
                    break;

                case "bgm": // @bgm nama_clip  |  @bgm stop
                    if (bagian.Length > 1)
                    {
                        if (bagian[1].ToLowerInvariant() == "stop")
                        {
                            target.hentikanBgm = true;
                        }
                        else
                        {
                            target.bgmBaru = CariAset<AudioClip>(bagian[1], folderAudio);
                            if (adegan.barisDialog.Count == 0 && adegan.bgmAwal == null)
                                adegan.bgmAwal = target.bgmBaru;
                        }
                    }
                    break;

                case "sfx": // @sfx nama_clip
                    if (bagian.Length > 1) target.sfx = CariAset<AudioClip>(bagian[1], folderAudio);
                    break;

                case "getar": // @getar [kuat]
                    target.efekKamera = (bagian.Length > 1 && bagian[1].ToLowerInvariant() == "kuat")
                        ? EfekKamera.GetarKuat
                        : EfekKamera.Getar;
                    break;

                case "kilat":
                    target.efekKamera = EfekKamera.Kilat;
                    break;

                case "zoom": // @zoom masuk|keluar
                    target.efekKamera = (bagian.Length > 1 && bagian[1].ToLowerInvariant() == "keluar")
                        ? EfekKamera.ZoomKeluar
                        : EfekKamera.ZoomMasuk;
                    break;

                case "jeda": // @jeda 1.5
                    target.jedaSebelum = bagian.Length > 1 ? ParseFloat(bagian[1], 0.5f) : 0.5f;
                    break;

                case "jedasesudah":
                    target.jedaSesudah = bagian.Length > 1 ? ParseFloat(bagian[1], 0.5f) : 0.5f;
                    break;

                case "auto":
                    target.lanjutOtomatis = true;
                    break;

                case "posisi": // @posisi kiri|tengah|kanan|tanpa
                    if (bagian.Length > 1) target.posisiPortrait = ParsePosisi(bagian[1]);
                    break;

                case "goyang": // @goyang 2
                    target.goyangTeks = bagian.Length > 1 ? ParseFloat(bagian[1], 2f) : 2f;
                    break;

                case "keluar": // @keluar  -> semua karakter keluar layar
                    target.keluarkanSemuaKarakter = true;
                    break;

                case "lambat":
                    target.kecepatanKetikOverride = 0.06f;
                    break;

                case "cepat":
                    target.kecepatanKetikOverride = 0.012f;
                    break;

                case "portrait": // @portrait nama_sprite -> override manual
                    if (bagian.Length > 1) target.portraitOverride = CariAset<Sprite>(bagian[1], folderPortrait);
                    break;

                default:
                    Debug.LogWarning($"[ImporterNaskah] Perintah '@{perintah}' tidak dikenali.");
                    break;
            }
        }

        // ---------------------------------------------------------------------
        // PEMBUATAN ASSET
        // ---------------------------------------------------------------------

        private AdeganData BuatAtauMuatAdegan(int nomor, string judul, string subJudul)
        {
            string namaFile = $"Adegan_{idCerita}_{nomor:00}.asset";
            string path = Path.Combine(folderOutput, namaFile).Replace("\\", "/");

            AdeganData adegan = AssetDatabase.LoadAssetAtPath<AdeganData>(path);
            if (adegan == null)
            {
                adegan = CreateInstance<AdeganData>();
                AssetDatabase.CreateAsset(adegan, path);
            }

            adegan.idAdegan = $"{idCerita}_{nomor:00}";
            adegan.judulBab = judul;
            adegan.subJudul = subJudul;
            adegan.barisDialog = new List<BarisDialog>(); // di-generate ulang tiap import
            return adegan;
        }

        private void BuatAtauUpdateCeritaData(List<AdeganData> daftarAdegan)
        {
            string path = Path.Combine(folderOutput, $"Cerita_{idCerita}.asset").Replace("\\", "/");
            CeritaData cerita = AssetDatabase.LoadAssetAtPath<CeritaData>(path);
            if (cerita == null)
            {
                cerita = CreateInstance<CeritaData>();
                AssetDatabase.CreateAsset(cerita, path);
            }

            cerita.idCerita = idCerita;
            cerita.judulCerita = judulCerita;
            cerita.hariMinimal = hariMinimal;
            cerita.sceneSetelahSelesai = sceneSetelahSelesai;
            cerita.daftarAdegan = new List<AdeganData>(daftarAdegan);
            EditorUtility.SetDirty(cerita);
        }

        // ---------------------------------------------------------------------
        // UTILITAS
        // ---------------------------------------------------------------------

        private static BarisDialog BarisBaru()
        {
            return new BarisDialog
            {
                tipe = TipeBaris.Narasi,
                transisiLatar = TransisiLatar.Fade,
                durasiTransisi = 0.8f,
                posisiPortrait = PosisiPortrait.TanpaPortrait
            };
        }

        private string BersihkanTeks(string teks)
        {
            if (!hapusTandaKutip) return teks;
            teks = teks.Trim();
            if (teks.Length >= 2 && teks.StartsWith("\"") && teks.EndsWith("\""))
                teks = teks.Substring(1, teks.Length - 2);
            return teks.Trim();
        }

        private static float ParseFloat(string nilai, float bawaan)
        {
            return float.TryParse(nilai, NumberStyles.Float, CultureInfo.InvariantCulture, out float hasil)
                ? hasil
                : bawaan;
        }

        private static TransisiLatar ParseTransisi(string nilai)
        {
            switch (nilai.ToLowerInvariant())
            {
                case "langsung": return TransisiLatar.Langsung;
                case "hitam": return TransisiLatar.FadeHitam;
                case "putih": return TransisiLatar.FadePutih;
                default: return TransisiLatar.Fade;
            }
        }

        private static PosisiPortrait ParsePosisi(string nilai)
        {
            switch (nilai.ToLowerInvariant())
            {
                case "kiri": return PosisiPortrait.Kiri;
                case "tengah": return PosisiPortrait.Tengah;
                case "kanan": return PosisiPortrait.Kanan;
                default: return PosisiPortrait.TanpaPortrait;
            }
        }

        private static T CariAset<T>(string nama, string folder) where T : Object
        {
            if (string.IsNullOrEmpty(nama)) return null;
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"[ImporterNaskah] Folder '{folder}' tidak valid.");
                return null;
            }

            string[] guid = AssetDatabase.FindAssets($"{nama} t:{typeof(T).Name}", new[] { folder });
            foreach (string g in guid)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (Path.GetFileNameWithoutExtension(path).Equals(nama, System.StringComparison.OrdinalIgnoreCase))
                {
                    T aset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (aset != null) return aset;
                }
            }

            // Fallback: ambil hasil pertama meski namanya tidak persis sama.
            if (guid.Length > 0)
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid[0]));

            return null;
        }

        private static void PastikanFolderAda(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string[] bagian = path.Split('/');
            string berjalan = bagian[0]; // "Assets"
            for (int i = 1; i < bagian.Length; i++)
            {
                string berikutnya = berjalan + "/" + bagian[i];
                if (!AssetDatabase.IsValidFolder(berikutnya))
                    AssetDatabase.CreateFolder(berjalan, bagian[i]);
                berjalan = berikutnya;
            }
        }
    }
}