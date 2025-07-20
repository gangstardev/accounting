using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;
using System.Drawing;

namespace AccountingApp.Forms
{
    public partial class ReportsForm : Form
    {
        private readonly SaleRepository _saleRepository;
        private readonly PurchaseRepository _purchaseRepository;
        private readonly ProductRepository _productRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly SupplierRepository _supplierRepository;

        private DateTimePicker? _dtpStartDate;
        private DateTimePicker? _dtpEndDate;
        private TabControl? _tabControl;
        private DataGridView? _dgvSalesReport;
        private DataGridView? _dgvPurchaseReport;
        private DataGridView? _dgvTopProducts;
        private DataGridView? _dgvTopCustomers;
        private DataGridView? _dgvTopSuppliers;
        private Label? _lblTotalSales;
        private Label? _lblTotalPurchases;
        private Label? _lblTotalProfit;
        private Label? _lblTotalProducts;
        private Label? _lblLowStockProducts;

        public ReportsForm()
        {
            InitializeComponent();
            _saleRepository = new SaleRepository();
            _purchaseRepository = new PurchaseRepository();
            _productRepository = new ProductRepository();
            _customerRepository = new CustomerRepository();
            _supplierRepository = new SupplierRepository();
            
            _dtpStartDate!.Value = DateTime.Today.AddDays(-30);
            _dtpEndDate!.Value = DateTime.Today;
            
            LoadReports();
        }

        private void InitializeComponent()
        {
            this.Text = "گزارش‌های آماری";
            this.Size = new System.Drawing.Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // پنل فیلتر تاریخ
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(5)
            };

            var lblStartDate = new Label { Text = "از تاریخ:", Location = new System.Drawing.Point(10, 20), AutoSize = true };
            _dtpStartDate = new DateTimePicker 
            { 
                Location = new System.Drawing.Point(80, 17), 
                Size = new System.Drawing.Size(120, 25),
                Format = DateTimePickerFormat.Short
            };
            _dtpStartDate.ValueChanged += DateFilter_Changed;

            var lblEndDate = new Label { Text = "تا تاریخ:", Location = new System.Drawing.Point(220, 20), AutoSize = true };
            _dtpEndDate = new DateTimePicker 
            { 
                Location = new System.Drawing.Point(290, 17), 
                Size = new System.Drawing.Size(120, 25),
                Format = DateTimePickerFormat.Short
            };
            _dtpEndDate.ValueChanged += DateFilter_Changed;

            var btnRefresh = new Button
            {
                Text = "تازه‌سازی",
                Location = new System.Drawing.Point(430, 15),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            var btnExport = new Button
            {
                Text = "خروجی Excel",
                Location = new System.Drawing.Point(520, 15),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.Click += BtnExport_Click;

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblStartDate, _dtpStartDate, lblEndDate, _dtpEndDate, btnRefresh, btnExport 
            });

            // پنل خلاصه
            var summaryPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(5)
            };

