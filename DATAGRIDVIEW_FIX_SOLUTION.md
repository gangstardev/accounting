# حل مشکل DataGridView در ویرایش فاکتور

## مشکل
خطای `"No row can be added to a DataGridView control that does not have columns"` زمانی رخ می‌دهد که سعی می‌کنید فاکتور را ویرایش کنید و تخفیف به اندازه قیمت فاکتور اعمال می‌شود.

## علت مشکل
1. **ترتیب نامناسب initialization**: ستون‌های DataGridView بعد از بارگذاری داده‌ها تنظیم می‌شد
2. **عدم مدیریت خطا**: خطاهای احتمالی در بارگذاری آیتم‌ها مدیریت نمی‌شد
3. **مشکلات null reference**: عدم بررسی null بودن اشیاء

## راه‌حل‌های پیاده‌سازی شده

### 1. تغییر ترتیب initialization
- تنظیم ستون‌های DataGridView قبل از بارگذاری داده‌ها
- اطمینان از وجود ستون‌ها قبل از اضافه کردن ردیف‌ها

### 2. بهبود مدیریت خطا
- اضافه کردن try-catch blocks در تمام متدهای مربوطه
- نمایش پیام‌های خطای مناسب
- بررسی null بودن اشیاء

### 3. بهبود محاسبات تخفیف
- اطمینان از عدم منفی بودن مبلغ نهایی
- تغییر رنگ مبلغ نهایی در صورت صفر بودن
- استفاده از InvoiceValidationHelper

## تغییرات اعمال شده

### AddEditSaleForm.cs
```csharp
// تغییر ترتیب initialization
public AddEditSaleForm(Sale? sale = null)
{
    InitializeComponent();
    // ...
    
    // ابتدا ستون‌های DataGridView را تنظیم کنیم
    SetupDataGridView();
    
    LoadCustomers();
    LoadProducts();

    if (_isEdit)
    {
        LoadSaleData();
    }
    // ...
}

// بهبود RefreshItemsGrid
private void RefreshItemsGrid()
{
    try
    {
        // اطمینان از وجود ستون‌ها
        if (_dgvItems.Columns.Count == 0)
        {
            SetupDataGridView();
        }
        // ...
    }
    catch (Exception ex)
    {
        MessageBox.Show($"خطا در بارگذاری آیتم‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### AddEditPurchaseForm.cs
- همان تغییرات برای فرم خرید
- بهبود مدیریت خطا و null checking
- استفاده از InvoiceValidationHelper

## ویژگی‌های جدید

### مدیریت خطای بهتر:
- بررسی وجود ستون‌ها قبل از اضافه کردن ردیف‌ها
- نمایش پیام‌های خطای مناسب
- جلوگیری از crash برنامه

### محاسبات بهبود یافته:
- اطمینان از عدم منفی بودن مبلغ نهایی
- تغییر رنگ مبلغ نهایی در صورت صفر بودن
- اعتبارسنجی کامل تخفیف‌ها

### Null Safety:
- بررسی null بودن Product.Name
- بررسی null بودن Customer.Name
- بررسی null بودن Items collection

## نحوه استفاده

### برای کاربران:
1. ویرایش فاکتور بدون خطا
2. اعمال تخفیف کامل بدون مشکل
3. نمایش مناسب مبالغ صفر
4. پیام‌های خطای واضح

### برای توسعه‌دهندگان:
```csharp
// همیشه ابتدا ستون‌ها را تنظیم کنید
SetupDataGridView();

// سپس داده‌ها را بارگذاری کنید
LoadData();

// از try-catch استفاده کنید
try
{
    RefreshItemsGrid();
}
catch (Exception ex)
{
    // مدیریت خطا
}
```

## قوانین جدید

### ترتیب initialization:
1. InitializeComponent()
2. SetupDataGridView()
3. LoadData()

### Null checking:
- همیشه از null-conditional operator استفاده کنید
- بررسی null بودن قبل از دسترسی به properties

### Error handling:
- تمام عملیات DataGridView در try-catch قرار گیرند
- نمایش پیام‌های خطای مناسب

## نکات مهم
- همیشه ابتدا ستون‌های DataGridView را تنظیم کنید
- از null checking استفاده کنید
- خطاها را مدیریت کنید
- از InvoiceValidationHelper برای اعتبارسنجی استفاده کنید

## عیب‌یابی
اگر هنوز مشکل دارید:
1. بررسی کنید که SetupDataGridView قبل از LoadData فراخوانی شود
2. بررسی کنید که تمام null checking ها اعمال شده باشند
3. بررسی کنید که try-catch blocks درست پیاده‌سازی شده باشند
4. از InvoiceValidationHelper برای اعتبارسنجی استفاده کنید 