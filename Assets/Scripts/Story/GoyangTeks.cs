// =============================================================================
// File        : GoyangTeks.cs
// Deskripsi   : Memberi efek getar per-huruf pada TextMeshPro. Dipakai untuk
//               momen panik Andrew atau saat Sanity rendah agar teks terasa
//               tidak stabil. Pasang pada GameObject yang sama dengan TMP_Text.
// Tim         : Gethuk Pisang
// =============================================================================

using UnityEngine;
using TMPro;

namespace DegreeOfSanity.Cerita
{
    [RequireComponent(typeof(TMP_Text))]
    public class GoyangTeks : MonoBehaviour
    {
        [Tooltip("0 = mati. 1-2 getaran halus, 4+ getaran kuat.")]
        public float kekuatan = 0f;

        [Tooltip("Kecepatan getaran.")]
        public float kecepatan = 22f;

        private TMP_Text teks;
        private Mesh mesh;
        private Vector3[] verticesAsli;

        private void Awake()
        {
            teks = GetComponent<TMP_Text>();
        }

        private void LateUpdate()
        {
            if (teks == null) return;

            if (kekuatan <= 0.01f)
            {
                return; // tidak ada efek, biarkan mesh apa adanya
            }

            teks.ForceMeshUpdate();
            mesh = teks.mesh;
            verticesAsli = mesh.vertices;

            TMP_TextInfo info = teks.textInfo;

            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo karakter = info.characterInfo[i];
                if (!karakter.isVisible) continue;

                int indeksVertex = karakter.vertexIndex;

                // Offset acak namun stabil per huruf, dibuat halus dengan sinus.
                float waktu = Time.unscaledTime * kecepatan + i * 0.7f;
                Vector3 offset = new Vector3(
                    Mathf.Sin(waktu) * kekuatan,
                    Mathf.Cos(waktu * 1.3f) * kekuatan,
                    0f);

                for (int j = 0; j < 4; j++)
                {
                    verticesAsli[indeksVertex + j] += offset;
                }
            }

            mesh.vertices = verticesAsli;
            teks.canvasRenderer.SetMesh(mesh);
        }
    }
}