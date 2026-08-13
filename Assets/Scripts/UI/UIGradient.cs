using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// --- Gradasi warna VERTIKAL (atas ke bawah) buat UI Graphic manapun (Image/Panel). Unity gak
// punya gradient bawaan buat UI, jadi ini ngubah warna tiap VERTEX mesh-nya langsung - cara
// standar & ringan, gak perlu shader/material tambahan. Tempel di GameObject yang SAMA
// dengan komponen Image/Panel yang mau digradasi. ---
[RequireComponent(typeof(Graphic))]
public class UIGradient : BaseMeshEffect
{
    [Tooltip("Warna di bagian PALING ATAS panel")]
    public Color warnaAtas = new Color(0f, 0f, 0f, 0f); // transparan di atas
    [Tooltip("Warna di bagian PALING BAWAH panel - makin bawah makin gelap")]
    public Color warnaBawah = new Color(0f, 0f, 0f, 0.85f); // gelap pekat di bawah

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        // --- Cari batas atas & bawah mesh, biar gradasinya pas ngikutin ukuran panel beneran ---
        float yMin = float.MaxValue, yMax = float.MinValue;
        foreach (var v in verts) {
            if (v.position.y < yMin) yMin = v.position.y;
            if (v.position.y > yMax) yMax = v.position.y;
        }

        float tinggi = yMax - yMin;

        for (int i = 0; i < verts.Count; i++) {
            UIVertex v = verts[i];
            float t = tinggi > 0f ? (v.position.y - yMin) / tinggi : 0f; // 0 = paling bawah, 1 = paling atas
            v.color = Color.Lerp(warnaBawah, warnaAtas, t);
            verts[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}