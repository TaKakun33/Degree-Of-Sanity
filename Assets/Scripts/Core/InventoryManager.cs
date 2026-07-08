using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Barang Konsumsi (Consumables)")]
    public int jumlahKopi = 0;
    public int jumlahMieAyam = 0;
    public int jumlahBoneka = 0;
    public int jumlahBahanMakanan = 0;

    [Header("Upgrade Permanen")]
    public bool punyaKeyboard = false;
    public bool punyaBuku = false;

    void Awake()
    {
        // Singleton agar mudah diakses dari script Shop dan Minigame nanti
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}