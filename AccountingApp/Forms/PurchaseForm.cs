using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;
using System.Drawing;

namespace AccountingApp.Forms
{
    public partial class PurchaseForm : Form
    {
        private readonly PurchaseRepository _purchaseRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly ProductRepository _productRepository;
        private DataGridView? _dataGridView;
        private PersianDateTimePicker? _dtpStartDate;
        private PersianDateTimePicker? _dtpEndDate;
        private ComboBox? _cmbSupplierFilter;
        private List<Purchase>? _purchases;

        public PurchaseForm()
        {
            InitializeComponent();
            _purchaseRepository = new PurchaseRepository();
            _supplierRepository = new SupplierRepository();
            _productRepository = new ProductRepository();
            
            // تنظیم تاریخ‌ها بعد از مقداردهی کنترل‌ها
            if (_dtpStartDate != null)
                _dtpStartDate.Value = DateTime.Today.AddDays(-30);
            if (_dtpEndDate != null)
                _dtpEndDate.Value = DateTime.Today;
            
            LoadSupplierFilter();
            LoadPurchases();
        }

        private void InitializeComponent()
        {
            this.Text = "مدیریت خرید";
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

            var lblStartDate = new Label { Text = "از تاریخ:", Location = new System.Drawing.Point(10, 25), AutoSize = true };
            _dtpStartDate = new PersianDateTimePicker 
            { 
                Location = new System.Drawing.Point(80, 22), 
                Size = new System.Drawing.Size(120, 25)
            };
            _dtpStartDate.ValueChanged += Filter_Changed;

            var lblEndDate = new Label { Text = "تا تاریخ:", Location = new System.Drawing.Point(220, 25), AutoSize = true };
            _dtpEndDate = new PersianDateTimePicker 
            { 
                Location = new System.Drawing.Point(290, 22), 
                Size = new System.Drawing.Size(120, 25)
            };
            _dtpEndDate.ValueChanged += Filter_Changed;

            var lblSupplier = new Label { Text = "تامین‌کننده:", Location = new System.Drawing.Point(430, 25), AutoSize = true };
            _cmbSupplierFilter = new ComboBox 
            { 
                Location = new System.Drawing.Point(520, 22), 
                Size = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbSupplierFilter.SelectedIndexChanged += Filter_Changed;

            var btnNewPurchase = new Button
            {
                Text = "خرید جدید",
                Location = new System.Drawing.Point(690, 20),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNewPurchase.Click += BtnNewPurchase_Click;

            var btnRefresh = new Button
            {
                Text = "تازه‌سازی",
                Location = new System.Drawing.Point(800, 20),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            var btnPrint = new Button
            {
                Text = "پرینت",
                Location = new System.Drawing.Point(890, 20),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrint.Click += BtnPrint_Click;

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblStartDate, _dtpStartDate, lblEndDate, _dtpEndDate, 
                lblSupplier, _cmbSupplierFilter, btnNewPurchase, btnRefresh, btnPrint 
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
            _dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "شناسه", Visible = false },
                new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "شماره فاکتور", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "SupplierName", HeaderText = "نام تامین‌کننده", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "PurchaseDate", HeaderText = "تاریخ خرید", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "مبلغ کل", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "DiscountAmount", HeaderText = "تخفیف", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "TaxAmount", HeaderText = "مالیات", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "FinalAmount", HeaderText = "مبلغ نهایی", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "ItemsCount", HeaderText = "تعداد آیتم", Width = 80 },
                new DataGridViewButtonColumn { Name = "Edit", HeaderText = "ویرایش", Text = "ویرایش", UseColumnTextForButtonValue = true, Width = 80 },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "حذف", Text = "حذف", UseColumnTextForButtonValue = true, Width = 80 },
                new DataGridViewButtonColumn { Name = "Print", HeaderText = "پرینت", Text = "پرینت", UseColumnTextForButtonValue = true, Width = 80 }
            });

            _dataGridView.CellClick += DataGridView_CellClick;

