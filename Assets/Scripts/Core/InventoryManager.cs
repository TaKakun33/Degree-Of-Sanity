using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Barang Konsumsi (Consumables)")]
    public int jumlahKopi = 0;
    public int jumlahMieAyam = 0;
    public int jumlahBoneka = 0;

    [Header("Bahan Makanan & Masakan")]
    public int jumlahBahan1 = 0; // Menghasilkan 1 porsi
    public int jumlahBahan2 = 0; // Menghasilkan 2 porsi
    public int jumlahBahan3 = 0; // Menghasilkan 3 porsi
    public int jumlahMakananJadi = 0; // Hasil masakan

    [Header("Upgrade Permanen")]
    public bool punyaKeyboard = false;
    public bool punyaBuku = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}