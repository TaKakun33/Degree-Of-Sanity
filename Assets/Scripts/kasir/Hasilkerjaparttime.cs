// --- Jembatan data statis untuk hasil kerja part time (Kasir, Ojek, Tutor, dst) ---
// Dipakai karena GameManager TIDAK persistent antar scene (Awake() sengaja tidak DontDestroyOnLoad,
// biar fitur Restart tetap berfungsi). Jadi kalau minigame kerja ada di SCENE TERPISAH (Single load,
// bukan Additive), hasilnya harus "dititipkan" di sini dulu sebelum scene utama dimuat ulang.
public static class HasilKerjaPartTime
{
    public static bool adaHasilPending = false;
    public static int uangDidapat = 0;
    public static float laparBerkurang = 0f;
    public static float sanityBerkurang = 0f;
    public static float jamYangDilewati = 0f;

    public static void SimpanHasil(int uang, float lapar, float sanity, float jam)
    {
        uangDidapat = uang;
        laparBerkurang = lapar;
        sanityBerkurang = sanity;
        jamYangDilewati = jam;
        adaHasilPending = true;
    }

    public static void Bersihkan()
    {
        adaHasilPending = false;
        uangDidapat = 0;
        laparBerkurang = 0f;
        sanityBerkurang = 0f;
        jamYangDilewati = 0f;
    }
}