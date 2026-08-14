using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using System.Collections;

public class JobMenuController : MonoBehaviour
{
    [Header("Pengaturan Scene Kerja Part Time")]
    public string sceneKasir = "KasirScene";
    public string sceneOjol = "OjolScene";
    public string sceneTutor = "TutorScene";

    [Header("Referensi Player")]
    [Tooltip("Tarik objek Player Anda ke sini")]
    public PlayerController player; 

    [Header("TAMBAHAN: Pintu Keluar & Titik Berhenti")]
    [Tooltip("Drag object PintuRuangan yang mewakili pintu keluar rumah ini - dibuka paksa begitu pemain milih salah satu kerja")]
    public PintuRuangan pintuExit;
    [Tooltip("Drag object Zona_StopKerja LANGSUNG ke sini - player bakal jalan lurus ke posisi object ini, sama kayak klik Bed/Kompor")]
    public Transform zonaStopKerja;

    [Header("Transisi Pindah Scene")]
    [Tooltip("Drag Image full-screen warna hitam ke sini, alpha awalnya 0")]
    public Image layarTransisi;
    public float durasiFade = 0.5f;

    // --- Nyimpen scene mana yang dituju, dipakai nanti pas player beneran nyampe di luar (lihat ZonaStopKerja.cs) ---
    private string sceneTujuanBerikutnya;

    // Panggil fungsi ini saat tombol kerja diklik
    public void PilihKasir()
    {
        sceneTujuanBerikutnya = sceneKasir;
        MulaiKeluarUntukKerja();
    }

    public void PilihOjekOnline()
    {
        sceneTujuanBerikutnya = sceneOjol;
        MulaiKeluarUntukKerja();
    }

    public void PilihHometutor()
    {
        sceneTujuanBerikutnya = sceneTutor;
        MulaiKeluarUntukKerja();
    }

    // --- TAMBAHAN: dipanggil ketiga tombol kerja di atas. Tutup menu, buka pintu, jalanin
    // player ke Zona_StopKerja. SCENE BELUM dimuat di sini - baru dimuat begitu player beneran
    // nyampe (lihat LanjutkanKeSceneKerja() di bawah, dipanggil ZonaStopKerja.cs). ---
    void MulaiKeluarUntukKerja()
    {
        Debug.Log($"[JobMenuController] MulaiKeluarUntukKerja() TERPANGGIL. Tujuan: {sceneTujuanBerikutnya}"); // --- SEMENTARA ---

        gameObject.SetActive(false); // tutup panel job menu

        if (GameManager.Instance != null) {
            GameManager.Instance.SetJedaWaktu(false); // waktu jalan lagi selagi player jalan keluar
            // --- PENTING: TandaiKerjaPartTimeSudahDilakukan() PINDAH ke LanjutkanKeSceneKerja() -
            // kalau ditandai DI SINI, PenghalangKeluar bakal langsung aktif dan malah ngeblok
            // perjalanan PERTAMA yang lagi sah ini juga. ---
        }

        if (pintuExit != null) {
            pintuExit.BukaOtomatis(); // paksa buka pintunya
        } else {
            Debug.LogWarning("[JobMenuController] Pintu Exit belum diisi - pintu gak akan kebuka otomatis."); // --- SEMENTARA ---
        }

        if (player == null) {
            Debug.LogError("[JobMenuController] Player belum diisi! Karakter gak akan pernah jalan."); // --- SEMENTARA ---
            return;
        }

        if (zonaStopKerja == null) {
            Debug.LogError("[JobMenuController] Zona Stop Kerja belum diisi! Karakter gak akan pernah jalan."); // --- SEMENTARA ---
            return;
        }

        player.SetMenuStatus(false); // biar MovePlayer() jalan lagi
        player.KunciKontrol(true);   // klik pemain diblokir, gak bisa dibelokin
        player.JalanKeTitik(zonaStopKerja.position);

        Debug.Log($"[JobMenuController] player.JalanKeTitik() dipanggil, tujuan posisi: {zonaStopKerja.position}"); // --- SEMENTARA ---
    }

    // --- TAMBAHAN: dipanggil ZonaStopKerja.cs begitu player beneran nyampe di luar rumah ---
    public void LanjutkanKeSceneKerja()
    {
        Debug.Log("[JobMenuController] LanjutkanKeSceneKerja() TERPANGGIL - player udah nyampe."); // --- SEMENTARA ---

        if (string.IsNullOrEmpty(sceneTujuanBerikutnya)) return;

        if (player != null) player.KunciKontrol(false); // buka lagi, jaga-jaga (scene mau ganti sebentar lagi)

        // --- TAMBAHAN: jatah harian ditandai DI SINI (bukan di awal), setelah player beneran
        // nyampe & pasti berangkat kerja - biar PenghalangKeluar gak ngeblok perjalanan ini sendiri ---
        if (GameManager.Instance != null) {
            GameManager.Instance.TandaiKerjaPartTimeSudahDilakukan();
            
            // --- FIX: Titipkan eksekusi Coroutine ke GameManager karena Panel_MenuKerja saat ini sedang mati (inactive) ---
            GameManager.Instance.StartCoroutine(ProsesPindahScene());
        }
    }

    private IEnumerator ProsesPindahScene()
    {
        // 1. Lakukan Fade out ke layar hitam
        if (layarTransisi != null)
        {
            layarTransisi.gameObject.SetActive(true);
            layarTransisi.raycastTarget = true; // Bmbali JobMenulok klik pemain saat fade berlangsung

            float t = 0f;
            while (t < durasiFade)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / durasiFade);
                Color c = layarTransisi.color;
                c.a = alpha;
                layarTransisi.color = c;
                yield return null; // Tunggu frame berikutnya
            }

            // Pastikan alpha benar-benar mentok di 1 (hitam pekat)
            Color akhir = layarTransisi.color;
            akhir.a = 1f;
            layarTransisi.color = akhir;
        }
        else
        {
            Debug.LogWarning("[JobMenuController] Layar Transisi belum diisi! Lompat langsung ke scene.");
        }

        // 2. PENTING: Autosave saat layar sudah gelap
        if (SaveManager.Instance != null) {
            SaveManager.Instance.SimpanGame(0);
            SaveManager.slotUntukDiload = 0;
        }

        // 3. Pindah ke Scene Kerja
        SceneManager.LoadScene(sceneTujuanBerikutnya);
        sceneTujuanBerikutnya = null;
    }

    // Dipanggil saat tombol Batal/Tutup diklik - pintu TETAP tertutup, tapi player DIJALANIN balik
    // ke titik spawn (biar gak bisa nongkrong di depan pintu terus keluar tanpa milih kerja lagi)
    public void TutupMenu() 
    { 
        gameObject.SetActive(false); 
        if (player != null)
        {
            player.SetMenuStatus(false);

            // --- TAMBAHAN: jalanin player balik ke titik spawn ---
            if (TitikSpawnPlayer.Instance != null) {
                player.KunciKontrol(true);
                player.JalanKeTitik(TitikSpawnPlayer.Instance.PosisiSpawn);
            }
        }
        else
        {
            Debug.LogError("Referensi player hilang di JobMenuController!");
        }

        // --- KEMBALIKAN WAKTU HARIAN ---
        if (GameManager.Instance != null) GameManager.Instance.SetJedaWaktu(false);
    }
}