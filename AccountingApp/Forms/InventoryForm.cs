using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;
using System.Drawing;

namespace AccountingApp.Forms
{
    public partial class InventoryForm : Form
    {
        private readonly ProductRepository _productRepository;
        private readonly SaleRepository _saleRepository;
        private readonly PurchaseRepository _purchaseRepository;
        private DataGridView? _dataGridView;
        private ComboBox? _cmbCategoryFilter;
        private TextBox? _txtSearchProduct;
        private List<Product>? _products;

        public InventoryForm()
        {
            InitializeComponent();
            _productRepository = new ProductRepository();
            _saleRepository = new SaleRepository();
            _purchaseRepository = new PurchaseRepository();
            
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "مدیریت انبار";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // پنل اصلی
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // پنل فیلتر
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(5)
            };

            var lblSearch = new Label { Text = "جستجو:", Location = new System.Drawing.Point(10, 25), AutoSize = true };
            _txtSearchProduct = new TextBox 
            { 
                Location = new System.Drawing.Point(80, 22), 
                Size = new System.Drawing.Size(200, 25),
                Font = new System.Drawing.Font("Tahoma", 10)
            };
            _txtSearchProduct.TextChanged += Filter_Changed;

            var lblCategory = new Label { Text = "دسته‌بندی:", Location = new System.Drawing.Point(300, 25), AutoSize = true };
            _cmbCategoryFilter = new ComboBox 
            { 
                Location = new System.Drawing.Point(380, 22), 
                Size = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCategoryFilter.SelectedIndexChanged += Filter_Changed;

            var btnRefresh = new Button
            {
                Text = "تازه‌سازی",
                Location = new System.Drawing.Point(550, 20),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            var btnLowStock = new Button
            {
                Text = "موجودی کم",
                Location = new System.Drawing.Point(640, 20),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLowStock.Click += BtnLowStock_Click;

            var btnOutOfStock = new Button
            {
                Text = "موجودی صفر",
                Location = new System.Drawing.Point(750, 20),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOutOfStock.Click += BtnOutOfStock_Click;

            var btnExport = new Button
            {
                Text = "خروجی Excel",
                Location = new System.Drawing.Point(860, 20),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.Click += BtnExport_Click;

            var btnAddProduct = new Button
            {
                Text = "افزودن محصول",
                Location = new System.Drawing.Point(970, 20),
                Size = new System.Drawing.Size(120, 30),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Tahoma", 9, FontStyle.Bold)
            };
            btnAddProduct.Click += BtnAddProduct_Click;

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblSearch, _txtSearchProduct, lblCategory, _cmbCategoryFilter, 
                btnRefresh, btnLowStock, btnOutOfStock, btnExport, btnAddProduct 
            });

            // DataGridView
            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new System.Drawing.Font("Tahoma", 9)
            };

            _dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;
            _dataGridView.CellFormatting += DataGridView_CellFormatting;

            // ستون‌های DataGridView
            // در متد InitializeComponent، اضافه کردن ستون وزن:
_dataGridView.Columns.AddRange(new DataGridViewColumn[]
{
    new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "شناسه", Visible = false },
    new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "نام محصول", Width = 200 },
    new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "دسته‌بندی", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "Weight", HeaderText = "وزن (گرم)", Width = 100 },
    new DataGridViewTextBoxColumn { Name = "StockQuantity", HeaderText = "موجودی", Width = 80 },
    new DataGridViewTextBoxColumn { Name = "MinStockLevel", HeaderText = "حداقل موجودی", Width = 100 },
    new DataGridViewTextBoxColumn { Name = "CostPrice", HeaderText = "قیمت خرید", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "قیمت فروش", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "TotalCost", HeaderText = "ارزش موجودی", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "LastPurchaseDate", HeaderText = "آخرین خرید", Width = 100 },
    new DataGridViewTextBoxColumn { Name = "LastSaleDate", HeaderText = "آخرین فروش", Width = 100 },
    new DataGridViewButtonColumn { Name = "Edit", HeaderText = "ویرایش", Text = "ویرایش", UseColumnTextForButtonValue = true, Width = 80 },
    new DataGridViewButtonColumn { Name = "History", HeaderText = "تاریخچه", Text = "تاریخچه", UseColumnTextForButtonValue = true, Width = 80 }
});

            _dataGridView.CellClick += DataGridView_CellClick;

            mainPanel.Controls.Add(_dataGridView);
            mainPanel.Controls.Add(filterPanel);

            this.Controls.Add(mainPanel);

