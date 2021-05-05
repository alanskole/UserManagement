using Dapper;
using ManageUsers.Repository.Interface;
using System;
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

        public async Task ChangePasswordPolicyAsync(int length, bool capital, bool number, bool specialCharacter)
        {
            var sql = @"UPDATE PasswordPolicy SET Length=@Length, Capital=@Capital, Number=@Number, SpecialCharacter=@SpecialCharacter WHERE Id=" + 1;

            await _sQLiteConnection.ExecuteAsync(sql, new { Length = length, Capital = capital, Number = number, SpecialCharacter = specialCharacter });

        }

        public async Task<Tuple<int, bool, bool, bool>> GetPasswordPolicyAsync()
        {
            var sql = @"SELECT Length as l, Capital as c, Number as n, SpecialCharacter as s FROM PasswordPolicy WHERE Id=" + 1;

            var res = await _sQLiteConnection.QueryFirstAsync<dynamic>(sql);

            var length = Convert.ToInt32(res.l);
            var capital = Convert.ToBoolean(res.c);
            var number = Convert.ToBoolean(res.n);
            var special = Convert.ToBoolean(res.s);

            return new Tuple<int, bool, bool, bool>(length, capital, number, special);
        }
    }
}
