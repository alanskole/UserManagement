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
        /// Sets the password policy minimum password length.
        /// </summary>
        /// <param name="minimumLength">Sets the minimum amount of characters in the password. Minumum accepted value is 6</param>
        Task SetPolicyAsync(int minimumLength);

        /// <summary>
        /// Sets the password policy with the values of the parameters.
        /// </summary>
        /// <param name="minimumLength">Sets the minimum amount of characters in the password. Minumum accepted value is 6</param>
        /// <param name="upperCaseLetter">True if password must contain at least one upper case letter, false otherwise</param>
        /// <param name="number">True if password must contain at least one number, false otherwise</param>
        /// <param name="specialCharacter">True if password must contain at least one special character, false otherwise</param>
        Task SetPolicyAsync(int minimumLength, bool upperCaseLetter, bool number, bool specialCharacter);
    
        /// <summary>
        /// Sets if the password policy should be set to contain at least one upper case letter, number and/or special character.
        /// </summary>
        /// <param name="upperCaseLetter">True if password must contain at least one upper case letter, false otherwise</param>
        /// <param name="number">True if password must contain at least one number, false otherwise</param>
        /// <param name="specialCharacter">True if password must contain at least one special character, false otherwise</param>
        Task SetPolicyAsync(bool upperCaseLetter, bool number, bool specialCharacter);
    }
}