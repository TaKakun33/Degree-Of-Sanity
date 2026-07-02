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

        // zoom ke arah kursir
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            targetZoom -= Mathf.Sign(scroll) * zoomStep;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Simpan posisi dunia kursor SEBELUM ukuran kamera diubah
        Vector3 mouseWorldPosBefore = cam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -transform.position.z));
        
        // Terapkan efek zoom secara mulus ke kamera
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
        
        // Simpan posisi dunia kursor SETELAH ukuran kamera diubah
        Vector3 mouseWorldPosAfter = cam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -transform.position.z));
        
        // Geser target posisi kamera untuk mengimbangi perbedaan jarak, 
        // sehingga kursor tetap berada di titik yang sama di dunia game.
        targetPosition += (mouseWorldPosBefore - mouseWorldPosAfter);


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
        // Menghitung batas ukuran layar berdasarkan zoom
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

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