using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class ProductsForm : Form
    {
        private readonly ProductRepository _productRepository;
        private DataGridView? _dataGridView;
        private TextBox? _searchTextBox;
        private List<Product>? _products;

        public ProductsForm()
        {
            InitializeComponent();
            _productRepository = new ProductRepository();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "مدیریت محصولات";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // پنل اصلی
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // پنل جستجو
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(5)
            };

            var searchLabel = new Label
            {
                Text = "جستجو:",
                Location = new System.Drawing.Point(10, 20),
                AutoSize = true
            };

            _searchTextBox = new TextBox
            {
                Location = new System.Drawing.Point(80, 17),
                Size = new System.Drawing.Size(200, 25),
                Font = new System.Drawing.Font("Tahoma", 10)
            };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            var btnAdd = new Button
            {
                Text = "افزودن محصول جدید",
                Location = new System.Drawing.Point(300, 15),
                Size = new System.Drawing.Size(120, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += BtnAdd_Click;

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

            var btnShowCodes = new Button
            {
                Text = "نمایش کدها",
                Location = new System.Drawing.Point(520, 15),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShowCodes.Click += BtnShowCodes_Click;

            var btnDiagnostic = new Button
            {
                Text = "تشخیص",
                Location = new System.Drawing.Point(630, 15),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDiagnostic.Click += BtnDiagnostic_Click;

            searchPanel.Controls.AddRange(new Control[] { searchLabel, _searchTextBox, btnAdd, btnRefresh, btnShowCodes, btnDiagnostic });

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
            _dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "شناسه", Visible = false },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "کد محصول", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "نام محصول", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "توضیحات", Width = 250 },
                new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "قیمت فروش", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "CostPrice", HeaderText = "قیمت تمام شده", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "StockQuantity", HeaderText = "موجودی", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "واحد", Width = 80 },
                new DataGridViewButtonColumn { Name = "Edit", HeaderText = "ویرایش", Text = "ویرایش", UseColumnTextForButtonValue = true, Width = 80 },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "حذف", Text = "حذف", UseColumnTextForButtonValue = true, Width = 80 }
            });

            _dataGridView.CellClick += DataGridView_CellClick;

            mainPanel.Controls.Add(_dataGridView);
            mainPanel.Controls.Add(searchPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadProducts()
        {
            try
            {
                _products = _productRepository.GetAll();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری محصولات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            _dataGridView?.Rows.Clear();

            foreach (var product in _products ?? new List<Product>())
            {
                _dataGridView?.Rows.Add(
                    product.Id,
                    product.Code,
                    product.Name,
                    product.Description,
                    product.Price.ToString("N0"),
                    product.CostPrice.ToString("N0"),
                    product.StockQuantity,
                    product.Unit
                );
            }
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                var searchTerm = _searchTextBox?.Text.Trim();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _products = _productRepository.GetAll();
                }
                else
                {
                    _products = _productRepository.Search(searchTerm);
                }
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در جستجو: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var addForm = new AddEditProductForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadProducts();
        }

        private void BtnShowCodes_Click(object? sender, EventArgs e)
        {
            try
            {
                AccountingApp.Utilities.ProductCodeChecker.ShowExistingCodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در نمایش کدها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDiagnostic_Click(object? sender, EventArgs e)
        {
            try
            {
                AccountingApp.Utilities.DatabaseDiagnostic.CheckProductCodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تشخیص پایگاه داده: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var productId = Convert.ToInt32(_dataGridView?.Rows[e.RowIndex].Cells["Id"].Value);
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
                var productId = Convert.ToInt32(_dataGridView?.Rows[e.RowIndex].Cells["Id"].Value);

                if (e.ColumnIndex == _dataGridView?.Columns["Edit"].Index)
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
                else if (e.ColumnIndex == _dataGridView?.Columns["Delete"].Index)
                {
                    if (MessageBox.Show("آیا از حذف این محصول اطمینان دارید؟", "تایید حذف", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            if (_productRepository.Delete(productId))
                            {
                                MessageBox.Show("محصول با موفقیت حذف شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadProducts();
                            }
                            else
                            {
                                MessageBox.Show("خطا در حذف محصول.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطا در حذف محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null && e.ColumnIndex == _dataGridView?.Columns["Price"].Index)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal price))
                {
                    e.Value = price.ToString("N0") + " تومان";
                    e.FormattingApplied = true;
                }
            }
            else if (e.Value != null && e.ColumnIndex == _dataGridView?.Columns["CostPrice"].Index)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal costPrice))
                {
                    e.Value = costPrice.ToString("N0") + " تومان";
                    e.FormattingApplied = true;
                }
            }
        }
    }
} 