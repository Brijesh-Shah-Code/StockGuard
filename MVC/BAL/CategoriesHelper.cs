using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC.Models;
using Npgsql;

namespace MVC.BAL
{
    public class CategoriesHelper
    {
        private readonly NpgsqlConnection _conn;

        public CategoriesHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<bool> CreateCategory(Categories category)
        {
            try
            {
                string qry = @"INSERT INTO sg_categories 
                (categoryname, description, isactive, createdat, updatedat)
                VALUES(@categoryname, @description, @isactive, @createdat, @updatedat)";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@categoryname", category.CategoryName);
                    cmd.Parameters.AddWithValue("@description", category.Description);
                    cmd.Parameters.AddWithValue("@isactive", category.IsActive);
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
                Console.WriteLine("Error to create category : " + ex.Message);
            }
            return false;
        }

        public async Task<bool> UpdateCategory(Categories category)
        {
            try
            {
                string qry = @"UPDATE sg_categories 
                               SET categoryname = @categoryname, 
                                   description = @description, 
                                   isactive = @isactive, 
                                   updatedat = @updatedat
                               WHERE categoryid = @categoryid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@categoryid", category.CategoryId);
                    cmd.Parameters.AddWithValue("@categoryname", category.CategoryName);
                    cmd.Parameters.AddWithValue("@description", category.Description);
                    cmd.Parameters.AddWithValue("@isactive", category.IsActive);
                    cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);


                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();
                    {
                        if (reader > 0)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to update category : " + ex.Message);
            }
            return false;
        }

        public async Task<List<Categories>> ReadAllCategories()
        {
            var categories = new List<Categories>();
            try
            {
                string qry = @"SELECT categoryid, categoryname, description, isactive FROM sg_categories";

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
                                CategoryName = reader[1]?.ToString() ?? string.Empty,
                                Description = reader[2] != DBNull.Value ? reader[2].ToString() : string.Empty,
                                IsActive = (bool)reader[3]
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to read categories : " + ex.Message);
            }
            return categories;
        }

        public async Task<bool> DeleteCategory(int categoryId)
        {
            try
            {
                string qry = @"DELETE FROM sg_categories WHERE categoryid = @catid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@catid", categoryId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteNonQueryAsync();
                    {
                        if (reader > 0)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to delete category : " + ex.Message);
            }
            return false;
        }

        public async Task<Categories> ReadCategoryById(int categoryId)
        {
            var category = new Categories();
            try
            {
                string qry = @"SELECT categoryid, categoryname, description, isactive 
                               FROM sg_categories 
                               WHERE categoryid = @catid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@catid", categoryId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            category.CategoryId = (int)reader[0];
                            category.CategoryName = reader[1]?.ToString() ?? string.Empty;
                            category.Description = reader[2] != DBNull.Value ? reader[2].ToString() : string.Empty;
                            category.IsActive = (bool)reader[3];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to get category by ID : " + ex.Message);
            }
            return category;
        }
    }
}