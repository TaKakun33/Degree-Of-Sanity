using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerSheetUI : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;      // parent di kertas, tempat 10 baris soal disusun (butuh Vertical Layout Group)
    [SerializeField] private AnswerRowUI rowPrefab;
    [SerializeField] private TMP_Text sheetProgressText;   // contoh: "Kertas 2 / 5"
    [SerializeField] private Button nextButton;            // tombol di pojok kanan bawah, aktif & terlihat dari awal

    private readonly List<AnswerRowUI> spawnedRows = new List<AnswerRowUI>();

    private void Awake()
    {
        // Next button statis, tidak bergantung isi kertas, jadi listener cukup dipasang sekali
        nextButton.onClick.AddListener(() => GradingGameManager.Instance.GoToNextSheet());
    }

    public void DisplaySheet(List<QuestionData> sheetData, int sheetNumber, int totalSheetsCount)
    {
        ClearRows();

        for (int i = 0; i < sheetData.Count; i++)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            row.Setup(i, sheetData[i]);
            spawnedRows.Add(row);
        }

        sheetProgressText.text = $"Kertas {sheetNumber} / {totalSheetsCount}";
    }

    private void ClearRows()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null) Destroy(row.gameObject);
        }
        spawnedRows.Clear();
    }

    // --- TAMBAHAN: dipanggil GradingGameManager begitu game selesai, biar tombol Next gak bisa
    // diklik lagi. Sengaja CUMA matiin tombolnya (bukan seluruh GameObject Paper), biar aman
    // gak peduli ScorePanel itu child dari Paper atau bukan di Hierarchy kamu. ---
    public void DisableInteraction()
    {
        if (nextButton != null) nextButton.interactable = false;
    }
}