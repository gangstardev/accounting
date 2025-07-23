# حل مشکل DataGridView در ویرایش فاکتور - نسخه بهبود یافته

## مشکل
خطای `"No row can be added to a DataGridView control that does not have columns"` زمانی رخ می‌دهد که سعی می‌کنید فاکتور را ویرایش کنید و تخفیف به اندازه قیمت فاکتور اعمال می‌شود.

## علت مشکل
1. **ترتیب نامناسب initialization**: ستون‌های DataGridView بعد از بارگذاری داده‌ها تنظیم می‌شد
2. **عدم مدیریت خطا**: خطاهای احتمالی در بارگذاری آیتم‌ها مدیریت نمی‌شد
3. **مشکلات null reference**: عدم بررسی null بودن اشیاء
4. **مشکلات threading**: عدم مدیریت صحیح thread-safe operations
5. **عدم آماده بودن کنترل**: دسترسی به DataGridView قبل از آماده شدن کامل

## راه‌حل‌های پیاده‌سازی شده

### 1. تغییر ترتیب initialization
- تنظیم ستون‌های DataGridView قبل از بارگذاری داده‌ها
- استفاده از Form.Load event برای اطمینان از آماده بودن فرم
- اطمینان از وجود ستون‌ها قبل از اضافه کردن ردیف‌ها

### 2. بهبود مدیریت خطا
- اضافه کردن try-catch blocks در تمام متدهای مربوطه
- نمایش پیام‌های خطای مناسب با جزئیات
- بررسی null بودن اشیاء
- بررسی آماده بودن کنترل‌ها

### 3. بهبود محاسبات تخفیف
- اطمینان از عدم منفی بودن مبلغ نهایی
- تغییر رنگ مبلغ نهایی در صورت صفر بودن
- استفاده از InvoiceValidationHelper

### 4. Thread-safe operations
- استفاده از InvokeRequired برای عملیات thread-safe
- مدیریت صحیح cross-thread operations
- اطمینان از آماده بودن handle کنترل‌ها

### 5. Robust initialization
- متد EnsureDataGridViewReady برای اطمینان از آماده بودن DataGridView
- بررسی وجود ستون‌ها قبل از هر عملیات
- ایجاد handle در صورت نیاز

## تغییرات اعمال شده

