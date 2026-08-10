using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// --- TAMBAHAN: Wrapper untuk menyimpan daftar nomor slot yang terpakai ---
// (JsonUtility tidak bisa serialize List<int> secara langsung tanpa wrapper class)
[System.Serializable]
public class DaftarSlotSave
{
    public List<int> slots = new List<int>();
}

[System.Serializable]
public class DataSimpanan
{
    // Status GameManager
    public int tanggal, bulan, uang;
    public float progresSkripsi, lapar, sanity, jamSaatIni;

    // Inventory
    public int kopi, mie, boneka, bahan1, bahan2, bahan3, makananJadi;
    public bool keyboard, buku;

    // Posisi Player
    public float playerX, playerY;
    public int lantai;

    // --- TAMBAHAN: batasan harian - WAJIB disimpan, soalnya KasirScene/OjolScene/TutorScene
    // di-load Single (GameManager beneran hancur & dibuat ulang), jadi tanpa ini flag-nya
    // bakal reset ke false lagi tiap kali GameManager baru dibuat, walau harusnya masih hari yang sama ---
    public bool skripsiSudahDikerjakanHariIni;
    public bool kerjaPartTimeSudahDilakukanHariIni;
    public bool prologSelesai;
    public System.Collections.Generic.List<string> peristiwaCeritaSudahTerjadi;
    public System.Collections.Generic.List<string> flagCeritaAktif;
    public float utangBank;
    public System.Collections.Generic.List<MingguCicilan> daftarMingguCicilan;
    public int cicilanNomorMingguBerikutnya;
    public int cicilanGagalBerturutTurut;
    public bool cicilanPertamaSudahLunas;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    public static int slotUntukDiload = -1; // -1 = Game Baru

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    public void SimpanGame(int nomorSlot)
    {
        DataSimpanan data = new DataSimpanan();

        // 1. Kumpulkan Data GameManager
        if (GameManager.Instance != null) {
            data.tanggal = GameManager.Instance.tanggal;
            data.bulan = GameManager.Instance.bulan;
            data.uang = GameManager.Instance.uang;
            data.progresSkripsi = GameManager.Instance.progresSkripsi;
            data.lapar = GameManager.Instance.lapar;
            data.sanity = GameManager.Instance.sanity;
            data.jamSaatIni = GameManager.Instance.jamSaatIni;
            data.skripsiSudahDikerjakanHariIni = GameManager.Instance.SkripsiSudahDikerjakanHariIni; // --- TAMBAHAN ---
            data.kerjaPartTimeSudahDilakukanHariIni = GameManager.Instance.KerjaPartTimeSudahDilakukanHariIni; // --- TAMBAHAN ---
            data.prologSelesai = GameManager.Instance.prologSelesai; // --- TAMBAHAN ---
            if (CeritaManager.Instance != null) {
                data.peristiwaCeritaSudahTerjadi = CeritaManager.Instance.DapatkanPeristiwaSudahTerjadi(); // --- TAMBAHAN ---
                data.flagCeritaAktif = CeritaManager.Instance.DapatkanFlagCerita(); // --- TAMBAHAN ---
            }
            data.utangBank = GameManager.Instance.utangBank; // --- TAMBAHAN ---
            if (CicilanManager.Instance != null) {
                data.daftarMingguCicilan = CicilanManager.Instance.DapatkanDaftarMinggu(); // --- TAMBAHAN ---
                data.cicilanNomorMingguBerikutnya = CicilanManager.Instance.DapatkanNomorMingguBerikutnya(); // --- TAMBAHAN ---
                data.cicilanGagalBerturutTurut = CicilanManager.Instance.DapatkanGagalBerturutTurut(); // --- TAMBAHAN ---
                data.cicilanPertamaSudahLunas = CicilanManager.Instance.DapatkanCicilanPertamaSudahLunas(); // --- TAMBAHAN ---
            }
        }

        // 2. Kumpulkan Data Inventory
        if (InventoryManager.Instance != null) {
            data.kopi = InventoryManager.Instance.jumlahKopi;
            data.mie = InventoryManager.Instance.jumlahMieAyam;
            data.boneka = InventoryManager.Instance.jumlahBoneka;
            data.bahan1 = InventoryManager.Instance.jumlahBahan1;
            data.bahan2 = InventoryManager.Instance.jumlahBahan2;
            data.bahan3 = InventoryManager.Instance.jumlahBahan3;
            data.makananJadi = InventoryManager.Instance.jumlahMakananJadi;
            data.keyboard = InventoryManager.Instance.punyaKeyboard;
            data.buku = InventoryManager.Instance.punyaBuku;
        }

        // 3. Kumpulkan Data Posisi Player
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) {
            data.playerX = player.transform.position.x;
            data.playerY = player.transform.position.y;
            data.lantai = player.lantaiSaatIni;
        }

