using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.Model;
using Dapper;
using System.Data.SqlClient;
using static UserManagement.Database.SetupTables;
using System.Threading.Tasks;

namespace UserManagement.Repository
{
    internal class AddressRepository
    {
        public async Task<Address> CreateAsync(string connectionString, Address address)
        {
            Address createdAddress =  new Address();

            string insertUserSql = 
                @"INSERT INTO dbo.[Address](Street, Number, Zip, Area, City, Country)
                OUTPUT INSERTED.*
                VALUES(@Street, @Number, @Zip, @Area, @City, @Country);";

            using (var conn = new SqlConnection(connectionString))
            {
                createdAddress = await conn.QuerySingleAsync<Address>(insertUserSql,
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

            return createdAddress;
        }

        public async Task UpdateAsync(string connectionString, int addressId, string street, string number, string zip, string area, string city, string country)
        {
            string sql = @"UPDATE [dbo].[Address] SET Street=@Street, Number=@Number, Zip=@Zip, Area=@Area, City=@City, Country=@Country  WHERE Id=@Id";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, new {
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
                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, new { Id = addressId });
            }
        }
    }
}
