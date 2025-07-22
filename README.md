# AccountingApp

A comprehensive accounting application built with C# and .NET 6, featuring modern UI design and robust functionality for managing business finances.

## 🌟 Features

### 📊 **Invoice Management**
- Create and manage professional invoices
- Multiple invoice formats (PDF, HTML, Direct Print)
- Custom paper sizes (80mm thermal printer support)
- Persian date formatting
- Random Hafez fortune quotes on invoices

### 👥 **Customer Management**
- Complete customer database
- Contact information tracking
- Customer history and analytics
- Search and filter capabilities

### 💰 **Sales Tracking**
- Sales recording and management
- Product/service catalog
- Price management
- Sales reports and analytics

### 🏢 **Supplier Management**
- Supplier database
- Contact and payment information
- Supplier performance tracking

### 🖨️ **Advanced Printing**
- **Direct Printing**: Bypass PDF generation for faster printing
- **Custom Paper Sizes**: Support for 80mm thermal printers
- **Multiple Formats**: PDF, HTML, and direct print options
- **Persian Calendar**: Full Persian date support
- **Hafez Fortune**: Random Persian poetry quotes on invoices

## 🚀 Getting Started

### Prerequisites
- .NET 6.0 or later
- Windows 10/11
- SQLite (included)

### Installation
1. Clone the repository
```bash
git clone https://github.com/yourusername/AccountingApp.git
```

2. Navigate to the project directory
```bash
cd AccountingApp
```

3. Build the application
```bash
dotnet build
```

4. Run the application
```bash
dotnet run
```

### 🚀 Quick Start (Using Pre-built Files)

If you want to run the application without building it:

1. Navigate to the build directory
```bash
cd AccountingApp/bin/Debug/net6.0-windows
```

2. Run the executable directly
```bash
./AccountingApp.exe
```

**Note**: The `bin/` and `obj/` directories are included in this repository for convenience, containing pre-built application files.

## 📁 Project Structure

```
AccountingApp/
├── Forms/                     # Windows Forms
│   ├── MainForm.cs           # Main application window
│   ├── InvoiceViewerForm.cs  # Invoice display and printing
│   ├── PdfViewerForm.cs      # PDF viewer and printing
│   ├── WebBrowserInvoiceForm.cs # HTML invoice viewer
│   └── ...                   # Other form files
├── Models/                    # Data models
│   ├── Customer.cs           # Customer entity
│   ├── Sale.cs               # Sale entity
│   ├── Supplier.cs           # Supplier entity
│   └── FalItem.cs            # Hafez fortune model
├── Resources/                 # Application resources
│   └── hafez_fal.json        # Persian fortune quotes
├── Database/                  # SQLite database files
├── bin/                       # Compiled application files
│   ├── Debug/net6.0-windows/ # Build output directory
│   ├── AccountingApp.exe     # Main executable
│   ├── AccountingApp.dll     # Main library
│   └── README.md             # Build directory documentation
└── obj/                       # Intermediate build files
    └── README.md             # Object files documentation
```

## 🎨 Key Features in Detail

### **Smart Invoice Printing**
- **Direct Print**: Uses `PrintDocument` for immediate printing without PDF generation
- **Custom Paper Size**: 80mm width with flexible length (up to 300mm)
- **Persian Typography**: Full RTL support with Vazir font
- **Random Fortune**: Each invoice includes a random Hafez quote

### **Modern UI Design**
- Clean and intuitive interface
- Responsive design
- Professional color scheme
- Easy navigation

### **Data Management**
- SQLite database for reliable data storage
- Backup and restore functionality
- Data export capabilities
- Search and filter options

## 🔧 Technical Details

### **Technologies Used**
- **.NET 6**: Modern C# framework
- **Windows Forms**: Desktop UI framework
- **SQLite**: Lightweight database
- **QuestPDF**: PDF generation library
- **System.Drawing.Printing**: Direct printing functionality

### **Key Libraries**
```xml
<PackageReference Include="QuestPDF" Version="2023.12.6" />
<PackageReference Include="System.Data.SQLite" Version="1.0.118" />
<PackageReference Include="PdfiumViewer" Version="2.13.0" />
```

## 📸 Screenshots

*[Screenshots will be added here]*

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Hafez Poetry**: Persian fortune quotes from Hafez's Divan
- **Vazir Font**: Beautiful Persian typography
- **QuestPDF**: Excellent PDF generation library
- **SQLite**: Reliable database solution

## 📞 Support



---

**Made with ❤️ for Persian businesses**

*Built with modern C# and .NET 6 for optimal performance and reliability.* 