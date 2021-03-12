using System.Threading.Tasks;
using UserManagement.UOW;

namespace UserManagement
{
    public class PasswordPolicyManager
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public async Task DefaultPolicyAtLeast6CharactersAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "default");
        }

        public async Task MinimumLength8AtLeastOneNumberAndLetterAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "first");
        }

        public async Task MinimumLength8AtLeastOneNumberAndLetterAndOneUpperAndLowerCaseAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "second");
        }

        public async Task MinimumLength8AtLeastOneNumberLetterAndSymbolAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "third");
        }

        public async Task MinimumLength8AtLeastOneNumberLetterAndSymbolAndOneUpperAndLowerCaseAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "fourth");
        }
    }
}
