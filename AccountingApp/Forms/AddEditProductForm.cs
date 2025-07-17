using System;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class AddEditProductForm : Form
    {
        private readonly ProductRepository _productRepository;
        private readonly Product? _product;
        private readonly bool _isEdit;

        private TextBox? _txtName;
        private TextBox? _txtCode;
        private TextBox? _txtDescription;
        private TextBox? _txtCategory;
        private NumericUpDown? _numPrice;
        private NumericUpDown? _numCostPrice;
        private NumericUpDown? _numStockQuantity;
        private NumericUpDown? _numMinStockLevel;
        private NumericUpDown? _numWeight;
        private ComboBox? _cmbUnit;
        private CheckBox? _chkIsActive;

        public AddEditProductForm(Product? product = null)
        {
            InitializeComponent();
            _productRepository = new ProductRepository();
            _product = product;
            _isEdit = product != null;

            if (_isEdit)
            {
                this.Text = "ویرایش محصول";
                LoadProductData();
            }
            else
            {
                this.Text = "افزودن محصول جدید";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // پنل اصلی
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // جدول چیدمان
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11,
                Padding = new Padding(10)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            // نام محصول
            var lblName = new Label { Text = "نام محصول:", TextAlign = ContentAlignment.MiddleRight };
            _txtName = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblName, 0, 0);
            tableLayout.Controls.Add(_txtName, 1, 0);

            // کد محصول
            var lblCode = new Label { Text = "کد محصول:", TextAlign = ContentAlignment.MiddleRight };
            _txtCode = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblCode, 0, 1);
            tableLayout.Controls.Add(_txtCode, 1, 1);

            // توضیحات
            var lblDescription = new Label { Text = "توضیحات:", TextAlign = ContentAlignment.MiddleRight };
            _txtDescription = new TextBox 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            tableLayout.Controls.Add(lblDescription, 0, 2);
            tableLayout.Controls.Add(_txtDescription, 1, 2);

            // دسته‌بندی
            var lblCategory = new Label { Text = "دسته‌بندی:", TextAlign = ContentAlignment.MiddleRight };
            _txtCategory = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblCategory, 0, 3);
            tableLayout.Controls.Add(_txtCategory, 1, 3);

            // قیمت فروش
            var lblPrice = new Label { Text = "قیمت فروش:", TextAlign = ContentAlignment.MiddleRight };
            _numPrice = new NumericUpDown 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 25,
                Maximum = 999999999,
                Minimum = 0,
                DecimalPlaces = 0
            };
            tableLayout.Controls.Add(lblPrice, 0, 4);
            tableLayout.Controls.Add(_numPrice, 1, 4);

            // قیمت تمام شده
            var lblCostPrice = new Label { Text = "قیمت تمام شده:", TextAlign = ContentAlignment.MiddleRight };
            _numCostPrice = new NumericUpDown 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 25,
                Maximum = 999999999,
                Minimum = 0,
                DecimalPlaces = 0
            };
            tableLayout.Controls.Add(lblCostPrice, 0, 5);
            tableLayout.Controls.Add(_numCostPrice, 1, 5);

            // موجودی
            var lblStockQuantity = new Label { Text = "موجودی اولیه:", TextAlign = ContentAlignment.MiddleRight };
            _numStockQuantity = new NumericUpDown 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 25,
                Maximum = 999999,
                Minimum = 0,
                DecimalPlaces = 0
            };
            tableLayout.Controls.Add(lblStockQuantity, 0, 6);
            tableLayout.Controls.Add(_numStockQuantity, 1, 6);

            // حداقل موجودی
            var lblMinStockLevel = new Label { Text = "حداقل موجودی:", TextAlign = ContentAlignment.MiddleRight };
            _numMinStockLevel = new NumericUpDown 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 25,
                Maximum = 999999,
                Minimum = 0,
                DecimalPlaces = 0,
                Value = 10
            };
            tableLayout.Controls.Add(lblMinStockLevel, 0, 7);
            tableLayout.Controls.Add(_numMinStockLevel, 1, 7);

            // وزن
            var lblWeight = new Label { Text = "وزن (گرم):", TextAlign = ContentAlignment.MiddleRight };
            _numWeight = new NumericUpDown 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 25,
                Maximum = 999999,
                Minimum = 0,
                DecimalPlaces = 2
            };
            tableLayout.Controls.Add(lblWeight, 0, 8);
            tableLayout.Controls.Add(_numWeight, 1, 8);

            // واحد
            var lblUnit = new Label { Text = "واحد:", TextAlign = ContentAlignment.MiddleRight };
            _cmbUnit = new ComboBox 
            { 
                Font = new System.Drawing.Font("Tahoma", 10), 
                Height = 25,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbUnit.Items.AddRange(new object[] { "عدد", "کیلو", "متر", "لیتر", "بسته", "جفت" });
            _cmbUnit.SelectedIndex = 0;
            tableLayout.Controls.Add(lblUnit, 0, 9);
            tableLayout.Controls.Add(_cmbUnit, 1, 9);

            // فعال
            var lblIsActive = new Label { Text = "فعال:", TextAlign = ContentAlignment.MiddleRight };
            _chkIsActive = new CheckBox { Text = "محصول فعال است", Checked = true };
            tableLayout.Controls.Add(lblIsActive, 0, 10);
            tableLayout.Controls.Add(_chkIsActive, 1, 10);

            // پنل دکمه‌ها
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            var btnSave = new Button
            {
                Text = "ذخیره",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(10, 12),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "انصراف",
                Size = new System.Drawing.Size(100, 35),
                Location = new System.Drawing.Point(120, 12),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += BtnCancel_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            mainPanel.Controls.Add(tableLayout);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadProductData()
        {
            if (_product != null)
            {
                _txtName!.Text = _product.Name;
                _txtCode!.Text = _product.Code;
                _txtDescription!.Text = _product.Description;
                _txtCategory!.Text = _product.Category;
                _numPrice!.Value = _product.Price;
                _numCostPrice!.Value = _product.CostPrice;
                _numStockQuantity!.Value = _product.StockQuantity;
                _numMinStockLevel!.Value = _product.MinStockLevel;
                _numWeight!.Value = _product.Weight;
                _cmbUnit!.Text = _product.Unit;
                _chkIsActive!.Checked = _product.IsActive;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    var product = _product ?? new Product();
                    product.Name = _txtName!.Text.Trim();
                    product.Code = _txtCode!.Text.Trim();
                    product.Description = _txtDescription!.Text.Trim();
                    product.Category = _txtCategory!.Text.Trim();
                    product.Price = _numPrice!.Value;
                    product.CostPrice = _numCostPrice!.Value;
                    product.StockQuantity = (int)_numStockQuantity!.Value;
                    product.MinStockLevel = (int)_numMinStockLevel!.Value;
                    product.Weight = _numWeight!.Value;
                    product.Unit = _cmbUnit!.Text;
                    product.IsActive = _chkIsActive!.Checked;

                    bool success;
                    if (_isEdit)
                    {
                        success = _productRepository.Update(product);
                        if (success)
                            MessageBox.Show("محصول با موفقیت به‌روزرسانی شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var id = _productRepository.Add(product);
                        success = id > 0;
                        if (success)
                            MessageBox.Show("محصول با موفقیت اضافه شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (success)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("خطا در ذخیره محصول.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtName!.Text))
            {
                MessageBox.Show("لطفاً نام محصول را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtCode!.Text))
            {
                MessageBox.Show("لطفاً کد محصول را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCode.Focus();
                return false;
            }

            // بررسی تکراری نبودن کد محصول
            if (!_isEdit)
            {
                var existingProduct = _productRepository.GetByCode(_txtCode.Text.Trim());
                if (existingProduct != null)
                {
                    MessageBox.Show("کد محصول تکراری است. لطفاً کد دیگری انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtCode.Focus();
                    return false;
                }
            }

            if (_numPrice!.Value <= 0)
            {
                MessageBox.Show("قیمت فروش باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numPrice.Focus();
                return false;
            }

            if (_numCostPrice!.Value <= 0)
            {
                MessageBox.Show("قیمت تمام شده باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numCostPrice.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_cmbUnit!.Text))
            {
                MessageBox.Show("لطفاً واحد محصول را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbUnit.Focus();
                return false;
            }

            return true;
        }
    }
}