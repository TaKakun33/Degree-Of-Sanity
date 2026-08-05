// =============================================================================
// File        : PenampilKarakter.cs
// Deskripsi   : Mengatur sprite karakter pada tiga slot (kiri, tengah, kanan)
//               lengkap dengan animasi geser-masuk, fade, dan peredupan
//               karakter yang sedang tidak berbicara.
// Tim         : Gethuk Pisang
// =============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DegreeOfSanity.Cerita
{
    public class PenampilKarakter : MonoBehaviour
    {
        [Header("Slot Portrait")]
        [SerializeField] private Image slotKiri;
        [SerializeField] private Image slotTengah;
        [SerializeField] private Image slotKanan;

        [Header("Animasi")]
        [SerializeField] private float durasiMasuk = 0.35f;
        [SerializeField] private float jarakGeserMasuk = 70f;
        [SerializeField] private float durasiSorot = 0.2f;
        [SerializeField] private float skalaPembicara = 1.03f;

        [Header("Warna")]
        [SerializeField] private Color warnaAktif = Color.white;
        [SerializeField] private Color warnaPasif = new Color(0.42f, 0.42f, 0.52f, 1f);

        private Vector2 posisiAsliKiri, posisiAsliTengah, posisiAsliKanan;
        private Coroutine animKiri, animTengah, animKanan;

        private void Awake()
        {
            if (slotKiri != null)
            {
                posisiAsliKiri = slotKiri.rectTransform.anchoredPosition;
                MatikanSlot(slotKiri);
            }
            if (slotTengah != null)
            {
                posisiAsliTengah = slotTengah.rectTransform.anchoredPosition;
                MatikanSlot(slotTengah);
            }
            if (slotKanan != null)
            {
                posisiAsliKanan = slotKanan.rectTransform.anchoredPosition;
                MatikanSlot(slotKanan);
            }
        }

        // ---------------------------------------------------------------------
        // API PUBLIK
        // ---------------------------------------------------------------------

        /// <summary>Menampilkan sprite karakter pada posisi tertentu.</summary>
        public void Tampilkan(Sprite sprite, PosisiPortrait posisi, bool langsung = false)
        {
            Image slot = AmbilSlot(posisi);
            if (slot == null || sprite == null) return;

            // Pengecekan WAJIB dilakukan sebelum sprite diganti, kalau tidak
            // perbandingan slot.sprite != sprite akan selalu bernilai false.
            bool perluAnimasiMasuk = !slot.gameObject.activeSelf || slot.sprite != sprite;

            slot.sprite = sprite;
            slot.gameObject.SetActive(true);
            slot.preserveAspect = true;

            if (!perluAnimasiMasuk) return;

            Vector2 posisiAsli = AmbilPosisiAsli(posisi);
            float arah = posisi == PosisiPortrait.Kanan ? 1f : -1f;

            if (langsung)
            {
                slot.rectTransform.anchoredPosition = posisiAsli;
                slot.color = warnaAktif;
                return;
            }

            JalankanAnim(posisi, AnimasiMasuk(slot, posisiAsli, arah));
        }

        /// <summary>Menyorot slot pembicara dan meredupkan slot lain.</summary>
        public void SorotPembicara(PosisiPortrait posisi)
        {
            SorotSlot(slotKiri, posisi == PosisiPortrait.Kiri);
            SorotSlot(slotTengah, posisi == PosisiPortrait.Tengah);
            SorotSlot(slotKanan, posisi == PosisiPortrait.Kanan);
        }

        /// <summary>Menyembunyikan satu karakter dengan animasi geser keluar.</summary>
        public void Sembunyikan(PosisiPortrait posisi, bool langsung = false)
        {
            Image slot = AmbilSlot(posisi);
            if (slot == null || !slot.gameObject.activeSelf) return;

            if (langsung)
            {
                MatikanSlot(slot);
                return;
            }

            float arah = posisi == PosisiPortrait.Kanan ? 1f : -1f;
            JalankanAnim(posisi, AnimasiKeluar(slot, AmbilPosisiAsli(posisi), arah));
        }

        /// <summary>Menyembunyikan seluruh karakter di layar.</summary>
        public void SembunyikanSemua(bool langsung = false)
        {
            Sembunyikan(PosisiPortrait.Kiri, langsung);
            Sembunyikan(PosisiPortrait.Tengah, langsung);
            Sembunyikan(PosisiPortrait.Kanan, langsung);
        }

        // ---------------------------------------------------------------------
        // COROUTINE ANIMASI
        // ---------------------------------------------------------------------

        private IEnumerator AnimasiMasuk(Image slot, Vector2 posisiAsli, float arah)
        {
            Vector2 posisiAwal = posisiAsli + new Vector2(jarakGeserMasuk * arah, 0f);
            Color warnaAwal = warnaAktif;
            warnaAwal.a = 0f;

            float t = 0f;
            while (t < durasiMasuk)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / durasiMasuk);
                float eased = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic

                slot.rectTransform.anchoredPosition = Vector2.Lerp(posisiAwal, posisiAsli, eased);
                slot.color = Color.Lerp(warnaAwal, warnaAktif, eased);
                yield return null;
            }

            slot.rectTransform.anchoredPosition = posisiAsli;
            slot.color = warnaAktif;
        }

        private IEnumerator AnimasiKeluar(Image slot, Vector2 posisiAsli, float arah)
        {
            Vector2 posisiAkhir = posisiAsli + new Vector2(jarakGeserMasuk * arah, 0f);
            Color warnaMulai = slot.color;
            Color warnaAkhir = warnaMulai;
            warnaAkhir.a = 0f;

            float t = 0f;
            while (t < durasiMasuk)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / durasiMasuk);
                slot.rectTransform.anchoredPosition = Vector2.Lerp(posisiAsli, posisiAkhir, p);
                slot.color = Color.Lerp(warnaMulai, warnaAkhir, p);
                yield return null;
            }

            slot.rectTransform.anchoredPosition = posisiAsli;
            MatikanSlot(slot);
        }

        private void SorotSlot(Image slot, bool aktif)
        {
            if (slot == null || !slot.gameObject.activeSelf) return;
            StartCoroutine(AnimasiSorot(slot, aktif));
        }

        private IEnumerator AnimasiSorot(Image slot, bool aktif)
        {
            Color warnaTarget = aktif ? warnaAktif : warnaPasif;
            Vector3 skalaTarget = aktif ? Vector3.one * skalaPembicara : Vector3.one;

            Color warnaMulai = slot.color;
            Vector3 skalaMulai = slot.rectTransform.localScale;

            float t = 0f;
            while (t < durasiSorot)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / durasiSorot);
                // Jaga alpha agar tidak ikut berubah saat proses fade-in belum selesai.
                Color warnaBaru = Color.Lerp(warnaMulai, warnaTarget, p);
                warnaBaru.a = Mathf.Max(warnaMulai.a, warnaBaru.a);
                slot.color = warnaBaru;
                slot.rectTransform.localScale = Vector3.Lerp(skalaMulai, skalaTarget, p);
                yield return null;
            }
        }

        // ---------------------------------------------------------------------
        // UTILITAS
        // ---------------------------------------------------------------------

        private void JalankanAnim(PosisiPortrait posisi, IEnumerator rutin)
        {
            switch (posisi)
            {
                case PosisiPortrait.Kiri:
                    if (animKiri != null) StopCoroutine(animKiri);
                    animKiri = StartCoroutine(rutin);
                    break;
                case PosisiPortrait.Tengah:
                    if (animTengah != null) StopCoroutine(animTengah);
                    animTengah = StartCoroutine(rutin);
                    break;
                case PosisiPortrait.Kanan:
                    if (animKanan != null) StopCoroutine(animKanan);
                    animKanan = StartCoroutine(rutin);
                    break;
            }
        }

        private Image AmbilSlot(PosisiPortrait posisi)
        {
            switch (posisi)
            {
                case PosisiPortrait.Kiri: return slotKiri;
                case PosisiPortrait.Tengah: return slotTengah;
                case PosisiPortrait.Kanan: return slotKanan;
                default: return null;
            }
        }

        private Vector2 AmbilPosisiAsli(PosisiPortrait posisi)
        {
            switch (posisi)
            {
                case PosisiPortrait.Kiri: return posisiAsliKiri;
                case PosisiPortrait.Tengah: return posisiAsliTengah;
                case PosisiPortrait.Kanan: return posisiAsliKanan;
                default: return Vector2.zero;
            }
        }

        private void MatikanSlot(Image slot)
        {
            slot.sprite = null;
            slot.gameObject.SetActive(false);
            slot.rectTransform.localScale = Vector3.one;
        }
    }
}