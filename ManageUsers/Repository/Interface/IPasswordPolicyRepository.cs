using System;
using System.Threading.Tasks;

namespace ManageUsers.Repository.Interface
{
    internal interface IPasswordPolicyRepository
    {
        Task ChangePasswordPolicyAsync(int length, bool capital, bool number, bool specialCharacter);
        Task<Tuple<int, bool, bool, bool>> GetPasswordPolicyAsync();
    }
}