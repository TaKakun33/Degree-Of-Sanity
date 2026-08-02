using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minZoom = 3f;
    public float maxZoom = 8f;
    public float zoomStep = 1f;
    public float zoomSmoothTime = 0.1f;

    [Header("Camera Bounds (Batas Peta)")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;
    private float targetZoom;
    private float zoomVelocity = 0f;
    private Vector3 targetPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
        targetPosition = transform.position;
    }

    // untuk bergerak setelah semua objek lain selesai bergerak 
    void LateUpdate()
    {
        HandleZoomAndPan();
        ClampCamera();
        
        // Menggerakkan posisi kamera ke targetPosition dengan halus
        transform.position = Vector3.Lerp(transform.position, targetPosition, 15f * Time.deltaTime);
    }

    void HandleZoomAndPan()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // zoom ke arah kursor
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            // --- FIX: hitung offset kursor dari TENGAH LAYAR pakai koordinat viewport (0-1),
            // BUKAN cam.ScreenToWorldPoint(). ScreenToWorldPoint bergantung ke transform.position
            // AKTUAL kamera, yang lagi "ngejar" targetPosition lewat Lerp terpisah di LateUpdate().
            // Dua smoothing (SmoothDamp buat zoom, Lerp buat posisi) yang gak sinkron itu yang
            // bikin gambar bergetar tiap kali di-recompute ulang tiap frame - sekarang cuma
            // dihitung SEKALI per input scroll, pakai nilai target (bukan nilai yang lagi lerping). ---
            Vector2 viewportPos = cam.ScreenToViewportPoint(mouseScreenPos);
            Vector2 offsetDariTengah = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);

            float zoomSebelum = targetZoom;
            targetZoom -= Mathf.Sign(scroll) * zoomStep;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            float zoomSesudah = targetZoom;

            // Ukuran dunia yang kelihatan di layar berubah proporsional sama orthographicSize
            float tinggiSebelum = zoomSebelum * 2f;
            float tinggiSesudah = zoomSesudah * 2f;
            float lebarSebelum = tinggiSebelum * cam.aspect;
            float lebarSesudah = tinggiSesudah * cam.aspect;

            Vector2 posisiDuniaSebelum = new Vector2(offsetDariTengah.x * lebarSebelum, offsetDariTengah.y * tinggiSebelum);
            Vector2 posisiDuniaSesudah = new Vector2(offsetDariTengah.x * lebarSesudah, offsetDariTengah.y * tinggiSesudah);

            // Geser targetPosition sebesar selisihnya, biar titik yang ada di bawah kursor tetap di tempat yang sama
            targetPosition.x += posisiDuniaSebelum.x - posisiDuniaSesudah.x;
            targetPosition.y += posisiDuniaSebelum.y - posisiDuniaSesudah.y;
        }

        // Terapkan efek zoom secara mulus ke kamera (setelah kompensasi posisi dihitung di atas)
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);

        // Geser kitika klik kanan
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            // Rumus ini membuat pergeseran mouse 1:1 dengan dunia game, 
            // artinya kecepatan geser akan otomatis menyesuaikan tingkat zoom saat ini.
            float panSensitivity = (cam.orthographicSize * 2f) / Screen.height;
            
            targetPosition.x -= mouseDelta.x * panSensitivity;
            targetPosition.y -= mouseDelta.y * panSensitivity;
        }
    }

    void ClampCamera()
    {
        // --- FIX: pakai targetZoom (nilai TUJUAN akhir, stabil), bukan cam.orthographicSize
        // (nilai AKTUAL yang lagi berubah tiap frame selama transisi SmoothDamp). Kalau pakai
        // orthographicSize, batas peta ikut goyang tiap frame selama proses zoom berlangsung,
        // dan targetPosition bisa ke-clamp bolak-balik di situ - itu sumber getaran kedua. ---
        float camHeight = targetZoom;
        float camWidth = targetZoom * cam.aspect;

        float minX = minBounds.x + camWidth;
        float maxX = maxBounds.x - camWidth;
        float minY = minBounds.y + camHeight;
        float maxY = maxBounds.y - camHeight;

        // Mencegah error jika ruangan/batas peta terlalu sempit
        if (minX > maxX) minX = maxX = (minBounds.x + maxBounds.x) / 2f;
        if (minY > maxY) minY = maxY = (minBounds.y + maxBounds.y) / 2f;

        // Kunci target posisi agar tidak melewati batas
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        targetPosition.z = -10f; // Jaga kamera agar tetap berada di belakang (2D layer)
    }
}