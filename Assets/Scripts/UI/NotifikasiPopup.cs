using System.Collections;
using TMPro;
using UnityEngine;

// --- Popup notifikasi singkat yang bisa dipanggil dari script MANAPUN (skripsi, part time, dll) ---
// Tempel di GameObject yang SELALU AKTIF (misal Canvas itu sendiri, atau Empty GameObject terpisah) -
// JANGAN ditempel di panel visual yang statusnya nonaktif dari awal, biar Awake()-nya gak ketunda
// (kalau GameObject-nya sendiri nonaktif dari load scene, Awake() baru jalan pas PERTAMA kali aktif,
// dan Instance bakal null terus sebelum itu - lihat referensi "panelVisual" di bawah buat solusinya).
public class NotifikasiPopup : MonoBehaviour
{
    public static NotifikasiPopup Instance;

    [Header("Referensi UI")]
    [Tooltip("Panel VISUAL (kotak + teks) yang ditampilkan/disembunyikan - BUKAN GameObject script ini sendiri")]
    public GameObject panelVisual;
    public TextMeshProUGUI textNotifikasi;
    [Tooltip("Berapa detik popup tampil sebelum otomatis hilang lagi, kalau gak diisi manual saat Tampilkan()")]
    public float durasiTampilDefault = 2.5f;

    private Coroutine coroutineAktif;

    void Awake()
    {
        Instance = this;
        if (panelVisual) panelVisual.SetActive(false);
    }

    public void Tampilkan(string pesan)
    {
        Tampilkan(pesan, durasiTampilDefault);
    }

    public void Tampilkan(string pesan, float durasi)
    {
        if (panelVisual == null) {
            Debug.Log(pesan); // jaga-jaga kalau panelVisual belum di-assign
            return;
        }

        // Kalau popup lagi tampil dan dipanggil lagi, restart timernya dari awal (bukan numpuk coroutine)
        if (coroutineAktif != null) StopCoroutine(coroutineAktif);
        coroutineAktif = StartCoroutine(TampilkanSementara(pesan, durasi));
    }

    IEnumerator TampilkanSementara(string pesan, float durasi)
    {
        if (textNotifikasi) textNotifikasi.text = pesan;
        panelVisual.SetActive(true);

        yield return new WaitForSeconds(durasi);

        panelVisual.SetActive(false);
        coroutineAktif = null;
    }
}