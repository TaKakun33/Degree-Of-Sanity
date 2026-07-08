using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// Daftar jenis barang untuk mempermudah identifikasi
public enum JenisItem { Kosong, Kopi, Mie, Boneka, Bahan, Keyboard, Buku }

public class InventoryUIController : MonoBehaviour
{
    [Header("Referensi UI Slot")]
    public Transform wadahGrid; 
    public List<InventorySlot> daftarSlot; 

    [Header("Ikon Barang")]
    public Sprite ikonKopi;
    public Sprite ikonMie;
    public Sprite ikonBoneka;
    public Sprite ikonBahan;
    public Sprite ikonKeyboard;
    public Sprite ikonBuku;

    [Header("Referensi UI Detail Item")]
    public GameObject panelDetail; 
    public TextMeshProUGUI textNamaItem;
    public TextMeshProUGUI textDeskripsiItem;
    public Button btnGunakan;
    public Button btnJual;
    public TextMeshProUGUI textHargaJual; 

    // Memori untuk menyimpan data barang yang sedang diklik
    private JenisItem itemTerpilih = JenisItem.Kosong;
    private int hargaJualTerpilih = 0;

    void OnEnable()
    {
        // Reset detail dan perbarui daftar barang saat inventory dibuka
        TutupDetail(); 
        UpdateTampilanInventory();
    }

    public void UpdateTampilanInventory()
    {
        if (InventoryManager.Instance == null) return;

        // Kosongkan semua slot agar barang tidak menumpuk ganda
        foreach (var slot in daftarSlot)
        {
            if (slot != null) { try { slot.KosongkanSlot(); } catch (System.Exception) { continue; } }
        }

        int indexSlot = 0; 

        // Masukkan barang yang dimiliki pemain ke dalam slot secara berurutan
        if (InventoryManager.Instance.jumlahKopi > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) 
                daftarSlot[indexSlot].IsiSlot(ikonKopi, InventoryManager.Instance.jumlahKopi, () => PilihItem(JenisItem.Kopi, "Kopi Espresso", "Menambah toleransi typo dan batas tidur +1 jam.", 7500, true));
            indexSlot++;
        }
        
