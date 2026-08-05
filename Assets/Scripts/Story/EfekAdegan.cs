// =============================================================================
// File        : EfekAdegan.cs
// Deskripsi   : Menangani seluruh efek sinematik pada scene cutscene: fade
//               hitam, crossfade latar belakang, kilat putih, getaran layar,
//               zoom, serta hook distorsi visual saat Sanity rendah.
// Tim         : Gethuk Pisang
// =============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DegreeOfSanity.Cerita
{
    public class EfekAdegan : MonoBehaviour
    {
        [Header("Latar Belakang (dua layer untuk crossfade)")]
        [SerializeField] private Image latarUtama;
        [SerializeField] private Image latarTransisi;

        [Header("Overlay")]
        [SerializeField] private CanvasGroup tiraiHitam;
        [SerializeField] private CanvasGroup kilatPutih;
        [Tooltip("Overlay vignette / noise merah untuk kondisi Sanity rendah.")]
        [SerializeField] private CanvasGroup overlayDistorsi;

        [Header("Wadah yang Digetarkan")]
        [Tooltip("Biasanya RectTransform 'WadahAdegan' yang memuat latar + karakter.")]
        [SerializeField] private RectTransform wadahAdegan;

        [Header("Parameter Efek")]
        [SerializeField] private float kekuatanGetarRingan = 8f;
        [SerializeField] private float kekuatanGetarKuat = 26f;
        [SerializeField] private float durasiGetar = 0.4f;
        [SerializeField] private float durasiKilat = 0.35f;
        [SerializeField] private float skalaZoom = 1.08f;
        [SerializeField] private float durasiZoom = 1.0f;

        private Vector2 posisiAsliWadah;
        private Coroutine rutinGetar;
        private Coroutine rutinZoom;

        private void Awake()
        {
            if (wadahAdegan != null) posisiAsliWadah = wadahAdegan.anchoredPosition;
            if (kilatPutih != null) kilatPutih.alpha = 0f;
            if (overlayDistorsi != null) overlayDistorsi.alpha = 0f;
            if (latarTransisi != null)
            {
                Color c = latarTransisi.color;
                c.a = 0f;
                latarTransisi.color = c;
            }
        }

        // ---------------------------------------------------------------------
        // FADE LAYAR
        // ---------------------------------------------------------------------

        public void SetGelapSeketika(bool gelap)
        {
            if (tiraiHitam == null) return;
            tiraiHitam.alpha = gelap ? 1f : 0f;
            tiraiHitam.blocksRaycasts = gelap;
        }

        public IEnumerator FadeKeGelap(float durasi)
        {
            yield return FadeTirai(1f, durasi);
        }

        public IEnumerator FadeKeTerang(float durasi)
        {
            yield return FadeTirai(0f, durasi);
        }

        private IEnumerator FadeTirai(float target, float durasi)
        {
            if (tiraiHitam == null) yield break;

            float awal = tiraiHitam.alpha;
            float t = 0f;
            tiraiHitam.blocksRaycasts = true;

            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                tiraiHitam.alpha = Mathf.Lerp(awal, target, t / durasi);
                yield return null;
            }

            tiraiHitam.alpha = target;
            tiraiHitam.blocksRaycasts = target > 0.9f;
        }

        // ---------------------------------------------------------------------
        // LATAR BELAKANG
        // ---------------------------------------------------------------------

        public void SetLatarSeketika(Sprite sprite)
        {
            if (latarUtama == null || sprite == null) return;
            latarUtama.sprite = sprite;
            latarUtama.color = Color.white;

            if (latarTransisi != null)
            {
                Color c = latarTransisi.color;
                c.a = 0f;
                latarTransisi.color = c;
            }
        }

        public IEnumerator GantiLatar(Sprite spriteBaru, TransisiLatar tipe, float durasi)
        {
            if (latarUtama == null || spriteBaru == null) yield break;
            if (latarUtama.sprite == spriteBaru) yield break;

            switch (tipe)
            {
                case TransisiLatar.Langsung:
                    SetLatarSeketika(spriteBaru);
                    break;

                case TransisiLatar.Fade:
                    yield return CrossfadeLatar(spriteBaru, durasi);
                    break;

                case TransisiLatar.FadeHitam:
                    yield return FadeKeGelap(durasi * 0.5f);
                    SetLatarSeketika(spriteBaru);
                    yield return FadeKeTerang(durasi * 0.5f);
                    break;

                case TransisiLatar.FadePutih:
                    yield return Kilat(durasi * 0.4f, tahanDiPuncak: true);
                    SetLatarSeketika(spriteBaru);
                    yield return FadeKilat(0f, durasi * 0.6f);
                    break;
            }
        }

        private IEnumerator CrossfadeLatar(Sprite spriteBaru, float durasi)
        {
            if (latarTransisi == null)
            {
                SetLatarSeketika(spriteBaru);
                yield break;
            }

            latarTransisi.sprite = spriteBaru;
            Color c = latarTransisi.color;
            c.a = 0f;
            latarTransisi.color = c;

            float t = 0f;
            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Clamp01(t / durasi);
                latarTransisi.color = c;
                yield return null;
            }

            // Pindahkan hasil ke layer utama lalu reset layer transisi.
            latarUtama.sprite = spriteBaru;
            latarUtama.color = Color.white;
            c.a = 0f;
            latarTransisi.color = c;
        }

        // ---------------------------------------------------------------------
        // EFEK DRAMATIS
        // ---------------------------------------------------------------------

        public IEnumerator JalankanEfek(EfekKamera efek)
        {
            switch (efek)
            {
                case EfekKamera.Getar:
                    yield return Getar(kekuatanGetarRingan, durasiGetar);
                    break;
                case EfekKamera.GetarKuat:
                    yield return Getar(kekuatanGetarKuat, durasiGetar * 1.4f);
                    break;
                case EfekKamera.Kilat:
                    yield return Kilat(durasiKilat);
                    break;
                case EfekKamera.ZoomMasuk:
                    yield return Zoom(skalaZoom, durasiZoom);
                    break;
                case EfekKamera.ZoomKeluar:
                    yield return Zoom(1f, durasiZoom);
                    break;
            }
        }

        public IEnumerator Getar(float kekuatan, float durasi)
        {
            if (wadahAdegan == null) yield break;
            if (rutinGetar != null) StopCoroutine(rutinGetar);
            rutinGetar = StartCoroutine(RutinGetar(kekuatan, durasi));
            yield return rutinGetar;
        }

        private IEnumerator RutinGetar(float kekuatan, float durasi)
        {
            float t = 0f;
            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                // Kekuatan meredam seiring waktu agar terasa natural.
                float peredam = 1f - (t / durasi);
                Vector2 offset = new Vector2(
                    Random.Range(-1f, 1f) * kekuatan * peredam,
                    Random.Range(-1f, 1f) * kekuatan * peredam);
                wadahAdegan.anchoredPosition = posisiAsliWadah + offset;
                yield return null;
            }
            wadahAdegan.anchoredPosition = posisiAsliWadah;
            rutinGetar = null;
        }

        public IEnumerator Kilat(float durasi, bool tahanDiPuncak = false)
        {
            if (kilatPutih == null) yield break;

            yield return FadeKilat(1f, durasi * 0.3f);
            if (tahanDiPuncak) yield break;
            yield return FadeKilat(0f, durasi * 0.7f);
        }

        private IEnumerator FadeKilat(float target, float durasi)
        {
            if (kilatPutih == null) yield break;
            float awal = kilatPutih.alpha;
            float t = 0f;
            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                kilatPutih.alpha = Mathf.Lerp(awal, target, t / durasi);
                yield return null;
            }
            kilatPutih.alpha = target;
        }

        public IEnumerator Zoom(float skalaTarget, float durasi)
        {
            if (wadahAdegan == null) yield break;
            if (rutinZoom != null) StopCoroutine(rutinZoom);
            rutinZoom = StartCoroutine(RutinZoom(skalaTarget, durasi));
            yield return rutinZoom;
        }

        private IEnumerator RutinZoom(float skalaTarget, float durasi)
        {
            Vector3 awal = wadahAdegan.localScale;
            Vector3 target = Vector3.one * skalaTarget;
            float t = 0f;
            while (t < durasi)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / durasi);
                wadahAdegan.localScale = Vector3.Lerp(awal, target, p);
                yield return null;
            }
            wadahAdegan.localScale = target;
            rutinZoom = null;
        }

        // ---------------------------------------------------------------------
        // HOOK DISTORSI SANITY
        // ---------------------------------------------------------------------

        /// <summary>
        /// Mengatur intensitas overlay distorsi (0-1). Bisa dipanggil dari
        /// GameManager agar cutscene ikut terdistorsi ketika Sanity di bawah 50%.
        /// </summary>
        public void SetIntensitasDistorsi(float intensitas)
        {
            if (overlayDistorsi == null) return;
            overlayDistorsi.alpha = Mathf.Clamp01(intensitas);
        }
    }
}