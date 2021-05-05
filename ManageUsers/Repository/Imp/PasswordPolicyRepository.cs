using Dapper;
using ManageUsers.Repository.Interface;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace ManageUsers.Repository.Imp
{
    internal class PasswordPolicyRepository : IPasswordPolicyRepository
    {
        private SQLiteConnection _sQLiteConnection;

        public PasswordPolicyRepository(string connectionString)
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
        }

        public async Task ChangePasswordPolicyAsync(string policy)
        {
            var sql = @"UPDATE PasswordPolicy SET Policy=@Policy WHERE Id=" + 1;

            await _sQLiteConnection.ExecuteAsync(sql, new { Policy = policy });

        }

        public async Task<string> GetPasswordPolicyAsync()
        {
            var sql = @"SELECT Policy FROM PasswordPolicy WHERE Id=" + 1;

            return await _sQLiteConnection.QueryFirstAsync<string>(sql);
        }
    }
}
