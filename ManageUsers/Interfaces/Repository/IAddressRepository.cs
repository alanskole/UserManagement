using ManageUsers.Model;
using System.Threading.Tasks;

namespace ManageUsers.Interfaces.Repository
{
    internal interface IAddressRepository
    {
        Task<Address> CreateAsync(Address address);
        Task DeleteAsync(int addressId);
        Task UpdateAsync(int addressId, string street, string number, string zip, string area, string city, string country);
    }
}