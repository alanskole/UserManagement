using System.Threading.Tasks;
using ManageUsers.UOW;

namespace ManageUsers
{
    /// <summary>
    /// A static class containig methods to set the password policy for user passwords.
    /// </summary>
    public static class PasswordPolicy
    {
        private static UnitOfWork _unitOfWork = new UnitOfWork();

        /// <summary>
        /// Sets the password policy to minimum 6 characters.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database.</param>
        public static async Task DefaultPolicyAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "default");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number and letter.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database.</param>
        public static async Task LevelOneAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "first");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, at least one upper and lower case letter.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database.</param>
        public static async Task LevelTwoAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "second");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, one letter and one symbol.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database.</param>
        public static async Task LevelThreeAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "third");
        }

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, one symbol, at least one upper and lower case letter.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database.</param>
        public static async Task LevelFourAsync(string connectionString)
        {
            await _unitOfWork.PasswordPolicyRepository.ChangePasswordPolicyAsync(connectionString, "fourth");
        }
    }
}
