using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingApp.Models;
using System.Globalization;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using WinFormsFont = System.Drawing.Font;
using WinFormsRectangle = System.Drawing.Rectangle;
using WinFormsColor = System.Drawing.Color;
using System.IO;
using System.Collections.Generic;
using System.Data;
using Stimulsoft.Report.Viewer;

namespace AccountingApp.Forms
{
    public partial class PrintSaleInvoiceForm : Form
    {
        private readonly Sale _sale;
        private StiViewerControl viewerControl;

        public PrintSaleInvoiceForm(Sale sale)
        {
            _sale = sale;
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Text = "نمایش فاکتور فروش";
            this.Size = new System.Drawing.Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            viewerControl = new StiViewerControl
            {
                Dock = DockStyle.Fill,
                ShowToolbar = true
            };

            this.Controls.Add(viewerControl);
        }

        private void LoadReport()
        {
            try
            {
                var report = CreateReport();
                if (report != null)
                {
                    viewerControl.Report = report;
                    report.Render(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private StiReport CreateReport()
        {
            try
            {
                // بارگذاری فایل MRT اصلی
                var mrtPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "SaleInvoice.mrt");
                if (File.Exists(mrtPath))
                {
                    var report = new StiReport();
                    report.Load(mrtPath);

                    // تنظیم متغیرهای گزارش
                    report.RegBusinessObject("Sale", _sale);
                    report.RegData("Items", _sale.Items);
                    
                    // تنظیم متغیرهای جداگانه برای دسترسی آسان‌تر
                    report["InvoiceNumber"] = _sale.InvoiceNumber;
                    report["SaleDate"] = _sale.SaleDate.ToString("yyyy/MM/dd");
                    report["CustomerName"] = _sale.Customer?.Name ?? "";
                    report["CustomerPhone"] = _sale.Customer?.Phone ?? "";
                    report["TotalAmount"] = _sale.TotalAmount;
                    report["FinalAmount"] = _sale.FinalAmount;

                    return report;
                }
                else
                {
                    // اگر فایل MRT وجود نداشت، پیام خطا نمایش می‌دهیم
                    MessageBox.Show("فایل گزارش MRT یافت نشد. لطفاً فایل SaleInvoice.mrt را در پوشه Reports قرار دهید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری گزارش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}