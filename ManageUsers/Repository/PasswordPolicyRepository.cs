using Dapper;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace ManageUsers.Repository
{
    internal class PasswordPolicyRepository
    {
        public async Task ChangePasswordPolicyAsync(string connectionString, string policy)
        {
            var sql = @"UPDATE PasswordPolicy SET Policy=@Policy WHERE Id=" + 1;

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Policy = policy });
            }
        }

        public async Task<string> GetPasswordPolicyAsync(string connectionString)
        {
            var sql = @"SELECT Policy FROM PasswordPolicy WHERE Id=" + 1;

            using (var connection = new SQLiteConnection(connectionString))
            {
                return await connection.QueryFirstAsync<string>(sql);
            }
        }
    }
}
