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
        /// Sets the password policy by specifying the policy explicitly.
        /// </summary>
        /// <param name="minimumLength">Sets the minimum amount of characters in the password. Minumum accepted value is 6</param>
        /// <param name="capitalLetter">True if password must contain at least one upper case letter, false otherwise</param>
        /// <param name="number">True if password must contain at least one number, false otherwise</param>
        /// <param name="specialCharacter">True if password must contain at least one special character, false otherwise</param>
        Task SetPolicyAsync(int minimumLength, bool capitalLetter, bool number, bool specialCharacter);
    }
}