using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Nama sengaja dibuat "GradingGameManager" (bukan "GameManager") supaya tidak
// bentrok dengan GameManager utama yang sudah ada di project (sistem hari/uang/sanity).
//
// --- Minigame Kerja Part Time: Home Tutor (Proposal 3.3.4.3) ---
// Arsitektur disamakan dengan KasirManager/OjolManager: scene TERPISAH (Single load),
// hasil shift (gaji, efek lapar/sanity, skip jam) dititipkan ke HasilKerjaPartTime,
// diterapkan lagi oleh GameManager begitu MainScene dimuat ulang.
public class GradingGameManager : MonoBehaviour
{
    public static GradingGameManager Instance { get; private set; }

    [Header("Pengaturan Minigame")]
    [Tooltip("Kalau dicentang, mata pelajaran DIACAK tiap kertas (sesuai proposal: 'beberapa mata pelajaran yang berbeda'). Kalau dimatikan, semua kertas pakai Subject di bawah.")]
    [SerializeField] private bool subjekAcakTiapKertas = true;
    [SerializeField] private SubjectType subject = SubjectType.Matematika;
    [SerializeField] private int totalSheets = 5;
    [SerializeField] private int questionsPerSheet = 10;

    [Header("Referensi UI")]
    [SerializeField] private AnswerSheetUI answerSheetUI;
    [SerializeField] private ScorePanelUI scorePanelUI;

    [Header("Pengaturan Bayaran & Efek Parameter (proposal: makin banyak benar, makin tinggi bayaran)")]
    [Tooltip("Bayaran PENUH kalau skor koreksi 100%; bayaran aktual = ini x (skorPercent/100)")]
    [SerializeField] private int bayaranMaksimal = 60000;
    [SerializeField] private float laparBerkurangPerShift = 15f;
    [SerializeField] private float sanityBerkurangPerShift = 5f;
    [Tooltip("Berapa jam in-game yang dilewati sepulang sesi tutor")]
    [SerializeField] private float jamDilewatiShift = 3f;
    [SerializeField] private string namaSceneUtama = "MainScene";

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
        int jumlahSubjek = System.Enum.GetValues(typeof(SubjectType)).Length;

        for (int i = 0; i < totalSheets; i++)
        {
            SubjectType subjekKertasIni = subjekAcakTiapKertas
                ? (SubjectType)Random.Range(0, jumlahSubjek)
                : subject;

            allSheets.Add(AnswerSheetGenerator.GenerateSheet(subjekKertasIni, questionsPerSheet));
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
        if (currentSheetIndex >= allSheets.Count) return; // --- TAMBAHAN: sudah selesai, abaikan klik lanjutan ---
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

    // Dipanggil tombol "Pulang Lebih Awal" (opsional) kalau ada, sudahi sesi tanpa nilai kertas yang belum selesai
    public void PulangLebihAwal()
    {
        FinishGame();
    }

    private void FinishGame()
    {
        Debug.Log("[DEBUG] FinishGame() TERPANGGIL."); // --- SEMENTARA, hapus lagi kalau udah beres ---

        // --- Matikan cuma tombol Next-nya (bukan seluruh GameObject Paper), biar aman
        // walaupun ScorePanel ternyata nested di dalam Paper di Hierarchy kamu ---
        if (answerSheetUI != null) answerSheetUI.DisableInteraction();
        else Debug.LogError("[DEBUG] answerSheetUI NULL di Inspector GradingGameManager!");

        int scorePercent = totalQuestionsGraded > 0
            ? Mathf.RoundToInt((float)totalCorrectGradings / totalQuestionsGraded * 100f)
            : 0;

        Debug.Log($"[DEBUG] Skor dihitung: {totalCorrectGradings}/{totalQuestionsGraded} ({scorePercent}%). scorePanelUI null? {scorePanelUI == null}");

        if (scorePanelUI != null)
        {
            scorePanelUI.ShowScore(totalCorrectGradings, totalQuestionsGraded, scorePercent);
        }
        else
        {
            Debug.LogError("[DEBUG] scorePanelUI NULL di Inspector GradingGameManager! Ini penyebabnya kalau ScorePanel gak pernah muncul.");
        }

        // Hitung bayaran dari skor koreksi, titipkan hasil shift ke HasilKerjaPartTime
        int bayaran = Mathf.RoundToInt(bayaranMaksimal * (scorePercent / 100f));
        HasilKerjaPartTime.SimpanHasil(bayaran, laparBerkurangPerShift, sanityBerkurangPerShift, jamDilewatiShift);

        Debug.Log("[DEBUG] FinishGame() SELESAI diproses.");
    }

    // Dipanggil tombol di ScorePanel (misal "Pulang"/"Selesai") setelah pemain baca skornya
    public void KembaliKeMainScene()
    {
        SceneManager.LoadScene(namaSceneUtama, LoadSceneMode.Single);
    }
}