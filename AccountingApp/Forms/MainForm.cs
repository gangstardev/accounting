using System;
using System.Windows.Forms;
using AccountingApp.Repositories;
using AccountingApp.Services;

namespace AccountingApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly ProductRepository _productRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly SaleRepository _saleRepository;
        private readonly PurchaseRepository _purchaseRepository;

        public MainForm()
        {
            InitializeComponent();
            _productRepository = new ProductRepository();
            _customerRepository = new CustomerRepository();
            _supplierRepository = new SupplierRepository();
            _saleRepository = new SaleRepository();
            _purchaseRepository = new PurchaseRepository();
            
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Text = "سیستم فروشگاهی";
            this.Size = new System.Drawing.Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // منوی اصلی
            var mainMenu = new MenuStrip();
            mainMenu.Items.AddRange(new ToolStripItem[]
            {
                CreateMenuItem("فروش", null, (s, e) => OpenSalesForm()),
                CreateMenuItem("خرید", null, (s, e) => OpenPurchaseForm()),
                CreateMenuItem("انبار", null, (s, e) => OpenInventoryForm()),
                CreateMenuItem("مشتریان", null, (s, e) => OpenCustomersForm()),
                CreateMenuItem("تامین‌کنندگان", null, (s, e) => OpenSuppliersForm()),
                CreateMenuItem("محصولات", null, (s, e) => OpenProductsForm()),
                CreateMenuItem("گزارشات", null, (s, e) => OpenReportsForm()),
                CreateMenuItem("آپدیت", null, (s, e) => CheckForUpdates())
            });

            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // پنل اصلی
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // عنوان
            var titleLabel = new Label
            {
                Text = "سیستم مدیریت فروشگاهی",
                Font = new System.Drawing.Font("Tahoma", 16, System.Drawing.FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // پنل آمار
            var statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(5)
            };

            var statsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };

            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // دکمه‌های آمار
            var btnSales = CreateStatButton("فروش امروز", "0 تومان", Color.LightGreen);
            var btnPurchases = CreateStatButton("خرید امروز", "0 تومان", Color.LightBlue);
            var btnProducts = CreateStatButton("تعداد محصولات", "0", Color.LightYellow);
            var btnCustomersStat = CreateStatButton("تعداد مشتریان", "0", Color.LightPink);

            statsLayout.Controls.Add(btnSales, 0, 0);
            statsLayout.Controls.Add(btnPurchases, 1, 0);
            statsLayout.Controls.Add(btnProducts, 2, 0);
            statsLayout.Controls.Add(btnCustomersStat, 3, 0);

            statsPanel.Controls.Add(statsLayout);

            // پنل دکمه‌های اصلی
            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var buttonsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10)
            };

            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // دکمه‌های اصلی
            var btnNewSale = CreateMainButton("فروش جدید", "➕", Color.Green, (s, e) => OpenSalesForm());
            var btnNewPurchase = CreateMainButton("خرید جدید", "📦", Color.Blue, (s, e) => OpenPurchaseForm());
            var btnInventory = CreateMainButton("مدیریت انبار", "📋", Color.Orange, (s, e) => OpenInventoryForm());
            var btnCustomersMain = CreateMainButton("مشتریان", "👥", Color.Purple, (s, e) => OpenCustomersForm());
            var btnSuppliers = CreateMainButton("تامین‌کنندگان", "🏢", Color.Teal, (s, e) => OpenSuppliersForm());
            var btnReports = CreateMainButton("گزارشات", "📊", Color.Gray, (s, e) => OpenReportsForm());

            buttonsLayout.Controls.Add(btnNewSale, 0, 0);
            buttonsLayout.Controls.Add(btnNewPurchase, 1, 0);
            buttonsLayout.Controls.Add(btnInventory, 2, 0);
            buttonsLayout.Controls.Add(btnCustomersMain, 0, 1);
            buttonsLayout.Controls.Add(btnSuppliers, 1, 1);
            buttonsLayout.Controls.Add(btnReports, 2, 1);

            buttonsPanel.Controls.Add(buttonsLayout);

            mainPanel.Controls.Add(buttonsPanel);
            mainPanel.Controls.Add(statsPanel);
            mainPanel.Controls.Add(titleLabel);

            this.Controls.Add(mainPanel);
        }

        private ToolStripMenuItem CreateMenuItem(string text, Image? image, EventHandler clickHandler)
        {
            var menuItem = new ToolStripMenuItem(text, image, clickHandler);
            return menuItem;
        }

        private Button CreateStatButton(string title, string value, Color color)
        {
            var button = new Button
            {
                Text = $"{title}\n{value}",
                Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold),
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            return button;
        }

        private Button CreateMainButton(string text, string icon, Color color, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = $"{icon}\n{text}",
                Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false
            };
            button.Click += clickHandler;
            return button;
        }

        private void LoadDashboard()
        {
            try
            {
                // تست تبدیل تاریخ
                var testDate = DateTime.Now;
                var persianTestDate = PersianDateConverter.ConvertToPersianDate(testDate);
                Console.WriteLine($"تست تاریخ میلادی: {testDate}");
                Console.WriteLine($"تست تاریخ شمسی: {persianTestDate}");
                
                var today = DateTime.Today;
                var todaySales = _saleRepository.GetTotalSales(today, today);
                var todayPurchases = _purchaseRepository.GetTotalPurchases(today, today);
                var productsCount = _productRepository.GetAll().Count;
                var customersCount = _customerRepository.GetAll().Count;

                // به‌روزرسانی آمار
                if (this.Controls.Count > 1 && this.Controls[1] is Panel mainPanel)
                {
                    if (mainPanel.Controls.Count > 1 && mainPanel.Controls[1] is Panel statsPanel)
                    {
                        if (statsPanel.Controls[0] is TableLayoutPanel statsLayout)
                        {
                            if (statsLayout.Controls[0] is Button btnSales)
                                btnSales.Text = $"فروش امروز\n{todaySales:N0} تومان";
                            if (statsLayout.Controls[1] is Button btnPurchases)
                                btnPurchases.Text = $"خرید امروز\n{todayPurchases:N0} تومان";
                            if (statsLayout.Controls[2] is Button btnProducts)
                                btnProducts.Text = $"تعداد محصولات\n{productsCount}";
                            if (statsLayout.Controls[3] is Button btnCustomersStat)
                                btnCustomersStat.Text = $"تعداد مشتریان\n{customersCount}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری آمار: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSalesForm()
        {
            var salesForm = new SalesForm();
            salesForm.ShowDialog();
            LoadDashboard();
        }

        private void OpenPurchaseForm()
        {
            var purchaseForm = new PurchaseForm();
            purchaseForm.ShowDialog();
            LoadDashboard();
        }

        private void OpenInventoryForm()
        {
            var inventoryForm = new InventoryForm();
            inventoryForm.ShowDialog();
            LoadDashboard();
        }

        private void OpenCustomersForm()
        {
            var customersForm = new CustomersForm();
            customersForm.ShowDialog();
            LoadDashboard();
        }

        private void OpenSuppliersForm()
        {
            var suppliersForm = new SuppliersForm();
            suppliersForm.ShowDialog();
            LoadDashboard();
        }

        private void OpenProductsForm()
        {
            var productsForm = new ProductsForm();
            productsForm.ShowDialog();
            LoadDashboard();
        }

        private void OpenReportsForm()
        {
            var reportsForm = new ReportsForm();
            reportsForm.ShowDialog();
        }
        
        private async void CheckForUpdates()
        {
            try
            {
                using var updateService = new UpdateService();
                await updateService.CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بررسی آپدیت: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 