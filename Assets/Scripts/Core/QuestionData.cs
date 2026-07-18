using System;

// Tambahkan subjek baru di sini nanti, misal: IPA, BahasaIndonesia, dll.
// QuestionGenerator perlu ditambah case baru sesuai subjek yang ditambahkan.
public enum SubjectType
{
    Matematika
}

[Serializable]
public class QuestionData
{
    public string questionText;      // contoh: "12 + 7 = ..."
    public int correctAnswer;        // jawaban yang benar secara matematis
    public int studentAnswer;        // jawaban yang tertulis di kertas siswa
    public bool isStudentAnswerActuallyCorrect; // ground truth, dipakai untuk validasi skor

    // Default TRUE: soal yang tidak dicoret pemain otomatis dianggap BENAR.
    // Diklik (dicoret) -> FALSE (dianggap SALAH). Klik lagi -> balik ke TRUE (undo).
    public bool playerMarkedCorrect = true;

    // Apakah penilaian pemain (coret/tidak) sesuai dengan kenyataan jawaban siswa
    public bool DidPlayerGradeCorrectly()
    {
        return playerMarkedCorrect == isStudentAnswerActuallyCorrect;
    }
}
