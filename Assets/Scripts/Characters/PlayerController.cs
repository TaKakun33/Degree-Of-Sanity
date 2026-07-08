using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isMenuOpen = false; 
    private SpriteRenderer spriteRenderer;

    // --- DAFTAR TARGET OBJEK ---
    private DoorController targetDoor = null;
    private BedController targetBed = null; 
    private DeskController targetDesk = null;
    private ExitDoorController targetExitDoor = null;
    private KomporController targetKompor = null; // Tambahan untuk Kompor

    void Start()
    {
        targetPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        isMenuOpen = false;
        isMoving = false;
        Debug.Log("Player diinisialisasi dalam kondisi normal.");
    }

    void Update()
    {
        // Jika menu terbuka, jangan lakukan apa pun di Update
        if (isMenuOpen) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            HandleClick();
        }

        if (isMoving)
        {
            MovePlayer();
        }
    }

    // Panggil fungsi ini dari UI saat panel buka/tutup
    public void SetMenuStatus(bool status)
    {
        isMenuOpen = status;
        if (isMenuOpen)
        {
            isMoving = false; // Hentikan gerakan saat ini
        }
        Debug.Log("Status Menu: " + (isMenuOpen ? "Terbuka (Kunci)" : "Tertutup (Normal)"));
    }

    void HandleClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        Vector2 clickPos2D = new Vector2(worldPos.x, worldPos.y);

        Debug.Log("Mouse diklik di posisi: " + clickPos2D);

        RaycastHit2D hit = Physics2D.Raycast(clickPos2D, Vector2.zero);

        // Reset semua target sebelum memproses klik baru
        targetDoor = null; targetBed = null; targetDesk = null; targetExitDoor = null; targetKompor = null;

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Door")) targetDoor = hit.collider.GetComponent<DoorController>();
            else if (hit.collider.CompareTag("Bed")) targetBed = hit.collider.GetComponent<BedController>();
            else if (hit.collider.CompareTag("Desk")) targetDesk = hit.collider.GetComponent<DeskController>();
            else if (hit.collider.CompareTag("ExitDoor")) targetExitDoor = hit.collider.GetComponent<ExitDoorController>();
            else if (hit.collider.CompareTag("Kompor")) targetKompor = hit.collider.GetComponent<KomporController>(); // Cek klik ke Kompor
            
            targetPosition = (hit.collider.CompareTag("Door") || hit.collider.CompareTag("Bed") || 
                             hit.collider.CompareTag("Desk") || hit.collider.CompareTag("ExitDoor") || 
                             hit.collider.CompareTag("Kompor")) // Tambahkan tag Kompor di sini
                             ? new Vector2(hit.collider.transform.position.x, transform.position.y) 
                             : new Vector2(worldPos.x, transform.position.y);
        }
        else { targetPosition = new Vector2(worldPos.x, transform.position.y); }

        isMoving = true;
        FlipSprite();
    }

    void FlipSprite()
    {
        if (targetPosition.x != transform.position.x)
            spriteRenderer.flipX = targetPosition.x < transform.position.x;
    }

    void MovePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            
            // Eksekusi fungsi saat karakter sampai di depan objek
            if (targetDoor != null) { targetDoor.UseDoor(gameObject); targetDoor = null; }
            else if (targetBed != null) { targetBed.Tidur(); targetBed = null; }
            else if (targetDesk != null) { targetDesk.MulaiSkripsi(); targetDesk = null; }
            else if (targetExitDoor != null) { targetExitDoor.BukaMenuKerja(); targetExitDoor = null; }
            else if (targetKompor != null) { targetKompor.BukaMenuMasak(); targetKompor = null; } // Buka UI Masak!
        }
    }
}