using System.Collections.Generic;
using UnityEngine;

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
                q.studentAnswer = GenerateWrongAnswer(q.correctAnswer);
                q.isStudentAnswerActuallyCorrect = false;
            }

            sheet.Add(q);
        }

        return sheet;
    }

    private static int GenerateWrongAnswer(int correctAnswer)
    {
        int wrong;
        do
        {
            // Selisih kecil (khas kesalahan hitung manual siswa) biar tetap masuk akal,
            // bukan angka ngasal yang jauh banget dari jawaban benar.
            int offset = Random.Range(1, 6) * (Random.value < 0.5f ? -1 : 1);
            wrong = correctAnswer + offset;
        } while (wrong == correctAnswer || wrong < 0);

        return wrong;
    }
}
