using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

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
            // پیاده‌سازی فرم مشابه AddEditProductForm
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ایجاد کنترل‌ها و چیدمان
            // ... کد مشابه AddEditProductForm
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
            // پیاده‌سازی ذخیره تامین‌کننده
        }
    }
}