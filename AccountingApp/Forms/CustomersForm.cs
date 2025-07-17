using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AccountingApp.Models;
using AccountingApp.Repositories;

namespace AccountingApp.Forms
{
    public partial class CustomersForm : Form
    {
        private readonly CustomerRepository _customerRepository;
        private DataGridView? _dataGridView;
        private TextBox? _searchTextBox;
        private List<Customer>? _customers;

        public CustomersForm()
        {
            InitializeComponent();
            _customerRepository = new CustomerRepository();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "مدیریت مشتریان";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(5) };

            var searchLabel = new Label { Text = "جستجو:", Location = new System.Drawing.Point(10, 20), AutoSize = true };
            _searchTextBox = new TextBox { Location = new System.Drawing.Point(80, 17), Size = new System.Drawing.Size(200, 25) };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;

            var btnAdd = new Button { Text = "افزودن مشتری جدید", Location = new System.Drawing.Point(300, 15), Size = new System.Drawing.Size(120, 30), BackColor = Color.Green, ForeColor = Color.White };
            btnAdd.Click += BtnAdd_Click;

            var btnRefresh = new Button { Text = "تازه‌سازی", Location = new System.Drawing.Point(430, 15), Size = new System.Drawing.Size(80, 30), BackColor = Color.Blue, ForeColor = Color.White };
            btnRefresh.Click += BtnRefresh_Click;

            searchPanel.Controls.AddRange(new Control[] { searchLabel, _searchTextBox, btnAdd, btnRefresh });

            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "شناسه", Visible = false },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "نام", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Phone", HeaderText = "تلفن", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Address", HeaderText = "آدرس", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "NationalCode", HeaderText = "کد ملی", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "ایمیل", Width = 150 },
                new DataGridViewButtonColumn { Name = "Edit", HeaderText = "ویرایش", Text = "ویرایش", UseColumnTextForButtonValue = true, Width = 80 },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "حذف", Text = "حذف", UseColumnTextForButtonValue = true, Width = 80 }
            });

            _dataGridView.CellClick += DataGridView_CellClick;

            mainPanel.Controls.Add(_dataGridView);
            mainPanel.Controls.Add(searchPanel);
            this.Controls.Add(mainPanel);
        }

        private void LoadCustomers()
        {
            try
            {
                _customers = _customerRepository.GetAll();
                RefreshDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری مشتریان: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            _dataGridView.Rows.Clear();
            foreach (var customer in _customers)
            {
                _dataGridView.Rows.Add(customer.Id, customer.Name, customer.Phone, customer.Address, customer.NationalCode, customer.Email);
            }
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                var searchTerm = _searchTextBox.Text.Trim();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _customers = _customerRepository.GetAll();
                }
                else
                {
                    _customers = _customerRepository.Search(searchTerm);
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
            var addForm = new AddEditCustomerForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadCustomers();
        }

        private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var customerId = Convert.ToInt32(_dataGridView.Rows[e.RowIndex].Cells["Id"].Value);

                if (e.ColumnIndex == _dataGridView.Columns["Edit"].Index)
                {
                    var customer = _customerRepository.GetById(customerId);
                    if (customer != null)
                    {
                        var editForm = new AddEditCustomerForm(customer);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadCustomers();
                        }
                    }
                }
                else if (e.ColumnIndex == _dataGridView.Columns["Delete"].Index)
                {
                    if (MessageBox.Show("آیا از حذف این مشتری اطمینان دارید؟", "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            if (_customerRepository.Delete(customerId))
                            {
                                MessageBox.Show("مشتری با موفقیت حذف شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadCustomers();
                            }
                            else
                            {
                                MessageBox.Show("خطا در حذف مشتری.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطا در حذف مشتری: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
} 