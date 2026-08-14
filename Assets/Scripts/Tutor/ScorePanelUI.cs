using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text scoreText;
    [Tooltip("TAMBAHAN: teks buat nampilin gaji/bayaran yang didapat dari sesi tutor ini")]
    [SerializeField] private TMP_Text gajiText;
    [Tooltip("Tombol buat pulang ke scene utama setelah baca hasil skor (misal 'Button' yang udah ada di ScorePanel)")]
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        // Hubungkan tombol balik ke MainScene, sama gaya kayak nextButton di AnswerSheetUI
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => GradingGameManager.Instance.KembaliKeMainScene());
        }
    }

    // --- TAMBAHAN: parameter gajiDidapat, ditampilkan di gajiText ---
    public void ShowScore(int correctGradings, int totalGraded, int scorePercent, int gajiDidapat)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling(); // biar tampil paling depan, gak ketutupan Paper apapun urutannya di Hierarchy
        }

        scoreText.text = $"Skor Koreksi: {correctGradings} / {totalGraded} benar ({scorePercent}%)";

        if (gajiText != null)
        {
            gajiText.text = $"Gaji Didapat: Rp {gajiDidapat:N0}";
        }
    }
}