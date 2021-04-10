using System.Threading.Tasks;

namespace ManageUsers.Interfaces.Repository
{
    internal interface IPasswordPolicyRepository
    {
        Task ChangePasswordPolicyAsync(string policy);
        Task<string> GetPasswordPolicyAsync();
    }
}