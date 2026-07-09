using UnityEngine;

[System.Serializable]
public class DataSimpanan
{
    // Status GameManager
    public int waktu, uang;
    public float progresSkripsi, lapar, sanity, jamSaatIni;

    // Inventory
    public int kopi, mie, boneka, bahan1, bahan2, bahan3, makananJadi;
    public bool keyboard, buku;

    // Posisi Player
    public float playerX, playerY;
    public int lantai;
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
            data.waktu = GameManager.Instance.waktu;
            data.uang = GameManager.Instance.uang;
            data.progresSkripsi = GameManager.Instance.progresSkripsi;
            data.lapar = GameManager.Instance.lapar;
            data.sanity = GameManager.Instance.sanity;
            data.jamSaatIni = GameManager.Instance.jamSaatIni;
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
                GameManager.Instance.waktu = data.waktu;
                GameManager.Instance.uang = data.uang;
                GameManager.Instance.progresSkripsi = data.progresSkripsi;
                GameManager.Instance.lapar = data.lapar;
                GameManager.Instance.sanity = data.sanity;
                GameManager.Instance.jamSaatIni = data.jamSaatIni;
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
                player.transform.position = new Vector3(data.playerX, data.playerY, player.transform.position.z);
                player.lantaiSaatIni = data.lantai;
            }

            Debug.Log("Data berhasil dimuat dari Slot: " + nomorSlot);
        }
    }

    // --- FITUR DINAMIS ---
    
    // Fungsi untuk mencari slot kosong otomatis (untuk tombol + New Save)
    public int GetNextAvailableSlot()
    {
        for (int i = 1; i <= 50; i++) {
            if (!PlayerPrefs.HasKey("SaveData_Slot_" + i)) return i;
        }
        return 1; // Default jika semua penuh
    }

    public void UpdateSlotTerakhir(int slot) { PlayerPrefs.SetInt("SlotSaveTerakhir", slot); PlayerPrefs.Save(); }
    public int DapatkanSlotTerakhir() => PlayerPrefs.GetInt("SlotSaveTerakhir", -1);
    public bool CekSaveAda(int nomorSlot) => PlayerPrefs.HasKey("SaveData_Slot_" + nomorSlot);
    
    public string DapatkanInfoSave(int nomorSlot)
    {
        if (nomorSlot == 0) return PlayerPrefs.HasKey("SaveData_Slot_0") ? "AUTOSAVE" : "Autosave Kosong";
        if (CekSaveAda(nomorSlot)) {
            DataSimpanan d = JsonUtility.FromJson<DataSimpanan>(PlayerPrefs.GetString("SaveData_Slot_" + nomorSlot));
            return "Slot " + nomorSlot + " | Hari " + d.waktu + " | Rp " + d.uang;
        }
        return "Slot " + nomorSlot + " (Empty)";
    }
}