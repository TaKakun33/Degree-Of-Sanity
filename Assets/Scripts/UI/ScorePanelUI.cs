using TMPro;
using UnityEngine;

public class ScorePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void ShowScore(int correctGradings, int totalGraded, int scorePercent)
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        scoreText.text = $"Skor Koreksi: {correctGradings} / {totalGraded} benar ({scorePercent}%)";
    }
}
