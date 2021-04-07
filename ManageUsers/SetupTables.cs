using Dapper;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using static ManageUsers.Helper.AllCities;

namespace ManageUsers
{
    /// <summary>
    /// A static class used to setup the tables of your database.
    /// </summary>
    public static class SetupTables
    {
        /// <summary>
        /// This method must be used to automatically create all the necessary tables for the database 
        /// before any other method in the library can be used.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database.</param>
        public static async Task CreateTablesAsync(string connectionString)
        {
            await DoesAddressTableExistAsync(connectionString);
            await DoesCityTableExistAsync(connectionString);
            await DoesUsertypeTableExistAsync(connectionString);
            await DoesUserTableExistAsync(connectionString);
            await DoesPasswordPolicyTableExistAsync(connectionString);
            await DoesAccountVerificationCodesTableExistAsync(connectionString);
        }

        private static async Task DoesAddressTableExistAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Address", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateAddressTableAsync(connectionString);
                    }
                }
            }
        }

        private static async Task CreateAddressTableAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();
                using (var cmd = new SQLiteCommand("CREATE TABLE Address (" +
                "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[Street] TEXT NOT NULL," +
                "[Number] TEXT NOT NULL," +
                "[Zip] TEXT NOT NULL," +
                "[Area] TEXT NOT NULL," +
                "[City] TEXT NOT NULL," +
                "[Country] TEXT NOT NULL);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                await con.CloseAsync();
            }
        }

        private static async Task DoesUsertypeTableExistAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Usertype", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateUsertypeTableAsync(connectionString);
                    }
                }
            }
        }

        private static async Task CreateUsertypeTableAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("CREATE TABLE Usertype (" +
                "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[Type] TEXT NOT NULL);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                string sql = "INSERT INTO Usertype (Type) Values (@Type);";

                await con.ExecuteAsync(sql, new { Type = "Admin" });
                await con.ExecuteAsync(sql, new { Type = "User" });

                await con.CloseAsync();
            }
        }

        private static async Task DoesUserTableExistAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM User", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateUserTableAsync(connectionString);
                    }
                }
            }
        }

        private static async Task CreateUserTableAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("CREATE TABLE User (" +
                "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[Email] TEXT NOT NULL," +
                "[Password] TEXT NOT NULL," +
                "[Firstname] TEXT NOT NULL," +
                "[Lastname] TEXT NOT NULL," +
                "[AddressId] INTEGER," +
                "[UsertypeId] INTEGER NOT NULL," +
                "[IsActivated] INTEGER NOT NULL," +
                "[MustChangePassword] INTEGER NOT NULL," +
                "[Picture] TEXT NULL," +
                "FOREIGN KEY ([AddressId]) REFERENCES Address ([Id]) ON DELETE NO ACTION," +
                "FOREIGN KEY ([UsertypeId]) REFERENCES Usertype ([Id]) ON DELETE NO ACTION);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                await con.CloseAsync();
            }
        }

        private static async Task DoesPasswordPolicyTableExistAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM PasswordPolicy", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreatePasswordPolicyTableAsync(connectionString);
                    }
                }
            }
        }

        private static async Task CreatePasswordPolicyTableAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("CREATE TABLE PasswordPolicy (" +
                "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[Policy] TEXT NOT NULL);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                string sql = "INSERT INTO PasswordPolicy (Policy) Values (@Policy);";

                await con.ExecuteAsync(sql, new { Policy = "default" });

                await con.CloseAsync();
            }
        }

        private static async Task DoesAccountVerificationCodesTableExistAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Verification", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateVerficationTableAsync(connectionString);
                    }
                }
            }
        }

        private static async Task CreateVerficationTableAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("CREATE TABLE Verification (" +
                "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[UserId] INTEGER NOT NULL," +
                "[Code] TEXT NOT NULL," +
                "FOREIGN KEY ([UserId]) REFERENCES User ([Id]) ON DELETE CASCADE);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                await con.CloseAsync();
            }
        }

        internal static async Task DoesCityTableExistAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM City", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateCityTableAsync(connectionString);
                    }
                }
            }
        }

        private static async Task CreateCityTableAsync(string connectionString)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SQLiteCommand("CREATE TABLE City (" +
                "[CountryId] INTEGER PRIMARY KEY AUTOINCREMENT," +
                "[Country] TEXT NOT NULL," +
                "[Cities] TEXT NOT NULL);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }
                await con.CloseAsync();

                await FillTableWithAllTheCities(connectionString);
            }
        }
    }
}
