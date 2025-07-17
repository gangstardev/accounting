using System;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class AddEditPurchaseItemForm : Form
    {
        private readonly ProductRepository _productRepository;
        private readonly PurchaseItem? _purchaseItem;
        private readonly bool _isEdit;

        private ComboBox? _cmbProduct;
        private NumericUpDown? _numQuantity;
        private NumericUpDown? _numUnitPrice;
        private NumericUpDown? _numDiscountAmount;
        private Label? _lblTotalPrice;
        private Label? _lblFinalPrice;
        private Label? _lblStockQuantity;

        public AddEditPurchaseItemForm(PurchaseItem? purchaseItem = null)
        {
            InitializeComponent();
            _productRepository = new ProductRepository();
            _purchaseItem = purchaseItem;
            _isEdit = purchaseItem != null;

            if (_isEdit)
            {
                this.Text = "ویرایش آیتم خرید";
                LoadPurchaseItemData();
            }
            else
            {
                this.Text = "افزودن آیتم خرید";
            }

            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(10)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // محصول
            var lblProduct = new Label { Text = "محصول:", TextAlign = ContentAlignment.MiddleRight };
            _cmbProduct = new ComboBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbProduct.SelectedIndexChanged += Product_SelectedIndexChanged;
            tableLayout.Controls.Add(lblProduct, 0, 0);
            tableLayout.Controls.Add(_cmbProduct, 1, 0);

            // موجودی
            var lblStock = new Label { Text = "موجودی فعلی:", TextAlign = ContentAlignment.MiddleRight };
            _lblStockQuantity = new Label { Text = "0", Font = new System.Drawing.Font("Tahoma", 10), ForeColor = Color.Blue };
            tableLayout.Controls.Add(lblStock, 0, 1);
            tableLayout.Controls.Add(_lblStockQuantity, 1, 1);

            // تعداد
            var lblQuantity = new Label { Text = "تعداد:", TextAlign = ContentAlignment.MiddleRight };
            _numQuantity = new NumericUpDown { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Maximum = 999999, Minimum = 1, DecimalPlaces = 0 };
            _numQuantity.ValueChanged += CalculatePrices;
            tableLayout.Controls.Add(lblQuantity, 0, 2);
            tableLayout.Controls.Add(_numQuantity, 1, 2);

            // قیمت واحد
            var lblUnitPrice = new Label { Text = "قیمت واحد (تومان):", TextAlign = ContentAlignment.MiddleRight };
            _numUnitPrice = new NumericUpDown { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Maximum = 999999999, Minimum = 0, DecimalPlaces = 0 };
            _numUnitPrice.ValueChanged += CalculatePrices;
            tableLayout.Controls.Add(lblUnitPrice, 0, 3);
            tableLayout.Controls.Add(_numUnitPrice, 1, 3);

            // تخفیف
            var lblDiscount = new Label { Text = "تخفیف (تومان):", TextAlign = ContentAlignment.MiddleRight };
            _numDiscountAmount = new NumericUpDown { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Maximum = 999999999, Minimum = 0, DecimalPlaces = 0 };
            _numDiscountAmount.ValueChanged += CalculatePrices;
            tableLayout.Controls.Add(lblDiscount, 0, 4);
            tableLayout.Controls.Add(_numDiscountAmount, 1, 4);

            // قیمت کل
            var lblTotalPrice = new Label { Text = "قیمت کل:", TextAlign = ContentAlignment.MiddleRight, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblTotalPrice = new Label { Text = "0 تومان", Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold), ForeColor = Color.Blue };
            tableLayout.Controls.Add(lblTotalPrice, 0, 5);
            tableLayout.Controls.Add(_lblTotalPrice, 1, 5);

            // قیمت نهایی
            var lblFinalPrice = new Label { Text = "قیمت نهایی:", TextAlign = ContentAlignment.MiddleRight, Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold) };
            _lblFinalPrice = new Label { Text = "0 تومان", Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold), ForeColor = Color.Green };
            tableLayout.Controls.Add(lblFinalPrice, 0, 6);
            tableLayout.Controls.Add(_lblFinalPrice, 1, 6);

            // پنل دکمه‌ها
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };

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

        private void LoadProducts()
        {
            try
            {
                var products = _productRepository.GetAll();
                _cmbProduct!.Items.Clear();
                _cmbProduct.Items.AddRange(products.Select(p => p.Name).ToArray());
                if (_cmbProduct.Items.Count > 0)
                    _cmbProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری محصولات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchaseItemData()
        {
            if (_purchaseItem != null)
            {
                _cmbProduct!.Text = _purchaseItem.Product.Name;
                _numQuantity!.Value = _purchaseItem.Quantity;
                _numUnitPrice!.Value = _purchaseItem.UnitPrice;
                _numDiscountAmount!.Value = _purchaseItem.DiscountAmount;
                CalculatePrices();
            }
        }

        private void Product_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_cmbProduct!.SelectedIndex >= 0)
                {
                    var productName = _cmbProduct.Text;
                    var products = _productRepository.GetAll();
                    var product = products.FirstOrDefault(p => p.Name == productName);
                    
                    if (product != null)
                    {
                        _lblStockQuantity!.Text = product.StockQuantity.ToString();
                        _numUnitPrice!.Value = product.CostPrice;
                        CalculatePrices();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری اطلاعات محصول: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculatePrices(object? sender = null, EventArgs? e = null)
        {
            try
            {
                var quantity = (int)_numQuantity!.Value;
                var unitPrice = _numUnitPrice!.Value;
                var discountAmount = _numDiscountAmount!.Value;

                var totalPrice = quantity * unitPrice;
                var finalPrice = totalPrice - discountAmount;

                _lblTotalPrice!.Text = totalPrice.ToString("N0") + " تومان";
                _lblFinalPrice!.Text = finalPrice.ToString("N0") + " تومان";
            }
            catch
            {
                _lblTotalPrice!.Text = "0 تومان";
                _lblFinalPrice!.Text = "0 تومان";
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    var purchaseItem = _purchaseItem ?? new PurchaseItem();
                    
                    // پیدا کردن محصول
                    var productName = _cmbProduct!.Text;
                    var products = _productRepository.GetAll();
                    var product = products.FirstOrDefault(p => p.Name == productName);
                    
                    if (product != null)
                    {
                        purchaseItem.ProductId = product.Id;
                        purchaseItem.Product = product;
                        purchaseItem.Quantity = (int)_numQuantity!.Value;
                        purchaseItem.UnitPrice = _numUnitPrice!.Value;
                        purchaseItem.TotalPrice = purchaseItem.Quantity * purchaseItem.UnitPrice;
                        purchaseItem.DiscountAmount = _numDiscountAmount!.Value;
                        purchaseItem.FinalPrice = purchaseItem.TotalPrice - purchaseItem.DiscountAmount;

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("محصول انتخاب شده یافت نشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره آیتم: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (_cmbProduct!.SelectedIndex < 0)
            {
                MessageBox.Show("لطفاً محصول را انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbProduct.Focus();
                return false;
            }

            if (_numQuantity!.Value <= 0)
            {
                MessageBox.Show("تعداد باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numQuantity.Focus();
                return false;
            }

            if (_numUnitPrice!.Value <= 0)
            {
                MessageBox.Show("قیمت واحد باید بزرگتر از صفر باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numUnitPrice.Focus();
                return false;
            }

            return true;
        }

        public PurchaseItem GetPurchaseItem()
        {
            var purchaseItem = _purchaseItem ?? new PurchaseItem();
            
            var productName = _cmbProduct!.Text;
            var products = _productRepository.GetAll();
            var product = products.FirstOrDefault(p => p.Name == productName);
            
            if (product != null)
            {
                purchaseItem.ProductId = product.Id;
                purchaseItem.Product = product;
                purchaseItem.Quantity = (int)_numQuantity!.Value;
                purchaseItem.UnitPrice = _numUnitPrice!.Value;
                purchaseItem.TotalPrice = purchaseItem.Quantity * purchaseItem.UnitPrice;
                purchaseItem.DiscountAmount = _numDiscountAmount!.Value;
                purchaseItem.FinalPrice = purchaseItem.TotalPrice - purchaseItem.DiscountAmount;
            }
            
            return purchaseItem;
        }
    }
} 