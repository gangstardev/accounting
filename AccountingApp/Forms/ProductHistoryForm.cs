using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class ProductHistoryForm : Form
    {
        private readonly Product _product;
        private readonly SaleRepository _saleRepository;
        private readonly PurchaseRepository _purchaseRepository;
        private DataGridView? _dgvHistory;
        private TabControl? _tabControl;

        public ProductHistoryForm(Product product)
        {
            _product = product;
            _saleRepository = new SaleRepository();
            _purchaseRepository = new PurchaseRepository();
            
            InitializeComponent();
            LoadProductHistory();
        }

        private void InitializeComponent()
        {
            this.Text = $"تاریخچه محصول: {_product.Name}";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // اطلاعات محصول
            var productInfoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            var infoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(5)
            };

            for (int i = 0; i < 4; i++)
            {
                infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            var lblName = new Label { Text = $"نام: {_product.Name}", Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            var lblCode = new Label { Text = $"کد: {_product.Code}", Font = new System.Drawing.Font("Tahoma", 10) };
            var lblStock = new Label { Text = $"موجودی: {_product.StockQuantity}", Font = new System.Drawing.Font("Tahoma", 10) };
            var lblPrice = new Label { Text = $"قیمت: {_product.Price:N0} تومان", Font = new System.Drawing.Font("Tahoma", 10) };

            infoLayout.Controls.Add(lblName, 0, 0);
            infoLayout.Controls.Add(lblCode, 1, 0);
            infoLayout.Controls.Add(lblStock, 2, 0);
            infoLayout.Controls.Add(lblPrice, 3, 0);

            productInfoPanel.Controls.Add(infoLayout);

            // TabControl
            _tabControl = new TabControl { Dock = DockStyle.Fill };

            // تب فروش
            var tabSales = new TabPage("تاریخچه فروش");
            _dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };
            tabSales.Controls.Add(_dgvHistory);

            _tabControl.TabPages.Add(tabSales);

            // پنل دکمه‌ها
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };

            var btnClose = new Button
            {
                Text = "بستن",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(10, 7),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += BtnClose_Click;

            buttonPanel.Controls.Add(btnClose);

            mainPanel.Controls.Add(_tabControl);
            mainPanel.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(productInfoPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadProductHistory()
        {
            try
            {
                var sales = _saleRepository.GetAll();
                var productSales = new List<object>();

                foreach (var sale in sales)
                {
                    var saleItem = sale.Items.FirstOrDefault(item => item.ProductId == _product.Id);
                    if (saleItem != null)
                    {
                        productSales.Add(new
                        {
                            Date = sale.SaleDate,
                            Type = "فروش",
                            InvoiceNumber = sale.InvoiceNumber,
                            CustomerName = sale.Customer.Name,
                            Quantity = saleItem.Quantity,
                            UnitPrice = saleItem.UnitPrice,
                            TotalPrice = saleItem.TotalPrice,
                            DiscountAmount = saleItem.DiscountAmount,
                            FinalPrice = saleItem.FinalPrice
                        });
                    }
                }

                var purchases = _purchaseRepository.GetAll();
                foreach (var purchase in purchases)
                {
                    var purchaseItem = purchase.Items.FirstOrDefault(item => item.ProductId == _product.Id);
                    if (purchaseItem != null)
                    {
                        productSales.Add(new
                        {
                            Date = purchase.PurchaseDate,
                            Type = "خرید",
                            InvoiceNumber = purchase.InvoiceNumber,
                            CustomerName = purchase.Supplier.Name,
                            Quantity = purchaseItem.Quantity,
                            UnitPrice = purchaseItem.UnitPrice,
                            TotalPrice = purchaseItem.TotalPrice,
                            DiscountAmount = purchaseItem.DiscountAmount,
                            FinalPrice = purchaseItem.FinalPrice
                        });
                    }
                }

                var sortedHistory = productSales.OrderByDescending(x => ((dynamic)x).Date).ToList();

                _dgvHistory!.Columns.Clear();
                _dgvHistory.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "تاریخ", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "نوع", Width = 80 },
                    new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "شماره فاکتور", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "مشتری/تامین‌کننده", Width = 150 },
                    new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "تعداد", Width = 80 },
                    new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "قیمت واحد", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "TotalPrice", HeaderText = "قیمت کل", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "FinalPrice", HeaderText = "قیمت نهایی", Width = 120 }
                });

                _dgvHistory.Rows.Clear();
                foreach (dynamic item in sortedHistory)
                {
                    _dgvHistory.Rows.Add(
                        item.Date.ToString("yyyy/MM/dd"),
                        item.Type,
                        item.InvoiceNumber,
                        item.CustomerName,
                        item.Quantity,
                        item.UnitPrice.ToString("N0"),
                        item.TotalPrice.ToString("N0"),
                        item.DiscountAmount.ToString("N0"),
                        item.FinalPrice.ToString("N0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری تاریخچه محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
} 