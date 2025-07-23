using System;
using System.Linq;
using AccountingApp.Models;

namespace AccountingApp.Utilities
{
    public static class InvoiceValidationHelper
    {
        public static (bool isValid, string errorMessage) ValidateSaleInvoice(Sale sale)
        {
            try
            {
                // بررسی وجود آیتم‌ها
                if (sale.Items == null || sale.Items.Count == 0)
                {
                    return (false, "فاکتور باید حداقل یک آیتم داشته باشد.");
                }

                // محاسبه مبالغ
                var totalAmount = sale.Items.Sum(item => item.TotalPrice);
                var itemsDiscount = sale.Items.Sum(item => item.DiscountAmount);
                var invoiceDiscount = sale.DiscountAmount;
                var totalDiscount = itemsDiscount + invoiceDiscount;
                var finalAmount = totalAmount - totalDiscount + sale.TaxAmount;

                // بررسی مبالغ منفی
                if (totalAmount < 0)
                {
                    return (false, "مبلغ کل نمی‌تواند منفی باشد.");
                }

                if (itemsDiscount < 0)
                {
                    return (false, "تخفیف آیتم‌ها نمی‌تواند منفی باشد.");
                }

                if (invoiceDiscount < 0)
                {
                    return (false, "تخفیف فاکتور نمی‌تواند منفی باشد.");
                }

                if (sale.TaxAmount < 0)
                {
                    return (false, "مبلغ مالیات نمی‌تواند منفی باشد.");
                }

                // بررسی تخفیف بیش از حد
                if (totalDiscount > totalAmount)
                {
                    return (false, "مجموع تخفیف‌ها نمی‌تواند از مبلغ کل بیشتر باشد.");
                }

                // بررسی مبلغ نهایی
                if (finalAmount < 0)
                {
                    return (false, "مبلغ نهایی نمی‌تواند منفی باشد.");
                }

                // بررسی آیتم‌های منفی
                foreach (var item in sale.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        return (false, $"تعداد آیتم '{item.Product?.Name}' باید بزرگتر از صفر باشد.");
                    }

                    if (item.UnitPrice < 0)
                    {
                        return (false, $"قیمت واحد آیتم '{item.Product?.Name}' نمی‌تواند منفی باشد.");
                    }

                    if (item.DiscountAmount < 0)
                    {
                        return (false, $"تخفیف آیتم '{item.Product?.Name}' نمی‌تواند منفی باشد.");
                    }

                    var itemTotal = item.Quantity * item.UnitPrice;
                    if (item.DiscountAmount > itemTotal)
                    {
                        return (false, $"تخفیف آیتم '{item.Product?.Name}' نمی‌تواند از قیمت کل آن بیشتر باشد.");
                    }
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, $"خطا در اعتبارسنجی فاکتور: {ex.Message}");
            }
        }

