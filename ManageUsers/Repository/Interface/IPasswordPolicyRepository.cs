using System;
using System.Threading.Tasks;

namespace ManageUsers.Repository.Interface
{
    internal interface IPasswordPolicyRepository
    {
        Task ChangePasswordPolicyAsync(int length, bool uppercase, bool number, bool specialCharacter);
        Task ChangePasswordPolicyAsync(bool uppercase, bool number, bool specialCharacter);
        Task ChangePasswordPolicyAsync(int length);
        Task<Tuple<int, bool, bool, bool>> GetPasswordPolicyAsync();
    }
}