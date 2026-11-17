-- =============================================================================
-- StockGuard - PostgreSQL Database Schema & Seed Data
-- Database Engine: PostgreSQL 12+
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Table: sg_users (System Users & Authentication)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sg_users (
    UserId INT GENERATED ALWAYS AS IDENTITY(START 1 INCREMENT 1) PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(30) NOT NULL DEFAULT 'User',
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP,
    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Admin', 'User'))
);

-- -----------------------------------------------------------------------------
-- 2. Table: sg_categories (Product Taxonomy & Organization)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sg_categories (
    CategoryId INT GENERATED ALWAYS AS IDENTITY(START 1 INCREMENT 1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(300),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP
);

-- -----------------------------------------------------------------------------
-- 3. Table: sg_suppliers (Vendor Directory & Sourcing Partners)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sg_suppliers (
    SupplierId INT GENERATED ALWAYS AS IDENTITY(START 1 INCREMENT 1) PRIMARY KEY,
    SupplierName VARCHAR(150) NOT NULL,
    ContactPerson VARCHAR(100),
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Address VARCHAR(250),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP
);

-- -----------------------------------------------------------------------------
-- 4. Table: sg_products (Master Catalog & Inventory Levels)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sg_products (
    ProductId INT GENERATED ALWAYS AS IDENTITY(START 1 INCREMENT 1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    SupplierId INT NOT NULL,
    SKU VARCHAR(50) NOT NULL UNIQUE,
    ProductName VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    UnitPrice NUMERIC(12,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    MinStockLevel INT NOT NULL DEFAULT 0,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP,

    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES sg_categories(CategoryId)
        ON DELETE RESTRICT,

    CONSTRAINT FK_Products_Suppliers
        FOREIGN KEY (SupplierId)
        REFERENCES sg_suppliers(SupplierId)
        ON DELETE RESTRICT,

    CONSTRAINT CK_Products_UnitPrice
        CHECK (UnitPrice >= 0),

    CONSTRAINT CK_Products_StockQuantity
        CHECK (StockQuantity >= 0),

    CONSTRAINT CK_Products_MinStockLevel
        CHECK (MinStockLevel >= 0)
);

-- -----------------------------------------------------------------------------
-- 5. Table: sg_stocktransactions (Stock In/Out/Adjustment Audit Ledger)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sg_stocktransactions (
    TransactionId INT GENERATED ALWAYS AS IDENTITY(START 1 INCREMENT 1) PRIMARY KEY,
    ProductId INT NOT NULL,
    TransactionType VARCHAR(20) NOT NULL,
    Quantity INT NOT NULL,
    TransactionDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Remarks VARCHAR(300),

    CONSTRAINT FK_StockTransactions_Products
        FOREIGN KEY (ProductId)
        REFERENCES sg_products(ProductId)
        ON DELETE RESTRICT,

    CONSTRAINT CK_StockTransactions_Type
        CHECK (TransactionType IN ('IN', 'OUT', 'ADJUSTMENT')),

    CONSTRAINT CK_StockTransactions_Quantity
        CHECK (Quantity > 0)
);

-- -----------------------------------------------------------------------------
-- 6. Performance Indexes
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS IX_Products_CategoryId
    ON sg_products(CategoryId);

CREATE INDEX IF NOT EXISTS IX_Products_SupplierId
    ON sg_products(SupplierId);

CREATE INDEX IF NOT EXISTS IX_StockTransactions_ProductId
    ON sg_stocktransactions(ProductId);

CREATE INDEX IF NOT EXISTS IX_StockTransactions_TransactionDate
    ON sg_stocktransactions(TransactionDate);

-- -----------------------------------------------------------------------------
-- 7. Initial Seed Data
-- -----------------------------------------------------------------------------

-- Default Admin User (Password: Admin@123)
-- StockGuard auto-upgrades this plaintext password to a salted PBKDF2 hash on first login.
INSERT INTO sg_users (FullName, Email, PasswordHash, Role, IsActive, CreatedAt)
VALUES ('Brijesh Shah', 'admin@gmail.com', 'Admin@123', 'Admin', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT (Email) DO NOTHING;

-- Sample Category
INSERT INTO sg_categories (CategoryName, Description, IsActive, CreatedAt)
VALUES ('Consumer Electronics', 'Hardware peripherals, accessories, and gadgets', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT (CategoryName) DO NOTHING;

-- Sample Supplier
INSERT INTO sg_suppliers (SupplierName, ContactPerson, Email, Phone, Address, IsActive, CreatedAt)
VALUES ('Logitech Distribution Inc.', 'Account Representative', 'support@logitech-dist.com', '+1 (555) 019-2834', '7700 Gateway Blvd, Newark, CA', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- Sample Product
INSERT INTO sg_products (CategoryId, SupplierId, SKU, ProductName, Description, UnitPrice, StockQuantity, MinStockLevel, IsActive, CreatedAt)
VALUES (1, 1, 'SKU-LOGI-001', 'Wireless Mouse', 'Ergonomic 2.4GHz wireless mouse with optical tracking', 750.00, 50, 10, TRUE, CURRENT_TIMESTAMP)
ON CONFLICT (SKU) DO NOTHING;

