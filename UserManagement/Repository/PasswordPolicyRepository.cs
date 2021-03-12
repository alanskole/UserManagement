using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using System.Threading.Tasks;

namespace UserManagement.Repository
{
    internal class PasswordPolicyRepository
    {
        public async Task ChangePasswordPolicyAsync(string connectionString, string policy)
        {
            string sql = @"UPDATE [dbo].[PasswordPolicy] SET Policy=@Policy WHERE Id=" + 1;

            using (var connection = new SqlConnection(connectionString))
            {
                var affectedRows = await connection.ExecuteAsync(sql, new { Policy = policy });
            }
        }

        public async Task<string> GetPasswordPolicyAsync(string connectionString)
        {
            string sql = @"SELECT Policy FROM [dbo].[PasswordPolicy] WHERE Id=" + 1;

            using (var connection = new SqlConnection(connectionString))
            {
                var policy = await connection.QueryFirstAsync<string>(sql);
                
                return policy;
            }
        }
    }
}
