using System.Collections.Generic;
using UnityEngine;

// --- Bikin satu KERTAS (kumpulan soal) untuk minigame Home Tutor (Proposal 3.3.4.3) ---
public static class AnswerSheetGenerator
{
    public static List<QuestionData> GenerateSheet(SubjectType subject, int questionCount)
    {
        var sheet = new List<QuestionData>();

        for (int i = 0; i < questionCount; i++)
        {
            var q = QuestionGenerator.GenerateQuestion(subject);

            // Acak sepenuhnya apakah jawaban siswa di kertas ini benar atau salah (50:50)
            bool answerIsCorrect = Random.value < 0.5f;

            if (answerIsCorrect)
            {
                q.studentAnswer = q.correctAnswer;
                q.isStudentAnswerActuallyCorrect = true;
            }
            else
            {
                q.studentAnswer = GenerateWrongAnswer(q);
                q.isStudentAnswerActuallyCorrect = false;
            }

            sheet.Add(q);
        }

        return sheet;
    }

    // --- Matematika: selisih angka kecil (khas typo hitung manual anak). Bahasa Inggris/IPU: minta
    // jawaban lain dari bank yang sama lewat QuestionGenerator, biar tetap masuk akal. ---
    private static string GenerateWrongAnswer(QuestionData q)
    {
        if (q.subject == SubjectType.Matematika && int.TryParse(q.correctAnswer, out int correctAnswerNum))
        {
            int wrong;
            do
            {
                int offset = Random.Range(1, 6) * (Random.value < 0.5f ? -1 : 1);
                wrong = correctAnswerNum + offset;
            } while (wrong == correctAnswerNum || wrong < 0);

            return wrong.ToString();
        }

        return QuestionGenerator.GenerateJawabanSalahAcak(q.subject, q.correctAnswer);
    }
}