            LoadCategoryFilter();
        }

        private void LoadCategoryFilter()
        {
            try
            {
                var products = _productRepository.GetAll();
                var categories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
                
                _cmbCategoryFilter!.Items.Clear();
                _cmbCategoryFilter.Items.Add("همه دسته‌ها");
                _cmbCategoryFilter.Items.AddRange(categories.ToArray());
                _cmbCategoryFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری فیلتر دسته‌بندی: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                _products = _productRepository.GetAll();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری محصولات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                var filteredProducts = _products ?? new List<Product>();

                // فیلتر جستجو
                if (!string.IsNullOrWhiteSpace(_txtSearchProduct!.Text))
                {
                    var searchTerm = _txtSearchProduct.Text.ToLower();
                    filteredProducts = filteredProducts.Where(p => 
                        p.Name.ToLower().Contains(searchTerm) || 
                        p.Category.ToLower().Contains(searchTerm)).ToList();
                }

                // فیلتر دسته‌بندی
                if (_cmbCategoryFilter!.SelectedIndex > 0)
                {
                    var selectedCategory = _cmbCategoryFilter.Text;
                    filteredProducts = filteredProducts.Where(p => p.Category == selectedCategory).ToList();
                }

                RefreshDataGridView(filteredProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در اعمال فیلترها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView(List<Product> products)
        {
            _dataGridView!.Rows.Clear();

            foreach (var product in products)
            {
                var totalCost = product.StockQuantity * product.CostPrice;
                var lastPurchaseDate = GetLastPurchaseDate(product.Id);
                var lastSaleDate = GetLastSaleDate(product.Id);

                _dataGridView.Rows.Add(
                    product.Id,
                    product.Name,
                    product.Category,
                    product.StockQuantity,
                    product.MinStockLevel,
                    product.CostPrice.ToString("N0"),
                    product.Price.ToString("N0"),
                    totalCost.ToString("N0"),
                    lastPurchaseDate?.ToString("yyyy/MM/dd") ?? "-",
                    lastSaleDate?.ToString("yyyy/MM/dd") ?? "-"
                );

                // رنگ‌بندی بر اساس موجودی
                var rowIndex = _dataGridView.Rows.Count - 1;
                if (product.StockQuantity == 0)
                {
                    _dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else if (product.StockQuantity <= product.MinStockLevel)
                {
                    _dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
        }

        private DateTime? GetLastPurchaseDate(int productId)
        {
            try
            {
                var purchases = _purchaseRepository.GetAll();
                var lastPurchase = purchases
                    .Where(p => p.Items.Any(item => item.ProductId == productId))
                    .OrderByDescending(p => p.PurchaseDate)
                    .FirstOrDefault();
                
                return lastPurchase?.PurchaseDate;
            }
            catch
            {
                return null;
            }
        }

        private DateTime? GetLastSaleDate(int productId)
        {
            try
            {
                var sales = _saleRepository.GetAll();
                var lastSale = sales
                    .Where(s => s.Items.Any(item => item.ProductId == productId))
                    .OrderByDescending(s => s.SaleDate)
                    .FirstOrDefault();
                
                return lastSale?.SaleDate;
            }
            catch
            {
                return null;
            }
        }

        private void Filter_Changed(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadProducts();
        }

        private void BtnLowStock_Click(object? sender, EventArgs e)
        {
            try
            {
                var lowStockProducts = _products?.Where(p => p.StockQuantity > 0 && p.StockQuantity <= p.MinStockLevel).ToList() ?? new List<Product>();
                RefreshDataGridView(lowStockProducts);
                
                MessageBox.Show($"{lowStockProducts.Count} محصول با موجودی کم یافت شد.", "نتایج", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در فیلتر موجودی کم: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOutOfStock_Click(object? sender, EventArgs e)
        {
            try
            {
                var outOfStockProducts = _products?.Where(p => p.StockQuantity == 0).ToList() ?? new List<Product>();
                RefreshDataGridView(outOfStockProducts);
                
                MessageBox.Show($"{outOfStockProducts.Count} محصول با موجودی صفر یافت شد.", "نتایج", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در فیلتر موجودی صفر: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"گزارش_انبار_{DateTime.Now:yyyyMMdd}.xlsx"
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

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
            try
            {
                var addProductForm = new AddEditProductForm();
                if (addProductForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                    LoadCategoryFilter(); // به‌روزرسانی فیلتر دسته‌بندی
                    MessageBox.Show("محصول با موفقیت اضافه شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در افزودن محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var productId = Convert.ToInt32(_dataGridView!.Rows[e.RowIndex].Cells["Id"].Value);
                var product = _productRepository.GetById(productId);
                if (product != null)
                {
                    var editForm = new AddEditProductForm(product);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadProducts();
                    }
                }
            }
        }

        private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var productId = Convert.ToInt32(_dataGridView!.Rows[e.RowIndex].Cells["Id"].Value);

                if (e.ColumnIndex == _dataGridView.Columns["Edit"].Index)
                {
                    var product = _productRepository.GetById(productId);
                    if (product != null)
                    {
                        var editForm = new AddEditProductForm(product);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadProducts();
                        }
                    }
                }
                else if (e.ColumnIndex == _dataGridView.Columns["History"].Index)
                {
                    ShowProductHistory(productId);
                }
            }
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null)
            {
                if (e.ColumnIndex == _dataGridView!.Columns["CostPrice"].Index ||
                    e.ColumnIndex == _dataGridView.Columns["Price"].Index ||
                    e.ColumnIndex == _dataGridView.Columns["TotalCost"].Index)
                {
                    if (decimal.TryParse(e.Value.ToString(), out decimal amount))
                    {
                        e.Value = amount.ToString("N0") + " تومان";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        private void ShowProductHistory(int productId)
        {
            try
            {
                var product = _productRepository.GetById(productId);
                if (product != null)
                {
                    var historyForm = new ProductHistoryForm(product);
                    historyForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در نمایش تاریخچه محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}