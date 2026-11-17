using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC.Models;
using Npgsql;

namespace MVC.BAL
{
    public class SuppliersHelper
    {
        private readonly NpgsqlConnection _conn;

        public SuppliersHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<bool> CreateSupplier(Suppliers supplier)
        {
            try
            {
                string qry = @"INSERT INTO sg_suppliers 
                (suppliername, contactperson, email, phone, address, isactive, createdat, updatedat)
                VALUES(@suppliername, @contactperson, @email, @phone, @address, @isactive, @createdat, @updatedat)";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@suppliername", supplier.SupplierName);
                    cmd.Parameters.AddWithValue("@contactperson", supplier.ContactPerson);
                    cmd.Parameters.AddWithValue("@email", supplier.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@address", supplier.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isactive", supplier.IsActive);
                    cmd.Parameters.AddWithValue("@createdat", DateTime.Now);
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
                Console.WriteLine("Error to create supplier : " + ex.Message);
            }
            return false;
        }


        public async Task<bool> UpdateSupplier(Suppliers supplier)
        {
            try
            {
                string qry = @"UPDATE sg_suppliers 
                               SET suppliername = @suppliername, 
                                   contactperson = @contactperson, 
                                   email = @email, 
                                   phone = @phone, 
                                   address = @address, 
                                   isactive = @isactive, 
                                   updatedat = @updatedat
                               WHERE supplierid = @supplierid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@suppliername", supplier.SupplierName);
                    cmd.Parameters.AddWithValue("@contactperson", supplier.ContactPerson);
                    cmd.Parameters.AddWithValue("@email", supplier.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@address", supplier.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isactive", supplier.IsActive);
                    cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);
                    cmd.Parameters.AddWithValue("@supplierid", supplier.SupplierId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();

                    if (reader > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to update supplier : " + ex.Message);
            }
            return false;
        }


        public async Task<bool> DeleteSupplier(int supplierId)
        {
            try
            {
                string qry = @"DELETE FROM sg_suppliers WHERE supplierid = @supplierid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@supplierid", supplierId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();

                    if (reader > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to delete supplier : " + ex.Message);
            }
            return false;
        }



        public async Task<List<Suppliers>> ReadAllSuppliers()
        {
            var suppliers = new List<Suppliers>();
            try
            {
                string qry = @"SELECT supplierid, suppliername, contactperson, email, phone, address, isactive FROM sg_suppliers";

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
                                SupplierName = reader[1].ToString(),
                                ContactPerson = reader[2].ToString(),
                                Email = reader[3]?.ToString(),
                                Phone = reader[4]?.ToString(),
                                Address = reader[5]?.ToString(),
                                IsActive = (bool)reader[6]
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to read suppliers : " + ex.Message);
            }
            return suppliers;
        }

        public async Task<Suppliers> ReadSupplierById(int supplierId)
        {
            Suppliers supplier = null;
            try
            {
                string qry = @"SELECT supplierid, suppliername, contactperson, email, phone, address, isactive FROM sg_suppliers WHERE supplierid = @supplierid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@supplierid", supplierId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            supplier = new Suppliers
                            {
                                SupplierId = (int)reader[0],
                                SupplierName = reader[1].ToString(),
                                ContactPerson = reader[2].ToString(),
                                Email = reader[3]?.ToString(),
                                Phone = reader[4]?.ToString(),
                                Address = reader[5]?.ToString(),
                                IsActive = (bool)reader[6]
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to read supplier by id : " + ex.Message);
            }
            return supplier;
        }


        public async Task<List<Suppliers>> ReadActiveSuppliers()
        {
            var suppliers = new List<Suppliers>();
            try
            {
                string qry = @"SELECT supplierid, suppliername, contactperson, email, phone, address FROM sg_suppliers WHERE isactive = true";

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
                                SupplierName = reader[1].ToString(),
                                ContactPerson = reader[2].ToString(),
                                Email = reader[3]?.ToString(),
                                Phone = reader[4]?.ToString(),
                                Address = reader[5]?.ToString(),
                                IsActive = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to read active suppliers : " + ex.Message);
            }
            return suppliers;
        }

    }
}