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
        private SQLiteConnection _sQLiteConnection;

        public UsertypeRepository(SQLiteConnection sQLiteConnection)
        {
            _sQLiteConnection = sQLiteConnection;
        }

        public async Task<List<Usertype>> CreateAsync(params string[] userTypes)
        {
            var sql = @"INSERT INTO Usertype (Type)
                        VALUES(@Type);
                        SELECT last_insert_rowid();";

            var select = @"SELECT * FROM Usertype WHERE Id=@Id";

            var types = new List<Usertype>();

            foreach (var userType in userTypes)
            {
                var exists = await _sQLiteConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM Usertype WHERE Type=@Type", new { Type = userType.Trim() });

                if (!exists)
                {
                    var usertypeId = await _sQLiteConnection.QuerySingleAsync<int>(sql, new { Type = userType.Trim() });

                    types.Add(await _sQLiteConnection.QuerySingleAsync<Usertype>(select, new { Id = usertypeId }));
                }
            }

            return types;
        }

        public async Task<Usertype> GetUsertypeAsync(string usertype)
        {
            var sql = @"SELECT * FROM Usertype WHERE Type=@Type";

            var type = new Usertype();

            var exists = await _sQLiteConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM Usertype WHERE Type=@Type", new { Type = usertype });

            if (exists)
            {
                type = await _sQLiteConnection.QuerySingleAsync<Usertype>(sql, new { Type = usertype });
            }

            if (!exists)
                throw new ParameterException("Invalid usertype!");

            return type;
        }

        public async Task<List<Usertype>> GetAllAsync()
        {
            var sql = @"SELECT * FROM Usertype";

            var type = await _sQLiteConnection.QueryAsync<Usertype>(sql);

            return type.AsList();
        }
    }
}
