using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AccountingApp.Models;

namespace AccountingApp.Utilities
{
    public static class SafeInvoicePrinter
    {
        public static void PrintSaleInvoice(Sale sale)
        {
            try
            {
                // اعتبارسنجی فاکتور قبل از پرینت
                var (isValid, errorMessage) = InvoiceValidationHelper.ValidateSaleInvoice(sale);
                if (!isValid)
                {
                    MessageBox.Show($"خطا در اعتبارسنجی فاکتور: {errorMessage}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // محاسبه مجدد مبالغ برای اطمینان
                InvoiceValidationHelper.RecalculateSaleAmounts(sale);

                // نمایش خلاصه فاکتور
                var summary = InvoiceValidationHelper.GetInvoiceSummary(sale);
                var result = MessageBox.Show(
                    $"خلاصه فاکتور:\n\n{summary}\n\nآیا می‌خواهید این فاکتور را پرینت کنید؟",
                    "تایید پرینت",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // استفاده از روش thread-safe برای پرینت
                    ActiveXThreadingFix.RunInStaThread(() =>
                    {
                        try
                        {
                            // تولید PDF موقت
                            var tempPdfPath = Path.Combine(Path.GetTempPath(), $"invoice_{sale.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                            
                            // تولید PDF با QuestPDF
                            GenerateInvoicePdf(sale, tempPdfPath);
                            
                            // باز کردن PDF با روش ایمن
                            ThreadSafePdfOpener.OpenPdfSafely(tempPdfPath);
                            
                            MessageBox.Show("فاکتور با موفقیت تولید و باز شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطا در تولید فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void PrintPurchaseInvoice(Purchase purchase)
        {
            try
            {
                // اعتبارسنجی فاکتور قبل از پرینت
                var (isValid, errorMessage) = InvoiceValidationHelper.ValidatePurchaseInvoice(purchase);
                if (!isValid)
                {
                    MessageBox.Show($"خطا در اعتبارسنجی فاکتور: {errorMessage}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // محاسبه مجدد مبالغ برای اطمینان
                InvoiceValidationHelper.RecalculatePurchaseAmounts(purchase);

                // نمایش خلاصه فاکتور
                var summary = GetPurchaseInvoiceSummary(purchase);
                var result = MessageBox.Show(
                    $"خلاصه فاکتور:\n\n{summary}\n\nآیا می‌خواهید این فاکتور را پرینت کنید؟",
                    "تایید پرینت",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // استفاده از روش thread-safe برای پرینت
                    ActiveXThreadingFix.RunInStaThread(() =>
                    {
                        try
                        {
                            // تولید PDF موقت
                            var tempPdfPath = Path.Combine(Path.GetTempPath(), $"purchase_{purchase.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                            
                            // تولید PDF با QuestPDF
                            GeneratePurchaseInvoicePdf(purchase, tempPdfPath);
                            
                            // باز کردن PDF با روش ایمن
                            ThreadSafePdfOpener.OpenPdfSafely(tempPdfPath);
                            
                            MessageBox.Show("فاکتور با موفقیت تولید و باز شد.", "پیام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطا در تولید فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در پرینت فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void GenerateInvoicePdf(Sale sale, string filePath)
        {
            try
            {
                // استفاده از QuestPDF برای تولید PDF
                using var document = new QuestPDF.Fluent.Document();
                document.GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در تولید PDF: {ex.Message}", ex);
            }
        }

        private static void GeneratePurchaseInvoicePdf(Purchase purchase, string filePath)
        {
            try
            {
                // استفاده از QuestPDF برای تولید PDF
                using var document = new QuestPDF.Fluent.Document();
                document.GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در تولید PDF: {ex.Message}", ex);
            }
        }

        private static string GetPurchaseInvoiceSummary(Purchase purchase)
        {
            try
            {
                var totalAmount = purchase.Items?.Sum(item => item.TotalPrice) ?? 0;
                var itemsDiscount = purchase.Items?.Sum(item => item.DiscountAmount) ?? 0;
                var totalDiscount = itemsDiscount + purchase.DiscountAmount;
                var finalAmount = totalAmount - totalDiscount + purchase.TaxAmount;

                return $"جمع کل: {totalAmount:N0} تومان\n" +
                       $"تخفیف کل: {totalDiscount:N0} تومان\n" +
                       $"مالیات: {purchase.TaxAmount:N0} تومان\n" +
                       $"مبلغ نهایی: {finalAmount:N0} تومان";
            }
            catch (Exception ex)
            {
                return $"خطا در محاسبه خلاصه: {ex.Message}";
            }
        }

        public static async Task PrintInvoiceAsync(Sale sale)
        {
            await Task.Run(() =>
            {
                try
                {
                    PrintSaleInvoice(sale);
                }
                catch (Exception ex)
                {
                    Application.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"خطا در پرینت فاکتور: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        public static void ShowInvoicePreview(Sale sale)
        {
            try
            {
                // اعتبارسنجی فاکتور
                var (isValid, errorMessage) = InvoiceValidationHelper.ValidateSaleInvoice(sale);
                if (!isValid)
                {
                    MessageBox.Show($"خطا در اعتبارسنجی فاکتور: {errorMessage}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // محاسبه مجدد مبالغ
                InvoiceValidationHelper.RecalculateSaleAmounts(sale);

                // نمایش خلاصه
                var summary = InvoiceValidationHelper.GetInvoiceSummary(sale);
                MessageBox.Show(
                    $"پیش‌نمایش فاکتور:\n\n{summary}",
                    "پیش‌نمایش فاکتور",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در نمایش پیش‌نمایش: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 