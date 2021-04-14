using System.Threading.Tasks;

namespace ManageUsers.BusinessLogic.Interface
{
    /// <summary>
    /// Interface containig methods to set the password policy for user passwords.
    /// </summary>
    public interface IPasswordPolicy
    {
        /// <summary>
        /// Sets the password policy to minimum 6 characters.
        /// </summary>
        Task DefaultPolicyAsync();

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number and letter.
        /// </summary>
        Task LevelFourAsync();

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, at least one upper and lower case letter.
        /// </summary>
        Task LevelOneAsync();

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, one letter and one symbol.
        /// </summary>
        Task LevelThreeAsync();

        /// <summary>
        /// Sets the password policy to minimum 8 characters, at least one number, one symbol, at least one upper and lower case letter.
        /// </summary>
        Task LevelTwoAsync();
    }
}