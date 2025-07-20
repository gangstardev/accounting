using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;


namespace AccountingApp.Forms
{
    public partial class SalesForm : Form
    {
        private readonly SaleRepository _saleRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private DataGridView? _dataGridView;
        private PersianDateTimePicker? _dtpStartDate;
        private PersianDateTimePicker? _dtpEndDate;
        private ComboBox? _cmbCustomerFilter;
        private List<Sale>? _sales;

        public SalesForm()
        {
            InitializeComponent();
            _saleRepository = new SaleRepository();
            _customerRepository = new CustomerRepository();
            _productRepository = new ProductRepository();
            
            // تنظیم تاریخ‌ها بعد از مقداردهی کنترل‌ها
            if (_dtpStartDate != null)
                _dtpStartDate.Value = DateTime.Today.AddDays(-30);
            if (_dtpEndDate != null)
                _dtpEndDate.Value = DateTime.Today;
            
            LoadCustomerFilter();
            LoadSales();
        }

        private void InitializeComponent()
        {
            this.Text = "مدیریت فروش";
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
                Size = new System.Drawing.Size(130, 25)
            };
            _dtpStartDate.ValueChanged += Filter_Changed;

            var lblEndDate = new Label { Text = "تا تاریخ:", Location = new System.Drawing.Point(220, 25), AutoSize = true };
            _dtpEndDate = new PersianDateTimePicker 
            { 
                Location = new System.Drawing.Point(290, 22), 
                Size = new System.Drawing.Size(120, 25)
            };
            _dtpEndDate.ValueChanged += Filter_Changed;

            var lblCustomer = new Label { Text = "مشتری:", Location = new System.Drawing.Point(430, 25), AutoSize = true };
            _cmbCustomerFilter = new ComboBox 
            { 
                Location = new System.Drawing.Point(500, 22), 
                Size = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCustomerFilter.SelectedIndexChanged += Filter_Changed;

            var btnNewSale = new Button
            {
                Text = "فروش جدید",
                Location = new System.Drawing.Point(670, 20),
                Size = new System.Drawing.Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNewSale.Click += BtnNewSale_Click;

            var btnRefresh = new Button
            {
                Text = "تازه‌سازی",
                Location = new System.Drawing.Point(780, 20),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            var btnPrint = new Button
            {
                Text = "پرینت",
                Location = new System.Drawing.Point(870, 20),
                Size = new System.Drawing.Size(80, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrint.Click += BtnPrint_Click;

            filterPanel.Controls.AddRange(new Control[] 
            { 
                lblStartDate, _dtpStartDate, lblEndDate, _dtpEndDate, 
                lblCustomer, _cmbCustomerFilter, btnNewSale, btnRefresh, btnPrint 
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
                new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "نام مشتری", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "SaleDate", HeaderText = "تاریخ فروش", Width = 100 },
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

        private void LoadCustomerFilter()
        {
            try
            {
                if (_cmbCustomerFilter == null)
                    return;
                var customers = _customerRepository.GetAll() ?? new List<Customer>();
                _cmbCustomerFilter.Items.Clear();
                _cmbCustomerFilter.Items.Add("همه مشتریان");
                _cmbCustomerFilter.Items.AddRange(customers.Select(c => c.Name).ToArray());
                _cmbCustomerFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری فیلتر مشتریان: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSales()
        {
            try
            {
                if (_dtpStartDate == null || _dtpEndDate == null)
                    return;
                    
                var startDate = _dtpStartDate.Value.Date;
                var endDate = _dtpEndDate.Value.Date;
                
                _sales = _saleRepository.GetByDateRange(startDate, endDate);

                // فیلتر بر اساس مشتری
                if (_cmbCustomerFilter != null && _cmbCustomerFilter.SelectedIndex > 0)
                {
                    var selectedCustomer = _cmbCustomerFilter.Text;
                    _sales = _sales.Where(s => s.Customer.Name == selectedCustomer).ToList();
                }

                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری فروش‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            if (_dataGridView == null)
                return;
                
            _dataGridView.Rows.Clear();



            foreach (var sale in _sales ?? new List<Sale>())
            {
                var persianDate = PersianDateConverter.ConvertToPersianDate(sale.SaleDate);
                Console.WriteLine($"تاریخ فروش میلادی: {sale.SaleDate}");
                Console.WriteLine($"تاریخ فروش شمسی: {persianDate}");
                
                _dataGridView.Rows.Add(
                    sale.Id,
                    sale.InvoiceNumber,
                    sale.Customer.Name,
                    persianDate,
                    sale.TotalAmount.ToString("N0"),
                    sale.DiscountAmount.ToString("N0"),
                    sale.TaxAmount.ToString("N0"),
                    sale.FinalAmount.ToString("N0"),
                    sale.Items.Count
                );
            }
        }

        private void Filter_Changed(object? sender, EventArgs e)
        {
            LoadSales();
        }

        private void BtnNewSale_Click(object? sender, EventArgs e)
        {
            var addSaleForm = new AddEditSaleForm();
            if (addSaleForm.ShowDialog() == DialogResult.OK)
            {
                LoadSales();
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadSales();
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (_dataGridView.SelectedRows.Count > 0)
            {
                var saleId = Convert.ToInt32(_dataGridView.SelectedRows[0].Cells["Id"].Value);
                var sale = _saleRepository.GetById(saleId);
                if (sale != null)
                {
                    PrintSaleInvoice(sale);
                }
            }
            else
            {
                MessageBox.Show("لطفاً یک فروش را انتخاب کنید.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var saleId = Convert.ToInt32(_dataGridView.Rows[e.RowIndex].Cells["Id"].Value);
                var sale = _saleRepository.GetById(saleId);
                if (sale != null)
                {
                    var editForm = new AddEditSaleForm(sale);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadSales();
                    }
                }
            }
        }

        private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var saleId = Convert.ToInt32(_dataGridView.Rows[e.RowIndex].Cells["Id"].Value);

                if (e.ColumnIndex == _dataGridView.Columns["Edit"].Index)
                {
                    var sale = _saleRepository.GetById(saleId);
                    if (sale != null)
                    {
                        var editForm = new AddEditSaleForm(sale);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadSales();
                        }
                    }
                }
                else if (e.ColumnIndex == _dataGridView.Columns["Delete"].Index)
                {
                    if (MessageBox.Show("آیا از حذف این فروش اطمینان دارید؟", "تایید حذف", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            if (_saleRepository.Delete(saleId))
                            {
                                MessageBox.Show("فروش با موفقیت حذف شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadSales();
                            }
                            else
                            {
                                MessageBox.Show("خطا در حذف فروش.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطا در حذف فروش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (e.ColumnIndex == _dataGridView.Columns["Print"].Index)
                {
                    var sale = _saleRepository.GetById(saleId);
                    if (sale != null)
                    {
                        PrintSaleInvoice(sale);
                    }
                }
            }
        }

        private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null)
            {
                if (e.ColumnIndex == _dataGridView.Columns["TotalAmount"].Index ||
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

        private void PrintSaleInvoice(Sale sale)
        {
            try
            {
                // استفاده از فرم WebBrowser برای نمایش PDF
                var printForm = new WebBrowserInvoiceForm(sale);
                printForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 