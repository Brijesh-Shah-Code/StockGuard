# StockGuard 🛡️
> **Enterprise Inventory Management & Stock Movement Auditing Platform**

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/Language-C%23_12-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Database](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql)](https://www.postgresql.org/)
[![UI Framework](https://img.shields.io/badge/UI-Bootstrap_5-7952B3?logo=bootstrap)](https://getbootstrap.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## 📖 Overview

**StockGuard** is an enterprise-grade ASP.NET Core MVC inventory management system engineered to provide complete visibility, control, and traceability over warehousing operations. 

Built with clean architecture principles, StockGuard pairs a robust **PostgreSQL** relational back-end with a modern **responsive dashboard interface**. It enables procurement teams, inventory managers, and warehouse operators to manage multi-tiered product catalogs, coordinate with suppliers, and record auditable stock movement histories with automated stock reconciliation.

---

## ✨ Key Features

### 🔐 1. Authentication & Role-Based Access Control (RBAC)
* **Secure Cookie Authentication**: Standard claims-based cookie tickets with automatic sliding expiration.
* **Cryptographic Password Security**: Passwords salted and hashed via PBKDF2 (`Microsoft.AspNetCore.Identity.PasswordHasher<T>`).
* **Role Segregation**:
  * **Admin**: Complete master catalog control (Products, Categories, Suppliers, Stock Transactions, User Management).
  * **User / Staff**: Access to stock movement logging and transaction history inquiry.
* **Protected Routes**: Server-side `[Authorize(Roles = "Admin")]` filters paired with custom Access Denied handling.

### 📦 2. Master Product Catalog
* **Unique SKU Validation**: Server-side and database-level unique constraint checks preventing SKU collisions.
* **Categorization & Sourcing**: Association of items with designated taxonomy categories and verified suppliers.
* **Pricing & Reorder Points**: Unit price tracking and automated **Minimum Stock Level** alerting.
* **Health Indicators**: Real-time visual status pills (*In Stock*, *Low Stock Alert*, *Inactive*).

### 🏷️ 3. Category & Taxonomy Management
* Organized taxonomy tree with active/inactive availability toggles.
* Relational protection restricting deletions of categories with assigned active products.

### 🚚 4. Supplier & Vendor Directory
* Centralized vendor contact directory recording contact representatives, facility locations, and communication channels.
* Direct action click-to-email (`mailto:`) and click-to-call (`tel:`) shortcuts.

### 🔄 5. Auditable Stock Transactions
* **IN (Intake)**: Records inbound inventory shipments and increments current stock.
* **OUT (Fulfillment)**: Records outbound sales shipments with server-side validation against negative inventory.
* **ADJUSTMENT (Audit)**: Allows warehouse supervisors to reconcile stock levels directly following physical counts.
* **Transaction Reversal**: Non-adjustment transactions can be reversed while maintaining database balance integrity.
* **Product Movement Ledger**: Item-specific chronological movement history tracking every unit intake and decrement.

### 🎨 6. Premium UI/UX & Responsive Design System
* **Modern Dashboard**: Live KPI metric cards with change indicators, quick operation shortcuts, and recent movement logs.
* **Client-Side Live Filtering**: Instant zero-latency search bars and dropdown filters across all data tables.
* **Responsive Drawer**: Collapsible mobile sidebar with backdrop toggle and desktop sticky navigation.
* **Enterprise Styling**: Inter font stack, Bootstrap Icons, subtle glassmorphism cards, and accessibility focus states.

---

## 🛠️ Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core 8.0 (MVC) |
| **Programming Language** | C# 12 |
| **Database** | PostgreSQL 12+ |
| **Data Provider** | Npgsql (v10.0.3) |
| **Security** | ASP.NET Core Cookie Authentication & PBKDF2 Password Hasher |
| **Styling** | Custom CSS3 Design System with Bootstrap 5.3 |
| **Typography & Icons** | Google Fonts (*Inter*) & Bootstrap Icons (v1.11.3) |
| **Client-Side Scripting** | Vanilla JavaScript (ES6+) & jQuery |

---

## 📁 Project Structure

```text
StockGuard/
├── Database/
│   └── schema.sql                   # Complete PostgreSQL DDL schema, indexes & seed data
├── MVC/
│   ├── BAL/                         # Business Access Layer (Data Access & Helpers)
│   │   ├── AuthHelper.cs            # PBKDF2 hashing, login verification & registration
│   │   ├── CategoriesHelper.cs      # Category CRUD queries
│   │   ├── ProductsHelper.cs        # Product catalog queries & foreign key checks
│   │   ├── StockTransactionsHelper.cs # Transaction posting & stock quantity adjustments
│   │   └── SuppliersHelper.cs       # Supplier directory queries
│   ├── Controllers/                 # MVC Controllers with [Authorize] RBAC filters
│   │   ├── AuthController.cs        # Login, Register, Logout, AccessDenied
│   │   ├── BaseController.cs        # Dynamic layout assignment & claims extraction
│   │   ├── CategoriesController.cs  # Category management actions
│   │   ├── HomeController.cs        # Executive KPI dashboard
│   │   ├── ProductsController.cs    # Product catalog actions & dropdown population
│   │   ├── StockTransactionsController.cs # Stock intake/out/reversal actions
│   │   └── SuppliersController.cs   # Vendor directory actions
│   ├── Models/                      # Strongly-typed Data Annotations ViewModels & Entities
│   │   ├── Categories.cs
│   │   ├── ErrorViewModel.cs
│   │   ├── Products.cs
│   │   ├── StockTransactions.cs
│   │   ├── Suppliers.cs
│   │   ├── Users.cs
│   │   └── vm_Login.cs
│   ├── Views/                       # Razor Views & Component Partials
│   │   ├── Auth/                    # Login, Register, AccessDenied
│   │   ├── Categories/              # Index, Create, Update, Display, Delete
│   │   ├── Home/                    # Dashboard (Index), Privacy
│   │   ├── Products/                # Index, Create, Update, Display, Delete
│   │   ├── Shared/                  # _Layout, _AuthLayout, _ValidationScriptsPartial, Error
│   │   ├── StockTransactions/       # Index, Create, Display, Delete, ProductHistory
│   │   └── Suppliers/               # Index, Create, Update, Display, Delete
│   ├── wwwroot/                     # Static Web Assets
│   │   ├── css/site.css             # StockGuard Enterprise Design System tokens & layout
│   │   └── js/site.js               # Table search, filters, drawer, password toggles
│   ├── appsettings.json             # Configuration template (sanitized)
│   ├── appsettings.Example.json     # Sample configuration file
│   ├── MVC.csproj                   # Project dependencies and targets
│   └── Program.cs                   # Service registration, Cookie auth & HTTP pipeline
├── StockGuard.sln                   # Visual Studio Solution File
├── .gitignore                       # Production .gitignore for .NET & VS
└── README.md                        # Project documentation
```

---

## 🗄️ Database Setup (PostgreSQL)

### 1. Create Database
Open **pgAdmin 4**, **psql**, or your favorite database GUI client, and execute:

```sql
CREATE DATABASE stockguard_db;
```

### 2. Run the Schema Script
Execute the SQL script located in [`Database/schema.sql`](Database/schema.sql). This will automatically create all required tables, constraints, indexes, and initial seed records:

```bash
# Using PostgreSQL psql CLI:
psql -U postgres -d stockguard_db -f Database/schema.sql
```

Alternatively, open `Database/schema.sql` inside **pgAdmin Query Tool** and press **F5 (Execute)**.

---

## 🚀 Getting Started & Installation

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
* [PostgreSQL 12+](https://www.postgresql.org/download/) running locally or in the cloud.
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (with *ASP.NET and web development* workload) or [Visual Studio Code](https://code.visualstudio.com/).

### Installation Steps

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/<your-username>/StockGuard.git
   cd StockGuard
   ```

2. **Configure Connection String**:
   Open `MVC/appsettings.json` (or create `MVC/appsettings.Development.json`) and configure your PostgreSQL connection:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=5432;Database=stockguard_db;User Id=postgres;Password=YOUR_POSTGRES_PASSWORD;"
     }
   }
   ```

3. **Restore Dependencies & Build**:
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run the Application**:
   ```bash
   dotnet run --project MVC/MVC.csproj
   ```

5. **Launch in Browser**:
   Open your browser and navigate to:
   ```text
   https://localhost:7150  or  http://localhost:5246
   ```

---

## 🔑 Default Credentials

The database script automatically provisions a default administrator account:

| Attribute | Details |
| :--- | :--- |
| **Email** | `admin@gmail.com` |
| **Password** | `Admin@123` |
| **Role** | `Admin` |
| **Status** | Active |

> [!NOTE]
> Upon your first sign-in, StockGuard automatically upgrades the plaintext initial password to a salted **PBKDF2 hash** in the database.

---

## 🧪 Testing User Roles

1. **Sign in as Admin** (`admin@gmail.com` / `Admin@123`):
   * Full access to **Products**, **Categories**, **Suppliers**, and **Stock Transactions**.
2. **Register a Standard User**:
   * Navigate to the Register page (`/Auth/Register`).
   * Create an account.
   * Standard users have access to record stock transactions and inspect movement logs, while catalog administrative controls are protected.

---

## 🛡️ Security Best Practices Enforced

* **Zero Plain-Text Credentials**: Password hashes cannot be reversed; brute force attacks are mitigated via PBKDF2 salting.
* **SQL Injection Prevention**: All queries in the Business Access Layer execute via parameterized `NpgsqlCommand` parameters (`@mail`, `@pass`, `@sku`, etc.).
* **Anti-Forgery Validation**: Critical mutation endpoints enforce ASP.NET Core anti-forgery request tokens.
* **Sensitive Configuration Protection**: Local development settings and database passwords are excluded from Git version control via `.gitignore`.

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author & Contributions

* **Author**: Brijesh Shah
* **Repository**: [StockGuard on GitHub](https://github.com/)
* Contributions, issues, and feature requests are welcome! Feel free to open an issue or submit a pull request.

