using Dapper;
using ManageUsers.Repository.Interface;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using static ManageUsers.Helper.ConnectionString;

namespace ManageUsers.Repository.Imp
{
    internal class PasswordPolicyRepository : IPasswordPolicyRepository
    {
        private SQLiteConnection _sQLiteConnection;

        public PasswordPolicyRepository()
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
        }

        public async Task ChangePasswordPolicyAsync(int length, bool uppercase, bool number, bool specialCharacter)
        {
            var sql = @"UPDATE PasswordPolicy SET Length=@Length, Uppercase=@Uppercase, Number=@Number, SpecialCharacter=@SpecialCharacter WHERE Id=" + 1;

            await _sQLiteConnection.ExecuteAsync(sql, new { Length = length, Uppercase = uppercase, Number = number, SpecialCharacter = specialCharacter });
        }

        public async Task ChangePasswordPolicyAsync(bool uppercase, bool number, bool specialCharacter)
        {
            var sql = @"UPDATE PasswordPolicy SET Uppercase=@Uppercase, Number=@Number, SpecialCharacter=@SpecialCharacter WHERE Id=" + 1;

            await _sQLiteConnection.ExecuteAsync(sql, new { Uppercase = uppercase, Number = number, SpecialCharacter = specialCharacter });
        }

        public async Task ChangePasswordPolicyAsync(int length)
        {
            var sql = @"UPDATE PasswordPolicy SET Length=@Length WHERE Id=" + 1;

            await _sQLiteConnection.ExecuteAsync(sql, new { Length = length });
        }

        public async Task<Tuple<int, bool, bool, bool>> GetPasswordPolicyAsync()
        {
            var sql = @"SELECT Length as l, Uppercase as u, Number as n, SpecialCharacter as s FROM PasswordPolicy WHERE Id=" + 1;

            var res = await _sQLiteConnection.QueryFirstAsync<dynamic>(sql);

            var length = Convert.ToInt32(res.l);
            var upper = Convert.ToBoolean(res.u);
            var number = Convert.ToBoolean(res.n);
            var special = Convert.ToBoolean(res.s);

            return new Tuple<int, bool, bool, bool>(length, upper, number, special);
        }
    }
}