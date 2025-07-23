# حل مشکل ActiveX Threading در پرینت فاکتور

## مشکل
خطای `'8856f961-340a-11d0-a96b-00c04fd705a2' cannot be instantiated because the current thread is not in a single-threaded apartment` زمانی رخ می‌دهد که سعی می‌کنید فایل PDF را باز کنید یا عملیات پرینت انجام دهید.

## علت مشکل
1. **ActiveX Controls**: برخی از کنترل‌های ActiveX نیاز به Single-Threaded Apartment (STA) دارند
2. **COM Threading**: عملیات COM در thread های مختلف ممکن است باعث تداخل شود
3. **Process.Start**: استفاده از `Process.Start` با `UseShellExecute = true` می‌تواند ActiveX components را فعال کند

## راه‌حل‌های پیاده‌سازی شده

### 1. Thread-Safe PDF Opener
- کلاس `ThreadSafePdfOpener` برای باز کردن ایمن فایل‌های PDF
- چندین روش جایگزین برای باز کردن PDF
- مدیریت خطاهای مختلف

### 2. ActiveX Threading Fix
- کلاس `ActiveXThreadingFix` برای مدیریت COM threading
- تنظیم STA (Single-Threaded Apartment) برای thread های جدید
- مدیریت صحیح COM initialization و cleanup

### 3. بهبود Program.cs
- تنظیم COM در ابتدای برنامه
- پاکسازی COM در پایان برنامه
- مدیریت خطاهای COM

## ویژگی‌های جدید

### ThreadSafePdfOpener
```csharp
// باز کردن ایمن PDF
ThreadSafePdfOpener.OpenPdfSafely(pdfPath);

// باز کردن با برنامه پیش‌فرض
ThreadSafePdfOpener.OpenPdfWithDefaultApp(pdfPath);

// باز کردن async
await ThreadSafePdfOpener.OpenPdfAsync(pdfPath);
```

### ActiveXThreadingFix
```csharp
// اجرای عملیات در STA thread
ActiveXThreadingFix.RunInStaThread(() => {
    // عملیات پرینت یا باز کردن فایل
});

// اجرای عملیات با return value
var result = ActiveXThreadingFix.RunInStaThread(() => {
    return someOperation();
});
```

## تغییرات اعمال شده

### فایل‌های جدید:
1. `ThreadSafePdfOpener.cs` - ابزار باز کردن ایمن PDF
2. `ActiveXThreadingFix.cs` - ابزار مدیریت ActiveX threading

### فایل‌های تغییر یافته:
1. `WebBrowserInvoiceForm.cs` - استفاده از ThreadSafePdfOpener
2. `InvoiceViewerForm.cs` - استفاده از ThreadSafePdfOpener
3. `Program.cs` - تنظیم COM در ابتدا و پایان برنامه

## نحوه استفاده

### برای حل مشکل فعلی:
1. برنامه را مجدداً اجرا کنید
2. از دکمه‌های پرینت یا باز کردن PDF استفاده کنید
3. در صورت بروز خطا، سیستم به طور خودکار روش‌های جایگزین را امتحان می‌کند

### برای توسعه‌دهندگان:
```csharp
// برای باز کردن هر فایلی به صورت ایمن
ThreadSafePdfOpener.OpenPdfSafely(filePath);

// برای اجرای عملیات ActiveX در thread ایمن
ActiveXThreadingFix.RunInStaThread(() => {
    // کد عملیات
});
```

## روش‌های جایگزین

### روش 1: STA Thread
- اجرای عملیات در Single-Threaded Apartment
- مناسب برای ActiveX controls

### روش 2: Command Line
- استفاده از `cmd.exe` برای باز کردن فایل
- دور زدن مشکلات ActiveX

### روش 3: Default Application
- یافتن و استفاده از برنامه پیش‌فرض PDF
- پشتیبانی از Adobe Reader، SumatraPDF و غیره

## نکات مهم
- همیشه از `ThreadSafePdfOpener` برای باز کردن فایل‌ها استفاده کنید
- در صورت بروز خطا، سیستم به طور خودکار روش‌های جایگزین را امتحان می‌کند
- تنظیمات COM در ابتدای برنامه انجام می‌شود
- پاکسازی COM در پایان برنامه انجام می‌شود

## عیب‌یابی
اگر هنوز مشکل دارید:
1. بررسی کنید که برنامه PDF viewer نصب باشد
2. بررسی کنید که فایل PDF قابل دسترسی باشد
3. بررسی کنید که antivirus فایل را مسدود نکرده باشد
4. از روش "پرینت مستقیم" استفاده کنید 