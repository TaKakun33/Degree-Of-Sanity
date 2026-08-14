using System.Collections;
using TMPro;
using UnityEngine;

// --- Bark System (Bagian 1 naskah): teks kecil melayang di atas Andrew, hilang ±2 detik,
// KONTROL TIDAK DIKUNCI. Terpisah dari CeritaManager - dipanggil manual dari script lain
// (ThresholdSkripsi, CicilanManager, ObjekKlikCerita) tiap ada momen bark. ---
public class PenampilBark : MonoBehaviour
{
    public static PenampilBark Instance;

    public GameObject panelBark;
    public TextMeshProUGUI textBark;
    public float durasiTampil = 2f;

    private Coroutine coroutineAktif;

    void Awake()
    {
        Instance = this;
        if (panelBark) panelBark.SetActive(false);
    }

    public void Tampilkan(string teks)
    {
        if (panelBark == null || textBark == null) {
            Debug.Log("[Bark] " + teks);
            return;
        }

        if (coroutineAktif != null) StopCoroutine(coroutineAktif);
        coroutineAktif = StartCoroutine(TampilkanSementara(teks));
    }

    IEnumerator TampilkanSementara(string teks)
    {
        textBark.text = teks;
        panelBark.SetActive(true);
        yield return new WaitForSeconds(durasiTampil);
        panelBark.SetActive(false);
        coroutineAktif = null;
    }
}