using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// --- Fitur Mandi (v2, pola sama kayak BedController): objek ini PASIF, gak punya deteksi
// klik sendiri lagi. Dipanggil DARI LUAR oleh PlayerController.MovePlayer() begitu karakter
// beneran SAMPAI di posisi objek ini (lewatin tangga dulu kalau beda lantai, sama kayak
// Kasur/Kompor/dll). Efeknya: layar fade gelap (lebih lama dari transisi cutscene biasa),
// Sanity nambah, fade balik - karakter TETAP diem begitu udah sampai (gak ada gerakan
// tambahan selama proses fade ini sendiri). ---
public class MandiController : MonoBehaviour
{
    [Header("TUNABLE")]
    public float sanityBertambah = 15f;
    [Tooltip("Durasi fade masuk/keluar - sengaja lebih lama dari transisi cutscene (0.5 detik)")]
    public float durasiFadeMandi = 1f;
    [Tooltip("Berapa lama layar tetap gelap total sebelum fade balik")]
    public float durasiTahanGelap = 2f;

    [Header("Referensi")]
    [Tooltip("Image full-screen KHUSUS buat efek mandi (Image baru, TERPISAH dari Layar Transisi Cutscene)")]
    public Image layarMandi;

    private bool sedangMandi = false;

    void Awake()
    {
        if (layarMandi != null) {
            layarMandi.raycastTarget = false;
            Color c = layarMandi.color;
            c.a = 0f;
            layarMandi.color = c;
        }
    }

    // --- Dipanggil PlayerController.MovePlayer() saat karakter sudah tiba di objek Mandi ini ---
    public void Mandi()
    {
        if (sedangMandi) return;
        StartCoroutine(ProsesMandi());
    }

    IEnumerator ProsesMandi()
    {
        sedangMandi = true;

        // --- Kunci kontrol player selama proses fade berlangsung (karakter udah sampai,
        // tinggal diem di tempat sampai efeknya kelar) ---
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(true);

        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(true);

        // --- Fade ke gelap ---
        if (layarMandi != null) {
            layarMandi.raycastTarget = true;
            float t = 0f;
            while (t < durasiFadeMandi) {
                t += Time.deltaTime;
                Color c = layarMandi.color;
                c.a = Mathf.Lerp(0f, 1f, t / durasiFadeMandi);
                layarMandi.color = c;
                yield return null;
            }
            Color penuh = layarMandi.color; penuh.a = 1f; layarMandi.color = penuh;
        }

        yield return new WaitForSeconds(durasiTahanGelap);

        // --- Efek: Sanity nambah, diterapkan pas layar masih gelap total ---
        if (GameManager.Instance != null) GameManager.Instance.TambahSanity(sanityBertambah);

        // --- Fade balik nampilin scene lagi ---
        if (layarMandi != null) {
            float t = 0f;
            while (t < durasiFadeMandi) {
                t += Time.deltaTime;
                Color c = layarMandi.color;
                c.a = Mathf.Lerp(1f, 0f, t / durasiFadeMandi);
                layarMandi.color = c;
                yield return null;
            }
            Color kosong = layarMandi.color; kosong.a = 0f; layarMandi.color = kosong;
            layarMandi.raycastTarget = false;
        }

        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);
        if (player != null) player.SetMenuStatus(false);

        sedangMandi = false;
    }
}