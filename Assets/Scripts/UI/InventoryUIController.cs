using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// Daftar jenis barang diperbarui dengan tambahan Bahan dan Makanan Jadi
public enum JenisItem { Kosong, Kopi, Mie, Boneka, Bahan1, Bahan2, Bahan3, MakananJadi, Keyboard, Buku }

public class InventoryUIController : MonoBehaviour
{
    [Header("Referensi UI Slot")]
    public Transform wadahGrid; 
    public List<InventorySlot> daftarSlot; 

    [Header("Ikon Barang")]
    public Sprite ikonKopi;
    public Sprite ikonMie;
    public Sprite ikonBoneka;
    public Sprite ikonBahan1;
    public Sprite ikonBahan2;
    public Sprite ikonBahan3;
    public Sprite ikonMakananJadi;
    public Sprite ikonKeyboard;
    public Sprite ikonBuku;

    [Header("Referensi UI Detail Item")]
    public GameObject panelDetail; 
    public TextMeshProUGUI textNamaItem;
    public TextMeshProUGUI textDeskripsiItem;
    public Button btnGunakan;
    public Button btnJual;
    public TextMeshProUGUI textHargaJual; 

    private JenisItem itemTerpilih = JenisItem.Kosong;
    private int hargaJualTerpilih = 0;

    void OnEnable()
    {
        TutupDetail(); 
        UpdateTampilanInventory();
    }

    public void UpdateTampilanInventory()
    {
        if (InventoryManager.Instance == null) return;

        foreach (var slot in daftarSlot)
        {
            if (slot != null) { try { slot.KosongkanSlot(); } catch (System.Exception) { continue; } }
        }

        int indexSlot = 0; 

        if (InventoryManager.Instance.jumlahKopi > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonKopi, InventoryManager.Instance.jumlahKopi, () => PilihItem(JenisItem.Kopi, "Kopi Espresso", "Menambah toleransi typo dan batas tidur +1 jam.", 7500, true));
            indexSlot++;
        }
        
