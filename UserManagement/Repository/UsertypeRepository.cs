using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using UserManagement.Model;
using UserManagement.CustomExceptions;
using Dapper;
using System.Threading.Tasks;

namespace UserManagement.Repository
{
    internal class UsertypeRepository
    {
        public async Task<List<Usertype>> Create(string connectionString, params string[] userTypes)
        {
            string insertUserSql = @"INSERT INTO [dbo].[Usertype](Type)
                        OUTPUT INSERTED.*
                        VALUES(@Type);";

            List<Usertype> types = new List<Usertype>();

            using (var conn = new SqlConnection(connectionString))
            {
                foreach (var userType in userTypes)
                {
                    var exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM [dbo].[Usertype] WHERE Type=@Type", new { Type = userType.Trim() });

                    if (!exists)
                    {
                        var usertype = await conn.QuerySingleAsync<Usertype>(insertUserSql, new { Type = userType.Trim() });

                        types.Add(usertype);
                    }
                }
            }
            return types;
        }

        public async Task<Usertype> GetUsertype(string connectionString, string usertype)
        {
            string insertUserSql = @"SELECT * FROM [dbo].[Usertype] WHERE Type=@Type";

            Usertype type = new Usertype();

            bool exists = false;

            using (var conn = new SqlConnection(connectionString))
            {
                exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM [dbo].[Usertype] WHERE Type=@Type", new { Type = usertype });

                if (exists)
                {
                    type = await conn.QuerySingleAsync<Usertype>(insertUserSql, new { Type = usertype });
                }
            }

            if (!exists)
                throw new ParameterException("Invalid usertype!");

            return type;
        }

        public async Task<List<Usertype>> GetAll(string connectionString)
        {
            string insertUserSql = @"SELECT * FROM [dbo].[Usertype]";

            using (var conn = new SqlConnection(connectionString))
            {
                var type = await conn.QueryAsync<Usertype>(insertUserSql);

                return type.AsList();
            }
        }
    }
}
