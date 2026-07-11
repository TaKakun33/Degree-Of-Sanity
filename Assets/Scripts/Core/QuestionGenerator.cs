using UnityEngine;

public static class QuestionGenerator
{
    public static QuestionData GenerateQuestion(SubjectType subject)
    {
        switch (subject)
        {
            case SubjectType.Matematika:
                return GenerateMathQuestion();

            default:
                Debug.LogWarning($"Belum ada generator soal untuk subjek: {subject}. Menggunakan Matematika sebagai fallback.");
                return GenerateMathQuestion();
        }
    }

    private static QuestionData GenerateMathQuestion()
    {
        int operatorType = Random.Range(0, 4); // 0=+, 1=-, 2=x, 3=:
        int a, b, correct;
        string opSymbol;

        switch (operatorType)
        {
            case 0: // Penjumlahan
                a = Random.Range(1, 50);
                b = Random.Range(1, 50);
                correct = a + b;
                opSymbol = "+";
                break;

            case 1: // Pengurangan (hasil selalu positif, biar sesuai aritmatika sederhana)
                a = Random.Range(10, 100);
                b = Random.Range(1, a);
                correct = a - b;
                opSymbol = "-";
                break;

            case 2: // Perkalian (angka kecil biar tetap "sederhana")
                a = Random.Range(2, 12);
                b = Random.Range(2, 12);
                correct = a * b;
                opSymbol = "x";
                break;

            default: // Pembagian, dikonstruksi mundur supaya hasilnya selalu bulat
                b = Random.Range(2, 12);
                correct = Random.Range(2, 12);
                a = b * correct;
                opSymbol = ":";
                break;
        }

        return new QuestionData
        {
            questionText = $"{a} {opSymbol} {b} = ...",
            correctAnswer = correct
        };
    }
}
