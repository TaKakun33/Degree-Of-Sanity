using UnityEngine;
using TMPro; 

public class ShopController : MonoBehaviour
{
    [Header("Harga Barang (Rp)")]
    public int hargaKopi = 15000;
    public int hargaMie = 20000;
    public int hargaBoneka = 50000;
    public int hargaBahan1 = 5000;
    public int hargaBahan2 = 10000;
    public int hargaBahan3 = 15000;
    public int hargaKeyboard = 150000;
    public int hargaBuku = 100000;

    [Header("Isi Keranjang (Cart)")]
    private int cartKopi = 0, cartMie = 0, cartBoneka = 0;
    private int cartBahan1 = 0, cartBahan2 = 0, cartBahan3 = 0;
    private bool cartKeyboard = false, cartBuku = false;

    private int totalHarga = 0;

    [Header("Referensi UI (Keranjang & Total)")]
    public TextMeshProUGUI textRincianKeranjang;
    public TextMeshProUGUI textTotalHarga;

    [Header("Referensi Teks Jumlah di Antara Tombol (+/-)")]
    public TextMeshProUGUI txtJmlKopi;
    public TextMeshProUGUI txtJmlMie;
    public TextMeshProUGUI txtJmlBoneka;
    public TextMeshProUGUI txtJmlBahan1;
    public TextMeshProUGUI txtJmlBahan2;
    public TextMeshProUGUI txtJmlBahan3;
    public TextMeshProUGUI txtJmlKeyboard;
    public TextMeshProUGUI txtJmlBuku;

    void OnEnable() { KosongkanKeranjang(); }

    // --- FUNGSI TAMBAH (+) ---
    public void TambahKopi() { cartKopi++; UpdateUIKeranjang(); }
    public void TambahMie() { cartMie++; UpdateUIKeranjang(); }
    public void TambahBoneka() { cartBoneka++; UpdateUIKeranjang(); }
    public void TambahBahan1() { cartBahan1++; UpdateUIKeranjang(); }
    public void TambahBahan2() { cartBahan2++; UpdateUIKeranjang(); }
    public void TambahBahan3() { cartBahan3++; UpdateUIKeranjang(); }
    public void TambahKeyboard() { if (!InventoryManager.Instance.punyaKeyboard && !cartKeyboard) { cartKeyboard = true; UpdateUIKeranjang(); } }
    public void TambahBuku() { if (!InventoryManager.Instance.punyaBuku && !cartBuku) { cartBuku = true; UpdateUIKeranjang(); } }

    // --- FUNGSI KURANG (-) ---
    public void KurangiKopi() { if (cartKopi > 0) { cartKopi--; UpdateUIKeranjang(); } }
    public void KurangiMie() { if (cartMie > 0) { cartMie--; UpdateUIKeranjang(); } }
    public void KurangiBoneka() { if (cartBoneka > 0) { cartBoneka--; UpdateUIKeranjang(); } }
    public void KurangiBahan1() { if (cartBahan1 > 0) { cartBahan1--; UpdateUIKeranjang(); } }
    public void KurangiBahan2() { if (cartBahan2 > 0) { cartBahan2--; UpdateUIKeranjang(); } }
    public void KurangiBahan3() { if (cartBahan3 > 0) { cartBahan3--; UpdateUIKeranjang(); } }
    public void KurangiKeyboard() { if (cartKeyboard) { cartKeyboard = false; UpdateUIKeranjang(); } }
    public void KurangiBuku() { if (cartBuku) { cartBuku = false; UpdateUIKeranjang(); } }

    // --- UPDATE TAMPILAN KERANJANG ---
    void UpdateUIKeranjang()
    {
        totalHarga = (cartKopi * hargaKopi) + (cartMie * hargaMie) + (cartBoneka * hargaBoneka) + 
                     (cartBahan1 * hargaBahan1) + (cartBahan2 * hargaBahan2) + (cartBahan3 * hargaBahan3) +
                     (cartKeyboard ? hargaKeyboard : 0) + (cartBuku ? hargaBuku : 0);

        if (textTotalHarga != null) textTotalHarga.text = "Total: Rp " + totalHarga;

        if (txtJmlKopi != null) txtJmlKopi.text = cartKopi.ToString();
        if (txtJmlMie != null) txtJmlMie.text = cartMie.ToString();
        if (txtJmlBoneka != null) txtJmlBoneka.text = cartBoneka.ToString();
        if (txtJmlBahan1 != null) txtJmlBahan1.text = cartBahan1.ToString();
        if (txtJmlBahan2 != null) txtJmlBahan2.text = cartBahan2.ToString();
        if (txtJmlBahan3 != null) txtJmlBahan3.text = cartBahan3.ToString();
        if (txtJmlKeyboard != null) txtJmlKeyboard.text = cartKeyboard ? "1" : "0";
        if (txtJmlBuku != null) txtJmlBuku.text = cartBuku ? "1" : "0";

        string rincian = "";
        if (cartKopi > 0) rincian += $"- Kopi x{cartKopi}\n";
        if (cartMie > 0) rincian += $"- Mie Ayam x{cartMie}\n";
        if (cartBoneka > 0) rincian += $"- Boneka x{cartBoneka}\n";
        if (cartBahan1 > 0) rincian += $"- Bahan Kualitas I x{cartBahan1}\n";
        if (cartBahan2 > 0) rincian += $"- Bahan Kualitas II x{cartBahan2}\n";
        if (cartBahan3 > 0) rincian += $"- Bahan Kualitas III x{cartBahan3}\n";
        if (cartKeyboard) rincian += "- Keyboard Ergonomis\n";
        if (cartBuku) rincian += "- Buku Referensi\n";

        if (rincian == "") rincian = "Keranjang kosong...";
        if (textRincianKeranjang != null) {
            textRincianKeranjang.text = rincian;
            textRincianKeranjang.color = Color.white; // --- TAMBAHAN: warna FFFFFF ---
        }
    }

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
                InventoryManager.Instance.jumlahBahan1 += cartBahan1;
                InventoryManager.Instance.jumlahBahan2 += cartBahan2;
                InventoryManager.Instance.jumlahBahan3 += cartBahan3;
                if (cartKeyboard) InventoryManager.Instance.punyaKeyboard = true;
                if (cartBuku) InventoryManager.Instance.punyaBuku = true;
            }
            KosongkanKeranjang(); 
        }
    }

    public void KosongkanKeranjang()
    {
        cartKopi = cartMie = cartBoneka = cartBahan1 = cartBahan2 = cartBahan3 = 0;
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