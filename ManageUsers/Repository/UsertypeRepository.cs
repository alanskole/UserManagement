using Dapper;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using ManageUsers.CustomExceptions;
using ManageUsers.Model;

namespace ManageUsers.Repository
{
    internal class UsertypeRepository
    {
        public async Task<List<Usertype>> CreateAsync(string connectionString, params string[] userTypes)
        {
            var sql = @"INSERT INTO Usertype (Type)
                        VALUES(@Type);
                        SELECT last_insert_rowid();";

            var select = @"SELECT * FROM Usertype WHERE Id=@Id";

            var types = new List<Usertype>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                foreach (var userType in userTypes)
                {
                    var exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM Usertype WHERE Type=@Type", new { Type = userType.Trim() });

                    if (!exists)
                    {
                        var usertypeId = await conn.QuerySingleAsync<int>(sql, new { Type = userType.Trim() });
                        
                        types.Add(await conn.QuerySingleAsync<Usertype>(select, new { Id = usertypeId }));
                    }
                }

                return types;
            }
        }

        public async Task<Usertype> GetUsertypeAsync(string connectionString, string usertype)
        {
            var sql = @"SELECT * FROM Usertype WHERE Type=@Type";

            var type = new Usertype();

            var exists = false;

            using (var conn = new SQLiteConnection(connectionString))
            {
                exists = await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM Usertype WHERE Type=@Type", new { Type = usertype });

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
            var sql = @"SELECT * FROM Usertype";

            using (var conn = new SQLiteConnection(connectionString))
            {
                var type = await conn.QueryAsync<Usertype>(sql);

                return type.AsList();
            }
        }
    }
}
