using System.Data.SqlClient;
using Dapper;
using static UserManagement.Helper.AllCities;
using System;
using System.Threading.Tasks;

namespace UserManagement.Database
{
    public static class SetupTables
    {
        public static async Task CreateTablesAsync(string connectionString)
        {
            await DoesAddressTableExistAsync(connectionString);
            await DoesCityTableExistAsync(connectionString);
            await DoesUsertypeTableExistAsync(connectionString);
            await DoesUserTableExistAsync(connectionString);
            await DoesPasswordPolicyTableExistAsync(connectionString);
            await DoesAccountVerificationCodesTableExistAsync(connectionString);
        }

        private static async Task<bool> DoesAddressTableExistAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                bool returnValue;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Address]", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                        returnValue = true;
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateAddressTableAsync(connectionString);
                        returnValue = false;
                    }
                }
                return returnValue;
            }
        }

        private static async Task CreateAddressTableAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("CREATE TABLE [dbo].[Address] (" +
                "[Id] INT IDENTITY (1, 1) NOT NULL," +
                "[Street] NVARCHAR (MAX) NOT NULL," +
                "[Number]  NVARCHAR (MAX) NOT NULL," +
                "[Zip] NVARCHAR (MAX) NOT NULL," +
                "[Area] NVARCHAR (MAX) NOT NULL," +
                "[City] NVARCHAR (MAX) NOT NULL," +
                "[Country] NVARCHAR (MAX) NOT NULL," +
                "CONSTRAINT[PK_Address] PRIMARY KEY CLUSTERED([Id] ASC));", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                await con.CloseAsync();
            }
        }

        private static async Task<bool> DoesUsertypeTableExistAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                bool returnValue;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Usertype]", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                        returnValue = true;
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateUsertypeTableAsync(connectionString);
                        returnValue = false;
                    }
                }

                return returnValue;
            }
        }

        private static async Task CreateUsertypeTableAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CREATE TABLE [dbo].[Usertype] (" +
                "[Id] INT IDENTITY (1, 1) NOT NULL," +
                "[Type]    NVARCHAR(MAX) NOT NULL," +
                "CONSTRAINT[PK_Usertype] PRIMARY KEY CLUSTERED([Id] ASC));", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                string sql = "INSERT INTO [dbo].[Usertype] (Type) Values (@Type);";

                await con.ExecuteAsync(sql, new { Type = "Admin" });
                await con.ExecuteAsync(sql, new { Type = "User" });

                await con.CloseAsync();
            }
        }

        private static async Task<bool> DoesUserTableExistAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                bool returnValue;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[User]", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                        returnValue = true;
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateUserTableAsync(connectionString);
                        returnValue = false;
                    }
                }
                return returnValue;
            }
        }

        private static async Task CreateUserTableAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CREATE TABLE [dbo].[User] (" +
                "[Id] INT IDENTITY (1, 1) NOT NULL," +
                "[Email] NVARCHAR (MAX) NOT NULL," +
                "[Password] NVARCHAR (MAX) NOT NULL," +
                "[Firstname] NVARCHAR (MAX) NOT NULL," +
                "[Lastname] NVARCHAR (MAX) NOT NULL," +
                "[AddressId] INT NULL," +
                "[UsertypeId] INT NOT NULL," +
                "[IsActivated] BIT NOT NULL," +
                "[MustChangePassword] BIT NOT NULL," +
                "CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)," +
                "CONSTRAINT [FK_User_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id]) ON DELETE NO ACTION," +
                "CONSTRAINT [FK_User_Usertype_UsertypeId] FOREIGN KEY ([UsertypeId]) REFERENCES [dbo].[Usertype] ([Id]) ON DELETE NO ACTION);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                await con.CloseAsync();
            }
        }

        private static async Task<bool> DoesPasswordPolicyTableExistAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                bool returnValue;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[PasswordPolicy]", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                        returnValue = true;
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreatePasswordPolicyTableAsync(connectionString);
                        returnValue = false;
                    }
                }

                return returnValue;
            }
        }

        private static async Task CreatePasswordPolicyTableAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CREATE TABLE [dbo].[PasswordPolicy] (" +
                "[Id] INT IDENTITY (1, 1) NOT NULL," +
                "[Policy]    NVARCHAR(MAX) NOT NULL," +
                "CONSTRAINT[PK_PasswordPolicy] PRIMARY KEY CLUSTERED([Id] ASC));", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                string sql = "INSERT INTO [dbo].[PasswordPolicy] (Policy) Values (@Policy);";

                await con.ExecuteAsync(sql, new { Policy = "default" });

                await con.CloseAsync();
            }
        }

        private static async Task<bool> DoesAccountVerificationCodesTableExistAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                bool returnValue;

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Verification]", con))
                {
                    try
                    {
                        await cmd.ExecuteScalarAsync();
                        await con.CloseAsync();
                        returnValue = true;
                    }
                    catch (Exception)
                    {
                        await con.CloseAsync();
                        await CreateVerficationTableAsync(connectionString);
                        returnValue = false;
                    }
                }

                return returnValue;
            }
        }

        private static async Task CreateVerficationTableAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CREATE TABLE [dbo].[Verification] (" +
                "[Id] INT IDENTITY (1, 1) NOT NULL," +
                "[UserId] INT NOT NULL," +
                "[Code] NVARCHAR (MAX) NOT NULL," +
                "CONSTRAINT [PK_Verification] PRIMARY KEY CLUSTERED ([Id] ASC)," +
                "CONSTRAINT [FK_Verification_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]) ON DELETE CASCADE);", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                    await cmd.DisposeAsync();
                }

                await con.CloseAsync();
            }
        }

        internal static async Task DoesCityTableExistAsync(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[City]", con))
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
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CREATE TABLE[dbo].[City] (" +
                "[CountryId] INT NOT NULL," +
                "[Country]   NVARCHAR (MAX) NOT NULL," +
                "[Cities]    NVARCHAR(MAX) NOT NULL," +
                "CONSTRAINT[PK_City] PRIMARY KEY CLUSTERED([CountryId] ASC));", con))
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
