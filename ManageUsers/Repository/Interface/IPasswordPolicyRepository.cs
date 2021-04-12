using System.Threading.Tasks;

namespace ManageUsers.Repository.Interface
{
    internal interface IPasswordPolicyRepository
    {
        Task ChangePasswordPolicyAsync(string policy);
        Task<string> GetPasswordPolicyAsync();
    }
}