using UnityEngine;

// Hanya sebagai penanda zona lajur. Deteksi klik ditangani sepenuhnya oleh EventSystem UI di OjolManager.
public class ZonaLaneOjol : MonoBehaviour
{
    [Tooltip("0 = kiri, 1 = tengah, 2 = kanan - HARUS sesuai urutan posisiLane[] di OjolManager")]
    [Range(0, 2)]
    public int laneIndex;
}