using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using UserManagement.CustomExceptions;
using UserManagement.Model;
using UserManagement.UOW;
using static UserManagement.Helper.AllCities;
using static UserManagement.Helper.Email;
using static UserManagement.Helper.PasswordHelper;
using static UserManagement.Helper.RegexChecker;

namespace UserManagement
{
    public class UserManager
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public async Task CreateUserAsync(string connectionString, User user, string passwordConfirmed)
        {
            if (user.Password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            NullOrEmptyChecker(user);

            await ValidateEmailAsync(connectionString, user.Email);

            await ValidatePasswordAsync(connectionString, user.Password);

            if (user.Address != null)
                user.Address = await CreateAddressAsync(connectionString, user.Address.Street, user.Address.Number, user.Address.Zip, user.Address.Area, user.Address.City, user.Address.Country);

            user.Usertype = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, user.Usertype.Type);

            user.Password = HashThePassword(user.Password, null, false);

            ValidateName(user.Firstname, user.Lastname);

            var createdUser = await _unitOfWork.UserRepository.CreateAsync(connectionString, user);

            if (createdUser.Id == 0)
                throw new FailedToCreateException("User");

            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            await _unitOfWork.UserRepository.UploadAccountActivationCodeToDbAsync(connectionString, createdUser.Id, accountActivationCodeHashed);

            EmailSender("aintbnb@outlook.com", "juStaRandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");
        }

        public async Task CreateUserAsync(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname)
        {
            await CreateUserAsync(connectionString, email, password, passwordConfirmed, firstname, lastname, "User");
        }

