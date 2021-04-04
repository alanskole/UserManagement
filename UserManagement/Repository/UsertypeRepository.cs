using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UserManagement.CustomExceptions;
using UserManagement.Model;

namespace UserManagement.Repository
{
    internal class UsertypeRepository
    {
        public async Task<List<Usertype>> CreateAsync(string connectionString, params string[] userTypes)
        {
            var sql = @"INSERT INTO [dbo].[Usertype](Type)
                        OUTPUT INSERTED.*
                        VALUES(@Type);";

            var types = new List<Usertype>();

            using (var conn = new SqlConnection(connectionString))
            {
                foreach (var userType in userTypes)
                {
                    var exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM [dbo].[Usertype] WHERE Type=@Type", new { Type = userType.Trim() });

                    if (!exists)
                    {
                        var usertype = await conn.QuerySingleAsync<Usertype>(sql, new { Type = userType.Trim() });

                        types.Add(usertype);
                    }
                }
            }
            return types;
        }

        public async Task<Usertype> GetUsertypeAsync(string connectionString, string usertype)
        {
            var sql = @"SELECT * FROM [dbo].[Usertype] WHERE Type=@Type";

            var type = new Usertype();

            var exists = false;

            using (var conn = new SqlConnection(connectionString))
            {
                exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM [dbo].[Usertype] WHERE Type=@Type", new { Type = usertype });

                if (exists)
                {
                    type = await conn.QuerySingleAsync<Usertype>(sql, new { Type = usertype });
                }
            }

            if (!exists)
                throw new ParameterException("Invalid usertype!");

            return type;
        }

        public async Task<List<Usertype>> GetAllAsync(string connectionString)
        {
            var sql = @"SELECT * FROM [dbo].[Usertype]";

            using (var conn = new SqlConnection(connectionString))
            {
                var type = await conn.QueryAsync<Usertype>(sql);

                return type.AsList();
            }
        }
    }
}
