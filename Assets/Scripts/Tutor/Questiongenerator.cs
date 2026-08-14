using UnityEngine;

// --- Bikin 1 soal untuk mata pelajaran yang diminta (Proposal 3.3.4.3) ---
public static class QuestionGenerator
{
    public static QuestionData GenerateQuestion(SubjectType subject)
    {
        switch (subject)
        {
            case SubjectType.BahasaInggris: return GenerateEnglishQuestion();
            case SubjectType.IlmuPengetahuanUmum: return GenerateGeneralKnowledgeQuestion();
            default: return GenerateMathQuestion();
        }
    }

    // --- Matematika: +, -, x, : - jawaban tetap angka ---
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

            case 1: // Pengurangan (hasil selalu positif)
                a = Random.Range(10, 100);
                b = Random.Range(1, a);
                correct = a - b;
                opSymbol = "-";
                break;

            case 2: // Perkalian (angka kecil biar tetap sederhana)
                a = Random.Range(2, 12);
                b = Random.Range(2, 12);
                correct = a * b;
                opSymbol = "x";
                break;

            default: // Pembagian, dikonstruksi mundur biar hasilnya selalu bulat
                b = Random.Range(2, 12);
                correct = Random.Range(2, 12);
                a = b * correct;
                opSymbol = ":";
                break;
        }

        return new QuestionData
        {
            subject = SubjectType.Matematika,
            questionText = $"{a} {opSymbol} {b} = ...",
            correctAnswer = correct.ToString()
        };
    }

    // --- Bank soal Bahasa Inggris (tinggal nambah baris di array ini kalau mau nambah variasi) ---
    private static readonly (string soal, string jawaban)[] bankInggris = new (string, string)[]
    {
        ("Translate 'Rumah' ke Bahasa Inggris", "House"),
        ("Translate 'Makan' ke Bahasa Inggris", "Eat"),
        ("Antonim dari 'Big' adalah", "Small"),
        ("Sinonim dari 'Happy' adalah", "Glad"),
        ("Translate 'Kucing' ke Bahasa Inggris", "Cat"),
        ("Translate 'Sekolah' ke Bahasa Inggris", "School"),
        ("Bentuk lampau dari 'Go' adalah", "Went"),
        ("Translate 'Air' ke Bahasa Inggris", "Water"),
        ("Antonim dari 'Fast' adalah", "Slow"),
        ("Translate 'Buku' ke Bahasa Inggris", "Book"),
        ("Translate 'Anjing' ke Bahasa Inggris", "Dog"),
        ("Jamak dari 'Child' adalah", "Children"),
    };

    private static QuestionData GenerateEnglishQuestion()
    {
        var soal = bankInggris[Random.Range(0, bankInggris.Length)];
        return new QuestionData { subject = SubjectType.BahasaInggris, questionText = soal.soal, correctAnswer = soal.jawaban };
    }

    // --- Bank soal Ilmu Pengetahuan Umum ---
    private static readonly (string soal, string jawaban)[] bankIPU = new (string, string)[]
    {
        ("Ibu kota Indonesia adalah", "Jakarta"),
        ("Planet terdekat dari Matahari adalah", "Merkurius"),
        ("Presiden pertama Indonesia adalah", "Soekarno"),
        ("Benua terbesar di dunia adalah", "Asia"),
        ("Hewan berkaki delapan disebut", "Laba-laba"),
        ("Proses tumbuhan membuat makanan disebut", "Fotosintesis"),
        ("Mata uang Jepang adalah", "Yen"),
        ("Gunung tertinggi di dunia adalah", "Everest"),
        ("Organ tubuh yang memompa darah adalah", "Jantung"),
        ("Satuan suhu yang umum di Indonesia adalah", "Celcius"),
        ("Lambang negara Indonesia adalah", "Garuda Pancasila"),
        ("Planet yang dijuluki 'Planet Merah' adalah", "Mars"),
    };

    private static QuestionData GenerateGeneralKnowledgeQuestion()
    {
        var soal = bankIPU[Random.Range(0, bankIPU.Length)];
        return new QuestionData { subject = SubjectType.IlmuPengetahuanUmum, questionText = soal.soal, correctAnswer = soal.jawaban };
    }

    // --- Dipakai AnswerSheetGenerator buat bikin jawaban SALAH yang genuine (bukan acak sembarangan) untuk
    // Bahasa Inggris/IPU: ambil jawaban LAIN dari bank yang sama, asal beda dari jawaban yang benar. ---
    public static string GenerateJawabanSalahAcak(SubjectType subject, string jawabanBenar)
    {
        (string soal, string jawaban)[] bank = subject == SubjectType.BahasaInggris ? bankInggris : bankIPU;

        string salah;
        int percobaan = 0;
        do
        {
            salah = bank[Random.Range(0, bank.Length)].jawaban;
            percobaan++;
        } while (salah == jawabanBenar && percobaan < 20);

        return salah;
    }
}