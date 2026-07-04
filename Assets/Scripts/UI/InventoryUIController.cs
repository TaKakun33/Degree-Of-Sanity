using UnityEngine;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    [Header("Referensi UI Slot")]
    public Transform wadahGrid; // Tarik objek WadahGrid yang memiliki Grid Layout Group
    public List<InventorySlot> daftarSlot; // Masukkan semua kotak slot ke list ini

    [Header("Ikon Barang")]
    public Sprite ikonKopi;
    public Sprite ikonMie;
    public Sprite ikonBoneka;
    public Sprite ikonBahan;
    public Sprite ikonKeyboard;
    public Sprite ikonBuku;

    // Dipanggil otomatis saat Panel_Inventory aktif
    void OnEnable()
    {
        UpdateTampilanInventory();
    }

    public void UpdateTampilanInventory()
    {
        // Pengaman: Pastikan InventoryManager sudah ada di Scene
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager tidak ditemukan! Pastikan script InventoryManager ada di GameManager.");
            return;
        }

        // 1. Kosongkan semua slot terlebih dahulu
        foreach (var slot in daftarSlot)
        {
            if (slot != null)
            {
                try 
                {
                    slot.KosongkanSlot();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Ada komponen yang hilang di salah satu Slot! Error: " + e.Message);
                }
            }
        }

        int indexSlot = 0; 

        // 2. Isi slot dengan barang yang dimiliki (InventoryManager)
        if (InventoryManager.Instance.jumlahKopi > 0 && indexSlot < daftarSlot.Count)
        {
            daftarSlot[indexSlot].IsiSlot(ikonKopi, InventoryManager.Instance.jumlahKopi, GunakanKopi);
            indexSlot++;
        }
        
        if (InventoryManager.Instance.jumlahMieAyam > 0 && indexSlot < daftarSlot.Count)
        {
            daftarSlot[indexSlot].IsiSlot(ikonMie, InventoryManager.Instance.jumlahMieAyam, GunakanMieAyam);
            indexSlot++;
        }
        
        if (InventoryManager.Instance.jumlahBoneka > 0 && indexSlot < daftarSlot.Count)
        {
            daftarSlot[indexSlot].IsiSlot(ikonBoneka, InventoryManager.Instance.jumlahBoneka, null);
            indexSlot++;
        }

        if (InventoryManager.Instance.jumlahBahanMakanan > 0 && indexSlot < daftarSlot.Count)
        {
            daftarSlot[indexSlot].IsiSlot(ikonBahan, InventoryManager.Instance.jumlahBahanMakanan, null);
            indexSlot++;
        }

        // Tampilkan Upgrade Permanen jika sudah terbeli
        if (InventoryManager.Instance.punyaKeyboard && indexSlot < daftarSlot.Count)
        {
            daftarSlot[indexSlot].IsiSlot(ikonKeyboard, 1, null);
            indexSlot++;
        }

        if (InventoryManager.Instance.punyaBuku && indexSlot < daftarSlot.Count)
        {
            daftarSlot[indexSlot].IsiSlot(ikonBuku, 1, null);
            indexSlot++;
        }
    }

    // --- FUNGSI PENGGUNAAN BARANG ---
    void GunakanKopi()
    {
        if (InventoryManager.Instance.jumlahKopi > 0)
        {
            InventoryManager.Instance.jumlahKopi--;
            GameManager.Instance.batasTidur += 1f;
            Debug.Log("Kopi diminum! Batas tidur +1 jam.");
            UpdateTampilanInventory(); 
        }
    }

    void GunakanMieAyam()
    {
        if (InventoryManager.Instance.jumlahMieAyam > 0)
        {
            InventoryManager.Instance.jumlahMieAyam--;
            GameManager.Instance.lapar = 100f;
            Debug.Log("Mie Ayam dimakan! Perut kenyang.");
            UpdateTampilanInventory();
        }
    }

    // Fungsi ini dipanggil oleh tombol "X" atau area tutup
    public void TutupInventory()
    {
        gameObject.SetActive(false);
        // Mengembalikan kendali pergerakan player
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }
}