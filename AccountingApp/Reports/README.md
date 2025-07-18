# راهنمای استفاده از Stimulsoft Reports

## مقدمه
این پوشه شامل فایل‌های گزارش‌سازی با استفاده از Stimulsoft Reports است.

## فایل‌های موجود

### SaleInvoice.mrt
فایل گزارش فاکتور فروش که شامل:
- هدر شرکت
- اطلاعات فاکتور (شماره و تاریخ)
- اطلاعات مشتری
- جدول آیتم‌های فاکتور
- جمع کل و مبلغ نهایی
- فوتر

## نحوه استفاده

### 1. طراحی گزارش
برای طراحی یا ویرایش گزارش‌ها:
1. دکمه "طراحی گزارش" را در فرم فاکتور کلیک کنید
2. Stimulsoft Report Designer باز می‌شود
3. تغییرات مورد نظر را اعمال کنید
4. فایل را ذخیره کنید

### 2. متغیرهای گزارش
گزارش از متغیرهای زیر استفاده می‌کند:
- `InvoiceNumber`: شماره فاکتور
- `SaleDate`: تاریخ فروش
- `CustomerName`: نام مشتری
- `CustomerPhone`: تلفن مشتری
- `TotalAmount`: جمع کل
- `FinalAmount`: مبلغ نهایی

### 3. داده‌های آیتم‌ها
آیتم‌های فاکتور از طریق DataSource با نام "Items" ارسال می‌شوند که شامل:
- `ProductName`: نام محصول
- `Quantity`: تعداد
- `UnitPrice`: قیمت واحد
- `TotalPrice`: قیمت کل

## نکات مهم

1. **فونت فارسی**: از فونت Vazir برای نمایش متن فارسی استفاده شده است
2. **جهت متن**: تمام متن‌ها با گزینه RightToLeft تنظیم شده‌اند
3. **اندازه صفحه**: A4 در حالت Portrait
4. **واحد اندازه‌گیری**: سانتی‌متر

## عیب‌یابی

### مشکل بارگذاری فایل MRT
اگر فایل MRT بارگذاری نشد، سیستم به صورت خودکار گزارش را از طریق کد ایجاد می‌کند.

### مشکل فونت
اگر فونت Vazir نصب نباشد، از فونت پیش‌فرض سیستم استفاده می‌شود.

## ایجاد گزارش جدید

برای ایجاد گزارش جدید:
1. فایل MRT جدید در پوشه Reports ایجاد کنید
2. کد مربوط به بارگذاری آن را در `PrintSaleInvoiceForm.cs` اضافه کنید
3. متغیرهای مورد نیاز را تنظیم کنید

## منابع مفید
- [مستندات Stimulsoft Reports](https://www.stimulsoft.com/en/documentation)
- [نمونه‌های گزارش](https://www.stimulsoft.com/en/samples) 