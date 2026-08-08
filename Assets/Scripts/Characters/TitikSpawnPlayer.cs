using System.Collections;
using UnityEngine;

// --- Titik Spawn Player - 3 fungsi:
// 1. Posisi awal pemain pas Game Baru
// 2. Posisi pemain muncul lagi begitu balik dari kerja part time (Kasir/Ojol/Tutor)
// 3. Tujuan jalan otomatis pas pemain klik Cancel di Job Menu (lihat JobMenuController.cs)
//
// Taruh di posisi yang kamu mau (biasanya di dalam rumah, deket pintu masuk), gak butuh
// Collider2D apapun - ini murni penanda posisi + sedikit logic. SISTEM BARU, gak nyentuh
// SaveManager.cs/GameManager.cs sama sekali. ---
public class TitikSpawnPlayer : MonoBehaviour
{
    public static TitikSpawnPlayer Instance;

    // --- Dibaca di Awake() (SEBELUM script lain sempat proses/reset flag ini di Start() masing-masing) ---
    private bool baruBalikDariKerja;

    void Awake()
    {
        Instance = this;
        baruBalikDariKerja = HasilKerjaPartTime.adaHasilPending;
    }

    void Start()
    {
        // --- FIX: JANGAN reposisi di sini langsung. Urutan Start() antar GameObject di Unity
        // GAK DIJAMIN - kalau GameManager.Start() (yang muat posisi lama dari save lewat
        // SaveManager.MuatGame()) jalan SETELAH kita, posisi spawn ini bakal ke-TIMPA lagi
        // sama posisi lama. Tunda 1 frame lewat Coroutine - itu DIJAMIN jalan setelah SEMUA
        // Start() (termasuk punya GameManager) di frame pertama selesai duluan. ---
        StartCoroutine(ReposisiSetelahSemuaStartSelesai());
    }

    IEnumerator ReposisiSetelahSemuaStartSelesai()
    {
        yield return null; // tunggu 1 frame - biar semua Start() lain (termasuk GameManager) kelar duluan

        bool gameBaru = SaveManager.slotUntukDiload == -1;

        if (gameBaru || baruBalikDariKerja)
        {
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                Vector3 posisiTujuan = transform.position;
                posisiTujuan.z = player.transform.position.z; // jangan ganggu Z asli player
                player.transform.position = posisiTujuan;

                Debug.Log($"[TitikSpawnPlayer] Player dipindah ke titik spawn: {posisiTujuan} (gameBaru={gameBaru}, baruBalikDariKerja={baruBalikDariKerja})"); // --- SEMENTARA ---
            }
        }
    }

    // --- Dipakai JobMenuController.TutupMenu() buat nyuruh player jalan balik ke sini ---
    public Vector3 PosisiSpawn => transform.position;
}