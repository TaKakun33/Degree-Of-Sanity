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
    public float sanityBertambah = 10f;
    [Tooltip("TAMBAHAN: berapa jam waktu in-game yang kelewat selama mandi")]
    public float jamYangDilewati = 1f;
    [Tooltip("Durasi fade masuk/keluar - sengaja lebih lama dari transisi cutscene (0.5 detik)")]
    public float durasiFadeMandi = 1f;
    [Tooltip("Berapa lama layar tetap gelap total sebelum fade balik")]
    public float durasiTahanGelap = 2f;

    [Header("Referensi")]
    [Tooltip("Image full-screen KHUSUS buat efek mandi (Image baru, TERPISAH dari Layar Transisi Cutscene)")]
    public Image layarMandi;

    // --- TAMBAHAN: Variabel untuk Audio Efek Mandi ---
    [Header("Audio Efek Mandi")]
    [Tooltip("Drag AudioSource ke sini (bisa dari GameObject ini)")]
    public AudioSource audioSourceMandi;
    [Tooltip("Drag file suara mandi (seperti air mengalir/shower) ke sini")]
    public AudioClip klipSuaraMandi;
    [Range(0f, 1f)]
    public float volumeMandi = 0.8f;

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

    // --- TAMBAHAN: Pastikan suara berhenti jika objek mendadak mati (seperti di panel masak) ---
    void OnDisable()
    {
        if (audioSourceMandi != null) audioSourceMandi.Stop();
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

        // --- Kunci kontrol player selama proses fade berlangsung ---
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMenuStatus(true);

        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(true);

        // --- TAMBAHAN: Mulai mainkan suara mandi secara berulang (loop) ---
        if (audioSourceMandi != null && klipSuaraMandi != null) {
            audioSourceMandi.clip = klipSuaraMandi;
            audioSourceMandi.loop = true; // Supaya suaranya berulang selama layar gelap
            audioSourceMandi.volume = volumeMandi;
            audioSourceMandi.Play();
        }

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

        // --- Efek: Sanity nambah, TAPI cuma SEKALI per hari ---
        if (GameManager.Instance != null && !GameManager.Instance.SudahMandiHariIni) {
            GameManager.Instance.TambahSanity(sanityBertambah);
            GameManager.Instance.TandaiSudahMandiHariIni();
        }

        // --- TAMBAHAN: waktu tetap kelewat SETIAP kali mandi ---
        if (GameManager.Instance != null) GameManager.Instance.jamSaatIni += jamYangDilewati;

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

        // --- TAMBAHAN: Berhentikan suara mandi begitu adegan/fade selesai ---
        if (audioSourceMandi != null) {
            audioSourceMandi.Stop();
        }

        sedangMandi = false;
    }
}