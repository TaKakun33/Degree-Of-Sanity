using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CookingController : MonoBehaviour
{
    [Header("Referensi UI Masak")]
    public GameObject panelMasak; 
    public List<InventorySlot> daftarSlotBahan; // Slot di sebelah kiri

    [Header("Ikon Bahan")]
    public Sprite ikonBahan1;
    public Sprite ikonBahan2;
    public Sprite ikonBahan3;

    [Header("Referensi Kanan (Detail)")]
    public TextMeshProUGUI textJudul;
    public TextMeshProUGUI textDeskripsi;
    public Button btnMasak;
    public TextMeshProUGUI textTombolMasak; // Teks di dalam tombol masak

    private JenisItem bahanTerpilih = JenisItem.Kosong;
    private bool sedangMemasak = false;

    [Header("TAMBAHAN")]
    [Tooltip("Berapa jam waktu in-game yang kelewat tiap kali selesai masak")]
    public float jamYangDilewatiSaatMasak = 1f;

    void OnEnable()
    {
        ResetDetail();
        UpdateTampilanBahan();
    }

    public void UpdateTampilanBahan()
    {
        if (InventoryManager.Instance == null) return;

        foreach (var slot in daftarSlotBahan)
        {
            if (slot != null) { try { slot.KosongkanSlot(); } catch (System.Exception) { continue; } }
        }

        int index = 0;

        if (InventoryManager.Instance.jumlahBahan1 > 0 && index < daftarSlotBahan.Count)
        {
            daftarSlotBahan[index].IsiSlot(ikonBahan1, InventoryManager.Instance.jumlahBahan1, () => PilihBahan(JenisItem.Bahan1, "Bahan Kualitas I", "Bahan dasar sederhana. Menghasilkan 1 porsi makanan jadi saat dimasak."));
            index++;
        }
        if (InventoryManager.Instance.jumlahBahan2 > 0 && index < daftarSlotBahan.Count)
        {
            daftarSlotBahan[index].IsiSlot(ikonBahan2, InventoryManager.Instance.jumlahBahan2, () => PilihBahan(JenisItem.Bahan2, "Bahan Kualitas II", "Bahan berkualitas sedang. Menghasilkan 2 porsi makanan jadi saat dimasak."));
            index++;
        }
        if (InventoryManager.Instance.jumlahBahan3 > 0 && index < daftarSlotBahan.Count)
        {
            daftarSlotBahan[index].IsiSlot(ikonBahan3, InventoryManager.Instance.jumlahBahan3, () => PilihBahan(JenisItem.Bahan3, "Bahan Kualitas III", "Bahan premium pilihan. Menghasilkan 3 porsi makanan jadi saat dimasak."));
            index++;
        }
    }

    public void PilihBahan(JenisItem jenis, string judul, string deskripsi)
    {
        if (sedangMemasak) return; // Kunci pilihan jika sedang masak

        bahanTerpilih = jenis;
        textJudul.text = judul;
        textDeskripsi.text = deskripsi;
        
        btnMasak.gameObject.SetActive(true);
        btnMasak.interactable = true;
        textTombolMasak.text = "Masak Bahan";

        btnMasak.onClick.RemoveAllListeners();
        btnMasak.onClick.AddListener(MulaiMasak);
    }

    public void MulaiMasak()
    {
        StartCoroutine(ProsesMasak());
    }

    private IEnumerator ProsesMasak()
    {
        sedangMemasak = true;
        btnMasak.interactable = false;
        textTombolMasak.text = "Memasak...";

        // Jeda waktu memasak (misal: 3 detik)
        yield return new WaitForSeconds(3f);

        // Eksekusi penambahan makanan
        switch (bahanTerpilih)
        {
            case JenisItem.Bahan1:
                InventoryManager.Instance.jumlahBahan1--;
                InventoryManager.Instance.jumlahMakananJadi += 1;
                break;
            case JenisItem.Bahan2:
                InventoryManager.Instance.jumlahBahan2--;
                InventoryManager.Instance.jumlahMakananJadi += 2;
                break;
            case JenisItem.Bahan3:
                InventoryManager.Instance.jumlahBahan3--;
                InventoryManager.Instance.jumlahMakananJadi += 3;
                break;
        }

        Debug.Log("Memasak selesai! Makanan disimpan di Inventory.");

        // --- TAMBAHAN: masak beneran makan waktu in-game ~1 jam ---
        if (GameManager.Instance != null) GameManager.Instance.jamSaatIni += jamYangDilewatiSaatMasak;

        sedangMemasak = false;
        UpdateTampilanBahan();
        ResetDetail();
    }

    private void ResetDetail()
    {
        if (sedangMemasak) return;
        bahanTerpilih = JenisItem.Kosong;
        textJudul.text = "Pilih Bahan";
        textDeskripsi.text = "Pilih bahan makanan di sebelah kiri untuk mulai memasak.";
        btnMasak.gameObject.SetActive(false);
    }

    public void TutupPanelMasak()
    {
        if (sedangMemasak) return; // Tidak bisa ditutup kalau lagi masak!
        panelMasak.SetActive(false);
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);
    }
}