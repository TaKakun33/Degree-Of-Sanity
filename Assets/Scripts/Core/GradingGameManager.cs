using System.Collections.Generic;
using UnityEngine;

// Nama sengaja dibuat "GradingGameManager" (bukan "GameManager") supaya tidak
// bentrok dengan GameManager utama yang sudah ada di project (sistem hari/uang/sanity).
public class GradingGameManager : MonoBehaviour
{
    public static GradingGameManager Instance { get; private set; }

    [Header("Pengaturan Minigame")]
    [SerializeField] private SubjectType subject = SubjectType.Matematika;
    [SerializeField] private int totalSheets = 5;
    [SerializeField] private int questionsPerSheet = 10;

    [Header("Referensi UI")]
    [SerializeField] private AnswerSheetUI answerSheetUI;
    [SerializeField] private ScorePanelUI scorePanelUI;

    private List<List<QuestionData>> allSheets = new List<List<QuestionData>>();
    private int currentSheetIndex = 0;
    private int totalCorrectGradings = 0;
    private int totalQuestionsGraded = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateAllSheets();
        LoadCurrentSheet();
    }

    private void GenerateAllSheets()
    {
        allSheets.Clear();
        for (int i = 0; i < totalSheets; i++)
        {
            allSheets.Add(AnswerSheetGenerator.GenerateSheet(subject, questionsPerSheet));
        }
    }

    private void LoadCurrentSheet()
    {
        if (currentSheetIndex >= allSheets.Count)
        {
            FinishGame();
            return;
        }

        answerSheetUI.DisplaySheet(allSheets[currentSheetIndex], currentSheetIndex + 1, allSheets.Count);
    }

    // Dipanggil AnswerRowUI tiap kali status coret sebuah soal berubah.
    public void UpdateGrade(int questionIndex, bool playerMarkedCorrect)
    {
        var currentSheet = allSheets[currentSheetIndex];
        if (questionIndex < 0 || questionIndex >= currentSheet.Count) return;
        currentSheet[questionIndex].playerMarkedCorrect = playerMarkedCorrect;
    }

    // Dipanggil tombol Next, selalu bisa ditekan kapan saja.
    public void GoToNextSheet()
    {
        TallyCurrentSheetScore();
        currentSheetIndex++;
        LoadCurrentSheet();
    }

    private void TallyCurrentSheetScore()
    {
        var currentSheet = allSheets[currentSheetIndex];
        foreach (var q in currentSheet)
        {
            totalQuestionsGraded++;
            if (q.DidPlayerGradeCorrectly()) totalCorrectGradings++;
        }
    }

    private void FinishGame()
    {
        int scorePercent = totalQuestionsGraded > 0
            ? Mathf.RoundToInt((float)totalCorrectGradings / totalQuestionsGraded * 100f)
            : 0;

        scorePanelUI.ShowScore(totalCorrectGradings, totalQuestionsGraded, scorePercent);
    }
}
