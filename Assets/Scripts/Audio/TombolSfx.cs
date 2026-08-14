using UnityEngine;
using UnityEngine.UI;

// --- Tempel komponen ini ke GameObject Button MANA PUN yang mau dikasih suara klik:
// tombol Buka Inventory, Buka Toko, Buka Utang, Tutup/Kembali/Keluar panel, Pilih Item,
// Jual, Beli, Tambah (+), Kurang (-), dll.
//
// Kerjanya independen dari onClick yang udah ada di Inspector ATAU yang di-AddListener lewat
// kode (PanelUtangController, ShopController, InventoryUIController, dst) - dia cuma NAMBAHIN
// listener SFX di atasnya, gak ngoprek/gak ngapus listener aslinya.
//
// --- FIX: pakai OnEnable/OnDisable, BUKAN Awake/OnDestroy. Sebabnya: panel-panel di project
// ini (Toko/Inventory/Utang/Masak) start dalam keadaan NONAKTIF (SetActive(false)), begitu
// juga semua Button di dalamnya. Awake() di Unity CUMA jalan kalau GameObject-nya aktif -
// kalau tombolnya masih mati pas scene pertama kali load, Awake() gak akan pernah kepanggil,
// jadi listener SFX gak pernah kepasang sama sekali (biarpun tombolnya tetap berfungsi normal,
// karena listener fungsi aslinya dipasang controller dengan cara/waktu yang beda).
//
// OnEnable() dijamin kepanggil ULANG setiap kali GameObject ini diaktifkan (baik pertama kali
// maupun tiap panel dibuka lagi), jadi listener SELALU kepasang. OnDisable() pasangannya -
// lepas listener tiap panel ditutup, biar pas dibuka lagi gak numpuk listener dobel.
//
// JANGAN tempel ini ke tombol Memasak (btnMasak di CookingController) - itu udah punya audio
// sendiri (klipSuaraMemasak, dimainkan lewat AudioSource-nya sendiri di ProsesMasak()).
// --- 
[RequireComponent(typeof(Button))]
public class TombolSfx : MonoBehaviour
{
    private Button tombol;

    void OnEnable()
    {
        if (tombol == null) tombol = GetComponent<Button>();
        if (tombol == null) return;

        tombol.onClick.RemoveListener(MainkanSfx); // jaga-jaga biar gak dobel kalau somehow udah kepasang
        tombol.onClick.AddListener(MainkanSfx);
    }

    void OnDisable()
    {
        if (tombol != null) tombol.onClick.RemoveListener(MainkanSfx);
    }

    void MainkanSfx()
    {
        if (UISfxManager.Instance != null) UISfxManager.Instance.MainkanKlik();
    }
}