        if (InventoryManager.Instance.jumlahMieAyam > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) 
                daftarSlot[indexSlot].IsiSlot(ikonMie, InventoryManager.Instance.jumlahMieAyam, () => PilihItem(JenisItem.Mie, "Mie Ayam", "Memulihkan parameter Lapar secara instan hingga penuh.", 10000, true));
            indexSlot++;
        }
        
        if (InventoryManager.Instance.jumlahBoneka > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) 
                daftarSlot[indexSlot].IsiSlot(ikonBoneka, InventoryManager.Instance.jumlahBoneka, () => PilihItem(JenisItem.Boneka, "Mainan / Boneka", "Diberikan kepada Adik untuk memulihkan Sanity.", 25000, true));
            indexSlot++;
        }

        if (InventoryManager.Instance.jumlahBahanMakanan > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) 
                // Parameter false mematikan tombol gunakan khusus untuk bahan makanan
                daftarSlot[indexSlot].IsiSlot(ikonBahan, InventoryManager.Instance.jumlahBahanMakanan, () => PilihItem(JenisItem.Bahan, "Bahan Makanan Mentah", "Bisa dimasak di dapur. (Hanya bisa dijual di sini)", 5000, false));
            indexSlot++;
        }

        // Upgrade permanen disetel false karena hanya bisa dijual
        if (InventoryManager.Instance.punyaKeyboard && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) 
                daftarSlot[indexSlot].IsiSlot(ikonKeyboard, 1, () => PilihItem(JenisItem.Keyboard, "Keyboard Ergonomis", "Memperlambat laju teks pada minigame skripsi.", 75000, false));
            indexSlot++;
        }

        if (InventoryManager.Instance.punyaBuku && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) 
                daftarSlot[indexSlot].IsiSlot(ikonBuku, 1, () => PilihItem(JenisItem.Buku, "Buku Referensi", "Menambah Progres Skripsi dari setiap penyelesaian minigame.", 50000, false));
            indexSlot++;
        }
    }

    public void PilihItem(JenisItem jenis, string nama, string deskripsi, int hargaJual, bool bisaDigunakan)
    {
        // Simpan data barang yang dipilih
        itemTerpilih = jenis;
        hargaJualTerpilih = hargaJual;

        // Munculkan panel detail beserta teksnya
        panelDetail.SetActive(true); 
        if (textNamaItem != null) textNamaItem.text = nama;
        if (textDeskripsiItem != null) textDeskripsiItem.text = deskripsi;
        if (textHargaJual != null) textHargaJual.text = hargaJual.ToString();

        // Atur kemunculan tombol Gunakan dan sambungkan perintahnya
        btnGunakan.gameObject.SetActive(bisaDigunakan);
        btnGunakan.onClick.RemoveAllListeners();
        btnGunakan.onClick.AddListener(EksekusiGunakan);

        btnJual.onClick.RemoveAllListeners();
        btnJual.onClick.AddListener(EksekusiJual);
    }

    public void EksekusiGunakan()
    {
        // Jalankan efek barang berdasarkan apa yang diklik
        switch (itemTerpilih)
        {
            case JenisItem.Kopi:
                InventoryManager.Instance.jumlahKopi--;
                GameManager.Instance.batasTidur += 1f;
                Debug.Log("Kopi diminum! Batas tidur +1 jam.");
                break;
            case JenisItem.Mie:
                InventoryManager.Instance.jumlahMieAyam--;
                GameManager.Instance.lapar = 100f;
                Debug.Log("Mie Ayam dimakan! Perut kenyang.");
                break;
            case JenisItem.Boneka:
                InventoryManager.Instance.jumlahBoneka--;
                Debug.Log("Boneka diberikan ke adik!");
                break;
        }
        
        CekSetelahInteraksi();
    }

    public void EksekusiJual()
    {
        // Tambah uang pemain dan kurangi jumlah barang di inventory
        if (GameManager.Instance != null) GameManager.Instance.uang += hargaJualTerpilih;

        switch (itemTerpilih)
        {
            case JenisItem.Kopi: InventoryManager.Instance.jumlahKopi--; break;
            case JenisItem.Mie: InventoryManager.Instance.jumlahMieAyam--; break;
            case JenisItem.Boneka: InventoryManager.Instance.jumlahBoneka--; break;
            case JenisItem.Bahan: InventoryManager.Instance.jumlahBahanMakanan--; break;
            case JenisItem.Keyboard: InventoryManager.Instance.punyaKeyboard = false; break;
            case JenisItem.Buku: InventoryManager.Instance.punyaBuku = false; break;
        }

        Debug.Log("Barang dijual seharga " + hargaJualTerpilih);
        CekSetelahInteraksi();
    }

    private void CekSetelahInteraksi()
    {
        // Periksa apakah barang sudah habis setelah dipakai atau dijual
        bool habis = false;
        switch (itemTerpilih)
        {
            case JenisItem.Kopi: if (InventoryManager.Instance.jumlahKopi <= 0) habis = true; break;
            case JenisItem.Mie: if (InventoryManager.Instance.jumlahMieAyam <= 0) habis = true; break;
            case JenisItem.Boneka: if (InventoryManager.Instance.jumlahBoneka <= 0) habis = true; break;
            case JenisItem.Bahan: if (InventoryManager.Instance.jumlahBahanMakanan <= 0) habis = true; break;
            case JenisItem.Keyboard: if (!InventoryManager.Instance.punyaKeyboard) habis = true; break;
            case JenisItem.Buku: if (!InventoryManager.Instance.punyaBuku) habis = true; break;
        }

        // Tutup panel jika barang habis dan perbarui tampilan slot
        if (habis) TutupDetail();
        UpdateTampilanInventory();
    }

    private void TutupDetail()
    {
        // Sembunyikan panel detail ke kondisi semula
        itemTerpilih = JenisItem.Kosong;
        if (panelDetail != null) panelDetail.SetActive(false);
    }

    public void TutupInventory()
    {
        // Tutup UI inventory dan kembalikan kendali pemain
        gameObject.SetActive(false);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }
}