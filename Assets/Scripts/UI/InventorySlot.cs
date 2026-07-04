using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image iconBarang;
    public TextMeshProUGUI textJumlah;
    public Button tombolGunakan;

    // Mengosongkan slot jika tidak ada barang
    public void KosongkanSlot()
    {
        iconBarang.sprite = null;
        iconBarang.color = new Color(1, 1, 1, 0); // Membuat transparan
        textJumlah.text = "";
        tombolGunakan.interactable = false;
    }

    // Mengisi slot dengan data barang
    public void IsiSlot(Sprite icon, int jumlah, UnityEngine.Events.UnityAction aksiGunakan)
    {
        iconBarang.sprite = icon;
        iconBarang.color = new Color(1, 1, 1, 1); // Menampilkan gambar
        
        // Tampilkan angka hanya jika lebih dari 1 (ala game survival)
        textJumlah.text = jumlah > 1 ? jumlah.ToString() : ""; 
        
        tombolGunakan.interactable = true;
        
        // Mengganti fungsi klik tombol sesuai barang yang ada di slot ini
        tombolGunakan.onClick.RemoveAllListeners();
        tombolGunakan.onClick.AddListener(aksiGunakan);
    }
}