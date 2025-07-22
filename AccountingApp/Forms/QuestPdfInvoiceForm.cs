using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingApp.Models;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfiumViewer;
using System.IO;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace AccountingApp.Forms
{
    public partial class QuestPdfInvoiceForm : Form
    {
        private readonly Sale _sale;
        private PdfViewer _pdfViewer;
        private ToolStrip _toolStrip;
        private string _currentPdfPath;

        static QuestPdfInvoiceForm()
        {
            // تنظیم مجوز QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public QuestPdfInvoiceForm(Sale sale)
        {
            _sale = sale;
            InitializeComponent();
            GenerateAndLoadPdf();
        }

        private void InitializeComponent()
        {
            this.Text = "نمایش فاکتور فروش - QuestPDF";
            this.Size = new System.Drawing.Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // ایجاد نوار ابزار
            _toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                RightToLeft = RightToLeft.Yes
            };

            // دکمه پرینت
            var btnPrint = new ToolStripButton("پرینت", CreatePrinterIcon(), BtnPrint_Click)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            // دکمه ذخیره
            var btnSave = new ToolStripButton("ذخیره", CreateSaveIcon(), BtnSave_Click)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            // دکمه راهنما
            var btnHelp = new ToolStripButton("راهنما", CreateHelpIcon(), BtnHelp_Click)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            _toolStrip.Items.AddRange(new ToolStripItem[] { btnPrint, btnSave, btnHelp });

            // ایجاد کنترل PDF Viewer
            _pdfViewer = new PdfViewer
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(_pdfViewer);
            this.Controls.Add(_toolStrip);
        }

        private void GenerateAndLoadPdf()
        {
            try
            {
                // ایجاد مسیر موقت برای فایل PDF
                var tempPath = Path.GetTempPath();
                var fileName = $"فاکتور_{_sale.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                _currentPdfPath = Path.Combine(tempPath, fileName);

                // تولید PDF با QuestPDF
                ExportInvoiceToPdf(_currentPdfPath);

                // بارگذاری PDF در viewer
                using (var stream = new FileStream(_currentPdfPath, FileMode.Open, FileAccess.Read))
                {
                    _pdfViewer.Document = PdfDocument.Load(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تولید PDF: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_pdfViewer.Document != null)
                {
                    using (var printDialog = new PrintDialog())
                    {
                        if (printDialog.ShowDialog() == DialogResult.OK)
                        {
                            // استفاده از PrintDocument استاندارد
                            using (var printDocument = new System.Drawing.Printing.PrintDocument())
                            {
                                printDocument.PrinterSettings = printDialog.PrinterSettings;
                                printDocument.PrintPage += (sender, e) =>
                                {
                                    // اینجا کد پرینت PDF اضافه می‌شود
                                    MessageBox.Show("پرینت در حال توسعه است.", "اطلاع", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                };
                                printDocument.Print();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        // تنظیم سایز فاکتور به 80mm x 210mm
                        page.Size(new PageSize(80, 210, Unit.Millimetre));
                        page.Margin(0, Unit.Millimetre);
                        page.DefaultTextStyle(x => x.FontFamily("Vazir").FontSize(10));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(container => ComposeContent(container, shamsiDate));
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
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
                    .FontFamily("Vazir").FontSize(16).Bold().FontColor(Colors.Black);
                column.Item().AlignCenter().Text("به بلندای کوه دماوند")
                    .FontFamily("Vazir").FontSize(10).FontColor(Colors.Grey.Darken2);
                column.Item().PaddingTop(5);
            });
        }

        private void ComposeContent(IContainer container, string shamsiDate)
        {
            container.Padding(5).Column(column =>
            {
                // اطلاعات فاکتور
                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text($"شماره فاکتور: {_sale.InvoiceNumber}").FontFamily("Vazir").FontSize(12).Bold();
                    row.RelativeItem().AlignCenter().Text($"تاریخ: {shamsiDate}").FontFamily("Vazir").FontSize(12).Bold();
                });

                // اطلاعات مشتری
                if (_sale.Customer != null)
                {
                    column.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text($"نام مشتری: {_sale.Customer.Name}").FontFamily("Vazir").FontSize(11);
                        row.RelativeItem().AlignCenter().Text($"تلفن: {_sale.Customer.Phone}").FontFamily("Vazir").FontSize(11);
                    });
                }

                // جدول کالاها
                column.Item().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // نام کالا
                        columns.RelativeColumn(1); // تعداد
                        columns.RelativeColumn(1.5f); // قیمت واحد
                        columns.RelativeColumn(1.5f); // جمع کل
                    });

                    // هدر جدول
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("نام کالا").FontFamily("Vazir").FontSize(10).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("تعداد").FontFamily("Vazir").FontSize(10).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("قیمت واحد").FontFamily("Vazir").FontSize(10).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("جمع کل").FontFamily("Vazir").FontSize(10).Bold();
                    });

                    // ردیف‌های جدول
                    foreach (var item in _sale.Items)
                    {
                        table.Cell().Padding(5).AlignRight().Text(item.Product.Name).FontFamily("Vazir").FontSize(9);
                        table.Cell().Padding(5).AlignCenter().Text(item.Quantity.ToString()).FontFamily("Vazir").FontSize(9);
                        table.Cell().Padding(5).AlignCenter().Text(item.UnitPrice.ToString("N0")).FontFamily("Vazir").FontSize(9);
                        table.Cell().Padding(5).AlignCenter().Text(item.TotalPrice.ToString("N0")).FontFamily("Vazir").FontSize(9);
                    }
                });

                // جمع کل و مبلغ نهایی
                column.Item().PaddingTop(10).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("جمع کل:").FontFamily("Vazir").FontSize(12).Bold();
                    row.RelativeItem().AlignLeft().Text(_sale.TotalAmount.ToString("N0") + " تومان").FontFamily("Vazir").FontSize(12).Bold();
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("مبلغ فاکتور:").FontFamily("Vazir").FontSize(12).Bold();
                    row.RelativeItem().AlignLeft().Text(_sale.FinalAmount.ToString("N0") + " تومان").FontFamily("Vazir").FontSize(12).Bold().FontColor(Colors.Red.Medium);
                });

                // فال حافظ
                column.Item().PaddingTop(15).Padding(10).Column(col =>
                {
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
                                var lines = fal.interpreter.Split('\n');
                                if (lines.Length >= 1)
                                {
                                    col.Item().AlignCenter().Text("به نام خداوند جان و خرد")
                                        .FontFamily("Vazir").FontSize(10).Bold();
                                    col.Item().AlignCenter().Text(lines[0])
                                        .FontFamily("Vazir").FontSize(9).Italic();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // اگر فایل فال موجود نبود، پیام پیش‌فرض
                        col.Item().AlignCenter().Text("به نام خداوند جان و خرد")
                            .FontFamily("Vazir").FontSize(10).Bold();
                    }
                });
            });
        }

        public class FalItem
        {
            public int id { get; set; }
            public string title { get; set; } = "";
            public string interpreter { get; set; } = "";
        }

        private System.Drawing.Image CreatePrinterIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(Pens.Black, 2, 4, 12, 8);
                g.DrawRectangle(Pens.Black, 4, 2, 8, 2);
                g.DrawLine(Pens.Black, 6, 8, 10, 8);
                g.DrawLine(Pens.Black, 6, 10, 10, 10);
                g.DrawLine(Pens.Black, 6, 12, 10, 12);
            }
            return bitmap;
        }

        private System.Drawing.Image CreateSaveIcon()
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(Brushes.Black, 3, 2, 10, 12);
                g.FillRectangle(Brushes.White, 5, 4, 6, 8);
                g.FillRectangle(Brushes.Black, 6, 6, 4, 4);
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
                g.DrawString("?", new Font("Arial", 8, FontStyle.Bold), Brushes.Black, 6, 4);
            }
            return bitmap;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pdfViewer?.Dispose();
                _toolStrip?.Dispose();
                
                // حذف فایل موقت
                try
                {
                    if (File.Exists(_currentPdfPath))
                    {
                        File.Delete(_currentPdfPath);
                    }
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
} 