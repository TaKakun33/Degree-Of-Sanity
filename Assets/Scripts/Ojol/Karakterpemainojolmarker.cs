using UnityEngine;

// --- Tempel di KarakterPemainOjol (yang punya Collider2D + Rigidbody2D) ---
// Cuma penanda identitas, dipakai RintanganOjol buat mastiin collider yang nabrak itu bener-bener si pemain.
// Sekalian auto-sync ukuran Box Collider 2D ke RectTransform, biar gak perlu samain angka manual.
public class KarakterPemainOjolMarker : MonoBehaviour
{
    void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (rect != null && collider != null) {
            collider.size = rect.rect.size;
        }
    }
}