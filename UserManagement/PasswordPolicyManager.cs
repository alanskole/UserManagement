using System.Threading.Tasks;
using UserManagement.UOW;

namespace UserManagement
{
    public class PasswordPolicyManager
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public async Task Default_Policy_At_Least_6_Characters(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicy(connectionString, "none");
        }

        public async Task Minimum_Length_8_At_Least_One_Number_And_Letter(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicy(connectionString, "first");
        }

        public async Task Minimum_Length_8_At_Least_One_Number_And_Letter_And_One_Upper_And_Lower_Case(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicy(connectionString, "second");
        }

        public async Task Minimum_Length_8_At_Least_One_Number_Letter_And_Symbol(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicy(connectionString, "third");
        }

        public async Task Minimum_Length_8_At_Least_One_Number_Letter_And_Symbol_And_One_Upper_And_Lower_Case(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicy(connectionString, "fourth");
        }
    }
}
