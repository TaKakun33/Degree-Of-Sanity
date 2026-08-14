using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Tujuan Pintu")]
    [Tooltip("Tarik objek/pintu di lantai tujuan ke kolom ini")]
    public Transform destination;
    
    [Tooltip("Pintu ini akan memindahkan pemain ke lantai berapa?")]
    public int lantaiTujuan = 2; 

    [Header("Audio Efek Pintu")]
    [Tooltip("Komponen AudioSource (bisa ditaruh di object pintu ini)")]
    public AudioSource audioSourcePintu;
    [Tooltip("Sound effect saat naik tangga")]
    public AudioClip klipNaik;
    [Tooltip("Sound effect saat turun tangga")]
    public AudioClip klipTurun;
    [Range(0f, 1f)]
    public float volumeAudio = 0.8f;

    public void UseDoor(GameObject player)
    {
        if (destination != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            
            // --- TAMBAHAN: Deteksi arah lantai untuk menentukan suara ---
            if (pc != null && audioSourcePintu != null)
            {
                if (lantaiTujuan > pc.lantaiSaatIni && klipNaik != null)
                {
                    audioSourcePintu.PlayOneShot(klipNaik, volumeAudio);
                }
                else if (lantaiTujuan < pc.lantaiSaatIni && klipTurun != null)
                {
                    audioSourcePintu.PlayOneShot(klipTurun, volumeAudio);
                }
            }

            // --- Logika perpindahan posisi ---
            float yTujuan = (KonfigurasiLantai.Instance != null)
                ? KonfigurasiLantai.Instance.DapatkanPosisiY(lantaiTujuan)
                : destination.position.y;

            Vector2 posisiBaru = new Vector2(destination.position.x, yTujuan);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.position = posisiBaru;
            } else {
                player.transform.position = new Vector3(posisiBaru.x, posisiBaru.y, player.transform.position.z);
            }

            // Beri tahu sistem bahwa pemain sudah pindah lantai
            if (pc != null) pc.lantaiSaatIni = lantaiTujuan;
        }
        else
        {
            Debug.LogWarning("Tujuan pintu belum diatur di Inspector!");
        }
    }
}