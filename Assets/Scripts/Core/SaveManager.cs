using UnityEngine;
using UnityEngine.SceneManagement;

// --- BLUEPRINT DATA YANG AKAN DISIMPAN ---
[System.Serializable]
public class DataSimpanan
{
    public int waktu, uang;
    public float progresSkripsi, lapar, sanity, jamSaatIni;

    public int kopi, mie, boneka, bahan1, bahan2, bahan3, makananJadi;
    public bool keyboard, buku;

    public float playerX, playerY;
    public int lantai;
}

// --- SISTEM UTAMA SAVE/LOAD ---
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    // -1 = Game Baru, 0 = Autosave, 1,2,3 = Save Manual
    public static int slotUntukDiload = -1; 

    void Awake()
    {
        // Membuat objek ini abadi (tidak hancur saat pindah scene)
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

        // Simpan dalam format Teks (JSON)
        string jsonString = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData_Slot_" + nomorSlot, jsonString);
        PlayerPrefs.Save();

        Debug.Log("Game berhasil disimpan di Slot: " + nomorSlot);
    }

    public void MuatGame()
    {
        if (slotUntukDiload == -1) return; // New Game, jangan muat data lama

        string key = "SaveData_Slot_" + slotUntukDiload;
        if (PlayerPrefs.HasKey(key))
        {
            string jsonString = PlayerPrefs.GetString(key);
            DataSimpanan data = JsonUtility.FromJson<DataSimpanan>(jsonString);

            // Sebar data kembali ke GameManager
            if (GameManager.Instance != null) {
                GameManager.Instance.waktu = data.waktu;
                GameManager.Instance.uang = data.uang;
                GameManager.Instance.progresSkripsi = data.progresSkripsi;
                GameManager.Instance.lapar = data.lapar;
                GameManager.Instance.sanity = data.sanity;
                GameManager.Instance.jamSaatIni = data.jamSaatIni;
            }

            // Sebar data kembali ke Inventory
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

            // Pindahkan Posisi Player ke tempat semula
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null) {
                player.transform.position = new Vector3(data.playerX, data.playerY, player.transform.position.z);
                player.lantaiSaatIni = data.lantai;
            }

            Debug.Log("Game berhasil dimuat dari Slot: " + slotUntukDiload);
        }
        else
        {
            Debug.LogWarning("Data save tidak ditemukan di Slot " + slotUntukDiload + ", memulai sebagai game baru.");
        }
    }
}