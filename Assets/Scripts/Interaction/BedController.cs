using UnityEngine;

public class BedController : MonoBehaviour
{
    // Fungsi ini dipanggil oleh PlayerController saat karakter sudah tiba di kasur
    public void Tidur()
    {
        GameManager.Instance.StartCoroutine(GameManager.Instance.ProsesTidur(false));
    }
}