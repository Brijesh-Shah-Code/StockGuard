using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC.Models;
using Npgsql;

namespace MVC.BAL
{
    public class StockTransactionsHelper
    {
        private readonly NpgsqlConnection _conn;

        public StockTransactionsHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<bool> CreateStockTransaction(StockTransactions transaction)
        {
            await _conn.CloseAsync();
            await _conn.OpenAsync();

            using (var dbTransaction = await _conn.BeginTransactionAsync())
            {
                try
                {
                    // Get current stock quantity
                    int currentStock = 0;
                    string stockQry = @"SELECT stockquantity FROM sg_products WHERE productid = @productid FOR UPDATE";
                    
                    using (var stockCmd = new NpgsqlCommand(stockQry, _conn, dbTransaction))
                    {
                        stockCmd.Parameters.AddWithValue("@productid", transaction.ProductId);
                        var result = await stockCmd.ExecuteScalarAsync();
                        currentStock = result != null ? Convert.ToInt32(result) : 0;
                    }

                    // Calculate new stock based on transaction type
                    int newStock = currentStock;
                    switch (transaction.TransactionType.ToUpper())
                    {
                        case "IN":
                            newStock = currentStock + transaction.Quantity;
                            break;
                        case "OUT":
                            newStock = currentStock - transaction.Quantity;
                            if (newStock < 0)
                            {
                                throw new InvalidOperationException("Insufficient stock. Cannot process OUT transaction.");
                            }
                            break;
                        case "ADJUSTMENT":
                            // For ADJUSTMENT, the quantity represents the new stock level
                            newStock = transaction.Quantity;
                            if (newStock < 0)
                            {
                                throw new InvalidOperationException("Adjusted stock cannot be negative.");
                            }
                            break;
                        default:
                            throw new ArgumentException("Invalid transaction type. Must be IN, OUT, or ADJUSTMENT.");
                    }

                    string insertQry = @"INSERT INTO sg_stocktransactions 
                        (productid, transactiontype, quantity, transactiondate, remarks)
                        VALUES(@productid, @transactiontype, @quantity, @transactiondate, @remarks)";

                    using (var insertCmd = new NpgsqlCommand(insertQry, _conn, dbTransaction))
                    {
                        insertCmd.Parameters.AddWithValue("@productid", transaction.ProductId);
                        insertCmd.Parameters.AddWithValue("@transactiontype", transaction.TransactionType.ToUpper());
                        insertCmd.Parameters.AddWithValue("@quantity", transaction.Quantity);
                        insertCmd.Parameters.AddWithValue("@transactiondate", transaction.TransactionDate);
                        insertCmd.Parameters.AddWithValue("@remarks", transaction.Remarks ?? (object)DBNull.Value);

                        await insertCmd.ExecuteNonQueryAsync();
                    }

                    // Update product stock quantity
                    string updateStockQry = @"UPDATE sg_products SET stockquantity = @stockquantity, updatedat = @updatedat WHERE productid = @productid";
                    using (var updateCmd = new NpgsqlCommand(updateStockQry, _conn, dbTransaction))
                    {
                        updateCmd.Parameters.AddWithValue("@productid", transaction.ProductId);
                        updateCmd.Parameters.AddWithValue("@stockquantity", newStock);
                        updateCmd.Parameters.AddWithValue("@updatedat", DateTime.Now);

                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    await dbTransaction.CommitAsync();
                    return true;
                }
                catch (PostgresException ex) when (ex.SqlState == "23503") // Foreign key violation
                {
                    await dbTransaction.RollbackAsync();
                    throw new InvalidOperationException("Invalid product selected. The product may not exist.", ex);
                }
                catch (InvalidOperationException)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    throw new InvalidOperationException("Error creating stock transaction: " + ex.Message, ex);
                }
            }
        }

