# Degree of Sanity 
---
Still Under development
---

## 🚧 Fitur yang Belum Dikerjakan
Daftar tugas yang perlu segera dikembangkan:

- [ ] **Semua ASET dan MUSIC** (PERLU SECEPATNYA)
- [ ] **Minigame Skripsi** (Sistem pengetikan presisi pada objek laptop)
- [ ] **Minigame Kerja Part Time** (Kasir, Ojek Online, dan Home Tutor)
- [ ] **Toko Item** (Antarmuka UI belanja barang/buff)
- [ ] **Sistem Distorsi Visual** (Efek visual saat Sanity < 50%)
- [ ] **Sistem NPC adiknya** (cara beriteraksi dengan adik)
- [ ] **Sistem Memasak di Dapur** (sistem untuk mengolah Bahan Makanan)
- [ ] **Sistem Liburan** (Butuh di dikusikan)
- [ ] **Kondisi Akhir Permainan** (Ending dari Game)
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

## ⚠️ Aturan Kolaborasi Unity
1. **Koordinasi Scene:** Beritahu tim di grup jika akan mengedit \`MainScene.unity\`.
2. **Meta Files:** Jangan pernah mengabaikan file \`.meta\`.
3. **Struktur Folder:**
   - \`Assets/Scripts/\`
   - \`Assets/Sprites/\`
   - \`Assets/Prefabs/\`

---
*Yok Selesikan kita. Harus JADI!* 🍌
