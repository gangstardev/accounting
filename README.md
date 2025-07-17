# سیستم حسابداری

یک برنامه حسابداری کامل با استفاده از C# + WinForms، SQLite و RDLC Reports

## ویژگی‌ها

### مدیریت حساب‌ها
- افزودن، ویرایش و حذف حساب‌های مالی
- دسته‌بندی حساب‌ها (دارایی، بدهی، سرمایه، درآمد، هزینه)
- نمایش موجودی حساب‌ها

### مدیریت تراکنش‌ها
- ثبت تراکنش‌های مالی (بدهکار/بستانکار)
- فیلتر تراکنش‌ها بر اساس تاریخ و حساب
- نمایش جزئیات تراکنش‌ها

### گزارشات
- گزارش ترازنامه
- گزارش سود و زیان
- گزارش دفتر کل
- نمایش گزارشات در DataGridView

### ویژگی‌های فنی
- رابط کاربری فارسی و راست‌چین
- پایگاه داده SQLite
- استفاده از Dapper برای ORM
- معماری Repository Pattern

## پیش‌نیازها

- .NET 6.0 یا بالاتر
- Visual Studio 2022 یا Visual Studio Code
- SQLite

## نصب و راه‌اندازی

1. کلون کردن پروژه:
```bash
git clone [repository-url]
cd accounting
```

2. باز کردن فایل `AccountingApp.sln` در Visual Studio

3. بازگردانی پکیج‌های NuGet:
```bash
dotnet restore
```

4. اجرای برنامه:
```bash
dotnet run
```

## ساختار پروژه

```
AccountingApp/
├── Models/                 # مدل‌های داده
│   ├── Account.cs
│   └── Transaction.cs
├── Database/              # مدیریت پایگاه داده
│   └── DatabaseManager.cs
├── Repositories/          # لایه دسترسی به داده
│   ├── AccountRepository.cs
│   └── TransactionRepository.cs
├── Forms/                 # فرم‌های برنامه
│   ├── MainForm.cs
│   ├── AccountsForm.cs
│   ├── AddAccountForm.cs
│   ├── EditAccountForm.cs
│   ├── AddTransactionForm.cs
│   ├── TransactionsForm.cs
│   ├── BalanceSheetReportForm.cs
│   ├── IncomeStatementReportForm.cs
│   └── GeneralLedgerReportForm.cs
├── Reports/               # پوشه گزارشات (آماده برای فایل‌های RDLC)
└── Program.cs            # نقطه شروع برنامه
```

## استفاده از برنامه

### شروع کار
1. اجرای برنامه
2. سیستم به طور خودکار حساب‌های پیش‌فرض را ایجاد می‌کند
3. می‌توانید حساب‌های جدید اضافه کنید

### ثبت تراکنش
1. از منوی "تراکنش‌ها" گزینه "ثبت تراکنش جدید" را انتخاب کنید
2. اطلاعات تراکنش را وارد کنید
3. نوع تراکنش (بدهکار/بستانکار) را انتخاب کنید
4. حساب مربوطه را انتخاب کنید

### مشاهده گزارشات
1. از منوی "گزارشات" گزارش مورد نظر را انتخاب کنید
2. گزارشات به صورت PDF قابل چاپ هستند

## پکیج‌های استفاده شده

- `System.Data.SQLite`: برای کار با پایگاه داده SQLite
- `Dapper`: برای ORM و دسترسی به داده

## مجوز

این پروژه تحت مجوز MIT منتشر شده است.

## پشتیبانی

برای گزارش مشکلات یا پیشنهادات، لطفاً یک Issue ایجاد کنید. 