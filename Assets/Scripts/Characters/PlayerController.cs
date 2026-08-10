using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public int lantaiSaatIni = 1; // 1 = Lantai Bawah, 2 = Lantai Atas
    
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isMenuOpen = false; 
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // --- Beda dari isMenuOpen - ini cuma blokir KLIK BARU, MovePlayer() TETAP jalan.
    // Dipakai pas karakter "dipaksa" jalan otomatis (JalanKeTitik) dan gak boleh dibelokin klik. ---
    private bool kontrolDikunci = false;

    // --- DAFTAR TARGET OBJEK ---
    private DoorController targetDoor = null;
    private BedController targetBed = null; 
    private DeskController targetDesk = null;
    private ExitDoorController targetExitDoor = null;
    private KomporController targetKompor = null;
    private MandiController targetMandi = null;
    private PemicuInteraktifCerita targetPemicu = null; // --- TAMBAHAN ---

    // --- SISTEM MEMORI LANTAI (WAYPOINT) - dipakai pas klik objek/lantai di LANTAI LAIN,
    // karakter otomatis jalan dulu ke pintu/tangga penghubung sebelum lanjut ke tujuan asli ---
    private bool sedangTransit = false; 
    private GameObject targetInteraksiAkhir = null; 
    private float targetXAkhir = 0f; 

    void Start()
    {
        targetPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        isMenuOpen = false;
        isMoving = false;
    }

    void Update()
    {
        if (isMenuOpen) return;

        if (!kontrolDikunci && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            HandleClick();
        }

        if (isMoving) MovePlayer();
    }

    public void SetMenuStatus(bool status)
    {
        isMenuOpen = status;
        if (isMenuOpen) isMoving = false; 
    }

    // --- Dipakai script lain (CutsceneUI, dll) buat ngecek udah nyampe tujuan belum ---
    public bool SedangBergerak => isMoving;

    // --- Kunci/buka kontrol klik pemain, dipanggil sebelum JalanKeTitik() ---
    public void KunciKontrol(bool kunci)
    {
        kontrolDikunci = kunci;
    }

    // --- Dipanggil script LAIN (bukan klik mouse) buat nyuruh karakter jalan otomatis.
    // Cuma X yang dipakai - Y TETAP ikutin posisi karakter sekarang (gaya side-scroller,
    // sama kayak gimana HandleClick() nanganin klik ke Bed/Desk/Kompor). ---
    public void JalanKeTitik(Vector2 tujuan)
    {
        targetPosition = new Vector2(tujuan.x, transform.position.y);
        isMoving = true;
        FlipSprite();
    }

    // --- Hentikan gerakan yang lagi jalan secara paksa (dipanggil PenghalangKeluar.cs, dll) ---
    public void BerhentiPaksa()
    {
        isMoving = false;
    }

    void HandleClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        Vector2 clickPos2D = new Vector2(worldPos.x, worldPos.y);

        // MENGGUNAKAN "RAYCAST ALL" AGAR SENSOR MOUSE TEMBUS PANDANG (Melewati Area_Lantai)
        RaycastHit2D[] hits = Physics2D.RaycastAll(clickPos2D, Vector2.zero);

        // Bersihkan memori klik sebelumnya
        targetDoor = null; targetBed = null; targetDesk = null; targetExitDoor = null; targetKompor = null;
        targetMandi = null;
        targetPemicu = null; // --- TAMBAHAN ---
        targetInteraksiAkhir = null;
        sedangTransit = false;

        GameObject objekDiklik = null;
        int lantaiTujuan = lantaiSaatIni;

        // PRIORITAS 1: Cari tahu apakah di titik yang diklik terdapat Barang Interaktif (Tembus kotak transparan)
        foreach (var hit in hits)
        {
            if (hit.collider != null && CekApakahBarangInteraktif(hit.collider.gameObject))
            {
                objekDiklik = hit.collider.gameObject;
                break; // Ketemu barangnya, langsung kunci target!
            }
        }

        // PRIORITAS 2: Jika terbukti tidak ada barang sama sekali, baru kita anggap itu klik Area Lantai biasa
        if (objekDiklik == null)
        {
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.GetComponent<LantaiInfo>() != null)
                {
                    objekDiklik = hit.collider.gameObject;
                    break;
                }
            }
        }

        // Ambil data lantai dari objek yang akhirnya terpilih
        if (objekDiklik != null)
        {
            LantaiInfo info = objekDiklik.GetComponent<LantaiInfo>();
            if (info != null) lantaiTujuan = info.nomorLantai;
        }
        
        // ==============================================================
        // LOGIKA PERGERAKAN (TRANSIT & SATU LANTAI)
        // ==============================================================

        // JIKA BARANG ATAU LANTAI ADA DI LANTAI YANG BERBEDA -> cari pintu/tangga penghubung
        if (lantaiTujuan != lantaiSaatIni)
        {
            DoorController pintuPenghubung = CariPintuKeLantai(lantaiTujuan);
            if (pintuPenghubung != null)
            {
                targetDoor = pintuPenghubung; 
                targetPosition = new Vector2(pintuPenghubung.transform.position.x, transform.position.y);
                
                sedangTransit = true;
                targetXAkhir = clickPos2D.x; // Ingat titik X akhir 
                
                if (objekDiklik != null && CekApakahBarangInteraktif(objekDiklik))
                {
                    // PENGAMAN: Jika yang diklik di atas adalah Pintu, jangan disetel sebagai target interaksi
                    // agar karakter tidak otomatis masuk pintu lagi dan mantul kembali ke lantai bawah.
                    if (objekDiklik.CompareTag("Door")) targetInteraksiAkhir = null; 
                    else targetInteraksiAkhir = objekDiklik; 
                }

                isMoving = true;
                FlipSprite();
                return; // Berangkat ke pintu sekarang!
            }
        }

        // JIKA SATU LANTAI (Atau tidak ada info lantai sama sekali)
        if (objekDiklik != null && CekApakahBarangInteraktif(objekDiklik))
        {
            SetTargetInteraksi(objekDiklik);
            targetPosition = new Vector2(objekDiklik.transform.position.x, transform.position.y);
        }
        else 
        { 
            targetPosition = new Vector2(clickPos2D.x, transform.position.y); 
        }

        isMoving = true;
        FlipSprite();
    }

    // Fungsi deteksi objek dengan tag
    bool CekApakahBarangInteraktif(GameObject obj)
    {
        return obj.CompareTag("Door") || obj.CompareTag("Bed") || 
               obj.CompareTag("Desk") || obj.CompareTag("ExitDoor") || 
               obj.CompareTag("Kompor") || obj.CompareTag("Mandi") ||
               obj.CompareTag("ObjekCerita"); // --- TAMBAHAN ---
    }

    // Fungsi pencari rute tangga - cari DoorController di lantai SAAT INI yang lantaiTujuan-nya cocok
    DoorController CariPintuKeLantai(int lantaiTujuan)
    {
        DoorController[] semuaPintu = Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);
        foreach (var pintu in semuaPintu)
        {
            LantaiInfo infoPintu = pintu.GetComponent<LantaiInfo>();
            if (infoPintu != null && infoPintu.nomorLantai == lantaiSaatIni && pintu.lantaiTujuan == lantaiTujuan)
            {
                return pintu;
            }
        }
        return null;
    }

    // Fungsi untuk menyambungkan script objek ke memori player
    void SetTargetInteraksi(GameObject obj)
    {
        if (obj.CompareTag("Door")) targetDoor = obj.GetComponent<DoorController>();
        else if (obj.CompareTag("Bed")) targetBed = obj.GetComponent<BedController>();
        else if (obj.CompareTag("Desk")) targetDesk = obj.GetComponent<DeskController>();
        else if (obj.CompareTag("ExitDoor")) targetExitDoor = obj.GetComponent<ExitDoorController>();
        else if (obj.CompareTag("Kompor")) targetKompor = obj.GetComponent<KomporController>();
        else if (obj.CompareTag("Mandi")) targetMandi = obj.GetComponent<MandiController>();
        else if (obj.CompareTag("ObjekCerita")) targetPemicu = obj.GetComponent<PemicuInteraktifCerita>(); // --- TAMBAHAN ---
    }

    void FlipSprite()
    {
        if (targetPosition.x != transform.position.x)
            spriteRenderer.flipX = targetPosition.x < transform.position.x;
    }

    void MovePlayer()
    {
        Vector2 posisiSebelum = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 posisiBaru = Vector2.MoveTowards(posisiSebelum, targetPosition, speed * Time.deltaTime);

        // Arah hadap dihitung dari pergerakan AKTUAL tiap frame, bukan snapshot sekali doang
        if (posisiBaru.x != posisiSebelum.x) {
            spriteRenderer.flipX = posisiBaru.x < posisiSebelum.x;
        }

        // Pakai rb.MovePosition() (physics-aware) - tetap ke-block collider solid, gak gemeteran
        if (rb != null) {
            rb.MovePosition(posisiBaru);
        } else {
            transform.position = posisiBaru;
        }
        
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            // JIKA SAMPAI DI DEPAN PINTU/TANGGA
            if (targetDoor != null) 
            { 
                targetDoor.UseDoor(gameObject); 
                targetDoor = null; 
                
                // Cek jika habis keluar pintu, apakah masih harus jalan ke kasur/kompor/dll?
                if (sedangTransit)
                {
                    sedangTransit = false; 
                    
                    if (targetInteraksiAkhir != null)
                    {
                        SetTargetInteraksi(targetInteraksiAkhir);
                        targetPosition = new Vector2(targetInteraksiAkhir.transform.position.x, transform.position.y);
                    }
                    else
                    {
                        targetPosition = new Vector2(targetXAkhir, transform.position.y);
                    }
                    
                    targetInteraksiAkhir = null; 
                    FlipSprite(); 
                    return; // Lanjut jalan ke lantai baru!
                }
            }
            // EKSEKUSI JIKA SAMPAI DI BARANG BUKAN PINTU
            else if (targetBed != null) { targetBed.Tidur(); targetBed = null; }
            else if (targetDesk != null) { targetDesk.MulaiSkripsi(); targetDesk = null; }
            else if (targetExitDoor != null) { targetExitDoor.BukaMenuKerja(); targetExitDoor = null; }
            else if (targetKompor != null) { targetKompor.BukaMenuMasak(); targetKompor = null; }
            else if (targetMandi != null) { targetMandi.Mandi(); targetMandi = null; }
            else if (targetPemicu != null) { targetPemicu.Sampai(); targetPemicu = null; } // --- TAMBAHAN ---

            // Karakter sampai di tujuan akhir, berhenti jalan.
            isMoving = false;
            kontrolDikunci = false; // otomatis buka kunci begitu sampai tujuan
        }
    }
}