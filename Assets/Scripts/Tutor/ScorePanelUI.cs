using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text scoreText;
    [Tooltip("Tombol buat pulang ke scene utama setelah baca hasil skor (misal 'Button' yang udah ada di ScorePanel)")]
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (panelRoot != null)
        {
            Debug.Log($"[DEBUG-AWAKE] panelRoot: nama='{panelRoot.name}', InstanceID={panelRoot.GetInstanceID()}"); // --- SEMENTARA ---
            panelRoot.SetActive(false);
        }
        else
        {
            Debug.LogError("[DEBUG-AWAKE] panelRoot NULL sejak Awake()! Field Panel Root belum diisi di Inspector.");
        }

        // Hubungkan tombol balik ke MainScene, sama gaya kayak nextButton di AnswerSheetUI
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => GradingGameManager.Instance.KembaliKeMainScene());
        }
    }

    public void ShowScore(int correctGradings, int totalGraded, int scorePercent)
    {
        Debug.Log($"[DEBUG] ShowScore() TERPANGGIL. panelRoot null? {panelRoot == null}"); // --- SEMENTARA ---

        if (panelRoot != null)
        {
            // --- TAMBAHAN: identitas PERSIS object yang direferensikan, biar ketauan kalau ternyata beda
            // object dari yang kamu lihat/cek manual di Hierarchy (misal ada duplikat/hidden object) ---
            string namaParent = panelRoot.transform.parent != null ? panelRoot.transform.parent.name : "(tidak ada parent / root Canvas)";
            Debug.Log($"[DEBUG] panelRoot yang dipegang script ini: nama='{panelRoot.name}', InstanceID={panelRoot.GetInstanceID()}, parent='{namaParent}', siblingIndex={panelRoot.transform.GetSiblingIndex()}, activeSelf SEBELUM SetActive={panelRoot.activeSelf}");

            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
            Debug.Log($"[DEBUG] Setelah SetActive(true): panelRoot.activeSelf = {panelRoot.activeSelf}, activeInHierarchy = {panelRoot.activeInHierarchy}");
        }
        scoreText.text = $"Skor Koreksi: {correctGradings} / {totalGraded} benar ({scorePercent}%)";
    }
}