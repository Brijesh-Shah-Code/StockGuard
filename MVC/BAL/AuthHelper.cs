using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MVC.Models;
using Npgsql;

namespace MVC.BAL
{
    public class AuthHelper
    {
        private readonly NpgsqlConnection _conn;
        private readonly PasswordHasher<Users> _passwordHasher = new PasswordHasher<Users>();

        public AuthHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<Users> Login(vm_Login login)
        {
            var usersData = new Users();
            try
            {
                string qry = @"SELECT userid, fullname, email, passwordhash, role, isactive FROM sg_users WHERE LOWER(email) = LOWER(@mail);";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@mail", login.Email.Trim());

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            usersData.UserId = reader.GetInt32(reader.GetOrdinal("userid"));
                            usersData.FullName = reader.GetString(reader.GetOrdinal("fullname"));
                            usersData.Email = reader.GetString(reader.GetOrdinal("email"));
                            usersData.PasswordHash = reader.GetString(reader.GetOrdinal("passwordhash"));
                            usersData.Role = reader.GetString(reader.GetOrdinal("role"));
                            usersData.IsActive = reader.GetBoolean(reader.GetOrdinal("isactive"));
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                // Verify password (hash)
                var verifyResult = _passwordHasher.VerifyHashedPassword(usersData, usersData.PasswordHash, login.PasswordHash);
                if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    return usersData;
                }
                else if (usersData.PasswordHash == login.PasswordHash)
                {
                    // Auto-convert password to secure hash in DB
                    try
                    {
                        var newHash = _passwordHasher.HashPassword(usersData, login.PasswordHash);
                        string updateQry = @"UPDATE sg_users SET passwordhash = @hash, updatedat = @updatedat WHERE userid = @uid;";
                        using (var updateCmd = new NpgsqlCommand(updateQry, _conn))
                        {
                            updateCmd.Parameters.AddWithValue("@hash", newHash);
                            updateCmd.Parameters.AddWithValue("@updatedat", DateTime.Now);
                            updateCmd.Parameters.AddWithValue("@uid", usersData.UserId);

                            await _conn.CloseAsync();
                            await _conn.OpenAsync();
                            await updateCmd.ExecuteNonQueryAsync();
                            usersData.PasswordHash = newHash;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Warning: Could not auto-upgrade password hash: " + ex.Message);
                    }
                    return usersData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to login : " + ex.Message);
                return null;
            }
        }

        public async Task<int> Register(Users user)
        {
            try
            {
                // Check if email already exists
                string qryCheck = @"SELECT 1 FROM sg_users WHERE LOWER(email) = LOWER(@mail);";

                using (var cmdCheck = new NpgsqlCommand(qryCheck, _conn))
                {
                    cmdCheck.Parameters.AddWithValue("@mail", user.Email.Trim());

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    using (var reader = await cmdCheck.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            return 0; // User with this email already exists
                        }
                    }
                }

                // Hash password securely
                var hashedPassword = _passwordHasher.HashPassword(user, user.PasswordHash);

                string qry = @"INSERT INTO sg_users (fullname, email, passwordhash, role, isactive, createdat, updatedat)
                               VALUES(@fullname, @email, @passwordhash, @role, @isactive, @createdat, @updatedat);";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@fullname", user.FullName.Trim());
                    cmd.Parameters.AddWithValue("@email", user.Email.Trim());
                    cmd.Parameters.AddWithValue("@passwordhash", hashedPassword);
                    cmd.Parameters.AddWithValue("@role", string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role);
                    cmd.Parameters.AddWithValue("@isactive", user.IsActive);
                    cmd.Parameters.AddWithValue("@createdat", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updatedat", DateTime.Now);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error to Register : " + ex.Message);
                return -1;
            }
            return 1;
        }
    }
}