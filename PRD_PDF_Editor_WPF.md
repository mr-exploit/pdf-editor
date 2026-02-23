# Product Requirements Document (PRD)
## PDF Editor — Desktop Application (WPF / C#)

---

### Document Information

| Field | Detail |
|---|---|
| **Product Name** | PDF Editor Desktop |
| **Platform** | Windows Desktop — WPF (.NET / C#) |
| **Version** | 1.0.0 |
| **Last Updated** | 2026-02-23 |
| **Status** | Draft |

---

## 1. Overview

PDF Editor Desktop adalah aplikasi desktop berbasis WPF (Windows Presentation Foundation) yang dibangun menggunakan C#. Aplikasi ini menyediakan serangkaian fitur manajemen dan konversi dokumen PDF secara lokal tanpa memerlukan koneksi internet. Target pengguna adalah profesional, pelajar, dan siapa pun yang membutuhkan solusi pengolahan dokumen PDF yang cepat, aman, dan mudah digunakan.

---

## 2. Goals & Objectives

- Menyediakan solusi all-in-one untuk kebutuhan pengolahan PDF di lingkungan desktop Windows.
- Memastikan seluruh pemrosesan file dilakukan secara lokal (tidak ada data yang dikirim ke server eksternal).
- Menghadirkan antarmuka yang intuitif dengan pengalaman drag-and-drop.
- Mendukung file berukuran besar dengan performa yang tetap responsif menggunakan async/await.

---

## 3. Tech Stack & Dependencies

| Komponen | Pilihan |
|---|---|
| **Framework UI** | WPF (.NET 8 / .NET 9) |
| **Bahasa** | C# |
| **PDF Library** | PdfSharp / iText7 / Aspose.PDF (pilih satu) |
| **Konversi Office** | Microsoft.Office.Interop atau OpenXML SDK + third-party converter |
| **MVVM Framework** | CommunityToolkit.Mvvm |
| **DI Container** | Microsoft.Extensions.DependencyInjection |
| **Async** | Task Parallel Library (async/await) |
| **UI Component** | MahApps.Metro atau HandyControl (opsional untuk styling) |
| **File Dialog** | Microsoft.Win32.OpenFileDialog / SaveFileDialog |

> **Rekomendasi Library Utama:**
> - **iText7** untuk manipulasi PDF (split, merge, compress, reorganize).
> - **Aspose.Words / Aspose.Cells / Aspose.Slides** untuk konversi ke/dari Office format (atau **OpenXML SDK** sebagai alternatif gratis namun terbatas).
> - Jika menggunakan Microsoft Office Interop, pastikan Office terinstal di mesin pengguna.

---

## 4. Arsitektur Aplikasi

```
PDFEditorApp/
├── App.xaml / App.xaml.cs
├── Core/
│   ├── Services/
│   │   ├── IConversionService.cs
│   │   ├── ConversionService.cs
│   │   ├── ICompressService.cs
│   │   ├── CompressService.cs
│   │   ├── IMergeService.cs
│   │   ├── MergeService.cs
│   │   ├── ISplitService.cs
│   │   ├── SplitService.cs
│   │   └── IReorganizeService.cs
│   │   └── ReorganizeService.cs
│   ├── Models/
│   │   ├── PdfPageModel.cs
│   │   ├── ConversionTask.cs
│   │   └── FileItem.cs
│   └── Helpers/
│       ├── FileHelper.cs
│       └── ThumbnailHelper.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ConvertViewModel.cs
│   ├── CompressViewModel.cs
│   ├── MergeViewModel.cs
│   ├── SplitViewModel.cs
│   └── ReorganizeViewModel.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── ConvertView.xaml
│   ├── CompressView.xaml
│   ├── MergeView.xaml
│   ├── SplitView.xaml
│   └── ReorganizeView.xaml
└── Resources/
    ├── Styles/
    └── Icons/
```

**Pattern:** MVVM (Model-View-ViewModel)  
**Navigation:** Tab-based atau Side Navigation Panel  

---

## 5. Fitur Utama

---

### 5.1 Fitur: Convert — PDF to Office

#### 5.1.1 PDF to Word (DOCX)

**Deskripsi:**  
Pengguna dapat mengkonversi file PDF menjadi dokumen Microsoft Word (.docx) dengan mempertahankan layout, teks, dan gambar semaksimal mungkin.

**User Story:**  
> Sebagai pengguna, saya ingin mengkonversi file PDF menjadi DOCX agar saya dapat mengeditnya di Microsoft Word.

**Acceptance Criteria:**
- Pengguna dapat memilih satu atau lebih file PDF melalui file dialog atau drag-and-drop.
- Pengguna dapat memilih folder output.
- Aplikasi menampilkan progress bar per file saat konversi berlangsung.
- Output file tersimpan dalam format `.docx`.
- Jika konversi gagal pada satu file, file lain tetap diproses dan error ditampilkan.
- Pengguna dapat membuka folder output setelah proses selesai.

**Technical Notes:**
- Gunakan `Aspose.Words` atau `iText7 + PdfDocument` untuk ekstraksi teks/layout, kemudian tulis ulang dengan `DocumentFormat.OpenXml`.
- Jalankan proses konversi di background thread (`Task.Run`) agar UI tidak freeze.

---

#### 5.1.2 PDF to Excel (XLSX)

**Deskripsi:**  
Mengkonversi tabel dan data dalam file PDF menjadi spreadsheet Microsoft Excel (.xlsx).

**User Story:**  
> Sebagai pengguna, saya ingin mengekstrak tabel dari PDF ke Excel agar data mudah diolah.

**Acceptance Criteria:**
- Pengguna dapat memilih satu atau lebih file PDF.
- Aplikasi mendeteksi tabel dalam PDF dan memetakannya ke sheet Excel.
- Setiap halaman PDF dapat dikonversi ke sheet terpisah atau digabung (dengan opsi).
- Output file tersimpan dalam format `.xlsx`.
- Progres ditampilkan dengan progress bar.

**Technical Notes:**
- Gunakan `Aspose.Cells` untuk membuat file Excel atau `ClosedXML` sebagai alternatif open-source.
- Deteksi tabel PDF bisa menggunakan `iText7` dengan parsing koordinat teks.

---

#### 5.1.3 PDF to PowerPoint (PPTX)

**Deskripsi:**  
Mengkonversi setiap halaman PDF menjadi slide PowerPoint (.pptx).

**User Story:**  
> Sebagai pengguna, saya ingin mengkonversi presentasi PDF saya kembali ke format PPTX.

**Acceptance Criteria:**
- Setiap halaman PDF menjadi satu slide.
- Gambar, teks, dan layout dipertahankan semaksimal mungkin.
- Output tersimpan dalam format `.pptx`.
- Pengguna dapat memilih resolusi render slide (Normal / High Quality).

**Technical Notes:**
- Render setiap halaman PDF sebagai gambar beresolusi tinggi menggunakan `PdfiumViewer` atau `Aspose.PDF`.
- Sisipkan gambar tersebut ke dalam slide PPTX menggunakan `Aspose.Slides` atau `OpenXml Presentation`.

---

### 5.2 Fitur: Convert — Office to PDF

#### 5.2.1 Word (DOCX) to PDF

**Deskripsi:**  
Mengkonversi file Microsoft Word (.docx / .doc) menjadi PDF.

**Acceptance Criteria:**
- Pengguna dapat memilih satu atau lebih file DOCX/DOC.
- Konversi mempertahankan formatting, header/footer, gambar, dan tabel.
- Output tersimpan sebagai `.pdf`.
- Progres ditampilkan.

**Technical Notes:**
- Gunakan `Microsoft.Office.Interop.Word` jika Office terinstal, atau `Aspose.Words` sebagai fallback yang tidak memerlukan Office.

---

#### 5.2.2 Excel (XLSX) to PDF

**Deskripsi:**  
Mengkonversi file Microsoft Excel (.xlsx / .xls) menjadi PDF.

**Acceptance Criteria:**
- Pengguna dapat memilih satu atau lebih file XLSX/XLS.
- Setiap sheet dapat dikonfigurasi: ekspor semua sheet atau sheet tertentu saja.
- Output tersimpan sebagai `.pdf`.

**Technical Notes:**
- Gunakan `Microsoft.Office.Interop.Excel` atau `Aspose.Cells`.

---

#### 5.2.3 PowerPoint (PPTX) to PDF

**Deskripsi:**  
Mengkonversi file Microsoft PowerPoint (.pptx / .ppt) menjadi PDF.

**Acceptance Criteria:**
- Pengguna dapat memilih satu atau lebih file PPTX/PPT.
- Setiap slide dikonversi menjadi satu halaman PDF.
- Output tersimpan sebagai `.pdf`.

**Technical Notes:**
- Gunakan `Microsoft.Office.Interop.PowerPoint` atau `Aspose.Slides`.

---

### 5.3 Fitur: Compress PDF

**Deskripsi:**  
Mengurangi ukuran file PDF dengan mengoptimalkan gambar, metadata, dan stream internal tanpa merusak konten secara signifikan.

**User Story:**  
> Sebagai pengguna, saya ingin mengkompres PDF agar ukurannya lebih kecil dan mudah dikirim via email.

**Acceptance Criteria:**
- Pengguna dapat memilih satu atau lebih file PDF.
- Tersedia tiga level kompresi: **Low** (kualitas tinggi, pengurangan kecil), **Medium** (seimbang), **High** (pengurangan maksimal, kualitas lebih rendah).
- Aplikasi menampilkan estimasi ukuran sebelum dan sesudah kompresi.
- Output tersimpan di folder yang ditentukan pengguna (default: folder yang sama dengan sumber, dengan suffix `_compressed`).
- Progres ditampilkan per file.

**Technical Notes:**
- Implementasi dengan `iText7`: downscale embedded images, remove unused resources, compress content streams.
- Gunakan `PdfWriter` dengan `CompressionLevel` yang sesuai.
- Kalkulasi ukuran preview dilakukan secara async sebelum menyimpan file.

---

### 5.4 Fitur: Merge (Combine) PDF

**Deskripsi:**  
Menggabungkan dua atau lebih file PDF menjadi satu file PDF tunggal.

**User Story:**  
> Sebagai pengguna, saya ingin menggabungkan beberapa laporan PDF menjadi satu dokumen.

**Acceptance Criteria:**
- Pengguna dapat menambahkan beberapa file PDF melalui file dialog atau drag-and-drop ke daftar.
- Pengguna dapat mengatur urutan file dengan cara drag-and-drop di dalam daftar.
- Pengguna dapat menghapus file dari daftar sebelum merge.
- Pengguna menentukan nama dan lokasi file output.
- Aplikasi menampilkan total jumlah halaman gabungan sebelum proses.
- Progres ditampilkan saat proses merge berlangsung.
- Setelah selesai, pengguna dapat langsung membuka file hasil.

**Technical Notes:**
- Gunakan `iText7 PdfMerger` atau `PdfSharp PdfDocument.AddPage()`.
- Urutan file dalam list view mencerminkan urutan halaman pada PDF output.

---

### 5.5 Fitur: Split PDF

**Deskripsi:**  
Memecah satu file PDF menjadi beberapa file PDF yang lebih kecil berdasarkan aturan yang ditentukan pengguna.

**User Story:**  
> Sebagai pengguna, saya ingin memecah laporan besar menjadi bagian-bagian yang lebih kecil.

**Acceptance Criteria:**
- Pengguna memilih satu file PDF sumber.
- Tersedia tiga mode split:
  - **Split per halaman**: setiap halaman menjadi file PDF terpisah.
  - **Split berdasarkan range halaman**: pengguna mendefinisikan range (contoh: `1-3, 4-7, 8-10`).
  - **Split setiap N halaman**: pengguna menentukan jumlah halaman per file output.
- Pengguna menentukan folder output dan prefix nama file.
- Aplikasi menampilkan preview daftar file yang akan dihasilkan sebelum proses dimulai.
- Progres ditampilkan.

**Technical Notes:**
- Parsing range input dengan regex: `\d+(-\d+)?(,\s*\d+(-\d+)?)*`
- Validasi bahwa range tidak melebihi jumlah halaman PDF.
- Gunakan `iText7 PdfDocument` dengan `PdfWriter` untuk setiap file output.

---

### 5.6 Fitur: Reorganize PDF

**Deskripsi:**  
Mengatur ulang, menghapus, mengurutkan halaman PDF, dan menggabungkan halaman dari PDF lain ke dalam dokumen yang sedang diedit.

**User Story:**  
> Sebagai pengguna, saya ingin menyusun ulang urutan halaman PDF saya, menghapus halaman yang tidak diperlukan, dan menyisipkan halaman dari file PDF lain.

**Acceptance Criteria:**

**Tampilan:**
- Pengguna membuka satu file PDF utama.
- Semua halaman PDF ditampilkan sebagai thumbnail grid (preview miniatur).
- Setiap thumbnail menampilkan nomor halaman.

**Operasi Delete:**
- Pengguna dapat memilih satu atau lebih halaman (klik + Ctrl/Shift untuk multi-select).
- Tombol "Delete" atau tombol keyboard `Delete` menghapus halaman yang dipilih dari daftar.
- Konfirmasi dialog muncul sebelum penghapusan.

**Operasi Sort / Reorder:**
- Pengguna dapat drag-and-drop thumbnail untuk mengubah urutan halaman.
- Tersedia opsi sort otomatis: **Ascending** (1, 2, 3, ...) dan **Descending** (N, N-1, ...).
- Perubahan urutan tercermin secara real-time pada thumbnail grid.

**Operasi Add / Insert dari PDF Lain:**
- Pengguna dapat menambahkan file PDF lain melalui tombol "Add PDF" atau drag-and-drop file ke thumbnail grid.
- Halaman dari PDF tambahan ditampilkan di panel samping untuk dipilih.
- Pengguna dapat memilih halaman tertentu dari PDF tambahan dan menyisipkannya di posisi tertentu (sebelum/sesudah halaman yang dipilih).
- Dapat menambahkan seluruh halaman dari PDF tambahan sekaligus.

**Simpan:**
- Pengguna dapat menyimpan sebagai file baru ("Save As") atau menimpa file asli ("Save").
- Progres ditampilkan saat menyimpan.

**Technical Notes:**
- Render thumbnail menggunakan `PdfiumViewer`, `Ghostscript.NET`, atau `Aspose.PDF` dengan render ke `BitmapSource`.
- Render thumbnail secara async dan lazy (hanya render yang terlihat di viewport).
- State reorganisasi disimpan dalam `ObservableCollection<PdfPageModel>` di ViewModel.
- `PdfPageModel` menyimpan: `SourceFilePath`, `OriginalPageIndex`, `CurrentIndex`, `ThumbnailImage`.
- Saat save, rekonstruksi PDF dari koleksi menggunakan `iText7`.

---

## 6. UI/UX Requirements

### 6.1 Layout Umum

```
┌─────────────────────────────────────────────────────────────┐
│  [Logo] PDF Editor                          [Minimize][X]   │
├──────────────┬──────────────────────────────────────────────┤
│              │                                              │
│  Navigation  │              Content Area                    │
│  Panel       │                                              │
│              │  (fitur aktif ditampilkan di sini)           │
│  > Convert   │                                              │
│    PDF→Word  │                                              │
│    PDF→Excel │                                              │
│    PDF→PPT   │                                              │
│    Word→PDF  │                                              │
│    Excel→PDF │                                              │
│    PPT→PDF   │                                              │
│  > Compress  │                                              │
│  > Merge     │                                              │
│  > Split     │                                              │
│  > Reorganize│                                              │
│              │                                              │
└──────────────┴──────────────────────────────────────────────┘
```

### 6.2 Komponen UI Standar

- **Drag-and-drop zone**: area visual yang jelas untuk melepas file, dengan border dashed dan ikon.
- **File list**: ListView dengan kolom Nama File, Ukuran, Status, Progres.
- **Progress bar**: per file dan overall.
- **Status message**: di bagian bawah layar (status bar).
- **Output folder picker**: TextBox + Browse Button.
- **Action buttons**: "Start", "Clear", "Open Output Folder".

### 6.3 Warna & Tema

- Mendukung **Light Mode** dan **Dark Mode** (mengikuti setting sistem Windows).
- Warna aksen: biru (#0078D4, konsisten dengan Windows Fluent Design).

### 6.4 Aksesibilitas

- Semua kontrol memiliki `AutomationProperties.Name` untuk screen reader.
- Keyboard navigation penuh (Tab, Enter, Delete).
- Tooltip pada setiap tombol.

---

## 7. Non-Functional Requirements

| Kategori | Requirement |
|---|---|
| **Performance** | Konversi file PDF ≤ 10MB selesai dalam < 30 detik. UI tetap responsif selama proses (async). |
| **Scalability** | Batch processing hingga 50 file sekaligus. |
| **Reliability** | Gagal pada satu file tidak menghentikan proses batch. Error tercatat di log. |
| **Security** | Semua pemrosesan dilakukan lokal. Tidak ada transmisi data ke internet. |
| **Compatibility** | Windows 10 (1809+) dan Windows 11. .NET 8 atau lebih baru. |
| **File Size Support** | File PDF hingga 500MB. |
| **Lokalisasi** | Bahasa Indonesia dan Inggris (i18n-ready dengan ResourceDictionary). |

---

## 8. Error Handling

- **File tidak valid / corrupt**: tampilkan pesan error spesifik per file, skip file tersebut, lanjutkan batch.
- **File sedang dibuka aplikasi lain**: tampilkan pesan "File sedang digunakan. Tutup file terlebih dahulu."
- **Tidak cukup ruang disk**: cek kapasitas sebelum proses, tampilkan warning jika kurang.
- **Library tidak tersedia**: validasi dependency saat startup, tampilkan dialog jika ada yang hilang.
- **Timeout**: untuk file sangat besar, tampilkan opsi cancel dan estimasi waktu.
- Semua error dicatat ke file log di `%AppData%\PDFEditor\logs\`.

---

## 9. Data Models

```csharp
// Model untuk item file dalam daftar
public class FileItem : ObservableObject
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public string FileSizeDisplay { get; set; }
    public FileStatus Status { get; set; } // Pending, Processing, Done, Error
    public double Progress { get; set; }
    public string ErrorMessage { get; set; }
}

// Enum status
public enum FileStatus { Pending, Processing, Done, Error }

// Model untuk halaman PDF (Reorganize)
public class PdfPageModel : ObservableObject
{
    public string SourceFilePath { get; set; }
    public int OriginalPageIndex { get; set; }
    public int CurrentDisplayIndex { get; set; }
    public BitmapSource Thumbnail { get; set; }
    public bool IsSelected { get; set; }
}

// Task konversi
public class ConversionTask
{
    public string InputPath { get; set; }
    public string OutputPath { get; set; }
    public ConversionType Type { get; set; }
    public CancellationToken CancellationToken { get; set; }
}

public enum ConversionType
{
    PdfToWord, PdfToExcel, PdfToPpt,
    WordToPdf, ExcelToPdf, PptToPdf
}
```

---

## 10. Service Interfaces

```csharp
// Konversi
public interface IConversionService
{
    Task<bool> PdfToWordAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> PdfToExcelAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> PdfToPptAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> WordToPdfAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> ExcelToPdfAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> PptToPdfAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
}

// Kompresi
public interface ICompressService
{
    Task<bool> CompressAsync(string inputPath, string outputPath, CompressionLevel level, IProgress<double> progress, CancellationToken ct);
    Task<long> EstimateCompressedSizeAsync(string inputPath, CompressionLevel level);
}

// Merge
public interface IMergeService
{
    Task<bool> MergeAsync(IEnumerable<string> inputPaths, string outputPath, IProgress<double> progress, CancellationToken ct);
}

// Split
public interface ISplitService
{
    Task<bool> SplitByPageAsync(string inputPath, string outputFolder, string filePrefix, IProgress<double> progress, CancellationToken ct);
    Task<bool> SplitByRangeAsync(string inputPath, string outputFolder, string filePrefix, IEnumerable<PageRange> ranges, IProgress<double> progress, CancellationToken ct);
    Task<bool> SplitEveryNPagesAsync(string inputPath, string outputFolder, string filePrefix, int n, IProgress<double> progress, CancellationToken ct);
}

// Reorganize
public interface IReorganizeService
{
    Task<IList<PdfPageModel>> LoadPagesAsync(string filePath);
    Task<bool> SaveReorganizedAsync(IEnumerable<PdfPageModel> pages, string outputPath, IProgress<double> progress, CancellationToken ct);
}
```

---

## 11. Milestones & Prioritas

| Fase | Fitur | Prioritas | Estimasi |
|---|---|---|---|
| **Phase 1** | Project setup, MVVM scaffold, Navigation shell | Wajib | 1 minggu |
| **Phase 2** | Convert Office ↔ PDF (semua 6 jenis) | Wajib | 2 minggu |
| **Phase 3** | Compress PDF | Wajib | 3 hari |
| **Phase 4** | Merge & Split PDF | Wajib | 1 minggu |
| **Phase 5** | Reorganize PDF (thumbnail, drag-drop, delete, insert) | Wajib | 2 minggu |
| **Phase 6** | UI polish, Dark Mode, Error Handling, Logging | Tinggi | 1 minggu |
| **Phase 7** | Testing, QA, Bug Fix | Wajib | 1 minggu |

---

## 12. Out of Scope (Versi 1.0)

- Edit teks langsung dalam PDF (PDF text editing).
- Tanda tangan digital / e-signature.
- OCR untuk PDF yang di-scan.
- Cloud storage integration (Google Drive, OneDrive).
- Watermark / password protection PDF.
- Mobile version.

> Fitur-fitur di atas dapat dipertimbangkan untuk roadmap versi 2.0.

---

## 13. Acceptance Testing Checklist

- [ ] Semua 6 jenis konversi menghasilkan file output yang valid dan dapat dibuka.
- [ ] Compress menghasilkan file lebih kecil dari original.
- [ ] Merge menghasilkan PDF dengan jumlah halaman = total halaman semua input.
- [ ] Split menghasilkan jumlah file sesuai rule yang dipilih.
- [ ] Reorganize: urutan halaman pada output sesuai urutan yang diatur pengguna.
- [ ] Reorganize: halaman yang dihapus tidak muncul di output.
- [ ] Reorganize: halaman dari PDF tambahan muncul di posisi yang benar.
- [ ] Semua fitur berjalan dengan async tanpa membekukan UI.
- [ ] Error pada satu file tidak menghentikan batch.
- [ ] Aplikasi berjalan di Windows 10 dan Windows 11.

---

*Dokumen ini dibuat sebagai panduan pengembangan untuk AI Agent dan developer. Setiap perubahan requirement harus diperbarui di dokumen ini sebelum implementasi dimulai.*
