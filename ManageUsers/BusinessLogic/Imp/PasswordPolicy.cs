using ManageUsers.BusinessLogic.Interface;
using ManageUsers.Repository.Imp;
using ManageUsers.Repository.Interface;
using System.Threading.Tasks;

namespace ManageUsers.BusinessLogic.Imp
{
    internal class PasswordPolicy : IPasswordPolicy
    {
        private IPasswordPolicyRepository _passwordPolicyRepository;

        internal PasswordPolicy(string connectionString)
        {
            _passwordPolicyRepository = new PasswordPolicyRepository(connectionString);
        }

        public async Task DefaultPolicyAsync()
        {
            await _passwordPolicyRepository.ChangePasswordPolicyAsync("default");
        }

        public async Task LevelOneAsync()
        {
            await _passwordPolicyRepository.ChangePasswordPolicyAsync("first");
        }

        public async Task LevelTwoAsync()
        {
            await _passwordPolicyRepository.ChangePasswordPolicyAsync("second");
        }

        public async Task LevelThreeAsync()
        {
            await _passwordPolicyRepository.ChangePasswordPolicyAsync("third");
        }

        public async Task LevelFourAsync()
        {
            await _passwordPolicyRepository.ChangePasswordPolicyAsync("fourth");
        }
    }
}
