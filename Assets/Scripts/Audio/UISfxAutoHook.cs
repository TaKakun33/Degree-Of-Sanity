using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// --- Versi UPGRADE: Otomatis mendeteksi tombol yang baru dibuat (Instantiate) 
// di tengah permainan tanpa perlu menambah kode manual di script lain! ---
public class UISfxAutoHook : MonoBehaviour
{
    [Header("Tombol Pengecualian")]
    [Tooltip("Masukkan tombol yang TIDAK BOLEH dikasih suara otomatis ini (misal: tombol Masak)")]
    public List<Button> pengecualian = new List<Button>();

    [Header("Pengaturan Auto-Scan")]
    [Tooltip("Sistem akan mengecek tombol baru setiap sekian detik (0.5 atau 1 detik sangat aman)")]
    public float intervalScan = 1f;

    void Start()
    {
        // 1. Pasang suara ke semua tombol yang sudah ada sejak awal
        PasangSfxKeSemuaTombol();

        // 2. Mulai fitur Auto-Scan! Dia akan mengulang fungsi secara otomatis tiap sekian detik
        InvokeRepeating(nameof(PasangSfxKeSemuaTombol), intervalScan, intervalScan);
    }

    void PasangSfxKeSemuaTombol()
    {
        // Ambil semua tombol, termasuk yang panelnya sedang tertutup/nonaktif
        Button[] semuaTombol = GetComponentsInChildren<Button>(true);

        foreach (Button tombol in semuaTombol)
        {
            if (tombol == null) continue;
            
            // Jika tombol ini masuk daftar pengecualian, lewati!
            if (pengecualian.Contains(tombol)) continue;
            
            // Jika tombol sudah punya script suara (TombolSfx), lewati agar tidak dobel
            if (tombol.GetComponent<TombolSfx>() != null) continue;

            // Jika tombol baru dan aman, pasangkan script suara secara otomatis
            tombol.gameObject.AddComponent<TombolSfx>();
        }
    }
}