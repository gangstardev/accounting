using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text.Json;
using System.Windows.Forms;
using AccountingApp.Models;
using PdfiumViewer;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = System.Drawing.Color;

namespace AccountingApp.Forms
{
    public partial class PdfViewerForm : Form
    {
        private readonly Sale _sale;
        private PdfViewer _pdfViewer;
        private ToolStrip _toolStrip;
        private string _currentPdfPath;

        static PdfViewerForm()
        {
            // تنظیم مجوز QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public PdfViewerForm(Sale sale)
        {
            _sale = sale;
            InitializeComponent();
            GenerateAndLoadPdf();
        }

        private void InitializeComponent()
        {
            this.Text = $"فاکتور فروش - شماره {_sale.InvoiceNumber}";
            this.Size = new System.Drawing.Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.WindowState = FormWindowState.Maximized;

            // ایجاد ToolStrip
            _toolStrip = new ToolStrip();
            _toolStrip.Dock = DockStyle.Top;
            _toolStrip.Renderer = new ToolStripProfessionalRenderer();

            // دکمه پرینت
            var btnPrint = new ToolStripButton();
            btnPrint.Text = "پرینت";
            btnPrint.Image = CreatePrinterIcon();
            btnPrint.Click += BtnPrint_Click;
            _toolStrip.Items.Add(btnPrint);

            // دکمه باز کردن
            var btnOpen = new ToolStripButton();
            btnOpen.Text = "باز کردن";
            btnOpen.Image = CreateOpenIcon();
            btnOpen.Click += BtnOpen_Click;
            _toolStrip.Items.Add(btnOpen);

            // دکمه ذخیره
            var btnSave = new ToolStripButton();
            btnSave.Text = "ذخیره";
            btnSave.Image = CreateSaveIcon();
            btnSave.Click += BtnSave_Click;
            _toolStrip.Items.Add(btnSave);

            // دکمه راهنما
            var btnHelp = new ToolStripButton();
            btnHelp.Text = "راهنما";
            btnHelp.Image = CreateHelpIcon();
            btnHelp.Click += BtnHelp_Click;
            _toolStrip.Items.Add(btnHelp);

            this.Controls.Add(_toolStrip);

            // ایجاد PdfViewer
            _pdfViewer = new PdfViewer();
            _pdfViewer.Dock = DockStyle.Fill;
            _pdfViewer.ShowToolbar = true;
            _pdfViewer.ShowBookmarks = true;

            this.Controls.Add(_pdfViewer);
        }

        private void GenerateAndLoadPdf()
        {
            try
            {
                // تولید PDF موقت
                _currentPdfPath = Path.Combine(Path.GetTempPath(), $"invoice_{_sale.InvoiceNumber}.pdf");
                ExportInvoiceToPdf(_currentPdfPath);

                // بارگذاری PDF در viewer
                using (var stream = new FileStream(_currentPdfPath, FileMode.Open, FileAccess.Read))
                {
                    _pdfViewer.Document = PdfDocument.Load(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری PDF: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                var doc = new System.Drawing.Printing.PrintDocument();

                doc.PrintPage += (s, ev) =>
                {
                    float y = 10;
                    var g = ev.Graphics;
                    var font = new Font("Vazir", 10);

                    // هدر شرکت
                    g.DrawString("قهوه سمکو", new Font("Vazir", 14, FontStyle.Bold), Brushes.Black, 10, y);
                    y += 30;
                    g.DrawString("به بلندای کوه دماوند", font, Brushes.Black, 10, y);
                    y += 30;

                    // تبدیل تاریخ به شمسی
                    var pc = new PersianCalendar();
                    var saleDate = _sale.SaleDate;
                    string shamsiDate = $"{pc.GetYear(saleDate)}/{pc.GetMonth(saleDate):00}/{pc.GetDayOfMonth(saleDate):00}";

                    // اطلاعات فاکتور
                    g.DrawString($"شماره فاکتور: {_sale.InvoiceNumber}", font, Brushes.Black, 10, y);
                    y += 20;
                    g.DrawString($"تاریخ: {shamsiDate}", font, Brushes.Black, 10, y);
                    y += 30;

                    // اطلاعات مشتری
                    if (_sale.Customer != null)
                    {
                        g.DrawString($"مشتری: {_sale.Customer.Name}", font, Brushes.Black, 10, y);
                        y += 20;
                        g.DrawString($"تلفن: {_sale.Customer.Phone}", font, Brushes.Black, 10, y);
                        y += 30;
                    }

                    // جدول کالاها
                    g.DrawString("نام کالا", font, Brushes.Black, 10, y);
                    g.DrawString("تعداد", font, Brushes.Black, 120, y);
                    g.DrawString("قیمت", font, Brushes.Black, 180, y);
                    g.DrawString("جمع", font, Brushes.Black, 240, y);
                    y += 20;

                    foreach (var item in _sale.Items)
                    {
                        g.DrawString(item.Product.Name, font, Brushes.Black, 10, y);
                        g.DrawString(item.Quantity.ToString(), font, Brushes.Black, 120, y);
                        g.DrawString(item.UnitPrice.ToString("N0"), font, Brushes.Black, 180, y);
                        g.DrawString(item.TotalPrice.ToString("N0"), font, Brushes.Black, 240, y);
                        y += 20;
                    }

                    y += 20;
                    g.DrawString($"جمع کل: {_sale.TotalAmount:N0} ریال", font, Brushes.Black, 10, y);
                    y += 20;
                    g.DrawString($"مبلغ نهایی: {_sale.FinalAmount:N0} ریال", new Font("Vazir", 12, FontStyle.Bold), Brushes.Red, 10, y);
                    y += 30;

                    // فال حافظ
                    try
                    {
                        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "hafez_fal.json");
                        if (File.Exists(jsonPath))
                        {
                            var json = File.ReadAllText(jsonPath);
                            var falList = System.Text.Json.JsonSerializer.Deserialize<List<FalItem>>(json);
                            if (falList != null && falList.Count > 0)
                            {
                                var rnd = new Random();
                                var fal = falList[rnd.Next(falList.Count)];
                                
                                // نمایش عنوان فال
                                g.DrawString($"فال حافظ - {fal.title}", new Font("Vazir", 8, FontStyle.Bold), Brushes.Black, 10, y);
                                y += 15;
                                
                                // تقسیم متن فال به خطوط کوتاه‌تر
                                var words = fal.interpreter.Split(' ');
                                var currentLine = "";
                                var maxWidth = 280; // عرض حداکثر برای هر خط
                                
                                foreach (var word in words)
                                {
                                    var testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
                                    var size = g.MeasureString(testLine, font);
                                    
                                    if (size.Width > maxWidth && currentLine.Length > 0)
                                    {
                                        g.DrawString(currentLine, font, Brushes.Black, 10, y);
                                        y += 15;
                                        currentLine = word;
                                    }
                                    else
                                    {
                                        currentLine = testLine;
                                    }
                                }
                                
                                // نمایش آخرین خط
                                if (currentLine.Length > 0)
                                {
                                    g.DrawString(currentLine, font, Brushes.Black, 10, y);
                                    y += 15;
                                }
                            }
                            else
                            {
                                g.DrawString("به نام خداوند جان و خرد", font, Brushes.Black, 10, y);
                                y += 15;
                                g.DrawString("کزین برتر اندیشه برنگذرد", font, Brushes.Black, 10, y);
                            }
                        }
                        else
                        {
                            g.DrawString("به نام خداوند جان و خرد", font, Brushes.Black, 10, y);
                            y += 15;
                            g.DrawString("کزین برتر اندیشه برنگذرد", font, Brushes.Black, 10, y);
                        }
                    }
                    catch
                    {
                        g.DrawString("به نام خداوند جان و خرد", font, Brushes.Black, 10, y);
                        y += 15;
                        g.DrawString("کزین برتر اندیشه برنگذرد", font, Brushes.Black, 10, y);
                    }
                };

                // تنظیم اندازه کاغذ 80mm × 300mm (قابل انعطاف)
                var paperSize = new System.Drawing.Printing.PaperSize("Custom", 315, 1181); // 80mm × 300mm
                doc.DefaultPageSettings.PaperSize = paperSize;
                doc.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                var printDialog = new PrintDialog
                {
                    Document = doc,
                    AllowSomePages = true,
                    AllowSelection = false,
                    AllowPrintToFile = false
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    doc.Print();
                    MessageBox.Show("فاکتور با موفقیت پرینت شد.", "پرینت", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpen_Click(object? sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "PDF Files|*.pdf|All Files|*.*",
                    Title = "باز کردن فایل PDF"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        _pdfViewer.Document = PdfDocument.Load(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در باز کردن فایل: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog 
                { 
                    Filter = "PDF Files|*.pdf", 
                    FileName = $"فاکتور_{_sale.InvoiceNumber}.pdf",
                    Title = "ذخیره فاکتور"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.Copy(_currentPdfPath, sfd.FileName, true);
                        MessageBox.Show("فاکتور با موفقیت ذخیره شد.", "ذخیره", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHelp_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "راهنمای استفاده:\n\n" +
                "• دکمه پرینت: برای پرینت فاکتور\n" +
                "• دکمه باز کردن: برای باز کردن فایل‌های PDF\n" +
                "• دکمه ذخیره: برای ذخیره فاکتور به صورت PDF\n" +
                "• دکمه راهنما: نمایش این راهنما\n\n" +
                "می‌توانید از نوار ابزار PDF برای زوم، جستجو و ناوبری استفاده کنید.",
                "راهنما",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ExportInvoiceToPdf(string filePath)
        {
            try
            {
                // تبدیل تاریخ به شمسی
                var pc = new PersianCalendar();
                var saleDate = _sale.SaleDate;
                string shamsiDate = $"{pc.GetYear(saleDate)}/{pc.GetMonth(saleDate):00}/{pc.GetDayOfMonth(saleDate):00}";

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // تنظیم سایز فاکتور به 80 میلیمتر عرض در 210 میلیمتر طول
                        page.Size(new PageSize(80, 210, Unit.Millimetre));
                        page.Margin(0, Unit.Millimetre);
                        page.DefaultTextStyle(x => x.FontFamily("Vazir").FontSize(8));

                        page.Content().Element(container => ComposeContent(container, shamsiDate));
                    });
                });

                document.GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در تولید PDF: {ex.Message}", ex);
            }
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("قهوه سمکو")
                    .FontFamily("Vazir").FontSize(12).Bold().FontColor(Colors.Black);
                column.Item().AlignCenter().Text("به بلندای کوه دماوند")
                    .FontFamily("Vazir").FontSize(8).FontColor(Colors.Grey.Darken2);
                column.Item().PaddingTop(2);
            });
        }

        private void ComposeContent(IContainer container, string shamsiDate)
        {
            container.Padding(2).Column(column =>
            {
                // هدر شرکت
                column.Item().AlignCenter().Text("قهوه سمکو")
                    .FontFamily("Vazir").FontSize(12).Bold().FontColor(Colors.Black);
                column.Item().AlignCenter().Text("به بلندای کوه دماوند")
                    .FontFamily("Vazir").FontSize(8).FontColor(Colors.Grey.Darken2);
                column.Item().PaddingTop(2);

                // اطلاعات فاکتور
                column.Item().PaddingBottom(2).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text($"شماره فاکتور: {_sale.InvoiceNumber}").FontFamily("Vazir").FontSize(7).Bold();
                    row.RelativeItem().AlignCenter().Text($"تاریخ و ساعت: {shamsiDate}").FontFamily("Vazir").FontSize(7).Bold();
                });

                // جدول کالاها
                column.Item().PaddingVertical(2).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // نام کالا
                        columns.RelativeColumn(1); // تعداد
                        columns.RelativeColumn(1.5f); // قیمت واحد
                        columns.RelativeColumn(1.5f); // جمع کل
                    });

                    // هدر جدول
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("نام کالا").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("تعداد").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("قیمت واحد").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("جمع کل").FontFamily("Vazir").FontSize(6).Bold();
                    });

                    // ردیف‌های جدول
                    foreach (var item in _sale.Items)
                    {
                        table.Cell().Padding(1).AlignRight().Text(item.Product.Name).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.Quantity.ToString()).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.UnitPrice.ToString("N0")).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.TotalPrice.ToString("N0")).FontFamily("Vazir").FontSize(6);
                    }
                });

                // جمع کل و مبلغ نهایی
                column.Item().PaddingTop(2).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(2).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("جمع کل:").FontFamily("Vazir").FontSize(7).Bold();
                    row.RelativeItem().AlignLeft().Text(_sale.TotalAmount.ToString("N0") + " ریال").FontFamily("Vazir").FontSize(7).Bold();
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("مبلغ فاکتور:").FontFamily("Vazir").FontSize(7).Bold();
                    row.RelativeItem().AlignLeft().Text(_sale.FinalAmount.ToString("N0") + " ریال").FontFamily("Vazir").FontSize(7).Bold().FontColor(Colors.Red.Medium);
                });

                // فال حافظ
                column.Item().PaddingTop(2).Padding(2).Column(col =>
                {
                    try
                    {
                        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "hafez_fal.json");
                        var json = File.ReadAllText(jsonPath);
                        var falList = System.Text.Json.JsonSerializer.Deserialize<List<FalItem>>(json);
                        if (falList != null && falList.Count > 0)
                        {
                            var rnd = new Random();
                            var fal = falList[rnd.Next(falList.Count)];
                            var lines = fal.interpreter.Split('\n');
                            if (lines.Length >= 1)
                                col.Item().AlignCenter().Text(lines[0]).FontFamily("Vazir").FontSize(7).FontColor(Colors.Black);
                            if (lines.Length >= 2)
                                col.Item().AlignCenter().Text(lines[1]).FontFamily("Vazir").FontSize(7).FontColor(Colors.Black);
                        }
                        else
                        {
                            col.Item().AlignCenter().Text("به نام خداوند جان و خرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Black);
                            col.Item().AlignCenter().Text("کزین برتر اندیشه برنگذرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Black);
                        }
                    }
                    catch
                    {
                        col.Item().AlignCenter().Text("به نام خداوند جان و خرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Black);
                        col.Item().AlignCenter().Text("کزین برتر اندیشه برنگذرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Black);
                    }
                });
            });
        }

        // مدل فال حافظ برای json
        public class FalItem
        {
            public int id { get; set; }
            public string title { get; set; }
            public string interpreter { get; set; }
        }

        // ایجاد آیکون‌های ساده
        private System.Drawing.Image CreatePrinterIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(Pens.Black, 2, 4, 12, 8);
                g.DrawRectangle(Pens.Black, 4, 2, 8, 4);
                g.DrawLine(Pens.Black, 6, 12, 6, 14);
                g.DrawLine(Pens.Black, 10, 12, 10, 14);
            }
            return bitmap;
        }

        private System.Drawing.Image CreateOpenIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(Pens.Black, 2, 6, 12, 8);
                g.DrawLine(Pens.Black, 6, 2, 6, 6);
                g.DrawLine(Pens.Black, 6, 2, 10, 2);
                g.DrawLine(Pens.Black, 10, 2, 10, 6);
            }
            return bitmap;
        }

        private System.Drawing.Image CreateSaveIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(Pens.Black, 2, 6, 12, 8);
                g.DrawRectangle(Pens.Black, 4, 2, 8, 4);
                g.DrawLine(Pens.Black, 6, 2, 6, 6);
                g.DrawLine(Pens.Black, 10, 2, 10, 6);
            }
            return bitmap;
        }

        private System.Drawing.Image CreateHelpIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawEllipse(Pens.Black, 2, 2, 12, 12);
                g.DrawString("?", new Font("Arial", 8, FontStyle.Bold), Brushes.Black, 6, 1);
            }
            return bitmap;
        }
    }
}