using UnityEngine;
using UnityEngine.EventSystems;

// --- Zona Kantong Belanja: cuma nerima barang yang SUDAH dipindai (otomatis lewat area Scanner) ---
public class DaerahKantong : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject objekDijatuhkan = eventData.pointerDrag;
        if (!objekDijatuhkan) return;

        BarangBelanjaan barang = objekDijatuhkan.GetComponent<BarangBelanjaan>();
        if (barang == null || !barang.sudahDipindai || barang.sudahDibungkus) return; // belum discan -> ditolak

        barang.sudahDibungkus = true;
        barang.berhasilDijatuhkanValid = true;

        if (KasirManager.Instance != null) KasirManager.Instance.ItemDimasukkanKantong(barang);

        // --- Barang LANGSUNG HILANG begitu masuk kantong, gak nyangkut kelihatan di layar lagi ---
        Destroy(objekDijatuhkan);
    }
}