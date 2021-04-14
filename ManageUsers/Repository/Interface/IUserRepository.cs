using ManageUsers.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManageUsers.Repository.Interface
{
    internal interface IUserRepository
    {
        Task ActivateAccountAsync(int userId);
        Task AddUserAddressAsync(int userId, int addressId);
        Task AddUserPicturesAsync(User user, byte[] pictureToAdd);
        Task ChangePasswordAsync(int userId, string password);
        Task<User> CreateAsync(User user);
        Task DeleteAllPicturesAsync(User user);
        Task DeleteAPictureAsync(User user, byte[] pictureToDelete);
        Task DeleteAPictureAsync(User user, int indexOfPicture);
        Task DeleteAsync(int userId);
        Task ForgottenPasswordAsync(int userId, string password);
        Task<string> GetActivationCodeAsync(int userId);
        Task<List<User>> GetAllAddressNullAsync();
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetAllOfAGivenTypeAddressNullAsync(int usertypeId);
        Task<List<User>> GetAllOfAGivenTypeAsync(int usertypeId);
        Task<User> GetByEmailAddressNullAsync(string email);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAddressNullAsync(int userId);
        Task<User> GetByIdAsync(int userId);
        Task<List<byte[]>> GetPicturesOfUserAsync(User user);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<bool> IsUserLoggedIn(int userId);
        Task LoginUserAsync(int userId);
        Task LogoutUserAsync(int userId);
        Task ResendAccountActivationCodeAsync(int userId, string activationCode);
        Task ResetTempPasswordAsync(string password, int userId);
        Task UpdateEmailAsync(User user);
        Task UpdateNameAsync(User user);
        Task UpdateUserTypeAsync(User user, Usertype usertype);
        Task UploadAccountActivationCodeToDbAsync(int userId, string activationCode);
    }
}