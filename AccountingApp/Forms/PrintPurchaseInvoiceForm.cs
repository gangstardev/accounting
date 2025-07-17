using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingApp.Models;

namespace AccountingApp.Forms
{
    public partial class PrintPurchaseInvoiceForm : Form
    {
        private readonly Purchase _purchase;

        public PrintPurchaseInvoiceForm(Purchase purchase)
        {
            _purchase = purchase;
            InitializeComponent();
            LoadInvoiceData();
        }

        private void InitializeComponent()
        {
            this.Text = "پرینت فاکتور خرید";
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
                BackColor = Color.White
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
                Text = "فاکتور خرید", 
                Font = new Font("Tahoma", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue
            };
            invoiceLayout.SetColumnSpan(lblInvoiceTitle, 2);
            invoiceLayout.Controls.Add(lblInvoiceTitle, 0, 0);

            // اطلاعات فاکتور
            var lblInvoiceNumber = new Label { Text = "شماره فاکتور:", Font = new Font("Tahoma", 10, FontStyle.Bold) };
            var lblInvoiceNumberValue = new Label { Name = "lblInvoiceNumberValue", Font = new Font("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblInvoiceNumber, 0, 1);
            invoiceLayout.Controls.Add(lblInvoiceNumberValue, 1, 1);

            var lblPurchaseDate = new Label { Text = "تاریخ خرید:", Font = new Font("Tahoma", 10, FontStyle.Bold) };
            var lblPurchaseDateValue = new Label { Name = "lblPurchaseDateValue", Font = new Font("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblPurchaseDate, 0, 2);
            invoiceLayout.Controls.Add(lblPurchaseDateValue, 1, 2);

            // اطلاعات تامین‌کننده
            var lblSupplierTitle = new Label 
            { 
                Text = "اطلاعات تامین‌کننده", 
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };
            invoiceLayout.SetColumnSpan(lblSupplierTitle, 2);
            invoiceLayout.Controls.Add(lblSupplierTitle, 0, 3);

            var lblSupplierName = new Label { Text = "نام تامین‌کننده:", Font = new Font("Tahoma", 10, FontStyle.Bold) };
            var lblSupplierNameValue = new Label { Name = "lblSupplierNameValue", Font = new Font("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblSupplierName, 0, 4);
            invoiceLayout.Controls.Add(lblSupplierNameValue, 1, 4);

            var lblSupplierPhone = new Label { Text = "تلفن:", Font = new Font("Tahoma", 10, FontStyle.Bold) };
            var lblSupplierPhoneValue = new Label { Name = "lblSupplierPhoneValue", Font = new Font("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblSupplierPhone, 0, 5);
            invoiceLayout.Controls.Add(lblSupplierPhoneValue, 1, 5);

            // جدول آیتم‌ها
            var lblItemsTitle = new Label 
            { 
                Text = "آیتم‌های فاکتور", 
                Font = new Font("Tahoma", 12, FontStyle.Bold),
                ForeColor = Color.DarkGreen
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
                Font = new Font("Tahoma", 9),
                BackgroundColor = Color.White,
                GridColor = Color.LightGray
            };
            invoiceLayout.SetColumnSpan(dgvItems, 2);
            invoiceLayout.Controls.Add(dgvItems, 0, 7);

            // جمع‌کل
            var lblTotalTitle = new Label { Text = "جمع کل:", Font = new Font("Tahoma", 10, FontStyle.Bold) };
            var lblTotalValue = new Label { Name = "lblTotalValue", Font = new Font("Tahoma", 10) };
            invoiceLayout.Controls.Add(lblTotalTitle, 0, 8);
            invoiceLayout.Controls.Add(lblTotalValue, 1, 8);

            var lblFinalTitle = new Label { Text = "مبلغ نهایی:", Font = new Font("Tahoma", 10, FontStyle.Bold) };
            var lblFinalValue = new Label { Name = "lblFinalValue", Font = new Font("Tahoma", 10, FontStyle.Bold), ForeColor = Color.DarkRed };
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
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrint.Click += BtnPrint_Click;

            var btnClose = new Button
            {
                Text = "بستن",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(120, 12),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += BtnClose_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnPrint, btnClose });

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
                lblInvoiceNumberValue!.Text = _purchase.InvoiceNumber;

                var lblPurchaseDateValue = this.Controls.Find("lblPurchaseDateValue", true)[0] as Label;
                lblPurchaseDateValue!.Text = _purchase.PurchaseDate.ToString("yyyy/MM/dd");

                // اطلاعات تامین‌کننده
                var lblSupplierNameValue = this.Controls.Find("lblSupplierNameValue", true)[0] as Label;
                lblSupplierNameValue!.Text = _purchase.Supplier.Name;

                var lblSupplierPhoneValue = this.Controls.Find("lblSupplierPhoneValue", true)[0] as Label;
                lblSupplierPhoneValue!.Text = _purchase.Supplier.Phone ?? "-";

                // جدول آیتم‌ها
                var dgvItems = this.Controls.Find("dgvItems", true)[0] as DataGridView;
                dgvItems!.Columns.Clear();
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
                foreach (var item in _purchase.Items)
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

                // جمع‌کل
                var lblTotalValue = this.Controls.Find("lblTotalValue", true)[0] as Label;
                lblTotalValue!.Text = _purchase.TotalAmount.ToString("N0") + " تومان";

                var lblFinalValue = this.Controls.Find("lblFinalValue", true)[0] as Label;
                lblFinalValue!.Text = _purchase.FinalAmount.ToString("N0") + " تومان";
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
                    // اینجا کد پرینت اضافه می‌شود
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
    }
} 