        public static (bool isValid, string errorMessage) ValidatePurchaseInvoice(Purchase purchase)
        {
            try
            {
                // بررسی وجود آیتم‌ها
                if (purchase.Items == null || purchase.Items.Count == 0)
                {
                    return (false, "فاکتور باید حداقل یک آیتم داشته باشد.");
                }

                // محاسبه مبالغ
                var totalAmount = purchase.Items.Sum(item => item.TotalPrice);
                var itemsDiscount = purchase.Items.Sum(item => item.DiscountAmount);
                var invoiceDiscount = purchase.DiscountAmount;
                var totalDiscount = itemsDiscount + invoiceDiscount;
                var finalAmount = totalAmount - totalDiscount + purchase.TaxAmount;

                // بررسی مبالغ منفی
                if (totalAmount < 0)
                {
                    return (false, "مبلغ کل نمی‌تواند منفی باشد.");
                }

                if (itemsDiscount < 0)
                {
                    return (false, "تخفیف آیتم‌ها نمی‌تواند منفی باشد.");
                }

                if (invoiceDiscount < 0)
                {
                    return (false, "تخفیف فاکتور نمی‌تواند منفی باشد.");
                }

                if (purchase.TaxAmount < 0)
                {
                    return (false, "مبلغ مالیات نمی‌تواند منفی باشد.");
                }

                // بررسی تخفیف بیش از حد
                if (totalDiscount > totalAmount)
                {
                    return (false, "مجموع تخفیف‌ها نمی‌تواند از مبلغ کل بیشتر باشد.");
                }

                // بررسی مبلغ نهایی
                if (finalAmount < 0)
                {
                    return (false, "مبلغ نهایی نمی‌تواند منفی باشد.");
                }

                // بررسی آیتم‌های منفی
                foreach (var item in purchase.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        return (false, $"تعداد آیتم '{item.Product?.Name}' باید بزرگتر از صفر باشد.");
                    }

                    if (item.UnitPrice < 0)
                    {
                        return (false, $"قیمت واحد آیتم '{item.Product?.Name}' نمی‌تواند منفی باشد.");
                    }

                    if (item.DiscountAmount < 0)
                    {
                        return (false, $"تخفیف آیتم '{item.Product?.Name}' نمی‌تواند منفی باشد.");
                    }

                    var itemTotal = item.Quantity * item.UnitPrice;
                    if (item.DiscountAmount > itemTotal)
                    {
                        return (false, $"تخفیف آیتم '{item.Product?.Name}' نمی‌تواند از قیمت کل آن بیشتر باشد.");
                    }
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, $"خطا در اعتبارسنجی فاکتور: {ex.Message}");
            }
        }

        public static void RecalculateSaleAmounts(Sale sale)
        {
            try
            {
                if (sale.Items != null && sale.Items.Count > 0)
                {
                    sale.TotalAmount = sale.Items.Sum(item => item.TotalPrice);
                    var itemsDiscount = sale.Items.Sum(item => item.DiscountAmount);
                    var totalDiscount = itemsDiscount + sale.DiscountAmount;
                    sale.FinalAmount = sale.TotalAmount - totalDiscount + sale.TaxAmount;

                    // اطمینان از عدم منفی بودن مبلغ نهایی
                    if (sale.FinalAmount < 0)
                    {
                        sale.FinalAmount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در محاسبه مجدد مبالغ: {ex.Message}", ex);
            }
        }

        public static void RecalculatePurchaseAmounts(Purchase purchase)
        {
            try
            {
                if (purchase.Items != null && purchase.Items.Count > 0)
                {
                    purchase.TotalAmount = purchase.Items.Sum(item => item.TotalPrice);
                    var itemsDiscount = purchase.Items.Sum(item => item.DiscountAmount);
                    var totalDiscount = itemsDiscount + purchase.DiscountAmount;
                    purchase.FinalAmount = purchase.TotalAmount - totalDiscount + purchase.TaxAmount;

                    // اطمینان از عدم منفی بودن مبلغ نهایی
                    if (purchase.FinalAmount < 0)
                    {
                        purchase.FinalAmount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در محاسبه مجدد مبالغ: {ex.Message}", ex);
            }
        }

        public static string GetInvoiceSummary(Sale sale)
        {
            try
            {
                var totalAmount = sale.Items?.Sum(item => item.TotalPrice) ?? 0;
                var itemsDiscount = sale.Items?.Sum(item => item.DiscountAmount) ?? 0;
                var totalDiscount = itemsDiscount + sale.DiscountAmount;
                var finalAmount = totalAmount - totalDiscount + sale.TaxAmount;

                return $"جمع کل: {totalAmount:N0} تومان\n" +
                       $"تخفیف کل: {totalDiscount:N0} تومان\n" +
                       $"مالیات: {sale.TaxAmount:N0} تومان\n" +
                       $"مبلغ نهایی: {finalAmount:N0} تومان";
            }
            catch (Exception ex)
            {
                return $"خطا در محاسبه خلاصه: {ex.Message}";
            }
        }
    }
} 