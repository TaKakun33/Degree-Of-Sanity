using UnityEngine;
using UnityEngine.SceneManagement;

// --- Tempel script ini di GameObject "Laptop" (atau object interaksi skripsi) di scene UTAMA ---
// Panggil BukaMinigameSkripsi() dari sistem interaksi kamu (InteractableObject/InteractionManager)
public class PemicuMinigameSkripsi : MonoBehaviour
{
    [Tooltip("Nama scene Minigame Skripsi, HARUS sama persis dengan nama file scene & yang didaftarkan di Build Settings")]
    public string namaSceneMinigame = "MinigameSkripsi";

    public void BukaMinigameSkripsi()
    {
        // Cegah dobel-load kalau tombol/interaksi kepencet 2x sebelum scene selesai load
        if (SceneManager.GetSceneByName(namaSceneMinigame).isLoaded) return;

        SceneManager.LoadScene(namaSceneMinigame, LoadSceneMode.Additive);
    }
}