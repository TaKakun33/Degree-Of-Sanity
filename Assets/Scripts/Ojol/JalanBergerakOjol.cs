using UnityEngine;
using UnityEngine.UI;

// --- Efek jalan/road "bergerak" di minigame Ojol - pakai UV SCROLL di RawImage (BUKAN Image
// biasa). Cara ini paling ringan & mulus buat efek "jalan tak berujung" - gak perlu recycle
// banyak sprite manual, cukup geser koordinat UV textur-nya terus-menerus.
//
// WAJIB: texture road di Import Settings HARUS di-set "Wrap Mode" = Repeat, biar nyambung
// mulus pas di-scroll (gak keliatan sambungan/jahitan). ---
[RequireComponent(typeof(RawImage))]
public class JalanBergerakOjol : MonoBehaviour
{
    [Tooltip("Kecepatan scroll jalan - SAMAIN atau sesuaikan sama 'Kecepatan Rintangan' di OjolManager, biar jalan & rintangan keliatan gerak konsisten satu sama lain")]
    public float kecepatanScroll = 200f;

    [Tooltip("Arah scroll - centang kalau arahnya kebalik dari yang kamu mau")]
    public bool balikArah = false;

    private RawImage rawImage;
    private float offsetY = 0f;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        float arah = balikArah ? -1f : 1f;
        // --- Dibagi 1000 biar skala kecepatan (yang satuannya piksel/detik di OjolManager)
        // masuk akal dikonversi ke skala UV (0-1) - sesuaikan pembaginya kalau scroll-nya
        // kerasa terlalu cepat/lambat dibanding gerakan rintangan aslinya ---
        offsetY += arah * (kecepatanScroll / 1000f) * Time.deltaTime;

        Rect uv = rawImage.uvRect;
        uv.y = offsetY;
        rawImage.uvRect = uv;
    }
}