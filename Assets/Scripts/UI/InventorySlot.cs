using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image iconBarang;
    public TextMeshProUGUI textJumlah;
    public Button tombolSlot; // Ini adalah komponen Button dari slot itu sendiri

    public void KosongkanSlot()
    {
        iconBarang.sprite = null;
        iconBarang.color = new Color(1, 1, 1, 0);
        textJumlah.text = "";
        tombolSlot.interactable = false;
        
        // Hapus semua perintah klik sebelumnya
        tombolSlot.onClick.RemoveAllListeners();
    }

    public void IsiSlot(Sprite icon, int jumlah, UnityEngine.Events.UnityAction aksiPilih)
    {
        iconBarang.sprite = icon;
        iconBarang.color = new Color(1, 1, 1, 1);
        textJumlah.text = jumlah > 1 ? jumlah.ToString() : "";
        
        tombolSlot.interactable = true;
        
        // Memasukkan perintah baru saat slot ini diklik
        tombolSlot.onClick.RemoveAllListeners();
        tombolSlot.onClick.AddListener(aksiPilih);
    }
}