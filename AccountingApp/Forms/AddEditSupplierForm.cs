using System;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;
using System.Drawing;

namespace AccountingApp.Forms
{
    public partial class AddEditSupplierForm : Form
    {
        private readonly SupplierRepository _supplierRepository;
        private readonly Supplier? _supplier;
        private readonly bool _isEdit;

        // کنترل‌ها
        private TextBox? _txtName;
        private TextBox? _txtPhone;
        private TextBox? _txtAddress;
        private TextBox? _txtNationalCode;
        private TextBox? _txtEmail;
        private CheckBox? _chkIsActive;
        private Button? _btnSave;
        private Button? _btnCancel;

        public AddEditSupplierForm(Supplier? supplier = null)
        {
            InitializeComponent();
            _supplierRepository = new SupplierRepository();
            _supplier = supplier;
            _isEdit = supplier != null;

            if (_isEdit)
            {
                this.Text = "ویرایش تامین‌کننده";
                LoadSupplierData();
            }
            else
            {
                this.Text = "افزودن تامین‌کننده جدید";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // عنوان
            var lblTitle = new Label
            {
                Text = _isEdit ? "ویرایش تامین‌کننده" : "افزودن تامین‌کننده جدید",
                Font = new System.Drawing.Font("Tahoma", 14, System.Drawing.FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // فرم
            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(10),
                AutoSize = true
            };

            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // نام
            var lblName = new Label { Text = "نام تامین‌کننده:", Font = new System.Drawing.Font("Tahoma", 10), TextAlign = ContentAlignment.MiddleRight };
            _txtName = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Width = 250 };

            // تلفن
            var lblPhone = new Label { Text = "تلفن:", Font = new System.Drawing.Font("Tahoma", 10), TextAlign = ContentAlignment.MiddleRight };
            _txtPhone = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Width = 250 };

            // آدرس
            var lblAddress = new Label { Text = "آدرس:", Font = new System.Drawing.Font("Tahoma", 10), TextAlign = ContentAlignment.MiddleRight };
            _txtAddress = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 60, Width = 250, Multiline = true };

            // کد ملی
            var lblNationalCode = new Label { Text = "کد ملی:", Font = new System.Drawing.Font("Tahoma", 10), TextAlign = ContentAlignment.MiddleRight };
            _txtNationalCode = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Width = 250 };

            // ایمیل
            var lblEmail = new Label { Text = "ایمیل:", Font = new System.Drawing.Font("Tahoma", 10), TextAlign = ContentAlignment.MiddleRight };
            _txtEmail = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25, Width = 250 };

            // فعال
            var lblIsActive = new Label { Text = "فعال:", Font = new System.Drawing.Font("Tahoma", 10), TextAlign = ContentAlignment.MiddleRight };
            _chkIsActive = new CheckBox { Text = "تامین‌کننده فعال است", Font = new System.Drawing.Font("Tahoma", 10), Checked = true };

            // دکمه‌ها
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
            _btnSave = new Button
            {
                Text = "ذخیره",
                Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(100, 35),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Location = new System.Drawing.Point(200, 15)
            };
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "انصراف",
                Font = new System.Drawing.Font("Tahoma", 10),
                Size = new System.Drawing.Size(100, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Location = new System.Drawing.Point(90, 15)
            };
            _btnCancel.Click += BtnCancel_Click;

            buttonPanel.Controls.AddRange(new Control[] { _btnSave, _btnCancel });

            // اضافه کردن کنترل‌ها به فرم
            formPanel.Controls.Add(lblName, 0, 0);
            formPanel.Controls.Add(_txtName, 1, 0);
            formPanel.Controls.Add(lblPhone, 0, 1);
            formPanel.Controls.Add(_txtPhone, 1, 1);
            formPanel.Controls.Add(lblAddress, 0, 2);
            formPanel.Controls.Add(_txtAddress, 1, 2);
            formPanel.Controls.Add(lblNationalCode, 0, 3);
            formPanel.Controls.Add(_txtNationalCode, 1, 3);
            formPanel.Controls.Add(lblEmail, 0, 4);
            formPanel.Controls.Add(_txtEmail, 1, 4);
            formPanel.Controls.Add(lblIsActive, 0, 5);
            formPanel.Controls.Add(_chkIsActive, 1, 5);

            mainPanel.Controls.Add(formPanel);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadSupplierData()
        {
            if (_supplier != null)
            {
                _txtName!.Text = _supplier.Name;
                _txtPhone!.Text = _supplier.Phone;
                _txtAddress!.Text = _supplier.Address;
                _txtNationalCode!.Text = _supplier.NationalCode;
                _txtEmail!.Text = _supplier.Email;
                _chkIsActive!.Checked = _supplier.IsActive;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (ValidateForm())
                {
                    var supplier = _supplier ?? new Supplier();
                    supplier.Name = _txtName!.Text.Trim();
                    supplier.Phone = _txtPhone!.Text.Trim();
                    supplier.Address = _txtAddress!.Text.Trim();
                    supplier.NationalCode = _txtNationalCode!.Text.Trim();
                    supplier.Email = _txtEmail!.Text.Trim();
                    supplier.IsActive = _chkIsActive!.Checked;

                    bool success;
                    if (_isEdit)
                    {
                        success = _supplierRepository.Update(supplier);
                        if (success)
                            MessageBox.Show("تامین‌کننده با موفقیت ویرایش شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var supplierId = _supplierRepository.Add(supplier);
                        success = supplierId > 0;
                        if (success)
                            MessageBox.Show("تامین‌کننده با موفقیت افزوده شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (success)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("خطا در ذخیره تامین‌کننده.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره تامین‌کننده: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(_txtName!.Text))
            {
                MessageBox.Show("لطفاً نام تامین‌کننده را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtPhone!.Text))
            {
                MessageBox.Show("لطفاً شماره تلفن را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPhone.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_txtEmail!.Text))
            {
                try
                {
                    var email = new System.Net.Mail.MailAddress(_txtEmail.Text);
                }
                catch
                {
                    MessageBox.Show("لطفاً ایمیل معتبر وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }
    }
}