        if (InventoryManager.Instance.jumlahMieAyam > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonMie, InventoryManager.Instance.jumlahMieAyam, () => PilihItem(JenisItem.Mie, "Mie Ayam", "Memulihkan parameter Lapar secara instan hingga penuh.", 10000, true));
            indexSlot++;
        }
        
        if (InventoryManager.Instance.jumlahBoneka > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonBoneka, InventoryManager.Instance.jumlahBoneka, () => PilihItem(JenisItem.Boneka, "Mainan / Boneka", "Diberikan kepada Adik untuk memulihkan Sanity.", 25000, true));
            indexSlot++;
        }

        // Tampilan 3 Bahan Makanan (Hanya bisa dijual di sini, 'false' di akhir)
        if (InventoryManager.Instance.jumlahBahan1 > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonBahan1, InventoryManager.Instance.jumlahBahan1, () => PilihItem(JenisItem.Bahan1, "Bahan Kualitas I", "Bisa dimasak di dapur. (Menghasilkan 1 Makanan)", 2500, false));
            indexSlot++;
        }
        if (InventoryManager.Instance.jumlahBahan2 > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonBahan2, InventoryManager.Instance.jumlahBahan2, () => PilihItem(JenisItem.Bahan2, "Bahan Kualitas II", "Bisa dimasak di dapur. (Menghasilkan 2 Makanan)", 5000, false));
            indexSlot++;
        }
        if (InventoryManager.Instance.jumlahBahan3 > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonBahan3, InventoryManager.Instance.jumlahBahan3, () => PilihItem(JenisItem.Bahan3, "Bahan Kualitas III", "Bisa dimasak di dapur. (Menghasilkan 3 Makanan)", 7500, false));
            indexSlot++;
        }

        // Tampilan Makanan Jadi (Bisa digunakan 'true' untuk dimakan)
        if (InventoryManager.Instance.jumlahMakananJadi > 0 && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonMakananJadi, InventoryManager.Instance.jumlahMakananJadi, () => PilihItem(JenisItem.MakananJadi, "Masakan Rumahan", "Masakan bergizi, mengisi perut sampai kenyang maksimal.", 12500, true));
            indexSlot++;
        }

        if (InventoryManager.Instance.punyaKeyboard && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonKeyboard, 1, () => PilihItem(JenisItem.Keyboard, "Keyboard Ergonomis", "Memperlambat laju teks pada minigame skripsi.", 75000, false));
            indexSlot++;
        }

        if (InventoryManager.Instance.punyaBuku && indexSlot < daftarSlot.Count)
        {
            if (daftarSlot[indexSlot] != null) daftarSlot[indexSlot].IsiSlot(ikonBuku, 1, () => PilihItem(JenisItem.Buku, "Buku Referensi", "Menambah Progres Skripsi dari setiap penyelesaian minigame.", 50000, false));
            indexSlot++;
        }
    }

    public void PilihItem(JenisItem jenis, string nama, string deskripsi, int hargaJual, bool bisaDigunakan)
    {
        itemTerpilih = jenis;
        hargaJualTerpilih = hargaJual;

        panelDetail.SetActive(true); 
        if (textNamaItem != null) textNamaItem.text = nama;
        if (textDeskripsiItem != null) textDeskripsiItem.text = deskripsi;
        if (textHargaJual != null) textHargaJual.text = hargaJual.ToString();

        btnGunakan.gameObject.SetActive(bisaDigunakan);
        btnGunakan.onClick.RemoveAllListeners();
        btnGunakan.onClick.AddListener(EksekusiGunakan);

        btnJual.onClick.RemoveAllListeners();
        btnJual.onClick.AddListener(EksekusiJual);
    }

    public void EksekusiGunakan()
    {
        switch (itemTerpilih)
        {
            case JenisItem.Kopi:
                InventoryManager.Instance.jumlahKopi--;
                GameManager.Instance.batasTidur += 1f;
                break;
            case JenisItem.Mie:
                InventoryManager.Instance.jumlahMieAyam--;
                GameManager.Instance.lapar = 100f;
                break;
            case JenisItem.Boneka:
                InventoryManager.Instance.jumlahBoneka--;
                break;
            case JenisItem.MakananJadi:
                InventoryManager.Instance.jumlahMakananJadi--;
                GameManager.Instance.lapar = 100f; // Bikin kenyang!
                break;
        }
        
        CekSetelahInteraksi();
    }

    public void EksekusiJual()
    {
        if (GameManager.Instance != null) GameManager.Instance.uang += hargaJualTerpilih;

        switch (itemTerpilih)
        {
            case JenisItem.Kopi: InventoryManager.Instance.jumlahKopi--; break;
            case JenisItem.Mie: InventoryManager.Instance.jumlahMieAyam--; break;
            case JenisItem.Boneka: InventoryManager.Instance.jumlahBoneka--; break;
            case JenisItem.Bahan1: InventoryManager.Instance.jumlahBahan1--; break;
            case JenisItem.Bahan2: InventoryManager.Instance.jumlahBahan2--; break;
            case JenisItem.Bahan3: InventoryManager.Instance.jumlahBahan3--; break;
            case JenisItem.MakananJadi: InventoryManager.Instance.jumlahMakananJadi--; break;
            case JenisItem.Keyboard: InventoryManager.Instance.punyaKeyboard = false; break;
            case JenisItem.Buku: InventoryManager.Instance.punyaBuku = false; break;
        }

        CekSetelahInteraksi();
    }

    private void CekSetelahInteraksi()
    {
        bool habis = false;
        switch (itemTerpilih)
        {
            case JenisItem.Kopi: if (InventoryManager.Instance.jumlahKopi <= 0) habis = true; break;
            case JenisItem.Mie: if (InventoryManager.Instance.jumlahMieAyam <= 0) habis = true; break;
            case JenisItem.Boneka: if (InventoryManager.Instance.jumlahBoneka <= 0) habis = true; break;
            case JenisItem.Bahan1: if (InventoryManager.Instance.jumlahBahan1 <= 0) habis = true; break;
            case JenisItem.Bahan2: if (InventoryManager.Instance.jumlahBahan2 <= 0) habis = true; break;
            case JenisItem.Bahan3: if (InventoryManager.Instance.jumlahBahan3 <= 0) habis = true; break;
            case JenisItem.MakananJadi: if (InventoryManager.Instance.jumlahMakananJadi <= 0) habis = true; break;
            case JenisItem.Keyboard: if (!InventoryManager.Instance.punyaKeyboard) habis = true; break;
            case JenisItem.Buku: if (!InventoryManager.Instance.punyaBuku) habis = true; break;
        }

        if (habis) TutupDetail();
        UpdateTampilanInventory();
    }

    private void TutupDetail()
    {
        itemTerpilih = JenisItem.Kosong;
        if (panelDetail != null) panelDetail.SetActive(false);
    }

    public void TutupInventory()
    {
        gameObject.SetActive(false);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }
}