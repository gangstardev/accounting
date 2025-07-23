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
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace AccountingApp.Forms
{
    public partial class InvoiceViewerForm : Form
    {
        private readonly Sale _sale;
        private Panel _contentPanel;
        private ToolStrip _toolStrip;

        static InvoiceViewerForm()
        {
            // تنظیم مجوز QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public InvoiceViewerForm(Sale sale)
        {
            _sale = sale;
            InitializeComponent();
            LoadInvoiceData();
        }

        private void InitializeComponent()
        {
            this.Text = $"فاکتور فروش - شماره {_sale.InvoiceNumber}";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

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

            // دکمه پرینت مستقیم
            var btnDirectPrint = new ToolStripButton();
            btnDirectPrint.Text = "پرینت مستقیم";
            btnDirectPrint.Image = CreatePrinterIcon();
            btnDirectPrint.Click += BtnDirectPrint_Click;
            _toolStrip.Items.Add(btnDirectPrint);

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

            // پنل محتوا
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            this.Controls.Add(_contentPanel);
        }

        private void LoadInvoiceData()
        {
            try
            {
                _contentPanel.Controls.Clear();

                // تبدیل تاریخ به شمسی
                var pc = new PersianCalendar();
                var saleDate = _sale.SaleDate;
                string shamsiDate = $"{pc.GetYear(saleDate)}/{pc.GetMonth(saleDate):00}/{pc.GetDayOfMonth(saleDate):00}";

                var mainLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 1,
                    Padding = new Padding(20)
                };

                // هدر فاکتور
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 80,
                    BackColor = Color.White
                };

                var lblTitle = new Label
                {
                    Text = "قهوه سمکو",
                    Font = new Font("Tahoma", 16, FontStyle.Bold),
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40
                };

                var lblSubtitle = new Label
                {
                    Text = "به بلندای کوه دماوند",
                    Font = new Font("Tahoma", 10),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 30
                };

                headerPanel.Controls.Add(lblSubtitle);
                headerPanel.Controls.Add(lblTitle);

                // اطلاعات فاکتور
                var infoPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    BackColor = Color.White
                };

                var lblInvoiceNumber = new Label
                {
                    Text = $"شماره فاکتور: {_sale.InvoiceNumber}",
                    Font = new Font("Tahoma", 10, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                var lblDate = new Label
                {
                    Text = $"تاریخ: {shamsiDate}",
                    Font = new Font("Tahoma", 10, FontStyle.Bold),
                    Location = new Point(10, 35),
                    AutoSize = true
                };

                infoPanel.Controls.Add(lblInvoiceNumber);
                infoPanel.Controls.Add(lblDate);

                // جدول کالاها
                var dataGridView = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    GridColor = Color.LightGray
                };

                dataGridView.Columns.Add("ProductName", "نام کالا");
                dataGridView.Columns.Add("Quantity", "تعداد");
                dataGridView.Columns.Add("UnitPrice", "قیمت واحد");
                dataGridView.Columns.Add("DiscountAmount", "تخفیف");
                dataGridView.Columns.Add("TotalPrice", "جمع کل");

                foreach (var item in _sale.Items)
                {
                    dataGridView.Rows.Add(
                        item.Product.Name,
                        item.Quantity.ToString(),
                        item.UnitPrice.ToString("N0") + " تومان",
                        item.DiscountAmount.ToString("N0") + " تومان",
                        item.TotalPrice.ToString("N0") + " تومان"
                    );
                }

                // جمع کل
                var totalPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 100,
                    BackColor = Color.White
                };

                var lblTotal = new Label
                {
                    Text = $"جمع کل: {_sale.TotalAmount.ToString("N0")} تومان",
                    Font = new Font("Tahoma", 12, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                var lblDiscount = new Label
                {
                    Text = $"تخفیف: {_sale.DiscountAmount.ToString("N0")} تومان",
                    Font = new Font("Tahoma", 12, FontStyle.Bold),
                    ForeColor = Color.Green,
                    Location = new Point(10, 35),
                    AutoSize = true
                };

                var lblFinal = new Label
                {
                    Text = $"مبلغ فاکتور: {_sale.FinalAmount.ToString("N0")} تومان",
                    Font = new Font("Tahoma", 12, FontStyle.Bold),
                    ForeColor = Color.Red,
                    Location = new Point(10, 60),
                    AutoSize = true
                };

                totalPanel.Controls.Add(lblTotal);
                totalPanel.Controls.Add(lblDiscount);
                totalPanel.Controls.Add(lblFinal);

                // فال حافظ
                var falPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 60,
                    BackColor = Color.White
                };

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
                        
                        var lblFal1 = new Label
                        {
                            Text = lines.Length >= 1 ? lines[0] : "به نام خداوند جان و خرد",
                            Font = new Font("Tahoma", 10),
                            ForeColor = Color.Black,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Dock = DockStyle.Top,
                            Height = 25
                        };

                        var lblFal2 = new Label
                        {
                            Text = lines.Length >= 2 ? lines[1] : "کزین برتر اندیشه برنگذرد",
                            Font = new Font("Tahoma", 10),
                            ForeColor = Color.Black,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Dock = DockStyle.Top,
                            Height = 25
                        };

                        falPanel.Controls.Add(lblFal2);
                        falPanel.Controls.Add(lblFal1);
                    }
                }
                catch
                {
                    var lblFal1 = new Label
                    {
                        Text = "به نام خداوند جان و خرد",
                        Font = new Font("Tahoma", 10),
                        ForeColor = Color.Black,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 25
                    };

                    var lblFal2 = new Label
                    {
                        Text = "کزین برتر اندیشه برنگذرد",
                        Font = new Font("Tahoma", 10),
                        ForeColor = Color.Black,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Height = 25
                    };

                    falPanel.Controls.Add(lblFal2);
                    falPanel.Controls.Add(lblFal1);
                }

                mainLayout.Controls.Add(falPanel, 0, 0);
                mainLayout.Controls.Add(totalPanel, 0, 0);
                mainLayout.Controls.Add(dataGridView, 0, 0);
                mainLayout.Controls.Add(infoPanel, 0, 0);
                mainLayout.Controls.Add(headerPanel, 0, 0);

                _contentPanel.Controls.Add(mainLayout);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری اطلاعات فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                // تولید PDF موقت برای پرینت
                var tempPdfPath = Path.Combine(Path.GetTempPath(), $"temp_invoice_{_sale.InvoiceNumber}.pdf");
                ExportInvoiceToPdf(tempPdfPath);
                
                // روش 1: باز کردن PDF و درخواست پرینت از کاربر
                try
                {
                    // استفاده از روش thread-safe برای باز کردن PDF
                    AccountingApp.Utilities.ThreadSafePdfOpener.OpenPdfSafely(tempPdfPath);
                    
                    var result = MessageBox.Show(
                        "فایل PDF باز شد.\n\n" +
                        "برای پرینت:\n" +
                        "1. از منوی File > Print استفاده کنید\n" +
                        "2. یا کلیدهای Ctrl+P را فشار دهید\n\n" +
                        "آیا می‌خواهید فایل PDF را در پوشه‌ای ذخیره کنید؟",
                        "پرینت فاکتور",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );
                    
                    if (result == DialogResult.Yes)
                    {
                        BtnSave_Click(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطا در باز کردن PDF: {ex.Message}\n\nلطفاً فایل را ذخیره کرده و سپس پرینت کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تولید فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpen_Click(object? sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "PDF Files|*.pdf|All Files|*.*",
                    Title = "باز کردن فایل فاکتور"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = openFileDialog.FileName,
                        UseShellExecute = true
                    });
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
                        ExportInvoiceToPdf(sfd.FileName);
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
                    var font = new Font("Tahoma", 10);

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
                "• دکمه پرینت: برای پرینت فاکتور (باز کردن PDF)\n" +
                "• دکمه پرینت مستقیم: برای پرینت مستقیم بدون PDF\n" +
                "• دکمه باز کردن: برای باز کردن فایل‌های PDF\n" +
                "• دکمه ذخیره: برای ذخیره فاکتور به صورت PDF\n" +
                "• دکمه راهنما: نمایش این راهنما",
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
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("قیمت واحد").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("تخفیف").FontFamily("Vazir").FontSize(6).Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(1).AlignCenter().Text("جمع کل").FontFamily("Vazir").FontSize(6).Bold();
                    });

                    // ردیف‌های جدول
                    foreach (var item in _sale.Items)
                    {
                        table.Cell().Padding(1).AlignRight().Text(item.Product.Name).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.Quantity.ToString()).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.UnitPrice.ToString("N0")).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.DiscountAmount.ToString("N0")).FontFamily("Vazir").FontSize(6);
                        table.Cell().Padding(1).AlignCenter().Text(item.TotalPrice.ToString("N0")).FontFamily("Vazir").FontSize(6);
                    }
                });

                // جمع کل، تخفیف و مبلغ نهایی
                column.Item().PaddingTop(2).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(2).Row(row =>
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