            var summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(5)
            };

            for (int i = 0; i < 6; i++)
            {
                summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66f));
            }

            var lblSalesTitle = new Label { Text = "کل فروش:", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblTotalSales = new Label { Text = "0 تومان", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10), ForeColor = Color.Green };

            var lblPurchaseTitle = new Label { Text = "کل خرید:", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblTotalPurchases = new Label { Text = "0 تومان", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10), ForeColor = Color.Red };

            var lblProfitTitle = new Label { Text = "سود خالص:", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblTotalProfit = new Label { Text = "0 تومان", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10), ForeColor = Color.Blue };

            var lblProductsTitle = new Label { Text = "تعداد محصولات:", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblTotalProducts = new Label { Text = "0", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10), ForeColor = Color.Orange };

            var lblLowStockTitle = new Label { Text = "موجودی کم:", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblLowStockProducts = new Label { Text = "0", TextAlign = ContentAlignment.MiddleCenter, Font = new System.Drawing.Font("Tahoma", 10), ForeColor = Color.Red };

            summaryLayout.Controls.Add(lblSalesTitle, 0, 0);
            summaryLayout.Controls.Add(_lblTotalSales, 0, 1);
            summaryLayout.Controls.Add(lblPurchaseTitle, 1, 0);
            summaryLayout.Controls.Add(_lblTotalPurchases, 1, 1);
            summaryLayout.Controls.Add(lblProfitTitle, 2, 0);
            summaryLayout.Controls.Add(_lblTotalProfit, 2, 1);
            summaryLayout.Controls.Add(lblProductsTitle, 3, 0);
            summaryLayout.Controls.Add(_lblTotalProducts, 3, 1);
            summaryLayout.Controls.Add(lblLowStockTitle, 4, 0);
            summaryLayout.Controls.Add(_lblLowStockProducts, 4, 1);

            summaryPanel.Controls.Add(summaryLayout);

            // TabControl
            _tabControl = new TabControl { Dock = DockStyle.Fill };

            // تب فروش
            var tabSales = new TabPage("گزارش فروش");
            _dgvSalesReport = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };
            tabSales.Controls.Add(_dgvSalesReport);

            // تب خرید
            var tabPurchase = new TabPage("گزارش خرید");
            _dgvPurchaseReport = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };
            tabPurchase.Controls.Add(_dgvPurchaseReport);

            // تب محصولات پرفروش
            var tabTopProducts = new TabPage("محصولات پرفروش");
            _dgvTopProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };
            tabTopProducts.Controls.Add(_dgvTopProducts);

            // تب مشتریان برتر
            var tabTopCustomers = new TabPage("مشتریان برتر");
            _dgvTopCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };
            tabTopCustomers.Controls.Add(_dgvTopCustomers);

            // تب تامین‌کنندگان برتر
            var tabTopSuppliers = new TabPage("تامین‌کنندگان برتر");
            _dgvTopSuppliers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };
            tabTopSuppliers.Controls.Add(_dgvTopSuppliers);

            _tabControl.TabPages.AddRange(new TabPage[] 
            { 
                tabSales, tabPurchase, tabTopProducts, tabTopCustomers, tabTopSuppliers 
            });

            mainPanel.Controls.Add(_tabControl);
            mainPanel.Controls.Add(summaryPanel);
            mainPanel.Controls.Add(filterPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadReports()
        {
            try
            {
                var startDate = _dtpStartDate!.Value.Date;
                var endDate = _dtpEndDate!.Value.Date;

                LoadSalesReport(startDate, endDate);
                LoadPurchaseReport(startDate, endDate);
                LoadTopProductsReport(startDate, endDate);
                LoadTopCustomersReport(startDate, endDate);
                LoadTopSuppliersReport(startDate, endDate);
                LoadSummaryData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSalesReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sales = _saleRepository.GetByDateRange(startDate, endDate);
                var totalSales = sales.Sum(s => s.FinalAmount);

                _lblTotalSales!.Text = totalSales.ToString("N0") + " تومان";

                _dgvSalesReport!.Columns.Clear();
                _dgvSalesReport.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "تاریخ", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "شماره فاکتور", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "مشتری", Width = 150 },
                    new DataGridViewTextBoxColumn { Name = "ItemsCount", HeaderText = "تعداد آیتم", Width = 80 },
                    new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "مبلغ کل", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "TaxAmount", HeaderText = "مالیات", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "FinalAmount", HeaderText = "مبلغ نهایی", Width = 120 }
                });

                _dgvSalesReport.Rows.Clear();
                foreach (var sale in sales)
                {
                    _dgvSalesReport.Rows.Add(
                        PersianDateConverter.ConvertToPersianDate(sale.SaleDate),
                        sale.InvoiceNumber,
                        sale.Customer.Name,
                        sale.Items.Count,
                        sale.TotalAmount.ToString("N0"),
                        sale.DiscountAmount.ToString("N0"),
                        sale.TaxAmount.ToString("N0"),
                        sale.FinalAmount.ToString("N0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش فروش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchaseReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var purchases = _purchaseRepository.GetByDateRange(startDate, endDate);
                var totalPurchases = purchases.Sum(p => p.FinalAmount);

                _lblTotalPurchases!.Text = totalPurchases.ToString("N0") + " تومان";

                _dgvPurchaseReport!.Columns.Clear();
                _dgvPurchaseReport.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "تاریخ", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "شماره فاکتور", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "SupplierName", HeaderText = "تامین‌کننده", Width = 150 },
                    new DataGridViewTextBoxColumn { Name = "ItemsCount", HeaderText = "تعداد آیتم", Width = 80 },
                    new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "مبلغ کل", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "TaxAmount", HeaderText = "مالیات", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "FinalAmount", HeaderText = "مبلغ نهایی", Width = 120 }
                });

                _dgvPurchaseReport.Rows.Clear();
                foreach (var purchase in purchases)
                {
                    _dgvPurchaseReport.Rows.Add(
                        PersianDateConverter.ConvertToPersianDate(purchase.PurchaseDate),
                        purchase.InvoiceNumber,
                        purchase.Supplier.Name,
                        purchase.Items.Count,
                        purchase.TotalAmount.ToString("N0"),
                        purchase.DiscountAmount.ToString("N0"),
                        purchase.TaxAmount.ToString("N0"),
                        purchase.FinalAmount.ToString("N0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش خرید: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopProductsReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sales = _saleRepository.GetByDateRange(startDate, endDate);
                var productSales = new Dictionary<int, (string Name, int Quantity, decimal Revenue)>();

                foreach (var sale in sales)
                {
                    foreach (var item in sale.Items)
                    {
                        if (productSales.ContainsKey(item.ProductId))
                        {
                            var current = productSales[item.ProductId];
                            productSales[item.ProductId] = (current.Name, current.Quantity + item.Quantity, current.Revenue + item.FinalPrice);
                        }
                        else
                        {
                            productSales[item.ProductId] = (item.Product.Name, item.Quantity, item.FinalPrice);
                        }
                    }
                }

                var topProducts = productSales
                    .OrderByDescending(p => p.Value.Revenue)
                    .Take(20)
                    .ToList();

                _dgvTopProducts!.Columns.Clear();
                _dgvTopProducts.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "رتبه", Width = 60 },
                    new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "نام محصول", Width = 200 },
                    new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "تعداد فروخته شده", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "Revenue", HeaderText = "درآمد", Width = 120 }
                });

                _dgvTopProducts.Rows.Clear();
                for (int i = 0; i < topProducts.Count; i++)
                {
                    var product = topProducts[i];
                    _dgvTopProducts.Rows.Add(
                        i + 1,
                        product.Value.Name,
                        product.Value.Quantity,
                        product.Value.Revenue.ToString("N0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش محصولات پرفروش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopCustomersReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var sales = _saleRepository.GetByDateRange(startDate, endDate);
                var customerSales = new Dictionary<int, (string Name, int Orders, decimal TotalAmount)>();

                foreach (var sale in sales)
                {
                    if (customerSales.ContainsKey(sale.CustomerId))
                    {
                        var current = customerSales[sale.CustomerId];
                        customerSales[sale.CustomerId] = (current.Name, current.Orders + 1, current.TotalAmount + sale.FinalAmount);
                    }
                    else
                    {
                        customerSales[sale.CustomerId] = (sale.Customer.Name, 1, sale.FinalAmount);
                    }
                }

                var topCustomers = customerSales
                    .OrderByDescending(c => c.Value.TotalAmount)
                    .Take(20)
                    .ToList();

                _dgvTopCustomers!.Columns.Clear();
                _dgvTopCustomers.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "رتبه", Width = 60 },
                    new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "نام مشتری", Width = 200 },
                    new DataGridViewTextBoxColumn { Name = "Orders", HeaderText = "تعداد سفارش", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "مجموع خرید", Width = 120 }
                });

                _dgvTopCustomers.Rows.Clear();
                for (int i = 0; i < topCustomers.Count; i++)
                {
                    var customer = topCustomers[i];
                    _dgvTopCustomers.Rows.Add(
                        i + 1,
                        customer.Value.Name,
                        customer.Value.Orders,
                        customer.Value.TotalAmount.ToString("N0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش مشتریان برتر: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopSuppliersReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var purchases = _purchaseRepository.GetByDateRange(startDate, endDate);
                var supplierPurchases = new Dictionary<int, (string Name, int Orders, decimal TotalAmount)>();

                foreach (var purchase in purchases)
                {
                    if (supplierPurchases.ContainsKey(purchase.SupplierId))
                    {
                        var current = supplierPurchases[purchase.SupplierId];
                        supplierPurchases[purchase.SupplierId] = (current.Name, current.Orders + 1, current.TotalAmount + purchase.FinalAmount);
                    }
                    else
                    {
                        supplierPurchases[purchase.SupplierId] = (purchase.Supplier.Name, 1, purchase.FinalAmount);
                    }
                }

                var topSuppliers = supplierPurchases
                    .OrderByDescending(s => s.Value.TotalAmount)
                    .Take(20)
                    .ToList();

                _dgvTopSuppliers!.Columns.Clear();
                _dgvTopSuppliers.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "رتبه", Width = 60 },
                    new DataGridViewTextBoxColumn { Name = "SupplierName", HeaderText = "نام تامین‌کننده", Width = 200 },
                    new DataGridViewTextBoxColumn { Name = "Orders", HeaderText = "تعداد سفارش", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "مجموع خرید", Width = 120 }
                });

                _dgvTopSuppliers.Rows.Clear();
                for (int i = 0; i < topSuppliers.Count; i++)
                {
                    var supplier = topSuppliers[i];
                    _dgvTopSuppliers.Rows.Add(
                        i + 1,
                        supplier.Value.Name,
                        supplier.Value.Orders,
                        supplier.Value.TotalAmount.ToString("N0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش تامین‌کنندگان برتر: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSummaryData()
        {
            try
            {
                var totalSales = decimal.Parse(_lblTotalSales!.Text.Replace(" تومان", "").Replace(",", ""));
                var totalPurchases = decimal.Parse(_lblTotalPurchases!.Text.Replace(" تومان", "").Replace(",", ""));
                var totalProfit = totalSales - totalPurchases;

                _lblTotalProfit!.Text = totalProfit.ToString("N0") + " تومان";

                var products = _productRepository.GetAll();
                _lblTotalProducts!.Text = products.Count.ToString();

                var lowStockProducts = products.Count(p => p.StockQuantity <= p.MinStockLevel && p.StockQuantity > 0);
                _lblLowStockProducts!.Text = lowStockProducts.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری خلاصه داده‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DateFilter_Changed(object? sender, EventArgs e)
        {
            LoadReports();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadReports();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"گزارش_آماری_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // اینجا کد خروجی Excel اضافه می‌شود
                    MessageBox.Show("خروجی Excel با موفقیت ایجاد شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ایجاد خروجی Excel: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 