            mainPanel.Controls.Add(_dataGridView);
            mainPanel.Controls.Add(filterPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadSupplierFilter()
        {
            try
            {
                if (_cmbSupplierFilter == null)
                    return;
                var suppliers = _supplierRepository.GetAll() ?? new List<Supplier>();
                _cmbSupplierFilter.Items.Clear();
                _cmbSupplierFilter.Items.Add("همه تامین‌کنندگان");
                _cmbSupplierFilter.Items.AddRange(suppliers.Select(s => s.Name).ToArray());
                _cmbSupplierFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری فیلتر تامین‌کنندگان: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPurchases()
        {
            try
            {
                var startDate = _dtpStartDate!.Value.Date;
                var endDate = _dtpEndDate!.Value.Date;
                
                _purchases = _purchaseRepository.GetByDateRange(startDate, endDate);

                // فیلتر بر اساس تامین‌کننده
                if (_cmbSupplierFilter!.SelectedIndex > 0)
                {
                    var selectedSupplier = _cmbSupplierFilter.Text;
                    _purchases = _purchases.Where(p => p.Supplier.Name == selectedSupplier).ToList();
                }

                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری خریدها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            _dataGridView!.Rows.Clear();



            foreach (var purchase in _purchases ?? new List<Purchase>())
            {
                _dataGridView.Rows.Add(
                    purchase.Id,
                    purchase.InvoiceNumber,
                    purchase.Supplier.Name,
                    PersianDateConverter.ConvertToPersianDate(purchase.PurchaseDate),
                    purchase.TotalAmount.ToString("N0"),
                    purchase.DiscountAmount.ToString("N0"),
                    purchase.TaxAmount.ToString("N0"),
                    purchase.FinalAmount.ToString("N0"),
                    purchase.Items.Count
                );
            }
        }

        private void Filter_Changed(object? sender, EventArgs e)
        {
            LoadPurchases();
        }

        private void BtnNewPurchase_Click(object? sender, EventArgs e)
        {
            var addPurchaseForm = new AddEditPurchaseForm();
            if (addPurchaseForm.ShowDialog() == DialogResult.OK)
            {
                LoadPurchases();
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadPurchases();
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (_dataGridView!.SelectedRows.Count > 0)
            {
                var purchaseId = Convert.ToInt32(_dataGridView.SelectedRows[0].Cells["Id"].Value);
                var purchase = _purchaseRepository.GetById(purchaseId);
                if (purchase != null)
                {
                    PrintPurchaseInvoice(purchase);
                }
            }
            else
            {
                MessageBox.Show("لطفاً یک خرید را انتخاب کنید.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var purchaseId = Convert.ToInt32(_dataGridView!.Rows[e.RowIndex].Cells["Id"].Value);
                var purchase = _purchaseRepository.GetById(purchaseId);
                if (purchase != null)
                {
                    var editForm = new AddEditPurchaseForm(purchase);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadPurchases();
                    }
                }
            }
        }

        private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var purchaseId = Convert.ToInt32(_dataGridView!.Rows[e.RowIndex].Cells["Id"].Value);

                if (e.ColumnIndex == _dataGridView.Columns["Edit"].Index)
                {
                    var purchase = _purchaseRepository.GetById(purchaseId);
                    if (purchase != null)
                    {
                        var editForm = new AddEditPurchaseForm(purchase);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadPurchases();
                        }
                    }
                }
                else if (e.ColumnIndex == _dataGridView.Columns["Delete"].Index)
                {
                    if (MessageBox.Show("آیا از حذف این خرید اطمینان دارید؟", "تایید حذف", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            if (_purchaseRepository.Delete(purchaseId))
                            {
                                MessageBox.Show("خرید با موفقیت حذف شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadPurchases();
                            }
                            else
                            {
                                MessageBox.Show("خطا در حذف خرید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطا در حذف خرید: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (e.ColumnIndex == _dataGridView.Columns["Print"].Index)
                {
                    var purchase = _purchaseRepository.GetById(purchaseId);
                    if (purchase != null)
                    {
                        PrintPurchaseInvoice(purchase);
                    }
                }
            }
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null)
            {
                if (e.ColumnIndex == _dataGridView!.Columns["TotalAmount"].Index ||
                    e.ColumnIndex == _dataGridView.Columns["DiscountAmount"].Index ||
                    e.ColumnIndex == _dataGridView.Columns["TaxAmount"].Index ||
                    e.ColumnIndex == _dataGridView.Columns["FinalAmount"].Index)
                {
                    if (decimal.TryParse(e.Value.ToString(), out decimal amount))
                    {
                        e.Value = amount.ToString("N0") + " تومان";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        private void PrintPurchaseInvoice(Purchase purchase)
        {
            try
            {
                var printForm = new PrintPurchaseInvoiceForm(purchase);
                printForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 