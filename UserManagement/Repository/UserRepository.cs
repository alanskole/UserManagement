using Dapper;
using Newtonsoft.Json;
using Nito.AsyncEx.Synchronous;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.CustomExceptions;
using UserManagement.Model;

namespace UserManagement.Repository
{
    internal class UserRepository
    {
        public async Task<User> CreateAsync(string connectionString, User user)
        {
            string picture = null;

            if (user.Picture != null && user.Picture.Count > 0)
                picture = JsonConvert.SerializeObject(user.Picture);
            if (user.Address == null)
                return await CreateWithoutAddressAsync(connectionString, user, picture);
            else
            {
                var sql =
                    @"INSERT INTO dbo.[User](Email, Password, Firstname, Lastname, AddressId, UsertypeId, IsActivated, MustChangePassword, Picture)
                    OUTPUT INSERTED.*
                    VALUES(@Email, @Password, @Firstname, @Lastname, @AddressId, @UsertypeId, @IsActivated, @MustChangePassword, @Picture);";


                using (var con = new SqlConnection(connectionString))
                {
                    return await con.QuerySingleAsync<User>(sql,
                                                new
                                                {
                                                    Email = user.Email,
                                                    Password = user.Password,
                                                    Firstname = user.Firstname,
                                                    Lastname = user.Lastname,
                                                    AddressId = user.Address.Id,
                                                    UsertypeId = user.Usertype.Id,
                                                    IsActivated = false,
                                                    MustChangePassword = false,
                                                    Picture = picture
                                                });
                }
            }
        }

        private async Task<User> CreateWithoutAddressAsync(string connectionString, User user, string picture)
        {
            var sql =
                @"INSERT INTO dbo.[User](Email, Password, Firstname, Lastname, UsertypeId, IsActivated, MustChangePassword, Picture)
                OUTPUT INSERTED.*
                VALUES(@Email, @Password, @Firstname, @Lastname, @UsertypeId, @IsActivated, @MustChangePassword, @Picture);";


            using (var con = new SqlConnection(connectionString))
            {
                return await con.QuerySingleAsync<User>(sql,
                                            new
                                            {
                                                Email = user.Email,
                                                Password = user.Password,
                                                Firstname = user.Firstname,
                                                Lastname = user.Lastname,
                                                UsertypeId = user.Usertype.Id,
                                                IsActivated = false,
                                                MustChangePassword = false,
                                                Picture = picture
                                            });
            }
        }

        public async Task AddUserAddressAsync(string connectionString, int userId, int addressId)
        {
            var sql = @"UPDATE [dbo].[User] SET AddressId=@AddressId WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = userId, AddressId = addressId });
            }

        }

        public async Task UploadAccountActivationCodeToDbAsync(string connectionString, int userId, string activationCode)
        {
            var sql = @"INSERT INTO [dbo].[Verification] (UserId, Code) VALUES(@UserId, @Code);";

            using (var con = new SqlConnection(connectionString))
            {
                await con.ExecuteAsync(sql, new { UserId = userId, Code = activationCode });
            }
        }