        string jsonString = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData_Slot_" + nomorSlot, jsonString);
        DaftarkanSlot(nomorSlot); // --- TAMBAHAN: catat slot ini ke daftar slot terpakai ---
        UpdateSlotTerakhir(nomorSlot);
        PlayerPrefs.Save();
        Debug.Log("Game berhasil disimpan di Slot: " + nomorSlot);
    }

    public void MuatGame(int nomorSlot)
    {
        if (nomorSlot == -1) return; // Jika -1, berarti New Game
        
        string key = "SaveData_Slot_" + nomorSlot;
        if (PlayerPrefs.HasKey(key))
        {
            string jsonString = PlayerPrefs.GetString(key);
            DataSimpanan data = JsonUtility.FromJson<DataSimpanan>(jsonString);

            // 1. Restore GameManager
            if (GameManager.Instance != null) {
                GameManager.Instance.tanggal = data.tanggal;
                GameManager.Instance.bulan = data.bulan;
                GameManager.Instance.uang = data.uang;
                GameManager.Instance.progresSkripsi = data.progresSkripsi;
                GameManager.Instance.lapar = data.lapar;
                GameManager.Instance.sanity = data.sanity;
                GameManager.Instance.jamSaatIni = data.jamSaatIni;
                GameManager.Instance.SkripsiSudahDikerjakanHariIni = data.skripsiSudahDikerjakanHariIni; // --- TAMBAHAN ---
                GameManager.Instance.KerjaPartTimeSudahDilakukanHariIni = data.kerjaPartTimeSudahDilakukanHariIni; // --- TAMBAHAN ---
                GameManager.Instance.prologSelesai = data.prologSelesai; // --- TAMBAHAN ---
                if (CeritaManager.Instance != null) {
                    CeritaManager.Instance.MuatPeristiwaSudahTerjadi(data.peristiwaCeritaSudahTerjadi); // --- TAMBAHAN ---
                    CeritaManager.Instance.MuatFlagCerita(data.flagCeritaAktif); // --- TAMBAHAN ---
                }
                GameManager.Instance.utangBank = data.utangBank; // --- TAMBAHAN ---
                GameManager.Instance.UpdateTombolUtang(); // --- TAMBAHAN: FIX bug tombol ilang ---
                if (CicilanManager.Instance != null) {
                    CicilanManager.Instance.MuatDaftarMinggu(data.daftarMingguCicilan, data.cicilanNomorMingguBerikutnya, data.cicilanGagalBerturutTurut, data.cicilanPertamaSudahLunas); // --- TAMBAHAN ---
                }
            }

            // 2. Restore Inventory
            if (InventoryManager.Instance != null) {
                InventoryManager.Instance.jumlahKopi = data.kopi;
                InventoryManager.Instance.jumlahMieAyam = data.mie;
                InventoryManager.Instance.jumlahBoneka = data.boneka;
                InventoryManager.Instance.jumlahBahan1 = data.bahan1;
                InventoryManager.Instance.jumlahBahan2 = data.bahan2;
                InventoryManager.Instance.jumlahBahan3 = data.bahan3;
                InventoryManager.Instance.jumlahMakananJadi = data.makananJadi;
                InventoryManager.Instance.punyaKeyboard = data.keyboard;
                InventoryManager.Instance.punyaBuku = data.buku;
            }

            // 3. Restore Posisi Player
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null) {
                Vector3 posisiTujuan = new Vector3(data.playerX, data.playerY, player.transform.position.z);

                // --- FIX: pakai rb.position (kalau ada Rigidbody2D), BUKAN transform.position
                // langsung - alasan sama kayak di TitikSpawnPlayer.cs, biar gak ada lompatan
                // koreksi physics pas load game ---
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) {
                    rb.position = posisiTujuan;
                } else {
                    player.transform.position = posisiTujuan;
                }

                player.lantaiSaatIni = data.lantai;
            }

            Debug.Log("Data berhasil dimuat dari Slot: " + nomorSlot);
        }
    }

    // --- FITUR DINAMIS ---

    private const string KEY_DAFTAR_SLOT = "DaftarSlotSaveList";

    // --- TAMBAHAN: Muat daftar nomor slot yang pernah dipakai ---
    private DaftarSlotSave MuatDaftarSlot()
    {
        if (PlayerPrefs.HasKey(KEY_DAFTAR_SLOT)) {
            return JsonUtility.FromJson<DaftarSlotSave>(PlayerPrefs.GetString(KEY_DAFTAR_SLOT));
        }
        return new DaftarSlotSave();
    }

    // --- TAMBAHAN: Simpan daftar nomor slot ke PlayerPrefs ---
    private void SimpanDaftarSlot(DaftarSlotSave daftar)
    {
        PlayerPrefs.SetString(KEY_DAFTAR_SLOT, JsonUtility.ToJson(daftar));
    }

    // --- TAMBAHAN: Daftarkan sebuah slot (kalau belum ada di daftar) ---
    private void DaftarkanSlot(int slot)
    {
        if (slot == 0) return; // slot 0 khusus autosave, tidak perlu didaftarkan
        DaftarSlotSave daftar = MuatDaftarSlot();
        if (!daftar.slots.Contains(slot)) {
            daftar.slots.Add(slot);
            SimpanDaftarSlot(daftar);
        }
    }

    // --- TAMBAHAN: Ambil daftar semua slot manual (1, 2, 3, ... tanpa batas) yang datanya masih ada ---
    // Dipakai PauseMenuController untuk generate tombol Save/Load secara dinamis
    public List<int> DapatkanDaftarSlotTersimpan()
    {
        DaftarSlotSave daftar = MuatDaftarSlot();
        List<int> slotValid = daftar.slots.Where(s => CekSaveAda(s)).ToList();
        slotValid.Sort();

        // Bersihkan daftar dari slot basi (misal dihapus manual dari PlayerPrefs)
        if (slotValid.Count != daftar.slots.Count) {
            daftar.slots = slotValid;
            SimpanDaftarSlot(daftar);
        }
        return slotValid;
    }

    // Fungsi untuk mencari slot kosong otomatis (untuk tombol + New Save)
    // Sekarang TANPA BATAS ATAS, tidak lagi dibatasi sampai 50
    public int GetNextAvailableSlot()
    {
        List<int> daftar = DapatkanDaftarSlotTersimpan();
        int slot = 1;
        while (daftar.Contains(slot)) slot++;
        return slot;
    }

    public void UpdateSlotTerakhir(int slot) { PlayerPrefs.SetInt("SlotSaveTerakhir", slot); PlayerPrefs.Save(); }
    public int DapatkanSlotTerakhir() => PlayerPrefs.GetInt("SlotSaveTerakhir", -1);
    public bool CekSaveAda(int nomorSlot) => PlayerPrefs.HasKey("SaveData_Slot_" + nomorSlot);

    // --- TAMBAHAN: Hapus data save di sebuah slot (dipakai tombol Delete di panel Load) ---
    public void HapusSave(int nomorSlot)
    {
        string key = "SaveData_Slot_" + nomorSlot;
        if (PlayerPrefs.HasKey(key)) {
            PlayerPrefs.DeleteKey(key);
        }

        // Keluarkan slot ini dari daftar registry (slot 0/autosave tidak ada di registry)
        if (nomorSlot != 0) {
            DaftarSlotSave daftar = MuatDaftarSlot();
            if (daftar.slots.Contains(nomorSlot)) {
                daftar.slots.Remove(nomorSlot);
                SimpanDaftarSlot(daftar);
            }
        }

        // Kalau slot yg dihapus adalah penanda "slot terakhir dipakai", reset penanda itu
        if (DapatkanSlotTerakhir() == nomorSlot) {
            PlayerPrefs.DeleteKey("SlotSaveTerakhir");
        }

        PlayerPrefs.Save();
        Debug.Log("Save di Slot " + nomorSlot + " berhasil dihapus.");
    }
    
    public string DapatkanInfoSave(int nomorSlot)
    {
        if (nomorSlot == 0) return PlayerPrefs.HasKey("SaveData_Slot_0") ? "AUTOSAVE" : "Autosave Kosong";
        if (CekSaveAda(nomorSlot)) {
            DataSimpanan d = JsonUtility.FromJson<DataSimpanan>(PlayerPrefs.GetString("SaveData_Slot_" + nomorSlot));
            string namaBulan = d.bulan == 3 ? "Maret" : (d.bulan == 4 ? "April" : "Mei");
            return "Slot " + nomorSlot + " | " + d.tanggal + " " + namaBulan + " | Rp " + d.uang;
        }
        return "Slot " + nomorSlot + " (Empty)";
    }

    // --- TAMBAHAN: hapus SEMUA save yang ada, TERMASUK autosave (slot 0) ---
    public void HapusSemuaSave()
    {
        // Hapus autosave
        if (PlayerPrefs.HasKey("SaveData_Slot_0")) {
            PlayerPrefs.DeleteKey("SaveData_Slot_0");
        }

        // Hapus semua slot manual yang terdaftar di registry
        DaftarSlotSave daftar = MuatDaftarSlot();
        foreach (int slot in daftar.slots) {
            string key = "SaveData_Slot_" + slot;
            if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
        }

        // Kosongkan registry-nya sendiri
        daftar.slots.Clear();
        SimpanDaftarSlot(daftar);

        // Reset penanda "slot terakhir dipakai"
        PlayerPrefs.DeleteKey("SlotSaveTerakhir");

        PlayerPrefs.Save();
        Debug.Log("Semua save (termasuk autosave) berhasil dihapus.");
    }

    // --- TAMBAHAN: cek apakah ADA save apapun - dipakai buat nentuin tombol "Continue" aktif/nonaktif ---
    public bool ApakahAdaSaveApapun()
    {
        if (CekSaveAda(0)) return true; // autosave
        return DapatkanDaftarSlotTersimpan().Count > 0;
    }
}