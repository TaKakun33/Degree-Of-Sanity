using System.Collections.Generic;
using UnityEngine;

// --- Tempel di area lantai tiap ruangan (LORONG/DAPUR/KAMAR_ANDREW/KAMAR_ANNA) - Collider2D
// IsTrigger, nutupin seluruh lantai ruangan itu. Begitu Player masuk (gameplay normal), lapor
// ke CeritaManager. TAMBAHAN: sekarang juga nyimpen TITIK BERDIRI Andrew & Anna buat dipakai
// CutsceneUI pas transisi layar hitam + teleport karakter antar-ruangan. ---
public class RuangTrigger : MonoBehaviour
{
    // --- Registry semua ruangan yang ada di scene, diisi otomatis tiap object ini Awake() ---
    public static readonly Dictionary<string, RuangTrigger> semuaRuang = new Dictionary<string, RuangTrigger>();

    [Tooltip("ID ruang ini - HARUS SAMA PERSIS sama yang diisi di 'Ruang Syarat' pada Peristiwa Terjadwal DAN 'Ruang Id' pada CutsceneScene")]
    public string ruangId;

    [Header("Titik Berdiri Pas Cutscene")]
    [Tooltip("WAJIB diisi: titik tempat Andrew muncul pas cutscene di ruangan ini")]
    public Transform titikAndrew;
    [Tooltip("Opsional: titik tempat Anna muncul, kalau adegan itu 'Karakter Anna Hadir' dicentang")]
    public Transform titikAnna;

    void Awake()
    {
        if (!string.IsNullOrEmpty(ruangId)) {
            semuaRuang[ruangId] = this;
        }
    }

    void OnTriggerEnter2D(Collider2D lain)
    {
        if (!lain.CompareTag("Player")) return;
        if (CeritaManager.Instance != null) CeritaManager.Instance.CobaMulaiMasukRuangan(ruangId);
    }
}