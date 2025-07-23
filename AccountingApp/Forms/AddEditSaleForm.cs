using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class AddEditSaleForm : Form
    {
        private readonly SaleRepository _saleRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly Sale? _sale;
        private readonly bool _isEdit;

        private TextBox? _txtInvoiceNumber;
        private ComboBox? _cmbCustomer;
        private PersianDateTimePicker? _dtpSaleDate;
        private DataGridView? _dgvItems;
        private NumericUpDown? _numDiscountAmount;
        private NumericUpDown? _numTaxAmount;
        private TextBox? _txtNotes;
        private Label? _lblTotalAmount;
        private Label? _lblDiscountAmount;
        private Label? _lblFinalAmount;
        private List<SaleItem> _saleItems;

        public AddEditSaleForm(Sale? sale = null)
        {
            InitializeComponent();
            _saleRepository = new SaleRepository();
            _customerRepository = new CustomerRepository();
            _productRepository = new ProductRepository();
            _sale = sale;
            _isEdit = sale != null;
            _saleItems = new List<SaleItem>();

            // ابتدا ستون‌های DataGridView را تنظیم کنیم
            SetupDataGridView();
            
            LoadCustomers();
            LoadProducts();

            // اضافه کردن event handler برای Load event
            this.Load += AddEditSaleForm_Load;
        }

        private void AddEditSaleForm_Load(object? sender, EventArgs e)
        {
            try
            {
                // اطمینان از آماده بودن فرم
                if (_isEdit)
                {
                    this.Text = "ویرایش فروش";
                    LoadSaleData();
                }
                else
                {
                    this.Text = "فروش جدید";
                    _txtInvoiceNumber.Text = _saleRepository.GenerateTodayInvoiceNumber();
                    _dtpSaleDate.Value = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری فرم: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // پنل اصلی
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // پنل اطلاعات اصلی
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(5)
            };

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(5)
            };

            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // شماره فاکتور
            var lblInvoiceNumber = new Label { Text = "شماره فاکتور:", TextAlign = ContentAlignment.MiddleRight };
            _txtInvoiceNumber = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            headerLayout.Controls.Add(lblInvoiceNumber, 0, 0);
            headerLayout.Controls.Add(_txtInvoiceNumber, 1, 0);

            // مشتری
            var lblCustomer = new Label { Text = "مشتری:", TextAlign = ContentAlignment.MiddleRight };
            _cmbCustomer = new ComboBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, DropDownStyle = ComboBoxStyle.DropDownList };
            headerLayout.Controls.Add(lblCustomer, 2, 0);
            headerLayout.Controls.Add(_cmbCustomer, 3, 0);

            // تاریخ فروش
            var lblSaleDate = new Label { Text = "تاریخ فروش:", TextAlign = ContentAlignment.MiddleRight };
            _dtpSaleDate = new PersianDateTimePicker { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            headerLayout.Controls.Add(lblSaleDate, 0, 1);
            headerLayout.Controls.Add(_dtpSaleDate, 1, 1);

            // تخفیف
            var lblDiscount = new Label { Text = "تخفیف (تومان):", TextAlign = ContentAlignment.MiddleRight };
            _numDiscountAmount = new NumericUpDown { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Maximum = 999999999, Minimum = 0, DecimalPlaces = 0 };
            _numDiscountAmount.ValueChanged += CalculateTotals;
            headerLayout.Controls.Add(lblDiscount, 2, 1);
            headerLayout.Controls.Add(_numDiscountAmount, 3, 1);

            // مالیات
            var lblTax = new Label { Text = "مالیات (تومان):", TextAlign = ContentAlignment.MiddleRight };
            _numTaxAmount = new NumericUpDown { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Maximum = 999999999, Minimum = 0, DecimalPlaces = 0 };
            _numTaxAmount.ValueChanged += CalculateTotals;
            headerLayout.Controls.Add(lblTax, 0, 2);
            headerLayout.Controls.Add(_numTaxAmount, 1, 2);

            // یادداشت
            var lblNotes = new Label { Text = "یادداشت:", TextAlign = ContentAlignment.MiddleRight };
            _txtNotes = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            headerLayout.Controls.Add(lblNotes, 2, 2);
            headerLayout.Controls.Add(_txtNotes, 3, 2);

            headerPanel.Controls.Add(headerLayout);

            // پنل آیتم‌ها
            var itemsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var itemsLabel = new Label { Text = "آیتم‌های فروش:", Dock = DockStyle.Top, Height = 25, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };

            _dgvItems = new DataGridView
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

            // پنل دکمه‌های آیتم‌ها
            var itemButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5)
            };

            var btnAddItem = new Button
            {
                Text = "افزودن آیتم",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(10, 5),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddItem.Click += BtnAddItem_Click;

            var btnEditItem = new Button
            {
                Text = "ویرایش آیتم",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(120, 5),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEditItem.Click += BtnEditItem_Click;

            var btnRemoveItem = new Button
            {
                Text = "حذف آیتم",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(230, 5),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRemoveItem.Click += BtnRemoveItem_Click;

            itemButtonsPanel.Controls.AddRange(new Control[] { btnAddItem, btnEditItem, btnRemoveItem });

            itemsPanel.Controls.Add(_dgvItems);
            itemsPanel.Controls.Add(itemButtonsPanel);
            itemsPanel.Controls.Add(itemsLabel);

            // پنل جمع‌کل
            var totalsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            var lblTotalLabel = new Label { Text = "جمع کل:", Location = new System.Drawing.Point(10, 20), AutoSize = true, Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold) };
            _lblTotalAmount = new Label { Text = "0 تومان", Location = new System.Drawing.Point(100, 20), AutoSize = true, Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold), ForeColor = Color.Blue };

            var lblDiscountLabel = new Label { Text = "تخفیف کل:", Location = new System.Drawing.Point(200, 20), AutoSize = true, Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold) };
            _lblDiscountAmount = new Label { Name = "lblDiscountAmount", Text = "0 تومان", Location = new System.Drawing.Point(300, 20), AutoSize = true, Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold), ForeColor = Color.Orange };

            var lblFinalLabel = new Label { Text = "مبلغ نهایی:", Location = new System.Drawing.Point(400, 20), AutoSize = true, Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold) };
            _lblFinalAmount = new Label { Text = "0 تومان", Location = new System.Drawing.Point(500, 20), AutoSize = true, Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold), ForeColor = Color.Green };

            totalsPanel.Controls.AddRange(new Control[] { lblTotalLabel, _lblTotalAmount, lblDiscountLabel, _lblDiscountAmount, lblFinalLabel, _lblFinalAmount });

            // پنل دکمه‌های اصلی
            var mainButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            var btnSave = new Button
            {
                Text = "ذخیره",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(10, 7),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "انصراف",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(120, 7),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += BtnCancel_Click;

            mainButtonsPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            mainPanel.Controls.Add(itemsPanel);
            mainPanel.Controls.Add(totalsPanel);
            mainPanel.Controls.Add(mainButtonsPanel);
            mainPanel.Controls.Add(headerPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadCustomers()
        {
            try
            {
                var customers = _customerRepository.GetAll();
                _cmbCustomer.Items.Clear();
                _cmbCustomer.Items.AddRange(customers.Select(c => c.Name).ToArray());
                if (_cmbCustomer.Items.Count > 0)
                    _cmbCustomer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری مشتریان: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAll();
                // محصولات در فرم افزودن آیتم استفاده می‌شوند
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری محصولات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            try
            {
                // اطمینان از وجود DataGridView
                if (_dgvItems == null)
                {
                    MessageBox.Show("خطا: DataGridView در دسترس نیست", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // پاک کردن ستون‌های موجود به صورت ایمن
                if (_dgvItems.InvokeRequired)
                {
                    _dgvItems.Invoke(new Action(() => _dgvItems.Columns.Clear()));
                }
                else
                {
                    _dgvItems.Columns.Clear();
                }

                // تعریف ستون‌ها
                var columns = new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn { Name = "ProductId", HeaderText = "شناسه محصول", Visible = false },
                    new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "نام محصول", Width = 200 },
                    new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "تعداد", Width = 80 },
                    new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "قیمت واحد", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "TotalPrice", HeaderText = "قیمت کل", Width = 120 },
                    new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                    new DataGridViewTextBoxColumn { Name = "FinalPrice", HeaderText = "قیمت نهایی", Width = 120 }
                };

                // اضافه کردن ستون‌ها به صورت ایمن
                if (_dgvItems.InvokeRequired)
                {
                    _dgvItems.Invoke(new Action(() => _dgvItems.Columns.AddRange(columns)));
                }
                else
                {
                    _dgvItems.Columns.AddRange(columns);
                }

                // تنظیمات اضافی DataGridView
                if (_dgvItems.InvokeRequired)
                {
                    _dgvItems.Invoke(new Action(() =>
                    {
                        _dgvItems.AllowUserToAddRows = false;
                        _dgvItems.AllowUserToDeleteRows = false;
                        _dgvItems.ReadOnly = true;
                        _dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        _dgvItems.MultiSelect = false;
                    }));
                }
                else
                {
                    _dgvItems.AllowUserToAddRows = false;
                    _dgvItems.AllowUserToDeleteRows = false;
                    _dgvItems.ReadOnly = true;
                    _dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    _dgvItems.MultiSelect = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در تنظیم DataGridView: {ex.Message}\n\nجزئیات: {ex.StackTrace}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSaleData()
        {
            try
            {
                if (_sale != null)
                {
                    _txtInvoiceNumber.Text = _sale.InvoiceNumber;
                    _cmbCustomer.Text = _sale.Customer?.Name ?? "";
                    _dtpSaleDate.Value = _sale.SaleDate;
                    _numDiscountAmount.Value = _sale.DiscountAmount;
                    _numTaxAmount.Value = _sale.TaxAmount;
                    _txtNotes.Text = _sale.Notes ?? "";

                    _saleItems = new List<SaleItem>(_sale.Items ?? new List<SaleItem>());
                    
                    // اطمینان از آماده بودن DataGridView
                    EnsureDataGridViewReady();
                    
                    RefreshItemsGrid();
                    CalculateTotals();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری اطلاعات فروش: {ex.Message}\n\nجزئیات: {ex.StackTrace}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureDataGridViewReady()
        {
            try
            {
                // اطمینان از وجود DataGridView
                if (_dgvItems == null)
                {
                    MessageBox.Show("خطا: DataGridView در دسترس نیست", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // اطمینان از وجود ستون‌ها
                if (_dgvItems.Columns.Count == 0)
                {
                    SetupDataGridView();
                    
                    // بررسی مجدد
                    if (_dgvItems.Columns.Count == 0)
                    {
                        MessageBox.Show("خطا: ستون‌های DataGridView تنظیم نشد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // اطمینان از آماده بودن کنترل
                if (!_dgvItems.IsHandleCreated)
                {
                    _dgvItems.CreateHandle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در آماده‌سازی DataGridView: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshItemsGrid()
        {
            try
            {
                // اطمینان از آماده بودن DataGridView
                EnsureDataGridViewReady();

                // پاک کردن ردیف‌ها به صورت ایمن
                if (_dgvItems.InvokeRequired)
                {
                    _dgvItems.Invoke(new Action(() => _dgvItems.Rows.Clear()));
                }
                else
                {
                    _dgvItems.Rows.Clear();
                }

                // اضافه کردن ردیف‌ها
                foreach (var item in _saleItems)
                {
                    if (_dgvItems.InvokeRequired)
                    {
                        _dgvItems.Invoke(new Action(() => _dgvItems.Rows.Add(
                            item.ProductId,
                            item.Product?.Name ?? "نامشخص",
                            item.Quantity,
                            item.UnitPrice.ToString("N0"),
                            item.TotalPrice.ToString("N0"),
                            item.DiscountAmount.ToString("N0"),
                            item.FinalPrice.ToString("N0")
                        )));
                    }
                    else
                    {
                        _dgvItems.Rows.Add(
                            item.ProductId,
                            item.Product?.Name ?? "نامشخص",
                            item.Quantity,
                            item.UnitPrice.ToString("N0"),
                            item.TotalPrice.ToString("N0"),
                            item.DiscountAmount.ToString("N0"),
                            item.FinalPrice.ToString("N0")
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری آیتم‌ها: {ex.Message}\n\nجزئیات: {ex.StackTrace}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateTotals(object? sender = null, EventArgs? e = null)
        {
            try
            {
                var totalAmount = _saleItems.Sum(item => item.TotalPrice);
                var itemsDiscount = _saleItems.Sum(item => item.DiscountAmount);
                var invoiceDiscount = _numDiscountAmount.Value;
                var totalDiscount = itemsDiscount + invoiceDiscount;
                var taxAmount = _numTaxAmount.Value;
                var finalAmount = totalAmount - totalDiscount + taxAmount;

                // اطمینان از عدم منفی بودن مبلغ نهایی
                if (finalAmount < 0)
                {
                    finalAmount = 0;
                }

                _lblTotalAmount.Text = totalAmount.ToString("N0") + " تومان";
                _lblDiscountAmount.Text = totalDiscount.ToString("N0") + " تومان";
                _lblFinalAmount.Text = finalAmount.ToString("N0") + " تومان";

                // تغییر رنگ مبلغ نهایی در صورت صفر بودن
                if (finalAmount == 0)
                {
                    _lblFinalAmount.ForeColor = Color.Orange;
                }
                else
                {
                    _lblFinalAmount.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در محاسبه مبالغ: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            var addItemForm = new AddEditSaleItemForm();
            if (addItemForm.ShowDialog() == DialogResult.OK)
            {
                var newItem = addItemForm.GetSaleItem();
                _saleItems.Add(newItem);
                RefreshItemsGrid();
                CalculateTotals();
            }
        }

        private void BtnEditItem_Click(object? sender, EventArgs e)
        {
            if (_dgvItems.SelectedRows.Count > 0)
            {
                var rowIndex = _dgvItems.SelectedRows[0].Index;
                var item = _saleItems[rowIndex];
                var editItemForm = new AddEditSaleItemForm(item);
                if (editItemForm.ShowDialog() == DialogResult.OK)
                {
                    var updatedItem = editItemForm.GetSaleItem();
                    _saleItems[rowIndex] = updatedItem;
                    RefreshItemsGrid();
                    CalculateTotals();
                }
            }
            else
            {
                MessageBox.Show("لطفاً یک آیتم را انتخاب کنید.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRemoveItem_Click(object? sender, EventArgs e)
        {
            if (_dgvItems.SelectedRows.Count > 0)
            {
                var rowIndex = _dgvItems.SelectedRows[0].Index;
                _saleItems.RemoveAt(rowIndex);
                RefreshItemsGrid();
                CalculateTotals();
            }
            else
            {
                MessageBox.Show("لطفاً یک آیتم را انتخاب کنید.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    var sale = _sale ?? new Sale();
                    sale.InvoiceNumber = _txtInvoiceNumber.Text.Trim();
                    sale.SaleDate = _dtpSaleDate.Value;
                    sale.DiscountAmount = _numDiscountAmount.Value;
                    sale.TaxAmount = _numTaxAmount.Value;
                    sale.Notes = _txtNotes.Text.Trim();
                    sale.Items = new List<SaleItem>(_saleItems);

                    // محاسبه مبالغ با استفاده از helper
                    AccountingApp.Utilities.InvoiceValidationHelper.RecalculateSaleAmounts(sale);

                    // پیدا کردن مشتری
                    var customerName = _cmbCustomer.Text;
                    var customer = _customerRepository.GetAll().FirstOrDefault(c => c.Name == customerName);
                    if (customer != null)
                    {
                        sale.CustomerId = customer.Id;
                    }

                    bool success;
                    if (_isEdit)
                    {
                        success = _saleRepository.Update(sale);
                        if (success)
                            MessageBox.Show("فروش با موفقیت به‌روزرسانی شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var id = _saleRepository.Add(sale);
                        success = id > 0;
                        if (success)
                            MessageBox.Show("فروش با موفقیت ثبت شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (success)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("خطا در ذخیره فروش.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره فروش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtInvoiceNumber.Text))
            {
                MessageBox.Show("لطفاً شماره فاکتور را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtInvoiceNumber.Focus();
                return false;
            }

            if (_cmbCustomer.SelectedIndex < 0)
            {
                MessageBox.Show("لطفاً مشتری را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbCustomer.Focus();
                return false;
            }

            if (_saleItems.Count == 0)
            {
                MessageBox.Show("لطفاً حداقل یک آیتم به فروش اضافه کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // استفاده از helper برای اعتبارسنجی کامل
            var sale = new Sale
            {
                Items = _saleItems,
                DiscountAmount = _numDiscountAmount.Value,
                TaxAmount = _numTaxAmount.Value
            };

            var (isValid, errorMessage) = AccountingApp.Utilities.InvoiceValidationHelper.ValidateSaleInvoice(sale);
            if (!isValid)
            {
                MessageBox.Show(errorMessage, "خطا در اعتبارسنجی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
} 