using ManageUsers.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ManageUsers.Interfaces.BusinessLogic
{
    /// <summary>
    /// Interface containing all the methods that are used to manage users in the library.
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// Activate the account of a user account by entering the activation code sent to the email of the user when creating the account.
        /// </summary>
        /// <param name="userId">The ID of the user to activate the account of.</param>
        /// <param name="activationCode">The activation code to activate the account with.</param>
        Task ActivateUserAsync(int userId, string activationCode);

        /// <summary>
        /// Activate the account of a user account by entering the activation code sent to the email of the user when creating the account.
        /// </summary>
        /// <param name="email">The email of the user to activate the account of.</param>
        /// <param name="activationCode">The activation code to activate the account with.</param>
        Task ActivateUserAsync(string email, string activationCode);

        /// <summary>
        /// Activate the account of a user account by entering the activation code sent to the email of the user when creating the account.
        /// </summary>
        /// <param name="user">The user object to activate the account of.</param>
        /// <param name="activationCode">The activation code to activate the account with.</param>
        Task ActivateUserAsync(User user, string activationCode);

        /// <summary>
        /// Adds more usertypes to the already existing ones. By default the only usertypes are Admin and User.
        /// </summary>
        /// <param name="userTypes">A string that contains the names of the new usertypes. Seperate by comma if adding more than one new usertype.</param>
        Task AddMoreUsertypesAsync(params string[] userTypes);

        /// <summary>
        /// Adds a picture to an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to upload the picture for.</param>
        /// <param name="picturePath">The file path of the picture to upload.</param>
        Task AddUserPictureAsync(int userId, string picturePath);

        /// <summary>
        /// Adds a picture to an existing user.
        /// </summary>
        /// <param name="userEmail">The email of the user to upload the picture for.</param>
        /// <param name="picturePath">The file path of the picture to upload.</param>
        Task AddUserPictureAsync(string userEmail, string picturePath);

        /// <summary>
        /// Adds a picture to an existing user.
        /// </summary>
        /// <param name="user">The user object of the user to upload the picture for.</param>
        /// <param name="picturePath">The file path of the picture to upload.</param>
        Task AddUserPictureAsync(User user, string picturePath);

        /// <summary>
        /// Changes the password of an existing user.
        /// </summary>
        /// <param name="email">The email of the user to update the email of.</param>
        /// <param name="old">The current password of the user.</param>
        /// <param name="new1">The new password of the user.</param>
        /// <param name="new2">The new password of the user must be confirmed to be set.</param>
        Task ChangePasswordAsync(string email, string old, string new1, string new2);

        /// <summary>
        /// Creates a new user and inserts it into the database. The address of the user will be null. No usertype set, so the default usertype "User" is assigned to the user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <param name="firstname">The firstname of the user.</param>
        /// <param name="lastname">The lastname of the user.</param>
        Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname);

        /// <summary>
        /// Creates a new user and inserts it into the database. The address of the user will be null.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <param name="firstname">The firstname of the user.</param>
        /// <param name="lastname">The lastname of the user.</param>
        /// <param name="usertype">The usertype/role of the user.</param>
        Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname, string usertype);

        /// <summary>
        /// Creates a new user and inserts it into the database. No usertype set, so the default usertype "User" is assigned to the user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <param name="firstname">The firstname of the user.</param>
        /// <param name="lastname">The lastname of the user.</param>
        /// <param name="streetAdr">The street the user's address.</param>
        /// <param name="buildingNumber">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname, string streetAdr, string buildingNumber, string zip, string area, string city, string country);

        /// <summary>
        /// Creates a new user and inserts it into the database.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <param name="firstname">The firstname of the user.</param>
        /// <param name="lastname">The lastname of the user.</param>
        /// <param name="streetAdr">The street the user's address.</param>
        /// <param name="buildingNumber">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        /// <param name="usertype">The usertype/role of the user.</param>
        Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname, string streetAdr, string buildingNumber, string zip, string area, string city, string country, string usertype);

        /// <summary>
        /// Creates a new user and inserts it into the database.
        /// </summary>
        /// <param name="user">The user object to create and insert into the database.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        Task CreateUserAsync(User user, string passwordConfirmed);

        /// <summary>
        /// Delets all the pictures of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        Task DeleteAllUserPicturesAsync(int userId);

        /// <summary>
        /// Delets all the pictures of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        Task DeleteAllUserPicturesAsync(string userEmail);

        /// <summary>
        /// Delets all the pictures of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        Task DeleteAllUserPicturesAsync(User user);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <param name="imageToRemove">The image object of the picture to be deleted.</param>
        Task DeleteAUserPictureAsync(int userId, Image imageToRemove);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <param name="indexOfPicture">The index picture to delete.</param>
        Task DeleteAUserPictureAsync(int userId, int indexOfPicture);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <param name="picturePath">The file path of the picture to delete.</param>
        Task DeleteAUserPictureAsync(int userId, string picturePath);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <param name="imageToRemove">The image object of the picture to be deleted.</param>
        Task DeleteAUserPictureAsync(string userEmail, Image imageToRemove);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <param name="indexOfPicture">The index picture to delete.</param>
        Task DeleteAUserPictureAsync(string userEmail, int indexOfPicture);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <param name="picturePath">The file path of the picture to delete.</param>
        Task DeleteAUserPictureAsync(string userEmail, string picturePath);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="imageToRemove">The image object of the picture to be deleted.</param> 
        Task DeleteAUserPictureAsync(User user, Image imageToRemove);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index picture to delete.</param>
        Task DeleteAUserPictureAsync(User user, int indexOfPicture);

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to delete.</param>       
        Task DeleteAUserPictureAsync(User user, string picturePath);

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        Task DeleteUserAsync(int userId);

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="email">The email of the user to delete.</param>
        Task DeleteUserAsync(string email);

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="user">The user object of the user to delete.</param>
        Task DeleteUserAsync(User user);

        /// <summary>
        /// Deserialize from csv, json or xml file and add the deserialized values to the database.
        /// </summary>
        /// <param name="filePathToReadFrom">The file path of the file to read and deserlialized objects from. The file extension must be csv, json or xml.</param>
        Task DeSerializeFromFileAsync(string filePathToReadFrom);

        /// <summary>
        /// Deserialize from csv, json or xml formatted string and add the deserialized values to the database.
        /// </summary>
        /// <param name="stringToDeSerialize">The string to deserlialized objects from. The string must be formatted as csv, json or xml.</param>
        Task DeSerializeFromStringAsync(string stringToDeSerialize);

        /// <summary>
        /// Sets the user's password to a new randomly generated temporary password and sends it by email to the user.
        /// </summary>
        /// <param name="userId">The ID of the user to send the new randomly generated password to.</param>
        Task ForgotPasswordAsync(int userId);

        /// <summary>
        /// Sets the user's password to a new randomly generated temporary password and sends it by email to the user.
        /// </summary>
        /// <param name="email">The email of the user to send the new randomly generated password to.</param>
        Task ForgotPasswordAsync(string email);

        /// <summary>
        /// Sets the user's password to a new randomly generated temporary password and sends it by email to the user.
        /// </summary>
        /// <param name="user">The user object of the user to send the new randomly generated password to.</param>
        Task ForgotPasswordAsync(User user);

        /// <summary>
        /// Generates a random string that can be used as a password accepted by the current password policy.
        /// </summary>
        /// <returns>The randomly generated value as a string.</returns>
        /// <param name="length">The length of the randomly generated string.</param>
        Task<string> GenerateRandomPasswordAsync(int length);

        /// <summary>
        /// Fetches all the images of a user.
        /// </summary>
        /// <returns>All the user's pictures in a list of image objects.</returns>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        Task<List<Image>> GetAllUserPictureAsync(int userId);

        /// <summary>
        /// Fetches all the images of a user.
        /// </summary>
        /// <returns>All the user's pictures in a list of image objects.</returns>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        Task<List<Image>> GetAllUserPictureAsync(string userEmail);

        /// <summary>
        /// Fetches all the images of a user.
        /// </summary>
        /// <returns>All the user's pictures in a list of image objects.</returns>
        /// <param name="user">The user object of the user to get the pictures of.</param>
        Task<List<Image>> GetAllUserPictureAsync(User user);

        /// <summary>
        /// Fetch all the users in the database.
        /// </summary>
        /// <returns>All the users in the database in a list with user objects.</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// Fetch all the users in the database of a given usertype.
        /// </summary>
        /// <returns>All the users in the database of the given usertype in a list with user objects.</returns>
        /// <param name="usertype">The usertype of the users to fetch.</param>
        Task<List<User>> GetAllUsersByUsertypeAsync(string usertype);

        /// <summary>
        /// Fetches all the existing usertypes from the database.
        /// </summary>
        /// <returns>All the usertypes in a list with usertype objects.</returns>
        Task<List<Usertype>> GetAllUsertypesAsync();

        /// <summary>
        /// Fetches a user from the database.
        /// </summary>
        /// <returns>A user object of the requested user.</returns>
        /// <param name="userId">The ID of the user to fetch.</param>
        Task<User> GetUserAsync(int userId);

        /// <summary>
        /// Fetches a user from the database.
        /// </summary>
        /// <returns>A user object of the requested user.</returns>
        /// <param name="email">The email of the user to fetch.</param>
        Task<User> GetUserAsync(string email);

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userId">The ID of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index of the picture to fetch.</param>
        Task<Image> GetUserPictureAsync(int userId, int indexOfPicture);

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userId">The ID of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to fetch.</param>
        Task<Image> GetUserPictureAsync(int userId, string picturePath);

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userEmail">The email of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index of the picture to fetch.</param>
        Task<Image> GetUserPictureAsync(string userEmail, int indexOfPicture);

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userEmail">The email of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to fetch.</param>
        Task<Image> GetUserPictureAsync(string userEmail, string picturePath);

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index of the picture to fetch.</param>
        Task<Image> GetUserPictureAsync(User user, int indexOfPicture);

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to fetch.</param>
        Task<Image> GetUserPictureAsync(User user, string picturePath);

        /// <summary>
        /// Checks if the user entered the correct credentials for login.
        /// </summary>
        /// <param name="email">The email of the user to login.</param>
        /// <param name="password">The password of the user to login.</param>
        Task LoginAsync(string email, string password);

        /// <summary>
        /// Checks if the user entered the correct credentials for login and assigns a JWT token to the user if login is successful.
        /// </summary>
        /// <returns>A string conatining the generated JWT token.</returns>
        /// <param name="email">The email of the user to login.</param>
        /// <param name="password">The password of the user to login.</param>
        /// <param name="jwtSecretKey">The string with the secret key that will be used to validate the JWT token.</param>
        Task<string> LoginAsync(string email, string password, string jwtSecretKey);

        /// <summary>
        /// Sends a new activation code to the email of the user that must be used to activate the user's account.
        /// </summary>
        /// <param name="userId">The ID of the user to activate the account of.</param>
        Task ResendAccountActivationCodeAsync(int userId);

        /// <summary>
        /// Sends a new activation code to the email of the user that must be used to activate the user's account.
        /// </summary>
        /// <param name="userEmail">The email of the user to activate the account of.</param>
        Task ResendAccountActivationCodeAsync(string userEmail);

        /// <summary>
        /// Sends a new activation code to the email of the user that must be used to activate the user's account.
        /// </summary>
        /// <param name="user">The user object of the user to send the activation to.</param>
        Task ResendAccountActivationCodeAsync(User user);

        /// <summary>
        /// Serialize one or a list of users to csv string.
        /// </summary>
        /// <returns>Csv string with the serialized object.</returns>
        /// <param name="userObj">The user or list of users to serialize.</param>
        string SerializeToCsvString(object userObj);

        /// <summary>
        /// Serialize a list of users to csv, json or xml file.
        /// </summary>
        /// <param name="listOfUsersToSerialize">The list of user objects to serlialize.</param>
        /// <param name="filePathToWriteTo">The file path of the file to write the serlialized objects to. The file extension must be csv, json or xml.</param>
        void SerializeToFile(List<User> listOfUsersToSerialize, string filePathToWriteTo);

        /// <summary>
        /// Serialize a user to csv, json or xml file.
        /// </summary>
        /// <param name="userToSerialize">The user object of the user to serlialize.</param>
        /// <param name="filePathToWriteTo">The file path of the file to write the serlialized object to. The file extension must be csv, json or xml.</param>
        void SerializeToFile(User userToSerialize, string filePathToWriteTo);

        /// <summary>
        /// Serialize one or a list of users to json string.
        /// </summary>
        /// <returns>Json string with the serialized object.</returns>
        /// <param name="userObj">The user or list of users to serialize.</param>
        string SerializeToJsonString(object userObj);

        /// <summary>
        /// Serialize one or a list of users to xml string.
        /// </summary>
        /// <returns>Xml string with the serialized object.</returns>
        /// <param name="userObj">The user or list of users to serialize.</param>
        string SerializeToXmlString(object userObj);

        /// <summary>
        /// Sets the new password after a user has forgotten their password and entered their temporary password that was sent to their email.
        /// </summary>
        /// <param name="email">The email of the user setting their new password.</param>
        /// <param name="temporaryPassword">The temporary password received on email.</param>
        /// <param name="newPassword">The new password of the user.</param>
        /// <param name="newPasswordConfirmed">The new password of the user must be confirmed to be set.</param>
        Task SetPasswordAfterGettingTemporaryPassword(string email, string temporaryPassword, string newPassword, string newPasswordConfirmed);

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to update the email of.</param>
        /// <param name="updatedEmail">The updated email of the user.</param>
        Task UpdateUserAsync(int userId, string updatedEmail);

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to update the name of.</param>
        /// <param name="updatedFirstname">The updated firstname of the user.</param>
        /// <param name="updatedLastname">The updated lastname of the user.</param>
        Task UpdateUserAsync(int userId, string updatedFirstname, string updatedLastname);

        /// <summary>
        /// Updates the address of an existing user.
        /// </summary>
        /// <param name="email">The email of the existing user to update the address of.</param>
        /// <param name="street">The street the user's address.</param>
        /// <param name="number">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        Task UpdateUserAsync(string email, string street, string number, string zip, string area, string city, string country);

        /// <summary>
        /// Updates the address of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the existing user to update the address of.</param>
        /// <param name="street">The street the user's address.</param>
        /// <param name="number">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        Task UpdateUserAsync(int userId, string street, string number, string zip, string area, string city, string country);

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="originalEmail">The current email of the user to update the email of.</param>
        /// <param name="updatedEmail">The updated email of the user.</param>
        Task UpdateUserAsync(string originalEmail, string updatedEmail);

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="email">The email of the user to update the name of.</param>
        /// <param name="updatedFirstname">The updated firstname of the user.</param>
        /// <param name="updatedLastname">The updated lastname of the user.</param>
        Task UpdateUserAsync(string email, string updatedFirstname, string updatedLastname);

        /// <summary>
        /// Updates firstname, lastname, email and/or address of an existing user.
        /// </summary>
        /// <param name="user">The user object to updates the names of; the names, email and address set in the user object will be the updated values.</param>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Updates the address of an existing user.
        /// </summary>
        /// <param name="user">A user object with the user to update the address of.</param>
        /// <param name="updatedAddress">An address object with the updated address.</param>
        Task UpdateUserAsync(User user, Address updatedAddress);

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="user">The user object to updates the email of.</param>
        /// <param name="updatedEmail">The updated email of the user.</param>
        Task UpdateUserAsync(User user, string updatedEmail);

        /// <summary>
        /// Updates the address of an existing user.
        /// </summary>
        /// <param name="user">A user object with the user to update the address of.</param>
        /// <param name="street">The street the user's address.</param>
        /// <param name="number">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        Task UpdateUserAsync(User user, string street, string number, string zip, string area, string city, string country);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the existing user to update the usertype for.</param>
        /// <param name="updatedUsertype">The name of the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(int userId, string updatedUsertype);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the existing user to update the usertype for.</param>
        /// <param name="updatedUsertype">The usertype object of the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(int userId, Usertype updatedUsertype);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="email">The email of the existing user to update the usertype for.</param>
        /// <param name="updatedUsertype">The name of the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(string email, string updatedUsertype);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="email">The email of the existing user to update the usertype for.</param>
        /// <param name="updatedUsertype">The usertype object of the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(string email, Usertype updatedUsertype);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="user">A user object with the user and the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(User user);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="user">A user object with the user to update the usertype of.</param>
        /// <param name="updatedUsertype">The name of the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(User user, string updatedUsertype);

        /// <summary>
        /// Updates the usertype of an existing user.
        /// </summary>
        /// <param name="user">A user object with the user to update the usertype of.</param>
        /// <param name="updatedUsertype">The usertype object of the updated usertype.</param>
        Task UpdateUsertypeOfUserAsync(User user, Usertype updatedUsertype);
    }
}