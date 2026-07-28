using System;

// --- Data satu soal di kertas jawaban ---
// correctAnswer & studentAnswer berupa STRING (bukan int seperti versi lama), supaya bisa
// menampung jawaban Bahasa Inggris/IPU yang bukan angka, bukan cuma Matematika.
[Serializable]
public class QuestionData
{
    public SubjectType subject;
    public string questionText;      // contoh: "12 + 7 = ..." atau "Ibu kota Indonesia adalah"
    public string correctAnswer;     // jawaban yang benar
    public string studentAnswer;     // jawaban yang tertulis di kertas siswa
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