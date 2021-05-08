using Dapper;
using ManageUsers.BusinessLogic.Interface;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using static ManageUsers.Helper.AllCities;
using static ManageUsers.Helper.ConnectionString;

namespace ManageUsers.BusinessLogic.Imp
{
    internal class SetupTables : ISetupTables
    {
        private SQLiteConnection _sQLiteConnection;

        public SetupTables()
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
        }

        public async Task CreateTablesAsync()
        {
            await DoesAddressTableExistAsync();
            await DoesCityTableExistAsync();
            await DoesUsertypeTableExistAsync();
            await DoesUserTableExistAsync();
            await DoesPasswordPolicyTableExistAsync();
            await DoesAccountVerificationCodesTableExistAsync();
            await DoesLoggedOutTableExistAsync();
        }

        private async Task DoesAddressTableExistAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Address", _sQLiteConnection))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await _sQLiteConnection.CloseAsync();
                }
                catch (Exception)
                {
                    await _sQLiteConnection.CloseAsync();
                    await CreateAddressTableAsync();
                }
            }
        }

        private async Task CreateAddressTableAsync()
        {

            await _sQLiteConnection.OpenAsync();
            using (var cmd = new SQLiteCommand("CREATE TABLE Address (" +
            "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
            "[Street] TEXT NOT NULL," +
            "[Number] TEXT NOT NULL," +
            "[Zip] TEXT NOT NULL," +
            "[Area] TEXT NOT NULL," +
            "[City] TEXT NOT NULL," +
            "[Country] TEXT NOT NULL);", _sQLiteConnection))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }

            await _sQLiteConnection.CloseAsync();
        }

        private async Task DoesUsertypeTableExistAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Usertype", _sQLiteConnection))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await _sQLiteConnection.CloseAsync();
                }
                catch (Exception)
                {
                    await _sQLiteConnection.CloseAsync();
                    await CreateUsertypeTableAsync();
                }
            }
        }

        private async Task CreateUsertypeTableAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("CREATE TABLE Usertype (" +
            "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
            "[Type] TEXT NOT NULL);", _sQLiteConnection))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }

            string sql = "INSERT INTO Usertype (Type) Values (@Type);";

            await _sQLiteConnection.ExecuteAsync(sql, new { Type = "Admin" });
            await _sQLiteConnection.ExecuteAsync(sql, new { Type = "User" });

            await _sQLiteConnection.CloseAsync();
        }

        private async Task DoesUserTableExistAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM User", _sQLiteConnection))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await _sQLiteConnection.CloseAsync();
                }
                catch (Exception)
                {
                    await _sQLiteConnection.CloseAsync();
                    await CreateUserTableAsync();
                }
            }
        }

        private async Task CreateUserTableAsync()
        {
            await _sQLiteConnection.OpenAsync();

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
            "FOREIGN KEY ([AddressId]) REFERENCES Address ([Id]) ON DELETE SET NULL," +
            "FOREIGN KEY ([UsertypeId]) REFERENCES Usertype ([Id]) ON DELETE NO ACTION);", _sQLiteConnection))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }

            await _sQLiteConnection.CloseAsync();
        }

        private async Task DoesPasswordPolicyTableExistAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM PasswordPolicy", _sQLiteConnection))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await _sQLiteConnection.CloseAsync();
                }
                catch (Exception)
                {
                    await _sQLiteConnection.CloseAsync();
                    await CreatePasswordPolicyTableAsync();
                }
            }
        }

        private async Task CreatePasswordPolicyTableAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("CREATE TABLE PasswordPolicy (" +
            "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
            "[Length] INTEGER NOT NULL," +
            "[Uppercase] INTEGER NOT NULL," +
            "[Number] INTEGER NOT NULL," +
            "[SpecialCharacter] INTEGER NOT NULL);", _sQLiteConnection))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }

            string sql = "INSERT INTO PasswordPolicy (Length, Uppercase, Number, SpecialCharacter) Values (@Length, @Uppercase, @Number, @SpecialCharacter);";

            await _sQLiteConnection.ExecuteAsync(sql, new { Length = 6, Uppercase = false, Number = false, SpecialCharacter = false });

            await _sQLiteConnection.CloseAsync();
        }

        private async Task DoesAccountVerificationCodesTableExistAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Verification", _sQLiteConnection))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await _sQLiteConnection.CloseAsync();
                }
                catch (Exception)
                {
                    await _sQLiteConnection.CloseAsync();
                    await CreateVerficationTableAsync();
                }
            }
        }

        private async Task CreateVerficationTableAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("CREATE TABLE Verification (" +
            "[Id] INTEGER PRIMARY KEY AUTOINCREMENT," +
            "[UserId] INTEGER NOT NULL," +
            "[Code] TEXT NOT NULL," +
            "FOREIGN KEY ([UserId]) REFERENCES User ([Id]) ON DELETE CASCADE);", _sQLiteConnection))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }

            await _sQLiteConnection.CloseAsync();
        }

        internal async Task DoesCityTableExistAsync()
        {
            var sqlCon = new SQLiteConnection(connectionStringCities);

            await sqlCon.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM City", sqlCon))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await sqlCon.CloseAsync();
                }
                catch (Exception)
                {
                    await sqlCon.CloseAsync();
                    await CreateCityTableAsync();
                }
            }
        }

        private async Task CreateCityTableAsync()
        {
            var sqlCon = new SQLiteConnection(connectionStringCities);

            await sqlCon.OpenAsync();

            using (var cmd = new SQLiteCommand("CREATE TABLE City (" +
            "[CountryId] INTEGER PRIMARY KEY AUTOINCREMENT," +
            "[Country] TEXT NOT NULL," +
            "[Cities] TEXT NOT NULL);", sqlCon))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }
            await sqlCon.CloseAsync();

            await FillTableWithAllTheCities();
        }

        private async Task DoesLoggedOutTableExistAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM LoggedOut", _sQLiteConnection))
            {
                try
                {
                    await cmd.ExecuteScalarAsync();
                    await _sQLiteConnection.CloseAsync();
                }
                catch (Exception)
                {
                    await _sQLiteConnection.CloseAsync();
                    await CreateLoggedOutTableAsync();
                }
            }
        }

        private async Task CreateLoggedOutTableAsync()
        {
            await _sQLiteConnection.OpenAsync();

            using (var cmd = new SQLiteCommand("CREATE TABLE LoggedOut (" +
            "[UserId] INTEGER PRIMARY KEY);", _sQLiteConnection))
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
            }
            await _sQLiteConnection.CloseAsync();
        }
    }
}