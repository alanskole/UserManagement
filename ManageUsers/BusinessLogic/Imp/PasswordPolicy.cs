using ManageUsers.BusinessLogic.Interface;
using ManageUsers.CustomExceptions;
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
            await _passwordPolicyRepository.ChangePasswordPolicyAsync(6, false, false, false);
        }

        public async Task SetPolicyAsync(int minimumLength, bool capitalLetter, bool number, bool specialCharacter)
        {
            if (minimumLength < 6)
                throw new ParameterException("The password policy minimum password length can't be set to less than 6 characters!");

            await _passwordPolicyRepository.ChangePasswordPolicyAsync(minimumLength, capitalLetter, number, specialCharacter);
        }
    }
}
