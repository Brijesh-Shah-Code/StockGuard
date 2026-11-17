using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVC.Models;
using Npgsql;

namespace MVC.BAL
{
    public class ProductsHelper
    {
        private readonly NpgsqlConnection _conn;

        public ProductsHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<bool> CreateProduct(Products product)
        {
            try
            {
                string qry = @"INSERT INTO sg_products 
                    (categoryid, supplierid, sku, productname, description, unitprice, stockquantity, minstocklevel, isactive, createdat, updatedat)
                    VALUES(@categoryid, @supplierid, @sku, @productname, @description, @unitprice, @stockquantity, @minstocklevel, @isactive, @createdat, @updatedat)";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@categoryid", product.CategoryId);
                    cmd.Parameters.AddWithValue("@supplierid", product.SupplierId);
                    cmd.Parameters.AddWithValue("@sku", product.SKU);
                    cmd.Parameters.AddWithValue("@productname", product.ProductName);
                    cmd.Parameters.AddWithValue("@description", product.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@unitprice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@stockquantity", product.StockQuantity);
                    cmd.Parameters.AddWithValue("@minstocklevel", product.MinStockLevel);
                    cmd.Parameters.AddWithValue("@isactive", product.IsActive);
                    cmd.Parameters.AddWithValue("@createdat", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();

                    if (reader > 0)
                        return true;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
            {
                throw new InvalidOperationException("SKU already exists. Please enter a unique SKU.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating product: " + ex.Message, ex);
            }
            return false;
        }

        public async Task<bool> UpdateProduct(Products product)
        {
            try
            {
                string qry = @"UPDATE sg_products 
                    SET categoryid = @categoryid, 
                        supplierid = @supplierid, 
                        sku = @sku, 
                        productname = @productname, 
                        description = @description, 
                        unitprice = @unitprice, 
                        stockquantity = @stockquantity, 
                        minstocklevel = @minstocklevel, 
                        isactive = @isactive, 
                        updatedat = @updatedat
                    WHERE productid = @productid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@productid", product.ProductId);
                    cmd.Parameters.AddWithValue("@categoryid", product.CategoryId);
                    cmd.Parameters.AddWithValue("@supplierid", product.SupplierId);
                    cmd.Parameters.AddWithValue("@sku", product.SKU);
                    cmd.Parameters.AddWithValue("@productname", product.ProductName);
                    cmd.Parameters.AddWithValue("@description", product.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@unitprice", product.UnitPrice);
                    cmd.Parameters.AddWithValue("@stockquantity", product.StockQuantity);
                    cmd.Parameters.AddWithValue("@minstocklevel", product.MinStockLevel);
                    cmd.Parameters.AddWithValue("@isactive", product.IsActive);
                    cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();

                    if (reader > 0)
                        return true;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
            {
                throw new InvalidOperationException("SKU already exists. Please enter a unique SKU.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating product: " + ex.Message, ex);
            }
            return false;
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try
            {
                string qry = @"DELETE FROM sg_products WHERE productid = @productid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@productid", productId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();

                    if (reader > 0)
                        return true;
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23503") // Foreign key violation
            {
                throw new InvalidOperationException("Cannot delete product. It has associated stock transactions.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error deleting product: " + ex.Message, ex);
            }
            return false;
        }

        public async Task<List<Products>> ReadAllProducts()
        {
            var products = new List<Products>();
            try
            {
                string qry = @"SELECT p.productid, p.categoryid, c.categoryname, p.supplierid, s.suppliername, 
                              p.sku, p.productname, p.description, p.unitprice, p.stockquantity, p.minstocklevel, p.isactive, p.createdat, p.updatedat
                              FROM sg_products p
                              LEFT JOIN sg_categories c ON p.categoryid = c.categoryid
                              LEFT JOIN sg_suppliers s ON p.supplierid = s.supplierid
                              ORDER BY p.productname";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Products
                            {
                                ProductId = (int)reader[0],
                                CategoryId = (int)reader[1],
                                CategoryName = reader[2]?.ToString(),
                                SupplierId = (int)reader[3],
                                SupplierName = reader[4]?.ToString(),
                                SKU = reader[5].ToString(),
                                ProductName = reader[6].ToString(),
                                Description = reader[7]?.ToString(),
                                UnitPrice = (decimal)reader[8],
                                StockQuantity = (int)reader[9],
                                MinStockLevel = (int)reader[10],
                                IsActive = (bool)reader[11],
                                CreatedAt = (DateTime)reader[12],
                                UpdatedAt = reader[13] != DBNull.Value ? (DateTime)reader[13] : DateTime.MinValue
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading products: " + ex.Message, ex);
            }
            return products;
        }

        public async Task<Products> ReadProductById(int productId)
        {
            Products product = null;
            try
            {
                string qry = @"SELECT p.productid, p.categoryid, c.categoryname, p.supplierid, s.suppliername, 
                              p.sku, p.productname, p.description, p.unitprice, p.stockquantity, p.minstocklevel, p.isactive, p.createdat, p.updatedat
                              FROM sg_products p
                              LEFT JOIN sg_categories c ON p.categoryid = c.categoryid
                              LEFT JOIN sg_suppliers s ON p.supplierid = s.supplierid
                              WHERE p.productid = @productid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@productid", productId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = new Products
                            {
                                ProductId = (int)reader[0],
                                CategoryId = (int)reader[1],
                                CategoryName = reader[2]?.ToString(),
                                SupplierId = (int)reader[3],
                                SupplierName = reader[4]?.ToString(),
                                SKU = reader[5].ToString(),
                                ProductName = reader[6].ToString(),
                                Description = reader[7]?.ToString(),
                                UnitPrice = (decimal)reader[8],
                                StockQuantity = (int)reader[9],
                                MinStockLevel = (int)reader[10],
                                IsActive = (bool)reader[11],
                                CreatedAt = (DateTime)reader[12],
                                UpdatedAt = reader[13] != DBNull.Value ? (DateTime)reader[13] : DateTime.MinValue
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading product by id: " + ex.Message, ex);
            }
            return product;
        }

        public async Task<Products> ReadProductBySKU(string sku)
        {
            Products product = null;
            try
            {
                string qry = @"SELECT p.productid, p.categoryid, c.categoryname, p.supplierid, s.suppliername, 
                              p.sku, p.productname, p.description, p.unitprice, p.stockquantity, p.minstocklevel, p.isactive, p.createdat, p.updatedat
                              FROM sg_products p
                              LEFT JOIN sg_categories c ON p.categoryid = c.categoryid
                              LEFT JOIN sg_suppliers s ON p.supplierid = s.supplierid
                              WHERE p.sku = @sku";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@sku", sku);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = new Products
                            {
                                ProductId = (int)reader[0],
                                CategoryId = (int)reader[1],
                                CategoryName = reader[2]?.ToString(),
                                SupplierId = (int)reader[3],
                                SupplierName = reader[4]?.ToString(),
                                SKU = reader[5].ToString(),
                                ProductName = reader[6].ToString(),
                                Description = reader[7]?.ToString(),
                                UnitPrice = (decimal)reader[8],
                                StockQuantity = (int)reader[9],
                                MinStockLevel = (int)reader[10],
                                IsActive = (bool)reader[11],
                                CreatedAt = (DateTime)reader[12],
                                UpdatedAt = reader[13] != DBNull.Value ? (DateTime)reader[13] : DateTime.MinValue
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading product by sku: " + ex.Message, ex);
            }
            return product;
        }

        public async Task<List<Categories>> GetCategoriesForDropdown()
        {
            var categories = new List<Categories>();
            try
            {
                string qry = @"SELECT categoryid, categoryname FROM sg_categories WHERE isactive = true ORDER BY categoryname";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Categories
                            {
                                CategoryId = (int)reader[0],
                                CategoryName = reader[1].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting categories for dropdown: " + ex.Message, ex);
            }
            return categories;
        }

        public async Task<List<Suppliers>> GetSuppliersForDropdown()
        {
            var suppliers = new List<Suppliers>();
            try
            {
                string qry = @"SELECT supplierid, suppliername FROM sg_suppliers WHERE isactive = true ORDER BY suppliername";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            suppliers.Add(new Suppliers
                            {
                                SupplierId = (int)reader[0],
                                SupplierName = reader[1].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting suppliers for dropdown: " + ex.Message, ex);
            }
            return suppliers;
        }

        public async Task<bool> UpdateStockQuantity(int productId, int newStockQuantity)
        {
            try
            {
                string qry = @"UPDATE sg_products SET stockquantity = @stockquantity, updatedat = @updatedat WHERE productid = @productid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@productid", productId);
                    cmd.Parameters.AddWithValue("@stockquantity", newStockQuantity);
                    cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();

                    if (reader > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating stock quantity: " + ex.Message, ex);
            }
            return false;
        }

        public async Task<int> GetStockQuantity(int productId)
        {
            try
            {
                string qry = @"SELECT stockquantity FROM sg_products WHERE productid = @productid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@productid", productId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting stock quantity: " + ex.Message, ex);
            }
        }
    }
}