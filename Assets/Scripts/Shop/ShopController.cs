using UnityEngine;
using TMPro; 

public class ShopController : MonoBehaviour
{
    [Header("Harga Barang (Rp)")]
    public int hargaKopi = 15000;
    public int hargaMie = 20000;
    public int hargaBoneka = 500000;
    public int hargaBahan = 10000;
    public int hargaKeyboard = 1500000;
    public int hargaBuku = 100000;

    [Header("Isi Keranjang (Cart)")]
    private int cartKopi = 0;
    private int cartMie = 0;
    private int cartBoneka = 0;
    private int cartBahan = 0;
    private bool cartKeyboard = false;
    private bool cartBuku = false;

    private int totalHarga = 0;

    [Header("Referensi UI (Keranjang & Total)")]
    public TextMeshProUGUI textRincianKeranjang;
    public TextMeshProUGUI textTotalHarga;

    [Header("Referensi Teks Jumlah di Antara Tombol (+/-)")]
    public TextMeshProUGUI txtJmlKopi;
    public TextMeshProUGUI txtJmlMie;
    public TextMeshProUGUI txtJmlBoneka;
    public TextMeshProUGUI txtJmlBahan;
    public TextMeshProUGUI txtJmlKeyboard;
    public TextMeshProUGUI txtJmlBuku;

    void OnEnable()
    {
        KosongkanKeranjang();
    }

    // --- FUNGSI TAMBAH BARANG (+) ---
    public void TambahKopi() { cartKopi++; UpdateUIKeranjang(); }
    public void TambahMie() { cartMie++; UpdateUIKeranjang(); }
    public void TambahBoneka() { cartBoneka++; UpdateUIKeranjang(); }
    public void TambahBahan() { cartBahan++; UpdateUIKeranjang(); }
    public void TambahKeyboard() { if (!InventoryManager.Instance.punyaKeyboard && !cartKeyboard) { cartKeyboard = true; UpdateUIKeranjang(); } }
    public void TambahBuku() { if (!InventoryManager.Instance.punyaBuku && !cartBuku) { cartBuku = true; UpdateUIKeranjang(); } }

    // --- FUNGSI KURANGI BARANG (-) ---
    public void KurangiKopi() { if (cartKopi > 0) { cartKopi--; UpdateUIKeranjang(); } }
    public void KurangiMie() { if (cartMie > 0) { cartMie--; UpdateUIKeranjang(); } }
    public void KurangiBoneka() { if (cartBoneka > 0) { cartBoneka--; UpdateUIKeranjang(); } }
    public void KurangiBahan() { if (cartBahan > 0) { cartBahan--; UpdateUIKeranjang(); } }
    public void KurangiKeyboard() { if (cartKeyboard) { cartKeyboard = false; UpdateUIKeranjang(); } }
    public void KurangiBuku() { if (cartBuku) { cartBuku = false; UpdateUIKeranjang(); } }

    // --- UPDATE TAMPILAN KESELURUHAN ---
    void UpdateUIKeranjang()
    {
        totalHarga = (cartKopi * hargaKopi) + (cartMie * hargaMie) + 
                     (cartBoneka * hargaBoneka) + (cartBahan * hargaBahan) +
                     (cartKeyboard ? hargaKeyboard : 0) + (cartBuku ? hargaBuku : 0);

        if (textTotalHarga != null) textTotalHarga.text = "Total: Rp " + totalHarga;

        // Update angka di tengah tombol +/-
        if (txtJmlKopi != null) txtJmlKopi.text = cartKopi.ToString();
        if (txtJmlMie != null) txtJmlMie.text = cartMie.ToString();
        if (txtJmlBoneka != null) txtJmlBoneka.text = cartBoneka.ToString();
        if (txtJmlBahan != null) txtJmlBahan.text = cartBahan.ToString();
        if (txtJmlKeyboard != null) txtJmlKeyboard.text = cartKeyboard ? "1" : "0";
        if (txtJmlBuku != null) txtJmlBuku.text = cartBuku ? "1" : "0";

        // Update Rincian Teks
        string rincian = "";
        if (cartKopi > 0) rincian += "- Kopi x" + cartKopi + "\n";
        if (cartMie > 0) rincian += "- Mie Ayam x" + cartMie + "\n";
        if (cartBoneka > 0) rincian += "- Boneka x" + cartBoneka + "\n";
        if (cartBahan > 0) rincian += "- Bahan x" + cartBahan + "\n";
        if (cartKeyboard) rincian += "- Keyboard Ergonomis\n";
        if (cartBuku) rincian += "- Buku Referensi\n";

        if (rincian == "") rincian = "Keranjang kosong...";
        if (textRincianKeranjang != null) textRincianKeranjang.text = rincian;
    }

    // --- CHECKOUT ---
    public void CheckoutBelanjaan()
    {
        if (totalHarga == 0) return; 

        if (GameManager.Instance != null && GameManager.Instance.uang >= totalHarga)
        {
            GameManager.Instance.uang -= totalHarga;
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.jumlahKopi += cartKopi;
                InventoryManager.Instance.jumlahMieAyam += cartMie;
                InventoryManager.Instance.jumlahBoneka += cartBoneka;
                InventoryManager.Instance.jumlahBahanMakanan += cartBahan;
                if (cartKeyboard) InventoryManager.Instance.punyaKeyboard = true;
                if (cartBuku) InventoryManager.Instance.punyaBuku = true;
            }
            Debug.Log("Checkout Sukses!");
            KosongkanKeranjang(); 
        }
    }

    public void KosongkanKeranjang()
    {
        cartKopi = cartMie = cartBoneka = cartBahan = 0;
        cartKeyboard = cartBuku = false;
        totalHarga = 0;
        UpdateUIKeranjang();
    }
    
    public void TutupToko()
    {
        gameObject.SetActive(false);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }
}