using UnityEngine;
using TMPro; // Wajib untuk mengatur teks UI

public class ShopController : MonoBehaviour
{
    [Header("Harga Barang")]
    public int hargaKopi = 15000;
    public int hargaMie = 20000;
    public int hargaBoneka = 250000;
    public int hargaKeyboard = 1500000;
    public int hargaBuku = 500000;
    public int hargaBahan = 30000;

    [Header("Isi Keranjang (Cart)")]
    private int cartKopi = 0;
    private int cartMie = 0;
    private int cartBoneka = 0;
    private int cartBahan = 0;
    private bool cartKeyboard = false;
    private bool cartBuku = false;

    private int totalHarga = 0;

    [Header("Referensi UI")]
    public TextMeshProUGUI textRincianKeranjang;
    public TextMeshProUGUI textTotalHarga;

    // Dipanggil otomatis setiap kali Panel Toko dibuka
    void OnEnable()
    {
        KosongkanKeranjang();
    }

    // --- 1. FUNGSI NAMBAH BARANG KE KERANJANG ---
    public void TambahKopi() { cartKopi++; UpdateUIKeranjang(); }
    public void TambahMie() { cartMie++; UpdateUIKeranjang(); }
    public void TambahBoneka() { cartBoneka++; UpdateUIKeranjang(); }
    public void TambahBahan() { cartBahan++; UpdateUIKeranjang(); }
    
    // Barang permanen dibatasi maksimal 1 di keranjang
    public void TambahKeyboard() 
    { 
        if (!InventoryManager.Instance.punyaKeyboard && !cartKeyboard) 
        { cartKeyboard = true; UpdateUIKeranjang(); }
    }
    public void TambahBuku() 
    { 
        if (!InventoryManager.Instance.punyaBuku && !cartBuku) 
        { cartBuku = true; UpdateUIKeranjang(); }
    }

    // --- 2. FUNGSI UPDATE TAMPILAN KERANJANG ---
    void UpdateUIKeranjang()
    {
        // Hitung Total Harga
        totalHarga = (cartKopi * hargaKopi) + (cartMie * hargaMie) + 
                     (cartBoneka * hargaBoneka) + (cartBahan * hargaBahan) +
                     (cartKeyboard ? hargaKeyboard : 0) + (cartBuku ? hargaBuku : 0);

        if (textTotalHarga != null) 
            textTotalHarga.text = "Total: Rp " + totalHarga;

        // Susun Teks Rincian Barang
        string rincian = "";
        if (cartKopi > 0) rincian += "- Kopi x" + cartKopi + " (Rp " + (cartKopi * hargaKopi) + ")\n";
        if (cartMie > 0) rincian += "- Mie Ayam x" + cartMie + " (Rp " + (cartMie * hargaMie) + ")\n";
        if (cartBoneka > 0) rincian += "- Boneka x" + cartBoneka + " (Rp " + (cartBoneka * hargaBoneka) + ")\n";
        if (cartBahan > 0) rincian += "- Bahan x" + cartBahan + " (Rp " + (cartBahan * hargaBahan) + ")\n";
        if (cartKeyboard) rincian += "- Keyboard (Rp " + hargaKeyboard + ")\n";
        if (cartBuku) rincian += "- Buku (Rp " + hargaBuku + ")\n";

        if (rincian == "") rincian = "Keranjang masih kosong...";
        
        if (textRincianKeranjang != null) 
            textRincianKeranjang.text = rincian;
    }

    // --- 3. FUNGSI CHECKOUT (BELI SEMUA) ---
    public void CheckoutBelanjaan()
    {
        if (totalHarga == 0) return; // Batal jika keranjang kosong

        // Pastikan GameManager ada dan uang cukup
        if (GameManager.Instance != null && GameManager.Instance.uang >= totalHarga)
        {
            // Potong Uang
            GameManager.Instance.uang -= totalHarga;

            // Masukkan Barang ke Inventory
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.jumlahKopi += cartKopi;
                InventoryManager.Instance.jumlahMieAyam += cartMie;
                InventoryManager.Instance.jumlahBoneka += cartBoneka;
                InventoryManager.Instance.jumlahBahanMakanan += cartBahan;
                
                if (cartKeyboard) InventoryManager.Instance.punyaKeyboard = true;
                if (cartBuku) InventoryManager.Instance.punyaBuku = true;
            }

            Debug.Log("Checkout Sukses! Uang dipotong Rp " + totalHarga);
            KosongkanKeranjang(); // Bersihkan keranjang setelah berhasil beli
        }
        else
        {
            Debug.LogWarning("Uang tidak cukup!");
        }
    }

    // --- 4. FUNGSI RESET KERANJANG ---
    public void KosongkanKeranjang()
    {
        cartKopi = cartMie = cartBoneka = cartBahan = 0;
        cartKeyboard = cartBuku = false;
        totalHarga = 0;
        UpdateUIKeranjang();
    }
    
    // --- 5. FUNGSI TUTUP PANEL ---
    public void TutupToko()
    {
        gameObject.SetActive(false);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false); // Player bisa gerak lagi
    }
}