        public async Task CreateUserAsync(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname, string usertype)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype(usertype) };

            await CreateUserAsync(connectionString, user, passwordConfirmed);
        }

        public async Task CreateUserAsync(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            await CreateUserAsync(connectionString, email, password, passwordConfirmed, firstname, lastname, streetAdr, buildingNumber, zip, area, city, country, "User");
        }

        public async Task CreateUserAsync(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country, string usertype)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype(usertype) };

            user.Address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await CreateUserAsync(connectionString, user, passwordConfirmed);
        }

        private static void NullOrEmptyChecker(User user)
        {
            try
            {
                user.Email = user.Email.Trim();
                user.Firstname = user.Firstname.Trim();
                user.Lastname = user.Lastname.Trim();
            }
            catch (NullReferenceException)
            {
                throw new ParameterException("Parameters", "null");
            }


            if (user.Email == "" || user.Firstname == "" || user.Lastname == "")
                throw new ParameterException("Parameters", "empty");
        }

        public static void NullOrEmptyChecker(string firstname, string lastname)
        {
            try
            {
                firstname = firstname.Trim();
                lastname = lastname.Trim();
            }
            catch (NullReferenceException)
            {
                throw new ParameterException("Parameters", "null");
            }

            if (firstname == "" || lastname == "")
                throw new ParameterException("Parameters", "empty");
        }

        private static void ValidateName(string firstname, string lastname)
        {
            if (!onlyLettersOneSpaceOrDash.IsMatch(firstname))
                throw new ParameterException("Firsname", "containing any other than letters and one space or dash between names");
            if (!onlyLettersOneSpaceOrDash.IsMatch(lastname))
                throw new ParameterException("Lastname", "containing any other than letters and one space or dash between names");
        }

        private async Task ValidateEmailAsync(string connectionString, string email)
        {
            if (!isEmailValidFormat.IsMatch(email))
                throw new ParameterException("Email not formatted correctly!");

            if (!await _unitOfWork.UserRepository.IsEmailAvailableAsync(connectionString, email))
                throw new ParameterException("Email is not available!");
        }

        private async Task ValidatePasswordAsync(string connectionString, string password)
        {
            var policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicyAsync(connectionString);

            if (password == null || password.Contains(" "))
                throw new ParameterException("Password can't contain space!");

            if (policy == "default")
            {
                if (password.Length < 6)
                    throw new ParameterException("Password", "shorter than 6");
            }
            else if (policy == "first")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetter.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number and letter, special characters are optional!");
            }
            else if (policy == "second")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetterOneUpperAndLowerCase.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number and letter, with at least one uppercase and lowercase letter, special characters are optional!");
            }
            else if (policy == "third")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacter.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number, letter and special character!");
            }
            else if (policy == "fourth")
                throw new ParameterException("Password must be at least 8 characters long with at least one number, letter and special character, with at least one uppercase and lowercase letter!");
        }

        private async Task<Address> CreateAddressAsync(string connectionString, string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddressAsync(connectionString, address);

            var createdAddress = await _unitOfWork.AddressRepository.CreateAsync(connectionString, address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            return createdAddress;
        }

        public async Task AddAddressToExisitingUserAsync(string connectionString, User user)
        {
            await AddAddressToExisitingUserAsync(connectionString, user, user.Address);
        }

        public async Task AddAddressToExisitingUserAsync(string connectionString, User user, Address address)
        {
            await ValidateAddressAsync(connectionString, address);

            var createdAddress = await _unitOfWork.AddressRepository.CreateAsync(connectionString, address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            await _unitOfWork.UserRepository.AddUserAddressAsync(connectionString, user.Id, createdAddress.Id);

            if (user.Address == null)
                throw new ParameterException("Address could not be assigned to user!");
        }

        public async Task AddAddressToExisitingUserAsync(
            string connectionString, int userId, string streetAdr, string buildingNumber, string zip, string area,
            string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            var user = await GetUserAsync(connectionString, userId);

            await AddAddressToExisitingUserAsync(connectionString, user, address);
        }

        public async Task AddAddressToExisitingUserAsync(
            string connectionString, string email, string streetAdr, string buildingNumber, string zip, string area,
            string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            var user = await GetUserAsync(connectionString, email);

            await AddAddressToExisitingUserAsync(connectionString, user, address);
        }

        public async Task ChangeAddressOfUserAsync(string connectionString, string userEmail, string street, string number, string zip, string area, string city, string country)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            if (user.Address == null)
                throw new NoAddressException();

            var address = new Address(street, number, zip, area, city, country);

            address.Id = user.Address.Id;

            await ChangeAddressOfUserAsync(connectionString, address);
        }

        public async Task ChangeAddressOfUserAsync(string connectionString, int userId, string street, string number, string zip, string area, string city, string country)
        {
            var user = await GetUserAsync(connectionString, userId);

            if (user.Address == null)
                throw new NoAddressException();

            var address = new Address(street, number, zip, area, city, country);

            address.Id = user.Address.Id;

            await ChangeAddressOfUserAsync(connectionString, address);
        }

        public async Task ChangeAddressOfUserAsync(string connectionString, Address address)
        {
            if (address == null)
                throw new NoAddressException();

            await ValidateAddressAsync(connectionString, address);

            await _unitOfWork.AddressRepository.UpdateAsync(connectionString, address.Id, address.Street, address.Number, address.Zip,
                address.Area, address.City, address.Country);
        }

        private async Task ValidateAddressAsync(string connectionString, Address address)
        {
            address.Street = address.Street.Trim();
            address.Number = address.Number.Trim();
            address.Zip = address.Zip.Trim();
            address.City = address.City.Trim();
            address.Country = address.Country.Trim();

            if (!onlyLettersNumbersOneSpaceOrDash.IsMatch(address.Street))
                throw new ParameterException("Street", "any other than letters or numbers with a space or dash betwwen them");
            if (!addressNumber.IsMatch(address.Number))
                throw new ParameterException("Number", "any other than numbers, where the first number is larger than zero, followed by one optional letter");
            if (!isZipValidFormat.IsMatch(address.Zip))
                throw new ParameterException("Zip", "any other than numbers, letters, space or dash between the numbers and letters");
            if (!onlyLettersNumbersOneSpaceOrDash.IsMatch(address.Area))
                throw new ParameterException("Area", "any other than letters or numbers with a space or dash betwwen them");
            await IsCountryAndCityCorrect(connectionString, address.Country, address.City);
        }

        public async Task AddUserPictureAsync(string connectionString, User user, string picturePath)
        {
            var img = ConvertImageToBytes(Image.FromFile(picturePath));
            await _unitOfWork.UserRepository.AddUserPicturesAsync(connectionString, user, img);
        }

        public async Task AddUserPictureAsync(string connectionString, string userEmail, string picturePath)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            await AddUserPictureAsync(connectionString, user, picturePath);
        }

        public async Task AddUserPictureAsync(string connectionString, int userId, string picturePath)
        {
            var user = await GetUserAsync(connectionString, userId);

            await AddUserPictureAsync(connectionString, user, picturePath);
        }

        public async Task<Image> GetUserPictureAsync(string connectionString, User user, string picturePath)
        {
            var pics = await _unitOfWork.UserRepository.GetPicturesOfUserAsync(connectionString, user);

            var convertedPic = ConvertImageToBytes(Image.FromFile(picturePath));

            if (pics == null || pics.Count == 0)
                throw new NotFoundException("User pictures");

            foreach (var picture in pics)
            {
                if (StructuralComparisons.StructuralEqualityComparer.Equals(picture, convertedPic))
                    return ConvertBytesToImage(picture);
            }

            throw new NotFoundException("Picture");
        }

        public async Task<Image> GetUserPictureAsync(string connectionString, int userId, string picturePath)
        {
            var user = await GetUserAsync(connectionString, userId);

            return await GetUserPictureAsync(connectionString, user, picturePath);
        }

        public async Task<Image> GetUserPictureAsync(string connectionString, string userEmail, string picturePath)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            return await GetUserPictureAsync(connectionString, user, picturePath);
        }

        public async Task<List<Image>> GetAllUserPictureAsync(string connectionString, User user)
        {
            var pics = await _unitOfWork.UserRepository.GetPicturesOfUserAsync(connectionString, user);

            if (pics == null || pics.Count == 0)
                throw new NotFoundException("User pictures");

            var images = new List<Image>();

            foreach (var pic in pics)
            {
                images.Add(ConvertBytesToImage(pic));
            }

            return images;
        }

        public async Task<List<Image>> GetAllUserPictureAsync(string connectionString, int userId)
        {
            var user = await GetUserAsync(connectionString, userId);

            return await GetAllUserPictureAsync(connectionString, user);
        }

        public async Task<List<Image>> GetAllUserPictureAsync(string connectionString, string userEmail)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            return await GetAllUserPictureAsync(connectionString, user);
        }

        public async Task DeleteAUserPictureAsync(string connectionString, User user, string picturePath)
        {
            var img = ConvertImageToBytes(Image.FromFile(picturePath));

            await _unitOfWork.UserRepository.DeleteAPictureAsync(connectionString, user, img);
        }

        public async Task DeleteAUserPictureAsync(string connectionString, int userId, string picturePath)
        {
            var user = await GetUserAsync(connectionString, userId);

            await DeleteAUserPictureAsync(connectionString, user, picturePath);
        }

        public async Task DeleteAUserPictureAsync(string connectionString, string userEmail, string picturePath)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            await DeleteAUserPictureAsync(connectionString, user, picturePath);
        }

        public async Task DeleteAllUserPicturesAsync(string connectionString, User user)
        {
            await _unitOfWork.UserRepository.DeleteAllPicturesAsync(connectionString, user);
        }

        public async Task DeleteAllUserPicturesAsync(string connectionString, int userId)
        {
            var user = await GetUserAsync(connectionString, userId);

            await DeleteAllUserPicturesAsync(connectionString, user);
        }

        public async Task DeleteAllUserPicturesAsync(string connectionString, string userEmail)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            await DeleteAllUserPicturesAsync(connectionString, user);
        }

        private byte[] ConvertImageToBytes(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private Image ConvertBytesToImage(byte[] img)
        {
            using (var mStream = new MemoryStream(img))
            {
                return Image.FromStream(mStream);
            }
        }

        public async Task UpdateUserAsync(string connectionString, User user)
        {
            NullOrEmptyChecker(user.Firstname, user.Lastname);

            ValidateName(user.Firstname, user.Lastname);

            await _unitOfWork.UserRepository.UpdateNameAsync(connectionString, user);
        }

        public async Task UpdateUserAsync(string connectionString, int userId, string updatedFirstname, string updatedLastname)
        {
            var user = await GetUserAsync(connectionString, userId);

            user.Firstname = updatedFirstname;
            user.Lastname = updatedLastname;

            await UpdateUserAsync(connectionString, user);
        }

        public async Task UpdateUserAsync(string connectionString, string email, string updatedFirstname, string updatedLastname)
        {
            var user = await GetUserAsync(connectionString, email);

            user.Firstname = updatedFirstname;
            user.Lastname = updatedLastname;

            await UpdateUserAsync(connectionString, user);
        }

        public async Task UpdateUserAsync(string connectionString, User user, string updatedEmail)
        {
            await ValidateEmailAsync(connectionString, updatedEmail);

            user.Email = updatedEmail;

            await _unitOfWork.UserRepository.UpdateEmailAsync(connectionString, user);
        }

        public async Task UpdateUserAsync(string connectionString, string originalEmail, string updatedEmail)
        {
            var user = await GetUserAsync(connectionString, originalEmail);

            await UpdateUserAsync(connectionString, user, updatedEmail);
        }

        public async Task UpdateUserAsync(string connectionString, int userId, string updatedEmail)
        {
            var user = await GetUserAsync(connectionString, userId);

            await UpdateUserAsync(connectionString, user, updatedEmail);
        }

        public async Task ChangePasswordAsync(string connectionString, string email, string old, string new1, string new2)
        {
            if (old == new1)
                throw new PasswordChangeException();

            if (new1 != new2)
                throw new PasswordChangeException("new");

            await ValidatePasswordAsync(connectionString, new1);

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(connectionString, email);

            if (VerifyThePassword(old, user.Password))
            {
                user.Password = HashThePassword(new1, null, false);

                await _unitOfWork.UserRepository.ChangePasswordAsync(connectionString, user.Id, user.Password);
            }
            else
                throw new PasswordChangeException("old");
        }

        public async Task<string> GenerateRandomPasswordAsync(string connectionString, int length)
        {
            string policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicyAsync(connectionString);

            if (policy != "default")
            {
                if (length < 8)
                    throw new ParameterException("Password length", "shorter than 8 characters");
            }
            string password = RandomGenerator(length);

            var currentPasswordRegex = new Regex("");

            if (policy == "default")
                if (length < 6)
                    throw new ParameterException("Password length", "shorter than 6");
                else
                    return password;

            if (policy == "first")
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetter;
            else if (policy == "second")
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetterOneUpperAndLowerCase;
            else if (policy == "third")
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacter;
            else if (policy == "fourth")
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacterOneUpperAndLowerCase;

            while (!currentPasswordRegex.IsMatch(password))
                password = RandomGenerator(length);

            return password;
        }

        public async Task ActivateUserAsync(string connectionString, User user, string activationCode)
        {
            if (user.IsActivated)
                throw new ParameterException("User is already activated!");

            if (VerifyThePassword(activationCode.Trim(), await _unitOfWork.UserRepository.GetActivationCodeAsync(connectionString, user.Id)))
                await _unitOfWork.UserRepository.ActivateAccountAsync(connectionString, user.Id);
            else
                throw new ParameterException("Activation code is incorrect!");
        }

        public async Task ActivateUserAsync(string connectionString, int userId, string activationCode)
        {
            var user = await GetUserAsync(connectionString, userId);

            await ActivateUserAsync(connectionString, user, activationCode);
        }

        public async Task ActivateUserAsync(string connectionString, string email, string activationCode)
        {
            var user = await GetUserAsync(connectionString, email);

            await ActivateUserAsync(connectionString, user, activationCode);
        }

        public async Task ResendAccountActivationCodeAsync(string connectionString, User user)
        {
            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            EmailSender("aintbnb@outlook.com", "juStaRandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await _unitOfWork.UserRepository.ResendAccountActivationCodeAsync(connectionString, user.Id, accountActivationCodeHashed);
        }

        public async Task ResendAccountActivationCodeAsync(string connectionString, string userEmail)
        {
            var user = await GetUserAsync(connectionString, userEmail);

            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            EmailSender("aintbnb@outlook.com", "juStaRandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await _unitOfWork.UserRepository.ResendAccountActivationCodeAsync(connectionString, user.Id, accountActivationCodeHashed);
        }

        public async Task ResendAccountActivationCodeAsync(string connectionString, int userId)
        {
            var user = await GetUserAsync(connectionString, userId);

            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            EmailSender("aintbnb@outlook.com", "juStaRandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await _unitOfWork.UserRepository.ResendAccountActivationCodeAsync(connectionString, user.Id, accountActivationCodeHashed);
        }
        public async Task ForgotPasswordAsync(string connectionString, User user)
        {
            var newPassUnhashed = await GenerateRandomPasswordAsync(connectionString, 8);

            var newPass = HashThePassword(newPassUnhashed, null, false);

            await _unitOfWork.UserRepository.ForgottenPasswordAsync(connectionString, user.Id, newPass);

            EmailSender("aintbnb@outlook.com", "juStaRandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Temporary password", "<h1>Your one-time password</h1> <p>Your temporary password is: </p> <p>" + newPassUnhashed + "</p>");
        }

        public async Task ForgotPasswordAsync(string connectionString, string email)
        {
            var user = await GetUserAsync(connectionString, email);

            await ForgotPasswordAsync(connectionString, user);
        }

        public async Task ForgotPasswordAsync(string connectionString, int userId)
        {
            var user = await GetUserAsync(connectionString, userId);

            await ForgotPasswordAsync(connectionString, user);
        }

        public async Task<User> GetUserAsync(string connectionString, string email)
        {
            User user = null;

            try
            {
                user = await _unitOfWork.UserRepository.GetByEmailAsync(connectionString, email);
            }
            catch (ArgumentOutOfRangeException)
            {
                user = await _unitOfWork.UserRepository.GetByEmailAddressNullAsync(connectionString, email);
            }

            if (user == null)
                throw new NotFoundException($"User with email {email}");

            return user;
        }

        public async Task<User> GetUserAsync(string connectionString, int userId)
        {
            User user = null;

            try
            {
                user = await _unitOfWork.UserRepository.GetByIdAsync(connectionString, userId);
            }
            catch (ArgumentOutOfRangeException)
            {
                user = await _unitOfWork.UserRepository.GetByIdAddressNullAsync(connectionString, userId);
            }

            if (user == null)
                throw new NotFoundException($"User with ID {userId}");

            return user;
        }

        public async Task SetPasswordAfterGettingTemporaryPassword(string connectionString, string email, string temporaryPassword, string newPassword, string newPasswordConfirmed)
        {
            var user = await GetUserAsync(connectionString, email);

            if (!user.MustChangePassword)
                throw new PasswordChangeException("You have not requested a temporary password after forgetting your password!");

            await ChangePasswordAsync(connectionString, email, temporaryPassword, newPassword, newPasswordConfirmed);

            await _unitOfWork.UserRepository.ResetTempPasswordAsync(connectionString, user.Password, user.Id);
        }

        public async Task<List<User>> GetAllUsersAsync(string connectionString)
        {
            var users = new List<User>();

            users = await _unitOfWork.UserRepository.GetAllAsync(connectionString);

            users = users.Concat(await _unitOfWork.UserRepository.GetAllAddressNullAsync(connectionString)).ToList();

            users.Sort((x, y) => x.Id.CompareTo(y.Id));

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task<List<User>> GetAllUsersByUsertypeAsync(string connectionString, string usertype)
        {
            var users = new List<User>();

            var type = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, usertype);

            users = await _unitOfWork.UserRepository.GetAllOfAGivenTypeAsync(connectionString, type.Id);

            users = users.Concat(await _unitOfWork.UserRepository.GetAllOfAGivenTypeAddressNullAsync(connectionString, type.Id)).ToList();

            if (users.Count == 0)
                throw new NotFoundException($"Users with type {type.Type}");

            return users;
        }

        public async Task DeleteUserAsync(string connectionString, User user)
        {
            await _unitOfWork.UserRepository.DeleteAsync(connectionString, user.Id);

            if (user.Address != null)
                await _unitOfWork.AddressRepository.DeleteAsync(connectionString, user.Address.Id);
        }

        public async Task DeleteUserAsync(string connectionString, int userId)
        {
            var user = await GetUserAsync(connectionString, userId);

            await DeleteUserAsync(connectionString, user);
        }

        public async Task DeleteUserAsync(string connectionString, string email)
        {
            var user = await GetUserAsync(connectionString, email);

            await DeleteUserAsync(connectionString, user);
        }

        public async Task LoginAsync(string connectionString, string email, string password)
        {
            foreach (var user in await GetAllUsersAsync(connectionString))
            {
                if (string.Equals(user.Email, email.Trim()))
                {
                    if (user.IsActivated == false)
                        throw new LoginException("Verify your account with the code you received in your email first!");
                    if (user.MustChangePassword == true)
                        throw new LoginException("Change your password first!");

                    if (VerifyThePassword(password, user.Password))
                        return;
                }
            }
            throw new LoginException("Username and/or password not correct!");
        }

        public async Task<string> LoginAsync(string connectionString, string email, string password, string jwtSecretKey)
        {
            foreach (var user in await GetAllUsersAsync(connectionString))
            {
                if (string.Equals(user.Email, email.Trim()))
                {
                    if (user.IsActivated == false)
                        throw new LoginException("Verify your account with the code you received in your email first!");
                    if (user.MustChangePassword == true)
                        throw new LoginException("Change your password first!");

                    if (VerifyThePassword(password, user.Password))
                    {
                        return generateJwtToken(user, jwtSecretKey);
                    }
                }
            }
            throw new LoginException("Username and/or password not correct!");
        }

        private string generateJwtToken(User user, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()), new Claim("email", user.Email), new Claim("usertype", user.Usertype.Type) }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task AddMoreUsertypesAsync(string connectionString, params string[] userTypes)
        {
            var types = await _unitOfWork.UsertypeRepository.CreateAsync(connectionString, userTypes);

            if (types.Count == 0)
                throw new FailedToCreateException("Usertype");
        }

        public async Task<List<Usertype>> GetAllUsertypesAsync(string connectionString)
        {
            var allTypes = new List<Usertype>();

            allTypes = await _unitOfWork.UsertypeRepository.GetAllAsync(connectionString);

            if (allTypes.Count == 0)
                throw new NoneFoundInDatabaseTableException("usertpes");

            return allTypes;
        }

        public void SerializeToFile(User userToSerialize, string filePathToWriteTo)
        {
            SerliazeObjToFile(userToSerialize, filePathToWriteTo);
        }

        public void SerializeToFile(List<User> listOfUsersToSerialize, string filePathToWriteTo)
        {
            SerliazeObjToFile(listOfUsersToSerialize, filePathToWriteTo);
        }

        private void SerliazeObjToFile(object userObj, string filePathToWriteTo)
        {
            var fileInfo = new FileInfo(filePathToWriteTo);
            var extn = fileInfo.Extension;
            if (extn == ".xml")
                XmlSerialize(userObj, filePathToWriteTo);
            else if (extn == ".json")
                JsonSerialize(userObj, filePathToWriteTo);
            else if (extn == ".csv")
                CsvSerialize(userObj, filePathToWriteTo);
        }

        private void XmlSerialize(object userObj, string filePathToWriteTo)
        {
            File.WriteAllText(filePathToWriteTo, SerializeToXmlString(userObj));
        }

        private void JsonSerialize(object userObj, string filePathToWriteTo)
        {
            File.WriteAllText(filePathToWriteTo, SerializeToJsonString(userObj));
        }

        private void CsvSerialize(object userObj, string filePathToWriteTo)
        {
            File.WriteAllText(filePathToWriteTo, SerializeToCsvString(userObj));
        }

        public async Task DeSerializeFromFileAsync(string connectionString, string filePathToReadFrom)
        {
            if (!File.Exists(filePathToReadFrom))
                throw new NotFoundException("The input file was");

            var fileInfo = new FileInfo(filePathToReadFrom);
            var extn = fileInfo.Extension;
            if (extn == ".xml")
                await XmlDeSerializeAsync(connectionString, filePathToReadFrom);
            else if (extn == ".json")
                await JsonDeSerializeAsync(connectionString, filePathToReadFrom);
            else if (extn == ".csv")
                await CsvDeSerializeAsync(connectionString, filePathToReadFrom);
        }

        private async Task XmlDeSerializeAsync(string connectionString, string filePathToReadFrom)
        {
            await DeSerializeXmlStringAsync(connectionString, File.ReadAllText(filePathToReadFrom));

        }

        private async Task JsonDeSerializeAsync(string connectionString, string filePathToReadFrom)
        {
            await DeSerializeJsonStringAsync(connectionString, File.ReadAllText(filePathToReadFrom));
        }

        private async Task CsvDeSerializeAsync(string connectionString, string filePathToReadFrom)
        {
            await DeSerializeCsvStringAsync(connectionString, File.ReadAllText(filePathToReadFrom));
        }

        public string SerializeToXmlString(object userObj)
        {
            XmlSerializer xmlSerializer;

            if (IsList(userObj))
                xmlSerializer = new XmlSerializer(typeof(List<User>));
            else
                xmlSerializer = new XmlSerializer(typeof(User));

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, userObj);
                var xmlStr = textWriter.ToString();
                textWriter.Close();
                return xmlStr;
            }
        }

        public string SerializeToJsonString(object userObj)
        {
            return JsonConvert.SerializeObject(userObj);
        }

        public string SerializeToCsvString(object userObj)
        {
            var stringBuilder = new StringBuilder();

            if (IsList(userObj))
            {
                var users = (List<User>)userObj;

                foreach (var user in users)
                {
                    WriteUserPropertiesToStringBuilder(stringBuilder, user);

                }
            }
            else
                WriteUserPropertiesToStringBuilder(stringBuilder, (User)userObj);

            if (stringBuilder.Length > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }

        private static bool IsList(object obj)
        {
            return obj is IList &&
                   obj.GetType().IsGenericType &&
                   obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        private static void WriteUserPropertiesToStringBuilder(StringBuilder sb, User u)
        {
            sb.Append(u.Id + "," + u.Email + "," + u.Password + "," + u.Firstname + "," + u.Lastname + ",");
            if (u.Address == null)
                sb.Append("0,,,,,,,");
            else
                sb.Append(u.Address.Id + "," + u.Address.Street + "," + u.Address.Number + "," + u.Address.Zip + "," + u.Address.Area + "," + u.Address.City + "," + u.Address.Country + ",");

            sb.Append(u.IsActivated + "," + u.MustChangePassword + "," + u.Usertype.Id + "," + u.Usertype.Type + ",");

            if (u.Picture != null && u.Picture.Count > 0)
            {
                foreach (var pic in u.Picture)
                    sb.Append(Convert.ToBase64String(pic) + ",");
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append("\n");
        }

        public async Task DeSerializeFromStringAsync(string connectionString, string stringToDeSerialize)
        {
            if (IsStringXml(stringToDeSerialize))
                await DeSerializeXmlStringAsync(connectionString, stringToDeSerialize);
            else if (IsStringJson(stringToDeSerialize))
                await DeSerializeJsonStringAsync(connectionString, stringToDeSerialize);
            else
                await DeSerializeCsvStringAsync(connectionString, stringToDeSerialize);
        }

        private bool IsStringJson(string json)
        {
            JToken token;

            try
            {
                token = JToken.Parse(json);
            }
            catch
            {
                return false;
            }

            if (token is JArray || token is JObject)
                return true;

            return false;
        }

        private static bool IsStringXml(string xml)
        {
            try
            {
                XDocument.Parse(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task DeSerializeXmlStringAsync(string connectionString, string xml)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var xmlNodeList = xmlDocument.GetElementsByTagName("Firstname");
            int count = xmlNodeList.Count;

            if (count > 1)
            {
                var xmlSerializer = new XmlSerializer(typeof(User[]));

                foreach (var user in (User[])xmlSerializer.Deserialize(new XmlNodeReader(xmlDocument)))
                    await CreateUserWithOrWithoutAddressAsync(connectionString, user);
            }
            else
            {
                var xmlSerializer = new XmlSerializer(typeof(User));
                var user = (User)xmlSerializer.Deserialize(new XmlNodeReader(xmlDocument));
                await CreateUserWithOrWithoutAddressAsync(connectionString, user);
            }
        }

        private async Task DeSerializeJsonStringAsync(string connectionString, string json)
        {
            var token = JToken.Parse(json);

            if (token is JArray)
            {
                var userList = JsonConvert.DeserializeObject<List<User>>(json);
                foreach (var user in userList)
                    await CreateUserWithOrWithoutAddressAsync(connectionString, user);
            }
            else if (token is JObject)
            {
                var user = JsonConvert.DeserializeObject<User>(json);
                await CreateUserWithOrWithoutAddressAsync(connectionString, user);
            }
        }

        private async Task DeSerializeCsvStringAsync(string connectionString, string csv)
        {
            using (var reader = new StringReader(csv))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] word = line.Split(',');

                    var user = new User();

                    user.Id = Int32.Parse(word[0]);
                    user.Email = word[1];
                    user.Password = word[2];
                    user.Firstname = word[3];
                    user.Lastname = word[4];
                    if (!String.IsNullOrEmpty(word[6]))
                    {
                        user.Address = new Address();

                        user.Address.Id = Int32.Parse(word[5]);
                        user.Address.Street = word[6];
                        user.Address.Number = word[7];
                        user.Address.Zip = word[8];
                        user.Address.Area = word[9];
                        user.Address.City = word[10];
                        user.Address.Country = word[11];
                    }
                    user.IsActivated = bool.Parse(word[12]);
                    user.MustChangePassword = bool.Parse(word[13]);
                    user.Usertype = new Usertype();
                    user.Usertype.Id = Int32.Parse(word[14]);
                    user.Usertype.Type = word[15];
                    if (!String.IsNullOrEmpty(word[16]))
                    {
                        user.Picture = new List<byte[]>();

                        user.Picture.Add(Convert.FromBase64String(word[16]));

                        var lengthOfPictureList = word.Length - 17;

                        for (int i = 0; i < lengthOfPictureList; i++)
                            user.Picture.Add(Convert.FromBase64String(word[i + 17]));
                    }

                    await CreateUserWithOrWithoutAddressAsync(connectionString, user);
                }

                reader.Close();
            }
        }

        private async Task CreateUserWithOrWithoutAddressAsync(string connectionString, User user)
        {
            var originalPassword = user.Password;

            var pics = user.Picture;

            var hasPics = false;

            if (pics != null && pics.Count > 0)
            {
                user.Picture = null;
                hasPics = true;
            }

            if (user.Address == null)
                await CreateUserAsync(connectionString, user, user.Password);
            else
                await CreateUserAsync(connectionString, user.Email, user.Password, user.Password, user.Firstname, user.Lastname, user.Address.Street, user.Address.Number, user.Address.Zip, user.Address.Area, user.Address.City, user.Address.Country, user.Usertype.Type);

            var createdUser = await GetUserAsync(connectionString, user.Email);

            if (hasPics)
            {
                foreach (var pic in pics)
                    await _unitOfWork.UserRepository.AddUserPicturesAsync(connectionString, createdUser, pic);
            }

            if (user.IsActivated)
                await _unitOfWork.UserRepository.ActivateAccountAsync(connectionString, user.Id);

            if (user.MustChangePassword)
                await _unitOfWork.UserRepository.ForgottenPasswordAsync(connectionString, user.Id, user.Password);
            else
                await _unitOfWork.UserRepository.ChangePasswordAsync(connectionString, createdUser.Id, originalPassword);
        }
    }
}
