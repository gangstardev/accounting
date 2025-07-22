using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class AddEditPurchaseForm : Form
    {
        private readonly PurchaseRepository _purchaseRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly ProductRepository _productRepository;
        private readonly Purchase? _purchase;
        private readonly bool _isEdit;

        private TextBox? _txtInvoiceNumber;
        private ComboBox? _cmbSupplier;
        private PersianDateTimePicker? _dtpPurchaseDate;
        private DataGridView? _dgvItems;
        private NumericUpDown? _numDiscountAmount;
        private NumericUpDown? _numTaxAmount;
        private TextBox? _txtNotes;
        private Label? _lblTotalAmount;
        private Label? _lblDiscountAmount;
        private Label? _lblFinalAmount;
        private List<PurchaseItem> _purchaseItems;

        public AddEditPurchaseForm(Purchase? purchase = null)
        {
            InitializeComponent();
            _purchaseRepository = new PurchaseRepository();
            _supplierRepository = new SupplierRepository();
            _productRepository = new ProductRepository();
            _purchase = purchase;
            _isEdit = purchase != null;
            _purchaseItems = new List<PurchaseItem>();

            if (_isEdit)
            {
                this.Text = "ویرایش خرید";
                LoadPurchaseData();
            }
            else
            {
                this.Text = "خرید جدید";
                _txtInvoiceNumber!.Text = _purchaseRepository.GenerateInvoiceNumber();
                _dtpPurchaseDate!.Value = DateTime.Now;
            }

            LoadSuppliers();
            LoadProducts();
            SetupDataGridView();
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

            // تامین‌کننده
            var lblSupplier = new Label { Text = "تامین‌کننده:", TextAlign = ContentAlignment.MiddleRight };
            _cmbSupplier = new ComboBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, DropDownStyle = ComboBoxStyle.DropDownList };
            headerLayout.Controls.Add(lblSupplier, 2, 0);
            headerLayout.Controls.Add(_cmbSupplier, 3, 0);

            // تاریخ خرید
            var lblPurchaseDate = new Label { Text = "تاریخ خرید:", TextAlign = ContentAlignment.MiddleRight };
            _dtpPurchaseDate = new PersianDateTimePicker { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            headerLayout.Controls.Add(lblPurchaseDate, 0, 1);
            headerLayout.Controls.Add(_dtpPurchaseDate, 1, 1);

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

            var itemsLabel = new Label { Text = "آیتم‌های خرید:", Dock = DockStyle.Top, Height = 25, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };

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

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = _supplierRepository.GetAll();
                _cmbSupplier!.Items.Clear();
                _cmbSupplier.Items.AddRange(suppliers.Select(s => s.Name).ToArray());
                if (_cmbSupplier.Items.Count > 0)
                    _cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری تامین‌کنندگان: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _dgvItems!.Columns.Clear();
            _dgvItems.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "ProductId", HeaderText = "شناسه محصول", Visible = false },
                new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "نام محصول", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "تعداد", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "قیمت واحد", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "TotalPrice", HeaderText = "قیمت کل", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "FinalPrice", HeaderText = "قیمت نهایی", Width = 120 }
            });
        }

        private void LoadPurchaseData()
        {
            if (_purchase != null)
            {
                _txtInvoiceNumber!.Text = _purchase.InvoiceNumber;
                _cmbSupplier!.Text = _purchase.Supplier.Name;
                _dtpPurchaseDate!.Value = _purchase.PurchaseDate;
                _numDiscountAmount!.Value = _purchase.DiscountAmount;
                _numTaxAmount!.Value = _purchase.TaxAmount;
                _txtNotes!.Text = _purchase.Notes;

                _purchaseItems = new List<PurchaseItem>(_purchase.Items);
                RefreshItemsGrid();
                CalculateTotals();
            }
        }

        private void RefreshItemsGrid()
        {
            _dgvItems!.Rows.Clear();
            foreach (var item in _purchaseItems)
            {
                _dgvItems.Rows.Add(
                    item.ProductId,
                    item.Product.Name,
                    item.Quantity,
                    item.UnitPrice.ToString("N0"),
                    item.TotalPrice.ToString("N0"),
                    item.DiscountAmount.ToString("N0"),
                    item.FinalPrice.ToString("N0")
                );
            }
        }

        private void CalculateTotals(object? sender = null, EventArgs? e = null)
        {
            var totalAmount = _purchaseItems.Sum(item => item.TotalPrice);
            var itemsDiscount = _purchaseItems.Sum(item => item.DiscountAmount);
            var invoiceDiscount = _numDiscountAmount!.Value;
            var totalDiscount = itemsDiscount + invoiceDiscount;
            var taxAmount = _numTaxAmount!.Value;
            var finalAmount = totalAmount - totalDiscount + taxAmount;

            _lblTotalAmount!.Text = totalAmount.ToString("N0") + " تومان";
            _lblDiscountAmount!.Text = totalDiscount.ToString("N0") + " تومان";
            _lblFinalAmount!.Text = finalAmount.ToString("N0") + " تومان";
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            var addItemForm = new AddEditPurchaseItemForm();
            if (addItemForm.ShowDialog() == DialogResult.OK)
            {
                var newItem = addItemForm.GetPurchaseItem();
                _purchaseItems.Add(newItem);
                RefreshItemsGrid();
                CalculateTotals();
            }
        }

        private void BtnEditItem_Click(object? sender, EventArgs e)
        {
            if (_dgvItems!.SelectedRows.Count > 0)
            {
                var rowIndex = _dgvItems.SelectedRows[0].Index;
                var item = _purchaseItems[rowIndex];
                var editItemForm = new AddEditPurchaseItemForm(item);
                if (editItemForm.ShowDialog() == DialogResult.OK)
                {
                    var updatedItem = editItemForm.GetPurchaseItem();
                    _purchaseItems[rowIndex] = updatedItem;
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
            if (_dgvItems!.SelectedRows.Count > 0)
            {
                var rowIndex = _dgvItems.SelectedRows[0].Index;
                _purchaseItems.RemoveAt(rowIndex);
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
                    var purchase = _purchase ?? new Purchase();
                    purchase.InvoiceNumber = _txtInvoiceNumber!.Text.Trim();
                    purchase.PurchaseDate = _dtpPurchaseDate!.Value;
                    purchase.DiscountAmount = _numDiscountAmount!.Value;
                    purchase.TaxAmount = _numTaxAmount!.Value;
                    purchase.Notes = _txtNotes!.Text.Trim();
                    purchase.Items = new List<PurchaseItem>(_purchaseItems);

                    // محاسبه مبالغ
                    purchase.TotalAmount = _purchaseItems.Sum(item => item.TotalPrice);
                    var itemsDiscount = _purchaseItems.Sum(item => item.DiscountAmount);
                    purchase.FinalAmount = purchase.TotalAmount - itemsDiscount - purchase.DiscountAmount + purchase.TaxAmount;

                    // پیدا کردن تامین‌کننده
                    var supplierName = _cmbSupplier!.Text;
                    var supplier = _supplierRepository.GetAll().FirstOrDefault(s => s.Name == supplierName);
                    if (supplier != null)
                    {
                        purchase.SupplierId = supplier.Id;
                    }

                    bool success;
                    if (_isEdit)
                    {
                        success = _purchaseRepository.Update(purchase);
                        if (success)
                            MessageBox.Show("خرید با موفقیت به‌روزرسانی شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var id = _purchaseRepository.Add(purchase);
                        success = id > 0;
                        if (success)
                            MessageBox.Show("خرید با موفقیت ثبت شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (success)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("خطا در ذخیره خرید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره خرید: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtInvoiceNumber!.Text))
            {
                MessageBox.Show("لطفاً شماره فاکتور را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtInvoiceNumber.Focus();
                return false;
            }

            if (_cmbSupplier!.SelectedIndex < 0)
            {
                MessageBox.Show("لطفاً تامین‌کننده را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbSupplier.Focus();
                return false;
            }

            if (_purchaseItems.Count == 0)
            {
                MessageBox.Show("لطفاً حداقل یک آیتم به خرید اضافه کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
} 