using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text.Json;
using System.Windows.Forms;
using AccountingApp.Models;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace AccountingApp.Forms
{
    public partial class WebBrowserInvoiceForm : Form
    {
        private readonly Sale _sale;
        private WebBrowser _webBrowser;
        private ToolStrip _toolStrip;
        private string _currentPdfPath;

        static WebBrowserInvoiceForm()
        {
            // تنظیم مجوز QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public WebBrowserInvoiceForm(Sale sale)
        {
            _sale = sale;
            InitializeComponent();
            GenerateAndLoadPdf();
        }

        private void InitializeComponent()
        {
            this.Text = "نمایش فاکتور فروش - WebBrowser";
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

            // دکمه پرینت مستقیم
            var btnDirectPrint = new ToolStripButton("پرینت مستقیم", CreatePrinterIcon(), BtnDirectPrint_Click)
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

            _toolStrip.Items.AddRange(new ToolStripItem[] { btnPrint, btnDirectPrint, btnSave, btnHelp });

            // ایجاد کنترل WebBrowser
            _webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true
            };

            this.Controls.Add(_webBrowser);
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

                // بارگذاری PDF در WebBrowser
                var uri = new Uri(_currentPdfPath);
                _webBrowser.Navigate(uri);
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
                if (_webBrowser.Document != null)
                {
                    _webBrowser.ShowPrintDialog();
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

        private void BtnDirectPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                var doc = new System.Drawing.Printing.PrintDocument();

                doc.PrintPage += (s, ev) =>
                {
                    float y = 10;
                    var g = ev.Graphics;
                    var font = new Font("Tahoma", 10); // تغییر به Tahoma برای جلوگیری از خطا

                    // هدر شرکت
                    g.DrawString("قهوه سمکو", new Font("Tahoma", 14, FontStyle.Bold), Brushes.Black, 10, y);
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
                    g.DrawString("تعداد", font, Brushes.Black, 100, y);
                    g.DrawString("قیمت", font, Brushes.Black, 140, y);
                    g.DrawString("تخفیف", font, Brushes.Black, 200, y);
                    g.DrawString("جمع", font, Brushes.Black, 260, y);
                    y += 20;

                    foreach (var item in _sale.Items)
                    {
                        g.DrawString(item.Product.Name, font, Brushes.Black, 10, y);
                        g.DrawString(item.Quantity.ToString(), font, Brushes.Black, 100, y);
                        g.DrawString(item.UnitPrice.ToString("N0"), font, Brushes.Black, 140, y);
                        g.DrawString(item.DiscountAmount.ToString("N0"), font, Brushes.Black, 200, y);
                        g.DrawString(item.TotalPrice.ToString("N0"), font, Brushes.Black, 260, y);
                        y += 20;
                    }

                    y += 20;
                    g.DrawString($"جمع کل: {_sale.TotalAmount:N0} تومان", font, Brushes.Black, 10, y);
                    y += 20;
                    
                    // محاسبه تخفیف کل
                    var itemsDiscount = _sale.Items.Sum(item => item.DiscountAmount);
                    var totalDiscount = itemsDiscount + _sale.DiscountAmount;
                    g.DrawString($"تخفیف کل: {totalDiscount:N0} تومان", font, Brushes.Green, 10, y);
                    y += 20;
                    g.DrawString($"مبلغ نهایی: {_sale.FinalAmount:N0} تومان", new Font("Tahoma", 12, FontStyle.Bold), Brushes.Red, 10, y);
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
                                g.DrawString($"فال حافظ - {fal.title}", new Font("Tahoma", 8, FontStyle.Bold), Brushes.Black, 10, y);
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
                MessageBox.Show($"خطا در پرینت مستقیم: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHelp_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "راهنمای استفاده:\n\n" +
                "• دکمه پرینت: برای پرینت فاکتور (PDF)\n" +
                "• دکمه پرینت مستقیم: برای پرینت مستقیم بدون PDF\n" +
                "• دکمه ذخیره: برای ذخیره فاکتور به صورت PDF\n" +
                "• دکمه راهنما: نمایش این راهنما\n\n" +
                "می‌توانید از نوار ابزار مرورگر برای زوم، جستجو و ناوبری استفاده کنید.",
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
                        // تنظیم اندازه صفحه دقیق به میلیمتر - 7cm x 21cm
                        page.Size(new PageSize(70, 210, Unit.Millimetre));
                        
                        // تنظیم حاشیه
                        page.Margin(0, Unit.Millimetre);
                        page.PageColor(Colors.White);
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



        private void ComposeContent(IContainer container, string shamsiDate)
        {
            container.Padding(2).Column(column =>
            {
                // هدر شرکت
                column.Item().AlignCenter().Text("قهوه سمکو")
                    .FontFamily("Vazir").FontSize(12).Bold().FontColor(Colors.Black);
                column.Item().AlignCenter().Text("به بلندای کوه دماوند")
                    .FontFamily("Vazir").FontSize(6).FontColor(Colors.Grey.Darken2);
                column.Item().PaddingTop(2);

                // اطلاعات فاکتور
                column.Item().PaddingBottom(3).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text($"شماره: {_sale.InvoiceNumber}").FontFamily("Vazir").FontSize(7).Bold();
                    row.RelativeItem().AlignCenter().Text($"تاریخ: {shamsiDate}").FontFamily("Vazir").FontSize(7).Bold();
                });

                // اطلاعات مشتری
                if (_sale.Customer != null)
                {
                    column.Item().PaddingBottom(3).Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text($"مشتری: {_sale.Customer.Name}").FontFamily("Vazir").FontSize(6);
                        row.RelativeItem().AlignCenter().Text($"تلفن: {_sale.Customer.Phone}").FontFamily("Vazir").FontSize(6);
                    });
                }

                // جدول کالاها
                column.Item().PaddingVertical(3).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.5f); // نام کالا
                        columns.RelativeColumn(0.8f); // تعداد
                        columns.RelativeColumn(1.2f); // قیمت واحد
                        columns.RelativeColumn(1.2f); // تخفیف
                        columns.RelativeColumn(1.2f); // جمع کل
                    });

                    // هدر جدول
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("نام کالا").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("تعداد").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("قیمت").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("تخفیف").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("جمع").FontFamily("Vazir").FontSize(6).Bold();
                    });

                    // ردیف‌های جدول
                    foreach (var item in _sale.Items)
                    {
                        table.Cell().Padding(1).AlignRight().Text(item.Product.Name).FontFamily("Vazir").FontSize(5);
                        table.Cell().Padding(1).AlignCenter().Text(item.Quantity.ToString()).FontFamily("Vazir").FontSize(5);
                        table.Cell().Padding(1).AlignCenter().Text(item.UnitPrice.ToString("N0")).FontFamily("Vazir").FontSize(5);
                        table.Cell().Padding(1).AlignCenter().Text(item.DiscountAmount.ToString("N0")).FontFamily("Vazir").FontSize(5);
                        table.Cell().Padding(1).AlignCenter().Text(item.TotalPrice.ToString("N0")).FontFamily("Vazir").FontSize(5);
                    }
                });

                // جمع کل، تخفیف و مبلغ نهایی
                column.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("جمع کل:").FontFamily("Vazir").FontSize(7).Bold();
                    row.RelativeItem().AlignLeft().Text(_sale.TotalAmount.ToString("N0") + " تومان").FontFamily("Vazir").FontSize(7).Bold();
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("تخفیف کل:").FontFamily("Vazir").FontSize(7).Bold();
                    var itemsDiscount = _sale.Items.Sum(item => item.DiscountAmount);
                    var totalDiscount = itemsDiscount + _sale.DiscountAmount;
                    row.RelativeItem().AlignLeft().Text(totalDiscount.ToString("N0") + " تومان").FontFamily("Vazir").FontSize(7).Bold().FontColor(Colors.Green.Medium);
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("مبلغ فاکتور:").FontFamily("Vazir").FontSize(7).Bold();
                    row.RelativeItem().AlignLeft().Text(_sale.FinalAmount.ToString("N0") + " تومان").FontFamily("Vazir").FontSize(7).Bold().FontColor(Colors.Red.Medium);
                });

                // فال حافظ
                column.Item().PaddingTop(5).Padding(3).Column(col =>
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
                                        .FontFamily("Vazir").FontSize(6).Bold();
                                    col.Item().AlignCenter().Text(lines[0])
                                        .FontFamily("Vazir").FontSize(5).Italic();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // اگر فایل فال موجود نبود، پیام پیش‌فرض
                        col.Item().AlignCenter().Text("به نام خداوند جان و خرد")
                            .FontFamily("Vazir").FontSize(6).Bold();
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
                _webBrowser?.Dispose();
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