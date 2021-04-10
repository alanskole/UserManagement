using Dapper;
using ManageUsers.CustomExceptions;
using ManageUsers.Interfaces.Repository;
using ManageUsers.Model;
using Newtonsoft.Json;
using Nito.AsyncEx.Synchronous;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace ManageUsers.Repository
{
    internal class UserRepository : IUserRepository
    {
        private SQLiteConnection _sQLiteConnection;

        public UserRepository(SQLiteConnection sQLiteConnection)
        {
            _sQLiteConnection = sQLiteConnection;
        }

        public async Task<User> CreateAsync(User user)
        {
            string picture = null;

            if (user.Picture != null && user.Picture.Count > 0)
                picture = JsonConvert.SerializeObject(user.Picture);
            if (user.Address == null)
                return await CreateWithoutAddressAsync(user, picture);
            else
            {
                var sql =
                    @"INSERT INTO User (Email, Password, Firstname, Lastname, AddressId, UsertypeId, IsActivated, MustChangePassword, Picture)
                    VALUES(@Email, @Password, @Firstname, @Lastname, @AddressId, @UsertypeId, @IsActivated, @MustChangePassword, @Picture);
                    SELECT last_insert_rowid();";

                var userId = await _sQLiteConnection.QuerySingleAsync<int>(sql,
                                            new
                                            {
                                                Email = user.Email,
                                                Password = user.Password,
                                                Firstname = user.Firstname,
                                                Lastname = user.Lastname,
                                                AddressId = user.Address.Id,
                                                UsertypeId = user.Usertype.Id,
                                                IsActivated = user.IsActivated,
                                                MustChangePassword = user.MustChangePassword,
                                                Picture = picture
                                            });

                return await GetByIdAsync(userId);

            }
        }

        private async Task<User> CreateWithoutAddressAsync(User user, string picture)
        {
            var sql =
                @"INSERT INTO User (Email, Password, Firstname, Lastname, UsertypeId, IsActivated, MustChangePassword, Picture)
                VALUES(@Email, @Password, @Firstname, @Lastname, @UsertypeId, @IsActivated, @MustChangePassword, @Picture);
                SELECT last_insert_rowid();";

            var userId = await _sQLiteConnection.QuerySingleAsync<int>(sql,
                                    new
                                    {
                                        Email = user.Email,
                                        Password = user.Password,
                                        Firstname = user.Firstname,
                                        Lastname = user.Lastname,
                                        UsertypeId = user.Usertype.Id,
                                        IsActivated = user.IsActivated,
                                        MustChangePassword = user.MustChangePassword,
                                        Picture = picture
                                    });

            return await GetByIdAddressNullAsync(userId);
        }

        public async Task AddUserAddressAsync(int userId, int addressId)
        {
            var sql = @"UPDATE User SET AddressId=@AddressId WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Id = userId, AddressId = addressId });
        }

        public async Task UploadAccountActivationCodeToDbAsync(int userId, string activationCode)
        {
            var sql = @"INSERT INTO Verification (UserId, Code) VALUES(@UserId, @Code);";

            await _sQLiteConnection.ExecuteAsync(sql, new { UserId = userId, Code = activationCode });
        }

        public async Task UpdateNameAsync(User user)
        {

            var sql = @"UPDATE User SET Firstname=@Firstname, Lastname=@Lastname WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Firstname = user.Firstname, Lastname = user.Lastname, Id = user.Id });
        }

        public async Task UpdateEmailAsync(User user)
        {

            var sql = @"UPDATE User SET Email=@Email WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Email = user.Email, Id = user.Id });
        }

        public async Task UpdateUserTypeAsync(User user, Usertype usertype)
        {

            var sql = @"UPDATE User SET UsertypeId=@UsertypeId WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { UsertypeId = user.Usertype.Id, Id = user.Id });
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var sql = "SELECT COUNT(1) FROM User WHERE Email=@Email";

            var exists = await _sQLiteConnection.ExecuteScalarAsync<bool>(sql, new { Email = email });

            if (exists)
                return false;

            return true;
        }

        public async Task ChangePasswordAsync(int userId, string password)
        {
            var sql = @"UPDATE User SET Password=@Password WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Password = password, Id = userId });
        }

        public async Task ForgottenPasswordAsync(int userId, string password)
        {
            var sql = @"UPDATE User SET Password=@Password, MustChangePassword=@MustChangePassword WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Password = password, MustChangePassword = true, Id = userId });
        }

        public async Task ResetTempPasswordAsync(string password, int userId)
        {
            var sql = @"UPDATE User SET Password=@Password, MustChangePassword=@MustChangePassword WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Password = password, MustChangePassword = false, Id = userId });
        }

        public async Task ActivateAccountAsync(int userId)
        {
            var sqlActivate = @"UPDATE User SET IsActivated=@IsActivated WHERE Id=@Id";
            var sqlDeleteFromTable = @"DELETE FROM Verification WHERE UserId=@UserId";

            await _sQLiteConnection.ExecuteAsync(sqlActivate, new { IsActivated = true, Id = userId });
            await _sQLiteConnection.ExecuteAsync(sqlDeleteFromTable, new { UserId = userId });
        }

        public async Task<string> GetActivationCodeAsync(int userId)
        {
            var sql = @"SELECT CODE FROM Verification WHERE UserId=@UserId";

            return await _sQLiteConnection.QuerySingleAsync<string>(sql, new { UserId = userId });
        }

        public async Task ResendAccountActivationCodeAsync(int userId, string activationCode)
        {
            var sql = @"UPDATE Verification SET Code=@Code WHERE UserId=@UserId;";

            await _sQLiteConnection.ExecuteAsync(sql, new { UserId = userId, Code = activationCode });
        }

        public async Task AddUserPicturesAsync(User user, byte[] pictureToAdd)
        {
            var sql = @"SELECT Picture FROM User WHERE Id=@Id";

            var picture = await _sQLiteConnection.QuerySingleAsync<string>(sql, new { Id = user.Id });

            if (string.IsNullOrEmpty(picture))
                user.Picture = new List<byte[]>();
            else
                user.Picture = JsonConvert.DeserializeObject<List<byte[]>>(picture);

            foreach (var pic in user.Picture)
                if (StructuralComparisons.StructuralEqualityComparer.Equals(pic, pictureToAdd))
                    throw new ParameterException("Picture already exists; user can't have duplicate pictures!");

            user.Picture.Add(pictureToAdd);

            var pictures = JsonConvert.SerializeObject(user.Picture);

            var updateSql = @"UPDATE User SET Picture=@Picture WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(updateSql, new { Id = user.Id, Picture = pictures });
        }

        public async Task<List<byte[]>> GetPicturesOfUserAsync(User user)
        {
            var sql = @"SELECT Picture FROM User WHERE Id=@Id";

            string picture;

            try
            {
                picture = await _sQLiteConnection.QuerySingleAsync<string>(sql, new { Id = user.Id });
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NotFoundException("User");
            }

            if (string.IsNullOrEmpty(picture))
                user.Picture = null;
            else
                user.Picture = JsonConvert.DeserializeObject<List<byte[]>>(picture);

            return user.Picture;
        }

        public async Task DeleteAPictureAsync(User user, byte[] pictureToDelete)
        {
            var allPicturesOfUser = await GetPicturesOfUserAsync(user);

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

            await UpdateUserPicturesAfterDeletingAsync(user, allPicturesOfUser);
        }

        public async Task DeleteAPictureAsync(User user, int indexOfPicture)
        {
            var allPicturesOfUser = await GetPicturesOfUserAsync(user);

            if (allPicturesOfUser == null || allPicturesOfUser.Count == 0)
                throw new NotFoundException("User pictures");

            try
            {
                allPicturesOfUser.RemoveAt(indexOfPicture);
            }
            catch (Exception)
            {
                throw new NotFoundException("Picture");
            }

            await UpdateUserPicturesAfterDeletingAsync(user, allPicturesOfUser);
        }

        private async Task UpdateUserPicturesAfterDeletingAsync(User user, List<byte[]> allPicturesOfUser)
        {
            string picturesToString = null;

            if (allPicturesOfUser.Count > 0)
                picturesToString = JsonConvert.SerializeObject(allPicturesOfUser);

            var sql = @"UPDATE User SET Picture=@Picture WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Id = user.Id, Picture = picturesToString });
        }

        public async Task DeleteAllPicturesAsync(User user)
        {
            var sql = @"UPDATE User SET Picture=@Picture WHERE Id=@Id";

            string picture = null;

            await _sQLiteConnection.ExecuteAsync(sql, new { Id = user.Id, Picture = picture });
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var sql = @"SELECT Id FROM User WHERE Email=@Email";

            try
            {
                var userId = await _sQLiteConnection.QuerySingleAsync<int>(sql, new { Email = email });

                return await GetByIdAsync(userId);
            }
            catch (InvalidOperationException)
            {
                throw new NotFoundException($"User with email {email}");
            }

        }

        public async Task<User> GetByEmailAddressNullAsync(string email)
        {
            var sql = @"SELECT Id FROM User WHERE Email=@Email";

            var userId = await _sQLiteConnection.QuerySingleAsync<int>(sql, new { Email = email });

            return await GetByIdAddressNullAsync(userId);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                a.Id, a.Street, a.Number, a.Zip, a.Area, a.City, a.Country,
                u.UsertypeId, ut.Id, ut.Type
                FROM User u
                INNER JOIN Address a ON a.Id = u.AddressId
                INNER JOIN Usertype ut ON ut.Id = u.UsertypeId
                WHERE u.Id = " + userId;

            var query = (await _sQLiteConnection
                    .QueryAsync<User, Address, Usertype, User>(sql,
                        (u, a, ut) =>
                        {
                            u.Address = a;
                            u.Usertype = ut;
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();


            return query.ToList()[0];
        }

        public async Task<User> GetByIdAddressNullAsync(int userId)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                u.UsertypeId, ut.Id, ut.Type
                FROM User u
                INNER JOIN Usertype ut ON ut.Id = u.UsertypeId
                WHERE u.AddressId IS NULL
                AND u.Id = " + userId;

            try
            {
                var query = (await _sQLiteConnection
                .QueryAsync<User, Usertype, User>(sql,
                    (u, ut) =>
                    {
                        u.Address = null;
                        u.Usertype = ut;
                        u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(u)).WaitAndUnwrapException();
                        return u;
                    }, splitOn: "UsertypeId"))
                .AsQueryable();

                return query.ToList()[0];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new NotFoundException($"User with ID {userId}");
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                a.Id, a.Street, a.Number, a.Zip, a.Area, a.City, a.Country,
                u.UsertypeId, ut.Id, ut.Type
                FROM User u
                INNER JOIN Address a ON a.Id = u.AddressId
                INNER JOIN Usertype ut ON ut.Id = u.UsertypeId";

            var query = (await _sQLiteConnection
                    .QueryAsync<User, Address, Usertype, User>(sql,
                        (u, a, ut) =>
                        {
                            u.Address = a;
                            u.Usertype = ut;
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

            return query.ToList();
        }

        public async Task<List<User>> GetAllAddressNullAsync()
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                u.UsertypeId, ut.Id, ut.Type
                FROM User u
                INNER JOIN Usertype ut ON ut.Id = u.UsertypeId
                WHERE u.AddressId IS NULL";

            var query = (await _sQLiteConnection
                .QueryAsync<User, Usertype, User>(sql,
                    (u, ut) =>
                    {
                        u.Address = null;
                        u.Usertype = ut;
                        u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(u)).WaitAndUnwrapException();
                        return u;
                    }, splitOn: "UsertypeId"))
                .AsQueryable();

            return query.ToList();
        }

        public async Task<List<User>> GetAllOfAGivenTypeAsync(int usertypeId)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                a.Id, a.Street, a.Number, a.Zip, a.Area, a.City, a.Country, 
                u.UsertypeId, ut.Id, ut.Type
                FROM User u
                INNER JOIN Address a ON a.Id = u.AddressId
                INNER JOIN Usertype ut ON ut.Id = u.UsertypeId
                WHERE ut.Id = " + usertypeId;

            var query = (await _sQLiteConnection
                    .QueryAsync<User, Address, Usertype, User>(sql,
                        (u, a, ut) =>
                        {
                            u.Address = a;
                            u.Usertype = ut;
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "AddressId, UsertypeId"))
                    .AsQueryable();

            return query.ToList();
        }

        public async Task<List<User>> GetAllOfAGivenTypeAddressNullAsync(int usertypeId)
        {
            var sql =
                @"SELECT u.Id, u.Email, u.Password, u.Firstname, u.Lastname, u.IsActivated, u.MustChangePassword, u.AddressId,
                u.UsertypeId, ut.Id, ut.Type
                FROM User u
                INNER JOIN Usertype ut ON ut.Id = u.UsertypeId
                WHERE u.AddressId IS NULL
                AND ut.Id = " + usertypeId;

            var query = (await _sQLiteConnection
                    .QueryAsync<User, Usertype, User>(sql,
                        (u, ut) =>
                        {
                            u.Address = null;
                            u.Usertype = ut;
                            u.Picture = Task.Run(async () => await GetPicturesOfUserAsync(u)).WaitAndUnwrapException();
                            return u;
                        }, splitOn: "UsertypeId"))
                    .AsQueryable();

            return query.ToList();
        }

        public async Task DeleteAsync(int userId)
        {
            var sql = "DELETE FROM User WHERE Id = @Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Id = userId });
        }
    }
}