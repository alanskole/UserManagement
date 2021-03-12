using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Model;
using System.Data.SqlClient;
using Dapper;
using UserManagement.CustomExceptions;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Repository
{
    internal class UserRepository
    {
        public async Task<User> CreateAsync(string connectionString, User user)
        {
            if (user.Address == null)
                return await CreateWithoutAddressAsync(connectionString, user);
            else
            {
                string insertUserSql =
                    @"INSERT INTO dbo.[User](Email, Password, Firstname, Lastname, AddressId, UsertypeId, IsActivated, MustChangePassword)
                    OUTPUT INSERTED.*
                    VALUES(@Email, @Password, @Firstname, @Lastname, @AddressId, @UsertypeId, @IsActivated, @MustChangePassword);";


                using (var con = new SqlConnection(connectionString))
                {
                    var createdUser = await con.QuerySingleAsync<User>(insertUserSql,
                                                new
                                                {
                                                    Email = user.Email,
                                                    Password = user.Password,
                                                    Firstname = user.Firstname,
                                                    Lastname = user.Lastname,
                                                    AddressId = user.Address.Id,
                                                    UsertypeId = user.Usertype.Id,
                                                    IsActivated = false,
                                                    MustChangePassword = false
                                                });

                    return createdUser;
                }
            }
        }

        private async Task<User> CreateWithoutAddressAsync(string connectionString, User user)
        {
            string insertUserSql =
                @"INSERT INTO dbo.[User](Email, Password, Firstname, Lastname, UsertypeId, IsActivated, MustChangePassword)
                OUTPUT INSERTED.*
                VALUES(@Email, @Password, @Firstname, @Lastname, @UsertypeId, @IsActivated, @MustChangePassword);";


            using (var con = new SqlConnection(connectionString))
            {
                var createdUser = await con.QuerySingleAsync<User>(insertUserSql,
                                            new
                                            {
                                                Email = user.Email,
                                                Password = user.Password,
                                                Firstname = user.Firstname,
                                                Lastname = user.Lastname,
                                                UsertypeId = user.Usertype.Id,
                                                IsActivated = false,
                                                MustChangePassword = false
                                            });

                return createdUser;
            }
        }

        public async Task AddUserAddressAsync(string connectionString, int userId, int addressId)
        {
            string sql = @"UPDATE [dbo].[User] SET AddressId=@AddressId WHERE Id=@Id";

            User user = new User();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = userId, AddressId = addressId });
            }

        }

        public async Task UploadAccountActivationCodeToDbAsync(string connectionString, int userId, string activationCode)
        {
            string insertUserSql =
                @"INSERT INTO [dbo].[Verification] (UserId, Code) VALUES(@UserId, @Code);";

            using (var con = new SqlConnection(connectionString))
            {
                await con.ExecuteAsync(insertUserSql, new { UserId = userId, Code = activationCode });
            }
        }

        public async Task UpdateNameAsync(string connectionString, User user)
        {

            string sql = @"UPDATE [dbo].[User] SET Firstname=@Firstname, Lastname=@Lastname WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Firstname = user.Firstname, Lastname = user.Lastname, Id = user.Id });
            }
        }

        public async Task UpdateEmailAsync(string connectionString, User user)
        {

            string sql = @"UPDATE [dbo].[User] SET Email=@Email WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Email = user.Email, Id = user.Id });
            }
        }

        public async Task<bool> IsEmailAvailableAsync(string connectionString, string email)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM [dbo].[User] WHERE Email=@Email", new { Email = email });

                if (exists)
                    return false;

                return true;
            }
        }

        public async Task ChangePasswordAsync(string connectionString, int userId, string password)
        {

            string sql = @"UPDATE [dbo].[User] SET Password=@Password WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Password = password, Id = userId });
            }
        }

        public async Task ForgottenPasswordAsync(string connectionString, int userId, string password)
        {
            string sql = @"UPDATE [dbo].[User] SET Password=@Password, MustChangePassword=@MustChangePassword WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Password = password, MustChangePassword = true, Id = userId });
            }
        }

        public async Task ResetTempPasswordAsync(string connectionString, string password, int userId)
        {
            string sql = @"UPDATE [dbo].[User] SET Password=@Password, MustChangePassword=@MustChangePassword WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Password = password, MustChangePassword = false, Id = userId });
            }
        }

        public async Task ActivateAccountAsync(string connectionString, int userId, string activationCode)
        {

            string sqlActivate = @"UPDATE [dbo].[User] SET IsActivated=@IsActivated WHERE Id=@Id";
            string sqlDeleteFromTable = @"DELETE [dbo].[Verification] WHERE UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                var exists = await connection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM [dbo].[Verification] WHERE UserId=@UserId AND Code=@Code", new { UserId = userId, Code = activationCode });

                if (exists)
                {
                    await connection.ExecuteAsync(sqlActivate, new { IsActivated = true, Id = userId });
                    await connection.ExecuteAsync(sqlDeleteFromTable, new { UserId = userId });
                }
                else
                    throw new ParameterException("Activation code is incorrect!");
            }
        }

        public async Task<User> GetByEmailAsync(string connectionString, string email)
        {
            string sql =
                @"SELECT * FROM [dbo].[User] WHERE Email=@Email";

            using (var conn = new SqlConnection(connectionString))
            {
                var createdUser = await conn.QuerySingleAsync<User>(sql, new { Email = email });

                return await GetByIdAsync(connectionString, createdUser.Id);
            }
        }

        public async Task<User> GetByIdAsync(string connectionString, int userId)
        {
            string sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                a.Id, a.Street, a.Number, a.Zip, a.Area, a.City, a.Country,
                u.UsertypeId, ut.Id, ut.Type
                FROM [dbo].[User] u
                INNER JOIN [dbo].[Address] a ON a.Id = u.AddressId
                INNER JOIN [dbo].[Usertype] ut ON ut.Id = u.UsertypeId
                WHERE u.Id = " + userId;

            using (var conn = new SqlConnection(connectionString))
            {
                var query = (await conn
                    .QueryAsync<User, Address, Usertype, User>(sql,
                        (u, a, ut) =>
                        {
                            u.Address = a;
                            u.Usertype = ut;
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

                return query.ToList()[0];
            }
        }

        public async Task<List<User>> GetAllAsync(string connectionString)
        {
            string sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                a.Id, a.Street, a.Number, a.Zip, a.Area, a.City, a.Country,
                u.UsertypeId, ut.Id, ut.Type
                FROM [dbo].[User] u
                INNER JOIN [dbo].[Address] a ON a.Id = u.AddressId
                INNER JOIN [dbo].[Usertype] ut ON ut.Id = u.UsertypeId";

            using (var conn = new SqlConnection(connectionString))
            {
                var query = (await conn
                    .QueryAsync<User, Address, Usertype, User>(sql,
                        (u, a, ut) =>
                        {
                            u.Address = a;
                            u.Usertype = ut;
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

                return query.ToList();
            }
        }

        public async Task<List<User>> GetAllOfAGivenTypeAsync(string connectionString, int usertypeId)
        {
            string sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                a.Id, a.Street, a.Number, a.Zip, a.Area, a.City, a.Country, 
                u.UsertypeId, ut.Id, ut.Type
                FROM [dbo].[User] u
                INNER JOIN [dbo].[Address] a ON a.Id = u.AddressId
                INNER JOIN [dbo].[Usertype] ut ON ut.Id = u.UsertypeId
                WHERE ut.Id = " + usertypeId;

            using (var conn = new SqlConnection(connectionString))
            {
                var query = (await conn
                    .QueryAsync<User, Address, Usertype, User>(sql,
                        (u, a, ut) =>
                        {
                            u.Address = a;
                            u.Usertype = ut;
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

                return query.ToList();
            }
        }

        public async Task DeleteAsync(string connectionString, int userId)
        {
            var sql = "DELETE [dbo].[User] WHERE Id = @Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, new { Id = userId });
            }
        }

    }
}
