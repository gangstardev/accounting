using System;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class AddEditCustomerForm : Form
    {
        private readonly CustomerRepository _customerRepository;
        private readonly Customer? _customer;
        private readonly bool _isEdit;

        private TextBox? _txtName;
        private TextBox? _txtPhone;
        private TextBox? _txtAddress;
        private TextBox? _txtNationalCode;
        private TextBox? _txtEmail;
        private CheckBox? _chkIsActive;

        public AddEditCustomerForm(Customer? customer = null)
        {
            InitializeComponent();
            _customerRepository = new CustomerRepository();
            _customer = customer;
            _isEdit = customer != null;

            if (_isEdit)
            {
                this.Text = "ویرایش مشتری";
                LoadCustomerData();
            }
            else
            {
                this.Text = "افزودن مشتری جدید";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 350);
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
                RowCount = 6,
                Padding = new Padding(10)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            var lblName = new Label { Text = "نام:", TextAlign = ContentAlignment.MiddleRight };
            _txtName = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblName, 0, 0);
            tableLayout.Controls.Add(_txtName, 1, 0);

            var lblPhone = new Label { Text = "تلفن:", TextAlign = ContentAlignment.MiddleRight };
            _txtPhone = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblPhone, 0, 1);
            tableLayout.Controls.Add(_txtPhone, 1, 1);

            var lblAddress = new Label { Text = "آدرس:", TextAlign = ContentAlignment.MiddleRight };
            _txtAddress = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical };
            tableLayout.Controls.Add(lblAddress, 0, 2);
            tableLayout.Controls.Add(_txtAddress, 1, 2);

            var lblNationalCode = new Label { Text = "کد ملی:", TextAlign = ContentAlignment.MiddleRight };
            _txtNationalCode = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblNationalCode, 0, 3);
            tableLayout.Controls.Add(_txtNationalCode, 1, 3);

            var lblEmail = new Label { Text = "ایمیل:", TextAlign = ContentAlignment.MiddleRight };
            _txtEmail = new TextBox { Font = new System.Drawing.Font("Tahoma", 10), Height = 25 };
            tableLayout.Controls.Add(lblEmail, 0, 4);
            tableLayout.Controls.Add(_txtEmail, 1, 4);

            var lblIsActive = new Label { Text = "فعال:", TextAlign = ContentAlignment.MiddleRight };
            _chkIsActive = new CheckBox { Text = "مشتری فعال است", Checked = true };
            tableLayout.Controls.Add(lblIsActive, 0, 5);
            tableLayout.Controls.Add(_chkIsActive, 1, 5);

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

        private void LoadCustomerData()
        {
            if (_customer != null)
            {
                _txtName.Text = _customer.Name;
                _txtPhone.Text = _customer.Phone;
                _txtAddress.Text = _customer.Address;
                _txtNationalCode.Text = _customer.NationalCode;
                _txtEmail.Text = _customer.Email;
                _chkIsActive.Checked = _customer.IsActive;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    var customer = _customer ?? new Customer();
                    customer.Name = _txtName.Text.Trim();
                    customer.Phone = _txtPhone.Text.Trim();
                    customer.Address = _txtAddress.Text.Trim();
                    customer.NationalCode = _txtNationalCode.Text.Trim();
                    customer.Email = _txtEmail.Text.Trim();
                    customer.IsActive = _chkIsActive.Checked;

                    bool success;
                    if (_isEdit)
                    {
                        success = _customerRepository.Update(customer);
                        if (success)
                            MessageBox.Show("مشتری با موفقیت به‌روزرسانی شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var id = _customerRepository.Add(customer);
                        success = id > 0;
                        if (success)
                            MessageBox.Show("مشتری با موفقیت اضافه شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (success)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("خطا در ذخیره مشتری.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره مشتری: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("لطفاً نام مشتری را وارد کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            return true;
        }
    }
} 