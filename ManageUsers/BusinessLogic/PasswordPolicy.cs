using ManageUsers.Interfaces.BusinessLogic;
using ManageUsers.Interfaces.UnitOfWork;
using ManageUsers.UOW;
using System.Threading.Tasks;

namespace ManageUsers.BusinessLogic
{
    internal class PasswordPolicy : IPasswordPolicy
    {
        internal IUnitOfWork _unitOfWork;

        internal PasswordPolicy(string connectionString)
        {
            _unitOfWork = new UnitOfWork(connectionString);
        }

        public async Task DefaultPolicyAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("default");
        }

        public async Task LevelOneAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("first");
        }

        public async Task LevelTwoAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("second");
        }

        public async Task LevelThreeAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("third");
        }

        public async Task LevelFourAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("fourth");
        }
    }
}