        public async Task<bool> DeleteStockTransaction(int transactionId)
        {
            await _conn.CloseAsync();
            await _conn.OpenAsync();

            using (var dbTransaction = await _conn.BeginTransactionAsync())
            {
                try
                {
                    // First get the transaction details
                    int productId = 0;
                    string transactionType = "";
                    int quantity = 0;

                    string getQry = @"SELECT productid, transactiontype, quantity FROM sg_stocktransactions WHERE transactionid = @transactionid";
                    using (var getCmd = new NpgsqlCommand(getQry, _conn, dbTransaction))
                    {
                        getCmd.Parameters.AddWithValue("@transactionid", transactionId);
                        using (var reader = await getCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                productId = (int)reader[0];
                                transactionType = reader[1].ToString();
                                quantity = (int)reader[2];
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    // Get current stock
                    int currentStock = 0;
                    string stockQry = @"SELECT stockquantity FROM sg_products WHERE productid = @productid FOR UPDATE";
                    using (var stockCmd = new NpgsqlCommand(stockQry, _conn, dbTransaction))
                    {
                        stockCmd.Parameters.AddWithValue("@productid", productId);
                        var result = await stockCmd.ExecuteScalarAsync();
                        currentStock = result != null ? Convert.ToInt32(result) : 0;
                    }

                    // Reverse the transaction effect on stock
                    int newStock = currentStock;
                    switch (transactionType.ToUpper())
                    {
                        case "IN":
                            newStock = currentStock - quantity;
                            if (newStock < 0) newStock = 0;
                            break;
                        case "OUT":
                            newStock = currentStock + quantity;
                            break;
                        case "ADJUSTMENT":
                            throw new InvalidOperationException("Cannot delete ADJUSTMENT transactions as previous stock level is unknown.");
                    }

                    // Delete the stock transaction
                    string deleteQry = @"DELETE FROM sg_stocktransactions WHERE transactionid = @transactionid";
                    using (var deleteCmd = new NpgsqlCommand(deleteQry, _conn, dbTransaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@transactionid", transactionId);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    // Update product stock quantity
                    string updateStockQry = @"UPDATE sg_products SET stockquantity = @stockquantity, updatedat = @updatedat WHERE productid = @productid";
                    using (var updateCmd = new NpgsqlCommand(updateStockQry, _conn, dbTransaction))
                    {
                        updateCmd.Parameters.AddWithValue("@productid", productId);
                        updateCmd.Parameters.AddWithValue("@stockquantity", newStock);
                        updateCmd.Parameters.AddWithValue("@updatedat", DateTime.Now);

                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    await dbTransaction.CommitAsync();
                    return true;
                }
                catch (InvalidOperationException)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    throw new InvalidOperationException("Error deleting stock transaction: " + ex.Message, ex);
                }
            }
        }

        public async Task<List<StockTransactions>> ReadAllStockTransactions()
        {
            var transactions = new List<StockTransactions>();
            try
            {
                string qry = @"SELECT st.transactionid, st.productid, p.productname, p.sku, st.transactiontype, st.quantity, st.transactiondate, st.remarks
                              FROM sg_stocktransactions st
                              LEFT JOIN sg_products p ON st.productid = p.productid
                              ORDER BY st.transactiondate DESC, st.transactionid DESC";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new StockTransactions
                            {
                                TransactionId = (int)reader[0],
                                ProductId = (int)reader[1],
                                ProductName = reader[2]?.ToString(),
                                ProductSKU = reader[3]?.ToString(),
                                TransactionType = reader[4].ToString(),
                                Quantity = (int)reader[5],
                                TransactionDate = (DateTime)reader[6],
                                Remarks = reader[7]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading stock transactions: " + ex.Message, ex);
            }
            return transactions;
        }

        public async Task<StockTransactions> ReadStockTransactionById(int transactionId)
        {
            StockTransactions transaction = null;
            try
            {
                string qry = @"SELECT st.transactionid, st.productid, p.productname, p.sku, st.transactiontype, st.quantity, st.transactiondate, st.remarks
                              FROM sg_stocktransactions st
                              LEFT JOIN sg_products p ON st.productid = p.productid
                              WHERE st.transactionid = @transactionid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@transactionid", transactionId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            transaction = new StockTransactions
                            {
                                TransactionId = (int)reader[0],
                                ProductId = (int)reader[1],
                                ProductName = reader[2]?.ToString(),
                                ProductSKU = reader[3]?.ToString(),
                                TransactionType = reader[4].ToString(),
                                Quantity = (int)reader[5],
                                TransactionDate = (DateTime)reader[6],
                                Remarks = reader[7]?.ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading stock transaction by id: " + ex.Message, ex);
            }
            return transaction;
        }

        public async Task<List<StockTransactions>> ReadStockTransactionsByProductId(int productId)
        {
            var transactions = new List<StockTransactions>();
            try
            {
                string qry = @"SELECT st.transactionid, st.productid, p.productname, p.sku, st.transactiontype, st.quantity, st.transactiondate, st.remarks
                              FROM sg_stocktransactions st
                              LEFT JOIN sg_products p ON st.productid = p.productid
                              WHERE st.productid = @productid
                              ORDER BY st.transactiondate DESC, st.transactionid DESC";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@productid", productId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new StockTransactions
                            {
                                TransactionId = (int)reader[0],
                                ProductId = (int)reader[1],
                                ProductName = reader[2]?.ToString(),
                                ProductSKU = reader[3]?.ToString(),
                                TransactionType = reader[4].ToString(),
                                Quantity = (int)reader[5],
                                TransactionDate = (DateTime)reader[6],
                                Remarks = reader[7]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading stock transactions by product: " + ex.Message, ex);
            }
            return transactions;
        }

        public async Task<List<Products>> GetProductsForDropdown()
        {
            var products = new List<Products>();
            try
            {
                string qry = @"SELECT productid, productname, sku FROM sg_products WHERE isactive = true ORDER BY productname";

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
                                ProductName = reader[1].ToString(),
                                SKU = reader[2].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting products for dropdown: " + ex.Message, ex);
            }
            return products;
        }
    }
}