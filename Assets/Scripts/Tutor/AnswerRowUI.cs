using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Satu baris soal di kertas. Seluruh baris bisa diklik (rowButton) untuk
// mencoret nomor+teks soal (menandai SALAH). Klik lagi untuk undo.
public class AnswerRowUI : MonoBehaviour
{
    [SerializeField] private Button rowButton;         // area klik, bisa mencakup seluruh baris (bg transparan + raycast target aktif)
    [SerializeField] private TMP_Text numberText;      // nomor soal, contoh: "3."
    [SerializeField] private TMP_Text questionText;    // contoh: "12 + 7 = ..."
    [SerializeField] private TMP_Text studentAnswerText;

    private int questionIndex;
    private QuestionData data;

    public void Setup(int index, QuestionData questionData)
    {
        questionIndex = index;
        data = questionData;

        numberText.text = $"{index + 1}.";
        questionText.text = data.questionText;
        studentAnswerText.text = data.studentAnswer;

        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(OnRowClicked);

        UpdateStrikeVisual();
    }

    private void OnRowClicked()
    {
        // Toggle: klik = tandai SALAH (coret), klik lagi = undo (kembali dianggap BENAR)
        data.playerMarkedCorrect = !data.playerMarkedCorrect;
        GradingGameManager.Instance.UpdateGrade(questionIndex, data.playerMarkedCorrect);
        UpdateStrikeVisual();
    }

    private void UpdateStrikeVisual()
    {
        bool markedWrong = !data.playerMarkedCorrect;

        var style = markedWrong ? FontStyles.Strikethrough : FontStyles.Normal;
        numberText.fontStyle = style;
        questionText.fontStyle = style;
        studentAnswerText.fontStyle = style;
    }
}