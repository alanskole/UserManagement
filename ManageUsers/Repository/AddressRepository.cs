using Dapper;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ManageUsers.Model;

namespace ManageUsers.Repository
{
    internal class AddressRepository
    {
        public async Task<Address> CreateAsync(string connectionString, Address address)
        {
            var sql =
                @"INSERT INTO dbo.[Address](Street, Number, Zip, Area, City, Country)
                OUTPUT INSERTED.*
                VALUES(@Street, @Number, @Zip, @Area, @City, @Country);";

            using (var conn = new SqlConnection(connectionString))
            {
                return await conn.QuerySingleAsync<Address>(sql,
                                                new
                                                {
                                                    Street = address.Street,
                                                    Number = address.Number,
                                                    Zip = address.Zip,
                                                    Area = address.Area,
                                                    City = address.City,
                                                    Country = address.Country
                                                });
            }

        }

        public async Task UpdateAsync(string connectionString, int addressId, string street, string number, string zip, string area, string city, string country)
        {
            var sql = @"UPDATE [dbo].[Address] SET Street=@Street, Number=@Number, Zip=@Zip, Area=@Area, City=@City, Country=@Country  WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
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
            var sql = "DELETE [dbo].[Address] WHERE Id = @Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new { Id = addressId });
            }
        }
    }
}
