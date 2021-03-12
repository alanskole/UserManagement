using System.Data.SqlClient;
using Dapper;
using static UserManagement.Helper.AllCities;
using System;
using System.Threading.Tasks;

namespace UserManagement.Database
{
    public static class SetupTables
    {
        public static async Task CreateTables(string connectionString)
        {
            await DoesAddressTableExist(connectionString);
            await DoesCityTableExist(connectionString);
            await DoesUsertypeTableExist(connectionString);
            await DoesUserTableExist(connectionString);
            await DoesPasswordPolicyTableExist(connectionString);
            await DoesAccountVerificationCodesTableExist(connectionString);
        }

        private static async Task<bool> DoesAddressTableExist(string connectionString)
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
                        await CreateAddressTable(connectionString);
                        returnValue = false;
                    }
                }
                return returnValue;
            }
        }

        private static async Task CreateAddressTable(string connectionString)
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

        private static async Task<bool> DoesUsertypeTableExist(string connectionString)
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
                        await CreateUsertypeTable(connectionString);
                        returnValue = false;
                    }
                }

                return returnValue;
            }
        }

        private static async Task CreateUsertypeTable(string connectionString)
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

        private static async Task<bool> DoesUserTableExist(string connectionString)
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
                        await CreateUserTable(connectionString);
                        returnValue = false;
                    }
                }
                return returnValue;
            }
        }

        private static async Task CreateUserTable(string connectionString)
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

        private static async Task<bool> DoesPasswordPolicyTableExist(string connectionString)
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
                        await CreatePasswordPolicyTable(connectionString);
                        returnValue = false;
                    }
                }

                return returnValue;
            }
        }

        private static async Task CreatePasswordPolicyTable(string connectionString)
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

                await con.ExecuteAsync(sql, new { Policy = "none" });

                await con.CloseAsync();
            }
        }

        private static async Task<bool> DoesAccountVerificationCodesTableExist(string connectionString)
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
                        await CreateVerficationTable(connectionString);
                        returnValue = false;
                    }
                }

                return returnValue;
            }
        }

        private static async Task CreateVerficationTable(string connectionString)
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

        internal static async Task DoesCityTableExist(string connectionString)
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
                        await CreateCityTable(connectionString);
                    }
                }
            }
        }

        private static async Task CreateCityTable(string connectionString)
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
