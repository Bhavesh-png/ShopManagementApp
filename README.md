<div align="center">

<img src="ShopManagementApp.UI/Assets/AppIcon.png" width="120" alt="Gayatri Electronics Logo"/>

# 🏪 Gayatri Electronics & Hardware
### Shop Management Desktop Application

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![WinForms](https://img.shields.io/badge/WinForms-Desktop-0078D4?style=for-the-badge&logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
[![ClosedXML](https://img.shields.io/badge/ClosedXML-Excel-217346?style=for-the-badge&logo=microsoft-excel)](https://github.com/ClosedXML/ClosedXML)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Release](https://img.shields.io/github/v/release/Bhavesh-png/ShopManagementApp?style=for-the-badge&logo=github&color=brightgreen)](https://github.com/Bhavesh-png/ShopManagementApp/releases/latest)

*A complete, portable, offline Shop Management System for electronics and electrical hardware shops.*

### ⬇️ [Download Latest Installer (Windows)](https://github.com/Bhavesh-png/ShopManagementApp/releases/latest)

</div>

---

## 📸 Screenshots

| Dashboard | Billing | Inventory |
|-----------|---------|-----------|
| ![Dashboard](docs/dashboard.png) | ![Billing](docs/billing.png) | ![Inventory](docs/inventory.png) |

---

## ✨ Features

### 📊 Dashboard
- Live stats: Today's Revenue, Active Repairs, Low Stock Alerts, Total Products
- Quick action buttons to jump to any module
- Auto-refreshes every time you navigate back

### 🧾 Billing / Sales
- Product dropdown with all inventory items
- **Custom / Other items** — add any item not in inventory (labour charges, misc parts)
- Auto quantity merge for duplicate products
- Discount support
- Print receipt (GDI+ based, no printer driver needed)
- Saves to Excel Sales sheet automatically

### 🔧 Repairs
- Track repairs with device type, fault description, technician, cost
- Status tracking: Pending → In Progress → Completed
- Full repair history with search and filter

### 📦 Inventory Management
- Add, Edit, Delete products
- Category and brand filtering
- Low-stock alert (highlights items with stock ≤ 10)
- Real-time search across name, brand, category

### 🔐 Admin Panel
- Secure login (default: `admin` / `1234`)
- View all sales and repair records
- Change admin password
- Full data overview

---

## 🗂️ Project Architecture

```
ShopManagementApp/
│
├── ShopManagementApp.UI/            ← WinForms UI layer
│   ├── Forms/
│   │   ├── MainForm.cs              ← SPA shell with sidebar navigation
│   │   ├── DashboardForm.cs
│   │   ├── BillingForm.cs
│   │   ├── RepairForm.cs
│   │   ├── InventoryForm.cs
│   │   ├── AdminPanelForm.cs
│   │   └── AdminLoginForm.cs
│   └── Assets/
│       ├── AppIcon.ico              ← App icon (EXE + taskbar)
│       └── AppIcon.png              ← Logo in top bar
│
├── ShopManagementApp.Business/      ← Business logic layer
│   └── Services/
│       ├── BillingService.cs
│       ├── RepairService.cs
│       └── InventoryService.cs
│
├── ShopManagementApp.Data/          ← Data access layer
│   ├── Excel/
│   │   └── ExcelManager.cs          ← Auto-create + schema versioning
│   └── Repositories/
│       ├── ProductRepository.cs
│       ├── SalesRepository.cs
│       └── RepairRepository.cs
│
├── ShopManagementApp.Models/        ← Data models
│   ├── Product.cs
│   ├── Sale.cs
│   ├── SaleItem.cs
│   └── Repair.cs
│
└── ShopManagementApp.Utils/         ← Shared utilities
    ├── Constants.cs
    ├── ValidationHelper.cs
    └── PrintHelper.cs
```

---

## 🗃️ Excel Database (Auto-Managed)

The app uses **ClosedXML** to manage a local `ShopData.xlsx` file.

| Sheet | Purpose |
|-------|---------|
| `Products` | All inventory items |
| `Sales` | Bill headers |
| `SaleItems` | Individual line items per bill |
| `Repairs` | Repair job records |
| `Settings` | Admin credentials + schema version |

> ✅ **The Excel file is created automatically on first run** — no manual setup needed.
> ✅ **Schema versioning** — if the file format changes, the app backs up old data and creates a fresh file.

---

## 🚀 Getting Started

### 🖥️ Option 1 — Install via Setup Wizard (Recommended)

1. Go to [**Releases**](https://github.com/Bhavesh-png/ShopManagementApp/releases/latest)
2. Download `GayatriElectronics_Setup_v1.0.0.exe`
3. Run the installer → follows a standard Windows setup wizard
4. App installs to `Program Files`, creates Desktop & Start Menu shortcuts
5. Launch and log in with default credentials (`admin` / `1234`)

> ✅ No .NET installation required — the runtime is bundled inside the installer.

---

### 💻 Option 2 — Run from Source (Developers)

**Prerequisites:**
- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or VS Code

```bash
# Clone the repository
git clone https://github.com/Bhavesh-png/ShopManagementApp.git
cd ShopManagementApp

# Restore NuGet packages
dotnet restore

# Run the application
dotnet run --project ShopManagementApp.UI
```

### 📦 Build Your Own Installer

```bash
# Step 1 — Publish self-contained EXE
dotnet publish ShopManagementApp.UI -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Step 2 — Compile installer (requires Inno Setup)
& "D:\Inno Setup 6\ISCC.exe" "installer\GayatriElectronics_Setup.iss"

# Output: installer\Output\GayatriElectronics_Setup_v1.0.0.exe
```

---

## 🔐 Default Admin Credentials

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `1234` |

> ⚠️ Change the password after first login via **Admin Panel → Change Password**.

---

## 🛠️ Tech Stack

| Technology | Purpose |
|-----------|---------|
| C# .NET 8 | Core language |
| WinForms | Desktop UI framework |
| ClosedXML | Excel read/write |
| GDI+ | Receipt printing |
| System.Drawing | Icon & image handling |

---

## 📦 NuGet Packages

```xml
<PackageReference Include="ClosedXML" Version="0.102.2" />
```

---

## 🏗️ Key Design Decisions

- **SPA Navigation** — All pages load inside a single `_pageHost` panel. No popup windows, no header overlap.
- **Double Buffering** — `SmoothPanel` class + `WS_EX_COMPOSITED` flag eliminates all flicker during page switching.
- **Schema Versioning** — `ExcelManager` checks a `SchemaVersion` key on startup. Stale schemas trigger auto-backup + fresh file creation.
- **Custom Billing Items** — `ProductId = 0` bypasses stock deduction for non-inventory items (labour charges, misc).
- **Portable Build** — Single self-contained EXE bundles the entire .NET runtime.

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

Made with ❤️ for **Gayatri Electronics & Hardware**

</div>
