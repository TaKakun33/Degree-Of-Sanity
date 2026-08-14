using TMPro;
using UnityEngine;
using UnityEngine.UI;

// --- Panel pembayaran Utang Bank. Dibuka lewat Tombol Utang. Nampilin sisa utang + daftar
// SEMUA minggu cicilan (lunas/belum/telat) di dalam Scroll View, tiap baris punya tombol Bayar
// sendiri (nonaktif kalau udah lunas atau uang gak cukup). ---
public class PanelUtangController : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelUtang;
    public TextMeshProUGUI textSisaUtang;

    [Header("Scroll View Daftar Minggu")]
    [Tooltip("Object 'Content' di dalam Scroll View - WAJIB punya Vertical Layout Group + Content Size Fitter")]
    public Transform wadahDaftarMinggu;
    [Tooltip("Prefab 1 baris: Text info minggu + Button Bayar")]
    public GameObject prefabBarisMinggu;

    public Button tombolTutup;

    void Awake()
    {
        if (panelUtang) panelUtang.SetActive(false);
        if (tombolTutup) tombolTutup.onClick.AddListener(Tutup);
    }

    public void Buka()
    {
        // --- FIX: pakai GameManager.BukaUtangAman() - satu sumber kebenaran yang sama dipakai
        // sistem ApakahAdaPanelAktif()/UpdateInteractableTombolPanel(), biar gak ada 2 field
        // panelUtang yang beda-beda kayak sebelumnya (rawan gak sinkron) ---
        if (GameManager.Instance != null) {
            if (GameManager.Instance.ApakahAdaPanelAktif()) return; // udah ada panel lain aktif
            GameManager.Instance.BukaUtangAman();
        } else if (panelUtang) {
            panelUtang.SetActive(true);
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null) player.SetMenuStatus(true);
        }

        BuatDaftarMinggu();
        UpdateSisaUtang();
    }

    public void Tutup()
    {
        if (panelUtang) panelUtang.SetActive(false);

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(false);

        if (GameManager.Instance != null) GameManager.Instance.UpdateInteractableTombolPanel(); // --- TAMBAHAN ---
    }

    void BuatDaftarMinggu()
    {
        if (wadahDaftarMinggu == null) { Debug.LogError("[PanelUtangController] Wadah Daftar Minggu belum diisi!"); return; }
        if (prefabBarisMinggu == null) { Debug.LogError("[PanelUtangController] Prefab Baris Minggu belum diisi!"); return; }
        if (CicilanManager.Instance == null) { Debug.LogError("[PanelUtangController] CicilanManager.Instance NULL - gak ada CicilanManager di scene?"); return; }

        foreach (Transform anak in wadahDaftarMinggu) Destroy(anak.gameObject);

        var daftar = CicilanManager.Instance.daftarMinggu;
        Debug.Log($"[PanelUtangController] Jumlah entri di daftarMinggu: {daftar.Count}"); // --- SEMENTARA ---

        for (int i = 0; i < daftar.Count; i++) {
            int indexLokal = i;
            var entri = daftar[i];

            GameObject baris = Instantiate(prefabBarisMinggu, wadahDaftarMinggu);

            TextMeshProUGUI label = baris.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) {
                string status = entri.sudahDibayar ? "LUNAS" : (entri.sudahTelat ? "TELAT" : "BELUM DIBAYAR");
                label.text = $"Minggu ke-{entri.nomorMinggu} - Rp{entri.nominal:N0} ({status})";
            }

            Button tombolBayar = baris.GetComponentInChildren<Button>();
            if (tombolBayar != null) {
                // --- TAMBAHAN: tombol ini dibuat runtime (Instantiate), gak bisa ditempelin
                // TombolSfx.cs lewat Editor - jadi ditempel lewat kode di sini, sistemnya tetap
                // sama (UISfxManager, terpisah dari AudioManager) ---
                if (tombolBayar.GetComponent<TombolSfx>() == null) tombolBayar.gameObject.AddComponent<TombolSfx>();

                tombolBayar.interactable = !entri.sudahDibayar && GameManager.Instance.uang >= entri.nominal;
                tombolBayar.onClick.AddListener(() => {
                    CicilanManager.Instance.BayarMinggu(indexLokal);
                    BuatDaftarMinggu(); // refresh seluruh daftar (status/interactable berubah)
                    UpdateSisaUtang();
                });
            }
        }
    }

    void UpdateSisaUtang()
    {
        if (GameManager.Instance != null && textSisaUtang != null) {
            textSisaUtang.text = $"Sisa Utang: Rp{Mathf.RoundToInt(GameManager.Instance.utangBank):N0}";
        }
    }
}