using Dapper;
using ManageUsers.Model;
using ManageUsers.Repository.Interface;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace ManageUsers.Repository.Imp
{
    internal class AddressRepository : IAddressRepository
    {
        private SQLiteConnection _sQLiteConnection;

        public AddressRepository(string connectionString)
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
        }

        public async Task<Address> CreateAsync(Address address)
        {
            var sql =
                @"INSERT INTO Address (Street, Number, Zip, Area, City, Country)
                VALUES(@Street, @Number, @Zip, @Area, @City, @Country); 
                SELECT last_insert_rowid();";

            var adrId = await _sQLiteConnection.QuerySingleAsync<int>(sql,
                                                new
                                                {
                                                    Street = address.Street,
                                                    Number = address.Number,
                                                    Zip = address.Zip,
                                                    Area = address.Area,
                                                    City = address.City,
                                                    Country = address.Country
                                                });

            return await _sQLiteConnection.QuerySingleAsync<Address>(@"SELECT * FROM Address WHERE Id=@Id", new { Id = adrId });
        }

        public async Task UpdateAsync(int addressId, string street, string number, string zip, string area, string city, string country)
        {
            var sql = @"UPDATE Address SET Street=@Street, Number=@Number, Zip=@Zip, Area=@Area, City=@City, Country=@Country  WHERE Id=@Id";

            await _sQLiteConnection.ExecuteAsync(sql, new
            {
                Id = addressId,
                Street = street,
                Number = number,
                Zip = zip,
                Area = area,
                City = city,
                Country = country
            });
        }

        public async Task DeleteAsync(int addressId)
        {
            var sql = "DELETE FROM Address WHERE Id = @Id";

            await _sQLiteConnection.ExecuteAsync(sql, new { Id = addressId });
        }
    }
}
