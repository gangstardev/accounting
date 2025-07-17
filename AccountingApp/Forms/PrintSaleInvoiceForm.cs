using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingApp.Models;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WinFormsFont = System.Drawing.Font;
using WinFormsRectangle = System.Drawing.Rectangle;
using WinFormsColor = System.Drawing.Color;
using System.IO;
using System.Collections.Generic;

namespace AccountingApp.Forms
{
    public partial class PrintSaleInvoiceForm : Form
    {
        private readonly Sale _sale;

        static PrintSaleInvoiceForm()
        {
            // تنظیم مجوز QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public PrintSaleInvoiceForm(Sale sale)
        {
            _sale = sale;
            InitializeComponent();
            LoadInvoiceData();
        }

        private void InitializeComponent()
        {
            this.Text = "پرینت فاکتور فروش";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // پنل فاکتور
            var invoicePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20),
                BackColor = WinFormsColor.White
            };

            var invoiceLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(10)
            };

            invoiceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            invoiceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // عنوان فاکتور
            var lblInvoiceTitle = new Label 
            { 
                Text = "فاکتور فروش", 
                Font = new WinFormsFont("Tahoma", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = WinFormsColor.DarkBlue
            };
            invoiceLayout.SetColumnSpan(lblInvoiceTitle, 2);
            invoiceLayout.Controls.Add(lblInvoiceTitle, 0, 0);

            // اطلاعات فاکتور
            var lblInvoiceNumber = new Label { Text = "شماره فاکتور:", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold) };
            var lblInvoiceNumberValue = new Label { Name = "lblInvoiceNumberValue", Font = new WinFormsFont("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblInvoiceNumber, 0, 1);
            invoiceLayout.Controls.Add(lblInvoiceNumberValue, 1, 1);

            var lblSaleDate = new Label { Text = "تاریخ فروش:", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold) };
            var lblSaleDateValue = new Label { Name = "lblSaleDateValue", Font = new WinFormsFont("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblSaleDate, 0, 2);
            invoiceLayout.Controls.Add(lblSaleDateValue, 1, 2);

            // اطلاعات مشتری
            var lblCustomerTitle = new Label 
            { 
                Text = "اطلاعات مشتری", 
                Font = new WinFormsFont("Tahoma", 12, FontStyle.Bold),
                ForeColor = WinFormsColor.DarkGreen
            };
            invoiceLayout.SetColumnSpan(lblCustomerTitle, 2);
            invoiceLayout.Controls.Add(lblCustomerTitle, 0, 3);

            var lblCustomerName = new Label { Text = "نام مشتری:", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold) };
            var lblCustomerNameValue = new Label { Name = "lblCustomerNameValue", Font = new WinFormsFont("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblCustomerName, 0, 4);
            invoiceLayout.Controls.Add(lblCustomerNameValue, 1, 4);

            var lblCustomerPhone = new Label { Text = "تلفن:", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold) };
            var lblCustomerPhoneValue = new Label { Name = "lblCustomerPhoneValue", Font = new WinFormsFont("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblCustomerPhone, 0, 5);
            invoiceLayout.Controls.Add(lblCustomerPhoneValue, 1, 5);

            // جدول آیتم‌ها
            var lblItemsTitle = new Label 
            { 
                Text = "آیتم‌های فاکتور", 
                Font = new WinFormsFont("Tahoma", 12, FontStyle.Bold),
                ForeColor = WinFormsColor.DarkGreen
            };
            invoiceLayout.SetColumnSpan(lblItemsTitle, 2);
            invoiceLayout.Controls.Add(lblItemsTitle, 0, 6);

            var dgvItems = new DataGridView
            {
                Name = "dgvItems",
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new WinFormsFont("Tahoma", 9),
                BackgroundColor = WinFormsColor.White,
                GridColor = WinFormsColor.LightGray
            };
            invoiceLayout.SetColumnSpan(dgvItems, 2);
            invoiceLayout.Controls.Add(dgvItems, 0, 7);

            // جمع‌کل
            var lblTotalTitle = new Label { Text = "جمع کل:", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold) };
            var lblTotalValue = new Label { Name = "lblTotalValue", Font = new WinFormsFont("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblTotalTitle, 0, 8);
            invoiceLayout.Controls.Add(lblTotalValue, 1, 8);

            var lblFinalTitle = new Label { Text = "مبلغ نهایی:", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold) };
            var lblFinalValue = new Label { Name = "lblFinalValue", Font = new WinFormsFont("Tahoma", 10, FontStyle.Bold), ForeColor = WinFormsColor.DarkRed };
            invoiceLayout.Controls.Add(lblFinalTitle, 0, 9);
            invoiceLayout.Controls.Add(lblFinalValue, 1, 9);

            invoicePanel.Controls.Add(invoiceLayout);

            // پنل دکمه‌ها
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };

            var btnPrint = new Button
            {
                Text = "پرینت",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(10, 12),
                BackColor = WinFormsColor.Blue,
                ForeColor = WinFormsColor.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrint.Click += BtnPrint_Click;

            var btnClose = new Button
            {
                Text = "بستن",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(120, 12),
                BackColor = WinFormsColor.Gray,
                ForeColor = WinFormsColor.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += BtnClose_Click;

            var btnSavePdf = new Button
            {
                Text = "ذخیره PDF",
                Size = new System.Drawing.Size(120, 35),
                Location = new System.Drawing.Point(230, 12),
                BackColor = WinFormsColor.DarkRed,
                ForeColor = WinFormsColor.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSavePdf.Click += BtnSavePdf_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnPrint, btnClose, btnSavePdf });

            mainPanel.Controls.Add(invoicePanel);
            mainPanel.Controls.Add(buttonPanel);
            this.Controls.Add(mainPanel);
        }

        private void LoadInvoiceData()
        {
            try
            {
                // اطلاعات فاکتور
                var lblInvoiceNumberValue = this.Controls.Find("lblInvoiceNumberValue", true)[0] as Label;
                lblInvoiceNumberValue!.Text = _sale.InvoiceNumber;

                var lblSaleDateValue = this.Controls.Find("lblSaleDateValue", true)[0] as Label;
                lblSaleDateValue!.Text = _sale.SaleDate.ToString("yyyy/MM/dd");

                // اطلاعات مشتری
                var lblCustomerNameValue = this.Controls.Find("lblCustomerNameValue", true)[0] as Label;
                lblCustomerNameValue!.Text = _sale.Customer.Name;

                var lblCustomerPhoneValue = this.Controls.Find("lblCustomerPhoneValue", true)[0] as Label;
                lblCustomerPhoneValue!.Text = _sale.Customer.Phone ?? "-";

                // جدول آیتم‌ها
                var dgvItems = this.Controls.Find("dgvItems", true)[0] as DataGridView;
                if (dgvItems != null)
                {
                    dgvItems.Columns.Clear();
                    dgvItems.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "نام محصول", Width = 200 },
                        new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "تعداد", Width = 80 },
                        new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "قیمت واحد", Width = 120 },
                        new DataGridViewTextBoxColumn { Name = "TotalPrice", HeaderText = "قیمت کل", Width = 120 },
                        new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                        new DataGridViewTextBoxColumn { Name = "FinalPrice", HeaderText = "قیمت نهایی", Width = 120 }
                    });

                    dgvItems.Rows.Clear();
                    if (_sale.Items != null && _sale.Items.Count > 0)
                    {
                        foreach (var item in _sale.Items)
                        {
                            if (item != null && item.Product != null)
                            {
                                dgvItems.Rows.Add(
                                    item.Product.Name,
                                    item.Quantity,
                                    item.UnitPrice.ToString("N0"),
                                    item.TotalPrice.ToString("N0"),
                                    item.DiscountAmount.ToString("N0"),
                                    item.FinalPrice.ToString("N0")
                                );
                            }
                        }
                    }
                }

                // جمع‌کل
                var lblTotalValue = this.Controls.Find("lblTotalValue", true)[0] as Label;
                lblTotalValue!.Text = _sale.TotalAmount.ToString("N0") + " تومان";

                var lblFinalValue = this.Controls.Find("lblFinalValue", true)[0] as Label;
                lblFinalValue!.Text = _sale.FinalAmount.ToString("N0") + " تومان";
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
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    // تولید PDF موقت برای پرینت
                    var tempPdfPath = Path.Combine(Path.GetTempPath(), $"temp_invoice_{_sale.InvoiceNumber}.pdf");
                    ExportInvoiceToPdf(tempPdfPath);
                    
                    // پرینت فایل PDF
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = tempPdfPath;
                    process.StartInfo.Verb = "print";
                    process.Start();
                    
                    MessageBox.Show("فاکتور با موفقیت پرینت شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSavePdf_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = $"فاکتور_{_sale.InvoiceNumber}.pdf" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        ExportInvoiceToPdf(sfd.FileName);
                        MessageBox.Show("فاکتور با موفقیت به PDF ذخیره شد.", "ذخیره PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره PDF: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                        // تنظیم سایز فاکتور به 72.1 میلی‌متر در 210 میلی‌متر
                        page.Size(new PageSize(72.1f, 210f, Unit.Millimetre));
                        page.Margin(2, Unit.Millimetre);
                        page.DefaultTextStyle(x => x.FontFamily("Vazir").FontSize(8));

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

                // اطلاعات تماس و آدرس
                column.Item().PaddingTop(2).Padding(2).Column(col =>
                {
                    try
                    {
                        // خواندن فال از فایل json به صورت تصادفی
                        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "hafez_fal.json");
                        var json = File.ReadAllText(jsonPath);
                        var falList = System.Text.Json.JsonSerializer.Deserialize<List<FalItem>>(json);
                        if (falList != null && falList.Count > 0)
                        {
                            var rnd = new Random();
                            var fal = falList[rnd.Next(falList.Count)];
                            var lines = fal.interpreter.Split('\n');
                            if (lines.Length >= 1)
                                col.Item().AlignCenter().Text(lines[0]).FontFamily("Vazir").FontSize(7).FontColor(Colors.Brown.Darken2);
                            if (lines.Length >= 2)
                                col.Item().AlignCenter().Text(lines[1]).FontFamily("Vazir").FontSize(7).FontColor(Colors.Brown.Darken2);
                        }
                        else
                        {
                            col.Item().AlignCenter().Text("به نام خداوند جان و خرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Brown.Darken2);
                            col.Item().AlignCenter().Text("کزین برتر اندیشه برنگذرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Brown.Darken2);
                        }
                    }
                    catch
                    {
                        // در صورت خطا، فال پیش‌فرض نمایش داده می‌شود
                        col.Item().AlignCenter().Text("به نام خداوند جان و خرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Brown.Darken2);
                        col.Item().AlignCenter().Text("کزین برتر اندیشه برنگذرد").FontFamily("Vazir").FontSize(7).FontColor(Colors.Brown.Darken2);
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
    }
} 