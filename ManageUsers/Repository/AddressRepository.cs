using Dapper;
using System.Data.SQLite;
using System.Threading.Tasks;
using ManageUsers.Model;

namespace ManageUsers.Repository
{
    internal class AddressRepository
    {
        public async Task<Address> CreateAsync(string connectionString, Address address)
        {
            var sql =
                @"INSERT INTO Address (Street, Number, Zip, Area, City, Country)
                VALUES(@Street, @Number, @Zip, @Area, @City, @Country); 
                SELECT last_insert_rowid();";

            using (var conn = new SQLiteConnection(connectionString))
            {
                var adrId = await conn.QuerySingleAsync<int>(sql,
                                                new
                                                {
                                                    Street = address.Street,
                                                    Number = address.Number,
                                                    Zip = address.Zip,
                                                    Area = address.Area,
                                                    City = address.City,
                                                    Country = address.Country
                                                });

                return await conn.QuerySingleAsync<Address>(@"SELECT * FROM Address WHERE Id=@Id", new { Id = adrId });
            }
        }

        public async Task UpdateAsync(string connectionString, int addressId, string street, string number, string zip, string area, string city, string country)
        {
            var sql = @"UPDATE Address SET Street=@Street, Number=@Number, Zip=@Zip, Area=@Area, City=@City, Country=@Country  WHERE Id=@Id";

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new
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
        }

        public async Task DeleteAsync(string connectionString, int addressId)
        {
            var sql = "DELETE FROM Address WHERE Id = @Id";

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = addressId });
            }
        }
    }
}
