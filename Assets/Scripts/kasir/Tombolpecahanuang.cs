using UnityEngine;
using UnityEngine.UI;

// --- Tempel di tiap tombol pecahan uang (Rp 100.000, Rp 50.000, dst) di panel pembayaran ---
public class TombolPecahanUang : MonoBehaviour
{
    [Tooltip("Nilai nominal pecahan uang ini, misal 50000")]
    public int nilaiPecahan = 10000;

    void Start()
    {
        Button tombol = GetComponent<Button>();
        if (tombol) {
            tombol.onClick.AddListener(() => {
                if (KasirManager.Instance != null) KasirManager.Instance.TambahKembalian(nilaiPecahan);
            });
        }
    }
}