### AddEditSaleForm.cs
```csharp
// تغییر ترتیب initialization با Form.Load event
public AddEditSaleForm(Sale? sale = null)
{
    InitializeComponent();
    // ...
    
    // ابتدا ستون‌های DataGridView را تنظیم کنیم
    SetupDataGridView();
    
    LoadCustomers();
    LoadProducts();

    // اضافه کردن event handler برای Load event
    this.Load += AddEditSaleForm_Load;
}

private void AddEditSaleForm_Load(object? sender, EventArgs e)
{
    try
    {
        // اطمینان از آماده بودن فرم
        if (_isEdit)
        {
            LoadSaleData();
        }
        // ...
    }
    catch (Exception ex)
    {
        MessageBox.Show($"خطا در بارگذاری فرم: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

// متد جدید برای اطمینان از آماده بودن DataGridView
private void EnsureDataGridViewReady()
{
    try
    {
        // اطمینان از وجود DataGridView
        if (_dgvItems == null)
        {
            MessageBox.Show("خطا: DataGridView در دسترس نیست", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // اطمینان از وجود ستون‌ها
        if (_dgvItems.Columns.Count == 0)
        {
            SetupDataGridView();
            
            // بررسی مجدد
            if (_dgvItems.Columns.Count == 0)
            {
                MessageBox.Show("خطا: ستون‌های DataGridView تنظیم نشد", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        // اطمینان از آماده بودن کنترل
        if (!_dgvItems.IsHandleCreated)
        {
            _dgvItems.CreateHandle();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"خطا در آماده‌سازی DataGridView: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

// بهبود RefreshItemsGrid با thread-safe operations
private void RefreshItemsGrid()
{
    try
    {
        // اطمینان از آماده بودن DataGridView
        EnsureDataGridViewReady();

        // پاک کردن ردیف‌ها به صورت ایمن
        if (_dgvItems.InvokeRequired)
        {
            _dgvItems.Invoke(new Action(() => _dgvItems.Rows.Clear()));
        }
        else
        {
            _dgvItems.Rows.Clear();
        }

        // اضافه کردن ردیف‌ها به صورت ایمن
        foreach (var item in _saleItems)
        {
            if (_dgvItems.InvokeRequired)
            {
                _dgvItems.Invoke(new Action(() => _dgvItems.Rows.Add(...)));
            }
            else
            {
                _dgvItems.Rows.Add(...);
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"خطا در بارگذاری آیتم‌ها: {ex.Message}\n\nجزئیات: {ex.StackTrace}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### AddEditPurchaseForm.cs
- همان تغییرات برای فرم خرید
- بهبود مدیریت خطا و null checking
- استفاده از InvoiceValidationHelper
- Thread-safe operations
- EnsureDataGridViewReady method
- Form.Load event handling

## ویژگی‌های جدید

### مدیریت خطای بهتر:
- بررسی وجود ستون‌ها قبل از اضافه کردن ردیف‌ها
- نمایش پیام‌های خطای مناسب با جزئیات
- جلوگیری از crash برنامه
- بررسی آماده بودن کنترل‌ها

### محاسبات بهبود یافته:
- اطمینان از عدم منفی بودن مبلغ نهایی
- تغییر رنگ مبلغ نهایی در صورت صفر بودن
- اعتبارسنجی کامل تخفیف‌ها

### Null Safety:
- بررسی null بودن Product.Name
- بررسی null بودن Customer.Name
- بررسی null بودن Items collection

### Thread Safety:
- استفاده از InvokeRequired برای عملیات thread-safe
- مدیریت صحیح cross-thread operations
- اطمینان از آماده بودن handle کنترل‌ها

### Robust Initialization:
- متد EnsureDataGridViewReady برای اطمینان از آماده بودن DataGridView
- استفاده از Form.Load event برای اطمینان از آماده بودن فرم
- بررسی وجود ستون‌ها قبل از هر عملیات

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

// از Form.Load event استفاده کنید
this.Load += Form_Load;

// از EnsureDataGridViewReady استفاده کنید
EnsureDataGridViewReady();

// از thread-safe operations استفاده کنید
if (_dgvItems.InvokeRequired)
{
    _dgvItems.Invoke(new Action(() => { /* عملیات */ }));
}
else
{
    /* عملیات */
}

// از try-catch با جزئیات استفاده کنید
try
{
    RefreshItemsGrid();
}
catch (Exception ex)
{
    MessageBox.Show($"خطا: {ex.Message}\n\nجزئیات: {ex.StackTrace}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

## قوانین جدید

### ترتیب initialization:
1. InitializeComponent()
2. SetupDataGridView()
3. LoadCustomers/LoadProducts
4. Form.Load event handler
5. LoadData() (در Form.Load event)

### Null checking:
- همیشه از null-conditional operator استفاده کنید
- بررسی null بودن قبل از دسترسی به properties
- بررسی آماده بودن کنترل‌ها

### Error handling:
- تمام عملیات DataGridView در try-catch قرار گیرند
- نمایش پیام‌های خطای مناسب با جزئیات
- بررسی آماده بودن کنترل‌ها قبل از عملیات

### Thread safety:
- استفاده از InvokeRequired برای عملیات thread-safe
- مدیریت صحیح cross-thread operations
- اطمینان از آماده بودن handle کنترل‌ها

### Robust initialization:
- استفاده از EnsureDataGridViewReady قبل از هر عملیات
- بررسی وجود ستون‌ها قبل از اضافه کردن ردیف‌ها
- ایجاد handle در صورت نیاز

## نکات مهم
- همیشه ابتدا ستون‌های DataGridView را تنظیم کنید
- از null checking استفاده کنید
- خطاها را مدیریت کنید
- از InvoiceValidationHelper برای اعتبارسنجی استفاده کنید
- از Form.Load event برای اطمینان از آماده بودن فرم استفاده کنید
- از thread-safe operations استفاده کنید
- از EnsureDataGridViewReady قبل از هر عملیات استفاده کنید

## عیب‌یابی
اگر هنوز مشکل دارید:
1. بررسی کنید که SetupDataGridView قبل از LoadData فراخوانی شود
2. بررسی کنید که Form.Load event handler تنظیم شده باشد
3. بررسی کنید که EnsureDataGridViewReady فراخوانی شود
4. بررسی کنید که تمام null checking ها اعمال شده باشند
5. بررسی کنید که try-catch blocks درست پیاده‌سازی شده باشند
6. بررسی کنید که thread-safe operations استفاده شده باشند
7. از InvoiceValidationHelper برای اعتبارسنجی استفاده کنید
8. بررسی کنید که handle کنترل‌ها ایجاد شده باشد

## تست
برای تست راه‌حل:
1. فاکتور با تخفیف کامل ایجاد کنید
2. فاکتور را ویرایش کنید
3. تخفیف را تغییر دهید
4. مطمئن شوید که خطا رخ نمی‌دهد 