using System.Threading.Tasks;
using ManageUsers.UOW;

namespace ManageUsers
{
    /// <summary>
    /// A class containig methods to set the password policy for user passwords.
    /// </summary>
    public class PasswordPolicy
    {
        private UnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the SQLite database.</param>
        public PasswordPolicy(string connectionString)
        {
            _unitOfWork = new UnitOfWork(connectionString);
        }

        /// <summary>
        /// Sets the password policy to minimum 6 characters.
        /// </summary>
        public async Task DefaultPolicyAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("default");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number and letter.
        /// </summary>
        public async Task LevelOneAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("first");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, at least one upper and lower case letter.
        /// </summary>
        public async Task LevelTwoAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("second");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, one letter and one symbol.
        /// </summary>
        public async Task LevelThreeAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("third");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, one symbol, at least one upper and lower case letter.
        /// </summary>
        public async Task LevelFourAsync()
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync("fourth");
        }
    }
}