        public async Task UpdateNameAsync(string connectionString, User user)
        {

            var sql = @"UPDATE [dbo].[User] SET Firstname=@Firstname, Lastname=@Lastname WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Firstname = user.Firstname, Lastname = user.Lastname, Id = user.Id });
            }
        }

        public async Task UpdateEmailAsync(string connectionString, User user)
        {

            var sql = @"UPDATE [dbo].[User] SET Email=@Email WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Email = user.Email, Id = user.Id });
            }
        }

        public async Task<bool> IsEmailAvailableAsync(string connectionString, string email)
        {
            var sql = "SELECT COUNT(1) FROM [dbo].[User] WHERE Email=@Email";

            using (var conn = new SqlConnection(connectionString))
            {
                var exists = await conn.ExecuteScalarAsync<bool>(sql, new { Email = email });

                if (exists)
                    return false;

                return true;
            }
        }

        public async Task ChangePasswordAsync(string connectionString, int userId, string password)
        {
            var sql = @"UPDATE [dbo].[User] SET Password=@Password WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Password = password, Id = userId });
            }
        }

        public async Task ForgottenPasswordAsync(string connectionString, int userId, string password)
        {
            var sql = @"UPDATE [dbo].[User] SET Password=@Password, MustChangePassword=@MustChangePassword WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Password = password, MustChangePassword = true, Id = userId });
            }
        }

        public async Task ResetTempPasswordAsync(string connectionString, string password, int userId)
        {
            var sql = @"UPDATE [dbo].[User] SET Password=@Password, MustChangePassword=@MustChangePassword WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Password = password, MustChangePassword = false, Id = userId });
            }
        }

        public async Task ActivateAccountAsync(string connectionString, int userId)
        {
            var sqlActivate = @"UPDATE [dbo].[User] SET IsActivated=@IsActivated WHERE Id=@Id";
            var sqlDeleteFromTable = @"DELETE [dbo].[Verification] WHERE UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sqlActivate, new { IsActivated = true, Id = userId });
                await connection.ExecuteAsync(sqlDeleteFromTable, new { UserId = userId });
            }
        }

        public async Task<string> GetActivationCodeAsync(string connectionString, int userId)
        {
            var sql = @"SELECT CODE FROM [dbo].[Verification] WHERE UserId=@UserId";

            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QuerySingleAsync<string>(sql, new { UserId = userId });
            }
        }

        public async Task ResendAccountActivationCodeAsync(string connectionString, int userId, string activationCode)
        {
            var sql = @"UPDATE [dbo].[Verification] SET Code=@Code WHERE UserId=@UserId;";

            using (var con = new SqlConnection(connectionString))
            {
                await con.ExecuteAsync(sql, new { UserId = userId, Code = activationCode });
            }
        }

        public async Task AddUserPicturesAsync(string connectionString, User user, byte[] pictureToAdd)
        {
            var sql = @"SELECT Picture FROM [dbo].[User] WHERE Id=@Id";

            using (var conn = new SqlConnection(connectionString))
            {
                var picture = await conn.QuerySingleAsync<string>(sql, new { Id = user.Id });

                if (string.IsNullOrEmpty(picture))
                    user.Picture = new List<byte[]>();
                else
                    user.Picture = JsonConvert.DeserializeObject<List<byte[]>>(picture);

                foreach (var pic in user.Picture)
                    if (StructuralComparisons.StructuralEqualityComparer.Equals(pic, pictureToAdd))
                    throw new ParameterException("Picture already exists; user can't have duplicate pictures!");

                user.Picture.Add(pictureToAdd);

                var pictures = JsonConvert.SerializeObject(user.Picture);

                var updateSql = @"UPDATE [dbo].[User] SET Picture=@Picture WHERE Id=@Id";

                await conn.ExecuteAsync(updateSql, new { Id = user.Id, Picture = pictures });
            }
        }

        public async Task<List<byte[]>> GetPicturesOfUserAsync(string connectionString, User user)
        {
            var sql = @"SELECT Picture FROM [dbo].[User] WHERE Id=@Id";

            using (var conn = new SqlConnection(connectionString))
            {
                var picture = await conn.QuerySingleAsync<string>(sql, new { Id = user.Id });

                if (string.IsNullOrEmpty(picture))
                    user.Picture = null;
                else
                    user.Picture = JsonConvert.DeserializeObject<List<byte[]>>(picture);

                return user.Picture;
            }
        }

        public async Task DeleteAPictureAsync(string connectionString, User user, byte[] pictureToDelete)
        {
            var allPicturesOfUser = await GetPicturesOfUserAsync(connectionString, user);

            if (allPicturesOfUser == null || allPicturesOfUser.Count == 0)
                throw new NotFoundException("User pictures");

            var countBeforeDeleting = allPicturesOfUser.Count;

            for (int i = 0; i < allPicturesOfUser.Count; i++)
                if (StructuralComparisons.StructuralEqualityComparer.Equals(pictureToDelete, allPicturesOfUser[i]))
                {
                    allPicturesOfUser.RemoveAt(i);
                    break;
                }

            if (allPicturesOfUser.Count == countBeforeDeleting)
                throw new NotFoundException("Picture");

            string picturesToString = null;

            if (allPicturesOfUser.Count > 0)
                picturesToString = JsonConvert.SerializeObject(allPicturesOfUser);

            var sql = @"UPDATE [dbo].[User] SET Picture=@Picture WHERE Id=@Id";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(sql, new { Id = user.Id, Picture = picturesToString });
            }
        }

        public async Task DeleteAllPicturesAsync(string connectionString, User user)
        {
            var sql = @"UPDATE [dbo].[User] SET Picture=@Picture WHERE Id=@Id";

            string picture = null;

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(sql, new { Id = user.Id, Picture = picture });
            }
        }

        private async Task SetPicturesOfUserAsync(string connectionString, User user)
        {
            var sql = @"SELECT Picture FROM [dbo].[User] WHERE Id=@Id";

            using (var conn = new SqlConnection(connectionString))
            {
                var picture = await conn.QuerySingleAsync<string>(sql, new { Id = user.Id });

                if (string.IsNullOrEmpty(picture))
                    user.Picture = null;
                else
                    user.Picture = JsonConvert.DeserializeObject<List<byte[]>>(picture);
            }
        }

        public async Task<User> GetByEmailAsync(string connectionString, string email)
        {
            var sql = @"SELECT * FROM [dbo].[User] WHERE Email=@Email";

            using (var conn = new SqlConnection(connectionString))
            {
                var createdUser = await conn.QuerySingleAsync<User>(sql, new { Email = email });

                return await GetByIdAsync(connectionString, createdUser.Id);
            }
        }

        public async Task<User> GetByEmailAddressNullAsync(string connectionString, string email)
        {
            var sql = @"SELECT * FROM [dbo].[User] WHERE Email=@Email";

            using (var conn = new SqlConnection(connectionString))
            {
                var createdUser = await conn.QuerySingleAsync<User>(sql, new { Email = email });

                return await GetByIdAddressNullAsync(connectionString, createdUser.Id);
            }
        }

        public async Task<User> GetByIdAsync(string connectionString, int userId)
        {
            var sql =
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

                await SetPicturesOfUserAsync(connectionString, query.ToList()[0]);

                return query.ToList()[0];
            }
        }

        public async Task<User> GetByIdAddressNullAsync(string connectionString, int userId)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                u.UsertypeId, ut.Id, ut.Type
                FROM [dbo].[User] u
                INNER JOIN [dbo].[Usertype] ut ON ut.Id = u.UsertypeId
                WHERE u.AddressId IS NULL
                AND u.Id = " + userId;

            using (var conn = new SqlConnection(connectionString))
            {
                var query = (await conn
                    .QueryAsync<User, Usertype, User>(sql,
                        (u, ut) =>
                        {
                            u.Address = null;
                            u.Usertype = ut;
                            return u;
                        }, splitOn: "UsertypeId"))
                    .AsQueryable();

                await SetPicturesOfUserAsync(connectionString, query.ToList()[0]);

                return query.ToList()[0];
            }
        }

        public async Task<List<User>> GetAllAsync(string connectionString)
        {
            var sql =
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
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(connectionString, u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

                return query.ToList();
            }
        }

        public async Task<List<User>> GetAllAddressNullAsync(string connectionString)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                u.UsertypeId, ut.Id, ut.Type
                FROM [dbo].[User] u
                INNER JOIN [dbo].[Usertype] ut ON ut.Id = u.UsertypeId
                WHERE u.AddressId IS NULL";

            using (var conn = new SqlConnection(connectionString))
            {
                var query = (await conn
                    .QueryAsync<User, Usertype, User>(sql,
                        (u, ut) =>
                        {
                            u.Address = null;
                            u.Usertype = ut;
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(connectionString, u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "UsertypeId"))
                    .AsQueryable();

                return query.ToList();
            }
        }

        public async Task<List<User>> GetAllOfAGivenTypeAsync(string connectionString, int usertypeId)
        {
            var sql =
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
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(connectionString, u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

                return query.ToList();
            }
        }

        public async Task<List<User>> GetAllOfAGivenTypeAddressNullAsync(string connectionString, int usertypeId)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                u.UsertypeId, ut.Id, ut.Type
                FROM [dbo].[User] u
                INNER JOIN [dbo].[Usertype] ut ON ut.Id = u.UsertypeId
                WHERE u.AddressId IS NULL
                AND ut.Id = " + usertypeId;

            using (var conn = new SqlConnection(connectionString))
            {
                var query = (await conn
                    .QueryAsync<User, Usertype, User>(sql,
                        (u, ut) =>
                        {
                            u.Address = null;
                            u.Usertype = ut;
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(connectionString, u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "UsertypeId"))
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
