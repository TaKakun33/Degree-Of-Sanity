# Degree of Sanity 
---
Still Under development
---

## 🚧 Fitur yang Belum Dikerjakan
Daftar tugas yang perlu segera dikembangkan:

- [ ] **Semua ASET dan MUSIC** (PERLU SECEPATNYA)
- [ ] **Minigame Skripsi** (Sistem pengetikan presisi pada objek laptop)
- [ ] **Minigame Kerja Part Time** (Kasir, Ojek Online, dan Home Tutor)
- [ ] **Sistem NPC adiknya** (cara beriteraksi dengan adik)
- [ ] **Sistem Liburan** (Butuh di dikusikan)
- [ ] **Terkait Cerita** (prolog maupun epilog)

*Tim: Segera ambil tugas di atas dengan membuat branch baru dan update statusnya jika sudah selesai!*

## 🛠️ Prasyarat
1. **Unity 6.3 LTS (6000.3.7f1)**
2. **Git & Git LFS** (Wajib untuk mengelola aset sprite/audio).
3. **Visual Studio Code** (Editor C#).

## 🚀 Cara Mulai
1.  Clone repository ini:
    ```bash
    git clone git@github.com:TaKakun33/Degree-Of-Sanity.git
    ```
2. Aktifkan LFS: 
   ```bash
   git lfs install
   git lfs pull
   ```
3. Buka proyek via **Unity Hub**.<br>
*Catatan: Pembukaan pertama kali memakan waktu lama karena proses rebuild folder Library.*

## 🌿 Aturan Branching (Workflow Tim)

**DILARANG PUSH LANGSUNG KE BRANCH `main`!**
Kita menggunakan alur kerja *branching* untuk memastikan stabilitas *build* dan menghindari *merge conflict* yang tidak perlu.

### Struktur Branch:
*   **`main`**: Branch utama yang berisi versi *game* yang stabil, siap dipresentasikan, dan sudah di-*build*.
*   **`dev`**: Branch integrasi utama. Semua perubahan fitur akan dikumpulkan di sini sebelum digabungkan ke `main`.
*   **`feature/nama-fitur`**: Branch khusus untuk pengerjaan tugas spesifik (Contoh: `feature/minigame-skripsi`, `feature/sistem-waktu`, `feature/ui-status`).

### Prosedur Kerja (Workflow):
1.  **Update Lokal**: Sebelum memulai, pastikan Anda berada di branch `dev` dan tarik perubahan terbaru dari tim:
    ```bash
    git checkout dev
    git pull origin dev
    ```
2.  **Buat Fitur Baru**: Buat branch baru khusus untuk fitur yang akan Anda kerjakan:
    ```bash
    git checkout -b feature/nama-fitur
    ```
3.  **Kerjakan & Commit**: Lakukan perubahan pada kode atau aset, lalu lakukan *commit* dengan pesan yang deskriptif[cite: 1].
4.  **Sync & Pull Request (PR)**:
    *   *Push* branch fitur Anda ke repositori: `git push origin feature/nama-fitur`
    *   Buka GitHub, buat **Pull Request (PR)** untuk menggabungkan branch fitur Anda ke branch `dev`.
    *   Tunggu anggota tim lain meninjau (review) kode Anda sebelum dilakukan *merge*.

*Catatan: Selalu komunikasikan di grup tim saat Anda akan menggabungkan (merge) fitur ke branch `dev` agar tidak terjadi tumpang tindih pengerjaan.*

# 🔄 Aturan Push & Pull (Alur Kerja Git)

## 1. Sebelum Mulai Bekerja (Update Lokal)

Jangan pernah mulai coding jika versi lokal Anda tertinggal. Selalu pastikan Anda mendapatkan update terbaru dari tim:

```bash
git checkout dev
git pull origin dev
git lfs pull
```

**Mengapa?** Agar Anda mengerjakan fitur di atas kodingan terbaru, bukan kodingan lama.

---

## 2. Saat Mengerjakan Fitur

Selalu kerjakan fitur di branch terpisah. Jangan pernah melakukan perubahan langsung di branch `dev` atau `main`.

```bash
git checkout -b feature/nama-fitur
# Lakukan pengerjaan di Unity/VS Code
```

---

## 3. Mengirim Perubahan (Push)

Setelah fitur selesai, tes di Unity, dan pastikan tidak ada error (Console bersih):

### a. Add & Commit

```bash
git add .
git commit -m "[FEAT] Nama fitur yang Anda kerjakan"
```

### b. Push ke GitHub

```bash
git push origin feature/nama-fitur
```

### c. Pull Request (PR)

1. Buka GitHub (di browser).
2. Klik tombol **"Compare & pull request"**.
3. Berikan deskripsi singkat fitur yang ditambahkan.
4. Tag anggota tim lain untuk melakukan review.
5. Setelah di-approve, fitur akan di-merge ke branch `dev`.

---

## ⚠️ Larangan Keras (PENTING!)

- 🚫 **DILARANG** melakukan `git push --force`. Ini bisa menghapus sejarah commit orang lain secara permanen!
- 🚫 **DILARANG** melakukan `git push` tanpa melakukan `git pull` terlebih dahulu.
- 🚫 **KOORDINASI SCENE**: Jika Anda harus menyentuh `MainScene.unity`, kabari tim di grup. Scene di Unity sangat rentan konflik dan sulit diperbaiki.

---

## 📝 Format Commit Message
- \`[FEAT]\` - Fitur baru.
- \`[ASSET]\` - Aset visual/audio.
- \`[FIX]\` - Bug fix.
- \`[DOCS]\` - Dokumentasi.

---
*Yok Selesikan kita. Harus JADI!* 🍌
