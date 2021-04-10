using ManageUsers.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManageUsers.Interfaces.Repository
{
    internal interface IUsertypeRepository
    {
        Task<List<Usertype>> CreateAsync(params string[] userTypes);
        Task<List<Usertype>> GetAllAsync();
        Task<Usertype> GetUsertypeAsync(string usertype);
    }
}