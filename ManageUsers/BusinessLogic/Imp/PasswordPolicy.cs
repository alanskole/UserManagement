using ManageUsers.BusinessLogic.Interface;
using ManageUsers.UOW.Interface;
using System.Threading.Tasks;

namespace ManageUsers.BusinessLogic.Imp
{
    internal class PasswordPolicy : IPasswordPolicy
    {
        internal IUnitOfWork _unitOfWork;

        internal PasswordPolicy(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
