using ManageUsers.BusinessLogic.Interface;
using ManageUsers.CustomExceptions;
using ManageUsers.Helper;
using ManageUsers.Model;
using ManageUsers.Repository.Imp;
using ManageUsers.Repository.Interface;
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
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static ManageUsers.Helper.AllCities;
using static ManageUsers.Helper.PasswordHelper;
using static ManageUsers.Helper.RegexChecker;

namespace ManageUsers.BusinessLogic.Imp
{

    internal class UserManager : IUserManager
    {
        private IUserRepository _userRepository;
        private IUsertypeRepository _usertypeRepository;
        private IAddressRepository _addressRepository;
        private IPasswordPolicyRepository _passwordPolicyRepository;
        private Email _email;

        internal UserManager(string connectionString, Email email)
        {
            _userRepository = new UserRepository(connectionString);
            _usertypeRepository = new UsertypeRepository(connectionString);
            _passwordPolicyRepository = new PasswordPolicyRepository(connectionString);
            _addressRepository = new AddressRepository(connectionString);
            _email = email;
        }

        private async Task CreateTheUserAsync(User user, string passwordConfirmed)
        {
            ValidateName(user.Firstname, user.Lastname);

            await ValidateEmailAsync(user.Email);

            await ValidatePasswordAsync(user.Password);

            if (user.Password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            if (user.Address != null)
                user.Address = await CreateAddressAsync(user.Address.Street, user.Address.Number, user.Address.Zip, user.Address.Area, user.Address.City, user.Address.Country);

            user.Usertype = await _usertypeRepository.GetUsertypeAsync(user.Usertype.Type);

            user.Password = HashThePassword(user.Password, null, false);

            var createdUser = await _userRepository.CreateAsync(user);

            if (createdUser.Id == 0)
                throw new FailedToCreateException("User");

            if (!user.IsActivated && _email.SenderEmail != "test@test.test" && _email.EmailPassword != "test")
            {
                var accountActivationCodeUnhashed = RandomGenerator(10);

                var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

                await _userRepository.UploadAccountActivationCodeToDbAsync(createdUser.Id, accountActivationCodeHashed);

                _email.EmailSender(user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");
            }
        }

        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname)
        {
            await CreateUserAsync(email, password, passwordConfirmed, firstname, lastname, "User");
        }

        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname, string usertype)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype(usertype) };

            await CreateTheUserAsync(user, passwordConfirmed);
        }

        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            await CreateUserAsync(email, password, passwordConfirmed, firstname, lastname, streetAdr, buildingNumber, zip, area, city, country, "User");
        }

        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country, string usertype)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype(usertype) };

            user.Address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await CreateTheUserAsync(user, passwordConfirmed);
        }

        private static void ValidateName(string firstname, string lastname)
        {
            try
            {
                firstname = firstname.Trim();
                lastname = lastname.Trim();
            }
            catch (NullReferenceException)
            {
                throw new ParameterException("Names", "null");
            }

            if (!onlyLettersOneSpaceOrDash.IsMatch(firstname) || !onlyLettersOneSpaceOrDash.IsMatch(lastname))
                throw new ParameterException("Names must be at least two letters and", "containing any other than letters and one space or dash between names");
        }

        private async Task ValidateEmailAsync(string email)
        {
            try
            {
                email = email.Trim();
            }
            catch (NullReferenceException)
            {
                throw new ParameterException("Email", "null");
            }

            if (!isEmailValidFormat.IsMatch(email))
                throw new ParameterException("Email not formatted correctly!");

            if (!await _userRepository.IsEmailAvailableAsync(email))
                throw new ParameterException("Email is not available!");
        }

        private async Task ValidatePasswordAsync(string password)
        {
            var policy = await _passwordPolicyRepository.GetPasswordPolicyAsync();

            if (String.IsNullOrEmpty(password))
                throw new ParameterException("Password", "null");

            if (password.Contains(" "))
                throw new ParameterException("Password can't contain space!");

            if (!ContainsLowerCase(password))
                throw new ParameterException("Password must contain at least one lower case letter!");

            if (policy.Item1 == 6 && policy.Item2 == false && policy.Item3 == false && policy.Item4 == false)
            {
                if (password.Length < 6)
                    throw new ParameterException("Password", "shorter than 6 characters");
                return;
            }

            var length = policy.Item1;

            if (!ContainsMinimumAmountOfCharacters(password, length))
                throw new ParameterException($"Password must be at least {length} characters long!");

            if (policy.Item2 == true)
            {
                if (!ContainsUpperCase(password))
                    throw new ParameterException("Password must contain at least one upper case letter!");
            }

            if (policy.Item3 == true)
            {
                if (!ContainsNumber(password))
                    throw new ParameterException("Password must contain at least one number!");
            }

            if (policy.Item4 == true)
            {
                if (!ContainsSpecialCharacter(password))
                    throw new ParameterException("Password must contain at least one special character!");
            }
        }

        private async Task<Address> CreateAddressAsync(string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddressAsync(address);

            var createdAddress = await _addressRepository.CreateAsync(address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            return createdAddress;
        }

        private async Task AddAddressToExisitingUserAsync(User user, Address address)
        {
            await ValidateAddressAsync(address);

            var createdAddress = await _addressRepository.CreateAsync(address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            await _userRepository.AddUserAddressAsync(user.Id, createdAddress.Id);

            var usr = await GetUserAsync(user.Email);

            if (usr.Address == null)
                throw new ParameterException("Address could not be assigned to user!");
        }

        private async Task ChangeAddressOfUserAsync(Address updatedAddress)
        {
            if (updatedAddress == null)
                throw new NoAddressException();

            await ValidateAddressAsync(updatedAddress);

            await _addressRepository.UpdateAsync(updatedAddress.Id, updatedAddress.Street, updatedAddress.Number, updatedAddress.Zip,
                updatedAddress.Area, updatedAddress.City, updatedAddress.Country);
        }

        private async Task ValidateAddressAsync(Address address)
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
            await IsCountryAndCityCorrect(address.Country, address.City);
        }

        private async Task AddThePicturesAsync(User user, params string[] picturePath)
        {
            foreach (var picpatch in picturePath)
            {
                var img = ConvertImageToBytes(Image.FromFile(picpatch));
                await _userRepository.AddUserPicturesAsync(user, img);
            }
        }

        public async Task AddUserPictureAsync(string userEmail, params string[] picturePath)
        {
            var user = await GetUserAsync(userEmail);
            await AddThePicturesAsync(user, picturePath);
        }

        public async Task AddUserPictureAsync(int userId, params string[] picturePath)
        {
            var user = await GetUserAsync(userId);
            await AddThePicturesAsync(user, picturePath);
        }

        private async Task<Image> GetThePictureAsync(User user, string picturePath)
        {
            var pics = await _userRepository.GetPicturesOfUserAsync(user);

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

        public async Task<Image> GetUserPictureAsync(int userId, int indexOfPicture)
        {
            var user = await GetUserAsync(userId);

            return await GetThePictureAsync(user, indexOfPicture);
        }

        public async Task<Image> GetUserPictureAsync(string userEmail, int indexOfPicture)
        {
            var user = await GetUserAsync(userEmail);

            return await GetThePictureAsync(user, indexOfPicture);
        }

        private async Task<Image> GetThePictureAsync(User user, int indexOfPicture)
        {
            var pics = await _userRepository.GetPicturesOfUserAsync(user);

            if (pics == null || pics.Count == 0)
                throw new NotFoundException("User pictures");

            try
            {
                return ConvertBytesToImage(pics[indexOfPicture - 1]);
            }
            catch (Exception)
            {
                throw new NotFoundException("Picture");
            }
        }

        public async Task<Image> GetUserPictureAsync(int userId, string picturePath)
        {
            var user = await GetUserAsync(userId);

            return await GetThePictureAsync(user, picturePath);
        }

        public async Task<Image> GetUserPictureAsync(string userEmail, string picturePath)
        {
            var user = await GetUserAsync(userEmail);

            return await GetThePictureAsync(user, picturePath);
        }

        private async Task<List<Image>> GetAllThePicturesAsync(User user)
        {
            var pics = await _userRepository.GetPicturesOfUserAsync(user);

            if (pics == null || pics.Count == 0)
                throw new NotFoundException("User pictures");

            var images = new List<Image>();

            foreach (var pic in pics)
            {
                images.Add(ConvertBytesToImage(pic));
            }
            return images;
        }

        public async Task<List<Image>> GetAllUserPictureAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            return await GetAllThePicturesAsync(user);
        }

        public async Task<List<Image>> GetAllUserPictureAsync(string userEmail)
        {
            var user = await GetUserAsync(userEmail);

            return await GetAllThePicturesAsync(user);
        }

        private async Task DeleteThePicturesAsync(User user, params string[] picturePath)
        {
            foreach (var path in picturePath)
            {
                var img = ConvertImageToBytes(Image.FromFile(path));

                await _userRepository.DeleteAPictureAsync(user, img);
            }
        }

        public async Task DeleteUserPictureAsync(int userId, params string[] picturePath)
        {
            var user = await GetUserAsync(userId);

            await DeleteThePicturesAsync(user, picturePath);
        }

        public async Task DeleteUserPictureAsync(string userEmail, params string[] picturePath)
        {
            var user = await GetUserAsync(userEmail);

            await DeleteThePicturesAsync(user, picturePath);
        }

        private async Task DeleteThePicturesAsync(User user, params int[] indexOfPicture)
        {
            Array.Sort(indexOfPicture);
            int i = 0;
            foreach (var index in indexOfPicture)
            {
                var j = index;
                if (i > 0)
                    j -= 1;

                await _userRepository.DeleteAPictureAsync(user, j);

                i++;
            }
        }

        public async Task DeleteUserPictureAsync(int userId, params int[] indexOfPicture)
        {
            var user = await GetUserAsync(userId);

            await DeleteThePicturesAsync(user, indexOfPicture);
        }

        public async Task DeleteUserPictureAsync(string userEmail, params int[] indexOfPicture)
        {
            var user = await GetUserAsync(userEmail);

            await DeleteThePicturesAsync(user, indexOfPicture);
        }

        private async Task DeleteAllUserPicturesAsync(User user)
        {
            await _userRepository.DeleteAllPicturesAsync(user);
        }

        public async Task DeleteAllUserPicturesAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await DeleteAllUserPicturesAsync(user);
        }

        public async Task DeleteAllUserPicturesAsync(string userEmail)
        {
            var user = await GetUserAsync(userEmail);

            await DeleteAllUserPicturesAsync(user);
        }

        private static byte[] ConvertImageToBytes(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                return ms.ToArray();
            }
        }

        private static Image ConvertBytesToImage(byte[] img)
        {
            using (var mStream = new MemoryStream(img))
            {
                return Image.FromStream(mStream);
            }
        }

        private async Task UpdateTheUserAsync(User user)
        {
            var userFromDb = await GetUserAsync(user.Id);

            if (user.Firstname != userFromDb.Firstname || user.Lastname != userFromDb.Lastname)
            {
                ValidateName(user.Firstname, user.Lastname);
                await _userRepository.UpdateNameAsync(user);
            }

            if (user.Email != userFromDb.Email)
                await UpdateTheUserAsync(user, user.Email);


            if (user.Address != null)
            {
                if (userFromDb.Address != null)
                {
                    if (user.Address.Street != userFromDb.Address.Street || user.Address.Number != userFromDb.Address.Number ||
                        user.Address.Zip != userFromDb.Address.Zip || user.Address.Area != userFromDb.Address.Area ||
                        user.Address.City != userFromDb.Address.City || user.Address.Country != userFromDb.Address.Country)
                    {
                        user.Address.Id = userFromDb.Address.Id;

                        await ChangeAddressOfUserAsync(user.Address);
                    }
                }
                else
                {
                    await AddAddressToExisitingUserAsync(user, user.Address);
                }
            }
        }

        public async Task UpdateUserAsync(string email, string street, string number, string zip, string area, string city, string country)
        {
            var user = await GetUserAsync(email);

            await UpdateUserAsync(user, street, number, zip, area, city, country);
        }

        public async Task UpdateUserAsync(int userId, string street, string number, string zip, string area, string city, string country)
        {
            var user = await GetUserAsync(userId);

            await UpdateUserAsync(user, street, number, zip, area, city, country);
        }

        public async Task UpdateUserAsync(User user, string street, string number, string zip, string area, string city, string country)
        {
            var address = new Address(street, number, zip, area, city, country);

            user.Address = address;

            await UpdateTheUserAsync(user);
        }

        public async Task UpdateUserAsync(int userId, string updatedFirstname, string updatedLastname)
        {
            var user = await GetUserAsync(userId);

            user.Firstname = updatedFirstname;
            user.Lastname = updatedLastname;

            await UpdateTheUserAsync(user);
        }

        public async Task UpdateUserAsync(string email, string updatedFirstname, string updatedLastname)
        {
            var user = await GetUserAsync(email);

            user.Firstname = updatedFirstname;
            user.Lastname = updatedLastname;

            await UpdateTheUserAsync(user);
        }

        private async Task UpdateTheUserAsync(User user, string updatedEmail)
        {
            await ValidateEmailAsync(updatedEmail);

            user.Email = updatedEmail;

            await _userRepository.UpdateEmailAsync(user);
        }

        public async Task UpdateUserAsync(string originalEmail, string updatedEmail)
        {
            var user = await GetUserAsync(originalEmail);

            await UpdateTheUserAsync(user, updatedEmail);
        }

        public async Task UpdateUserAsync(int userId, string updatedEmail)
        {
            var user = await GetUserAsync(userId);

            await UpdateTheUserAsync(user, updatedEmail);
        }

        public async Task UpdateUsertypeOfUserAsync(User user, string updatedUsertype)
        {
            var usertype = await _usertypeRepository.GetUsertypeAsync(updatedUsertype);
            user.Usertype = usertype;
            await _userRepository.UpdateUserTypeAsync(user, usertype);
        }

        public async Task UpdateUsertypeOfUserAsync(int userId, string updatedUsertype)
        {
            var user = await GetUserAsync(userId);
            await UpdateUsertypeOfUserAsync(user, updatedUsertype);
        }

        public async Task UpdateUsertypeOfUserAsync(string userEmail, string updatedUsertype)
        {
            var user = await GetUserAsync(userEmail);
            await UpdateUsertypeOfUserAsync(user, updatedUsertype);
        }

        public async Task UpdateUsertypeOfUserAsync(User user, Usertype updatedUsertype)
        {
            await UpdateUsertypeOfUserAsync(user, updatedUsertype.Type);
        }

        public async Task UpdateUsertypeOfUserAsync(int userId, Usertype updatedUsertype)
        {
            var user = await GetUserAsync(userId);
            await UpdateUsertypeOfUserAsync(user, updatedUsertype.Type);
        }

        public async Task UpdateUsertypeOfUserAsync(string userEmail, Usertype updatedUsertype)
        {
            var user = await GetUserAsync(userEmail);
            await UpdateUsertypeOfUserAsync(user, updatedUsertype.Type);
        }

        public async Task ChangePasswordAsync(int userId, string old, string newPassword, string newConfirmed)
        {
            var user = await GetUserAsync(userId);
            await ChangePasswordAsync(user.Email, old, newPassword, newConfirmed);
        }

        public async Task ChangePasswordAsync(string email, string old, string newPassword, string newConfirmed)
        {
            if (old == newPassword)
                throw new PasswordChangeException();

            if (newPassword != newConfirmed)
                throw new PasswordChangeException("new");

            await ValidatePasswordAsync(newPassword);

            var user = await GetUserAsync(email);

            if (VerifyThePassword(old, user.Password))
            {
                user.Password = HashThePassword(newPassword, null, false);

                await _userRepository.ChangePasswordAsync(user.Id, user.Password);
            }
            else
                throw new PasswordChangeException("old");
        }

        public async Task<string> GenerateRandomPasswordAsync(int length)
        {
            var policy = await _passwordPolicyRepository.GetPasswordPolicyAsync();

            var policyLength = policy.Item1;

            if (length < policyLength)
                throw new ParameterException("Random password length", $"shorter than {policyLength} characters");

            var password = RandomGenerator(length);

            var continueLoop = true;

            while (continueLoop)
            {
                try
                {
                    await ValidatePasswordAsync(password);
                    continueLoop = false;
                }
                catch (ParameterException)
                {
                    password = RandomGenerator(length);
                }
            }

            return password;
        }

        private async Task ActivateTheUserAsync(User user, string activationCode)
        {
            if (_email.SenderEmail == "test@test.test" && _email.EmailPassword == "test")
            {
                await _userRepository.ActivateAccountAsync(user.Id);
                return;
            }

            if (user.IsActivated)
                throw new ParameterException("User is already activated!");

            if (VerifyThePassword(activationCode.Trim(), await _userRepository.GetActivationCodeAsync(user.Id)))
                await _userRepository.ActivateAccountAsync(user.Id);
            else
                throw new ParameterException("Activation code is incorrect!");
        }

        public async Task ActivateUserAsync(int userId, string activationCode)
        {
            var user = await GetUserAsync(userId);

            await ActivateTheUserAsync(user, activationCode);
        }

        public async Task ActivateUserAsync(string email, string activationCode)
        {
            var user = await GetUserAsync(email);

            await ActivateTheUserAsync(user, activationCode);
        }

        private async Task ResendAccountTheActivationCodeAsync(User user)
        {
            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            if (_email.SenderEmail != "test@test.test" && _email.EmailPassword != "test")
                _email.EmailSender(user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await ResendAccountActivationCodeAsync(user.Id);
        }

        public async Task ResendAccountActivationCodeAsync(string userEmail)
        {
            var user = await GetUserAsync(userEmail);

            await ResendAccountTheActivationCodeAsync(user);
        }

        public async Task ResendAccountActivationCodeAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await ResendAccountTheActivationCodeAsync(user);
        }

        private async Task ForgotThePasswordAsync(User user)
        {
            var newPassUnhashed = await GenerateRandomPasswordAsync(8);

            var newPass = HashThePassword(newPassUnhashed, null, false);

            await _userRepository.ForgottenPasswordAsync(user.Id, newPass);

            if (_email.SenderEmail != "test@test.test" && _email.EmailPassword != "test")
                _email.EmailSender(user.Email, "smtp.office365.com", 587, "Temporary password", "<h1>Your one-time password</h1> <p>Your temporary password is: </p> <p>" + newPassUnhashed + "</p>");
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await GetUserAsync(email);

            await ForgotThePasswordAsync(user);
        }

        public async Task ForgotPasswordAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await ForgotThePasswordAsync(user);
        }

        public async Task<User> GetUserAsync(string email)
        {
            User user;

            try
            {
                user = await _userRepository.GetByEmailAsync(email);
            }
            catch (ArgumentOutOfRangeException)
            {
                user = await _userRepository.GetByEmailAddressNullAsync(email);
            }

            return user;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            User user;

            try
            {
                user = await _userRepository.GetByIdAsync(userId);
            }
            catch (ArgumentOutOfRangeException)
            {
                user = await _userRepository.GetByIdAddressNullAsync(userId);
            }

            return user;
        }

        public async Task SetPasswordAfterGettingTemporaryPasswordAsync(string email, string temporaryPassword, string newPassword, string newPasswordConfirmed)
        {
            var user = await GetUserAsync(email);

            if (!user.MustChangePassword)
                throw new PasswordChangeException("You have not requested a temporary password after forgetting your password!");

            await ChangePasswordAsync(email, temporaryPassword, newPassword, newPasswordConfirmed);

            await _userRepository.ResetTempPasswordAsync(user.Password, user.Id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            users = await _userRepository.GetAllAsync();

            users = users.Concat(await _userRepository.GetAllAddressNullAsync()).ToList();

            users.Sort((x, y) => x.Id.CompareTo(y.Id));

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task<List<User>> GetAllUsersAsync(string usertype)
        {
            var users = new List<User>();

            var type = await _usertypeRepository.GetUsertypeAsync(usertype);

            users = await _userRepository.GetAllOfAGivenTypeAsync(type.Id);

            users = users.Concat(await _userRepository.GetAllOfAGivenTypeAddressNullAsync(type.Id)).ToList();

            users.Sort((x, y) => x.Id.CompareTo(y.Id));

            if (users.Count == 0)
                throw new NotFoundException($"Users with type {type.Type}");

            return users;
        }

        private async Task DeleteTheUserAsync(User user)
        {
            await _userRepository.DeleteAsync(user.Id);

            if (user.Address != null)
                await _addressRepository.DeleteAsync(user.Address.Id);
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await DeleteTheUserAsync(user);
        }

        public async Task DeleteUserAsync(string email)
        {
            var user = await GetUserAsync(email);

            await DeleteTheUserAsync(user);
        }

        private async Task CheckLoginCredentials(string email, string password)
        {
            foreach (var user in await GetAllUsersAsync())
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

        public async Task<string> LoginAsync(string email, string password, string jwtSecretKey)
        {
            try
            {
                await CheckLoginCredentials(email, password);
                var user = await GetUserAsync(email);
                var token = generateJwtToken(user, jwtSecretKey);
                await _userRepository.LoginUserAsync(user.Id);
                return token;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string generateJwtToken(User user, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()), new Claim("email", user.Email), new Claim("usertype", user.Usertype.Type) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task LogoutAsync(int userId)
        {
            await _userRepository.LogoutUserAsync(userId);
        }

        public async Task LogoutAsync(string email)
        {
            var user = GetUserAsync(email);

            await LogoutAsync(user.Id);
        }

        public async Task<bool> ValidateJwtTokenAsync(string jwtToken, string secret)
        {
            var userId = GetUserIdFromJwtToken(jwtToken);

            if (!await _userRepository.IsUserLoggedIn(userId))
                return false;

            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = mySecurityKey,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<Usertype> GetUsertypeFromJwtTokenAsync(string jwtToken)
        {
            var claimType = "usertype";
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(jwtToken) as JwtSecurityToken;

            var usertype = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return await _usertypeRepository.GetUsertypeAsync(usertype);
        }

        public int GetUserIdFromJwtToken(string jwtToken)
        {
            var claimType = "id";
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(jwtToken) as JwtSecurityToken;

            var userId = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return Int32.Parse(userId);
        }

        public string GetUserEmailFromJwtToken(string jwtToken)
        {
            var claimType = "email";
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(jwtToken) as JwtSecurityToken;

            var email = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return email;
        }

        public async Task<bool> IsAdminAsync(string jwtToken)
        {
            var userType = await GetUsertypeFromJwtTokenAsync(jwtToken);
            return userType.Type == "Admin";
        }

        public async Task<bool> DoesUserHaveCorrectUsertypeAsync(string jwtToken, string requiredUsertype)
        {
            var userType = await GetUsertypeFromJwtTokenAsync(jwtToken);
            return userType.Type == requiredUsertype;
        }

        public async Task AddMoreUsertypesAsync(params string[] userTypes)
        {
            var types = await _usertypeRepository.CreateAsync(userTypes);

            if (types.Count == 0)
                throw new FailedToCreateException("Usertype");
        }

        public async Task<List<Usertype>> GetAllUsertypesAsync()
        {
            var allTypes = await _usertypeRepository.GetAllAsync();

            return allTypes;
        }

        public void SerializeToFile(object userObj, string filePathToWriteTo)
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


        public async Task DeSerializeFromFileAsync(string filePathToReadFrom)
        {
            if (!File.Exists(filePathToReadFrom))
                throw new NotFoundException("The input file was");

            var fileInfo = new FileInfo(filePathToReadFrom);
            var extn = fileInfo.Extension;
            if (extn == ".xml")
                await XmlDeSerializeAsync(filePathToReadFrom);
            else if (extn == ".json")
                await JsonDeSerializeAsync(filePathToReadFrom);
            else if (extn == ".csv")
                await CsvDeSerializeAsync(filePathToReadFrom);
        }

        private async Task XmlDeSerializeAsync(string filePathToReadFrom)
        {
            await DeSerializeXmlStringAsync(File.ReadAllText(filePathToReadFrom));

        }

        private async Task JsonDeSerializeAsync(string filePathToReadFrom)
        {
            await DeSerializeJsonStringAsync(File.ReadAllText(filePathToReadFrom));
        }

        private async Task CsvDeSerializeAsync(string filePathToReadFrom)
        {
            await DeSerializeCsvStringAsync(File.ReadAllText(filePathToReadFrom));
        }

        public string SerializeToXmlString(object userObj)
        {
            DataContractSerializer xmlSerializer;

            if (IsList(userObj))
                xmlSerializer = new DataContractSerializer(typeof(List<User>));
            else
                xmlSerializer = new DataContractSerializer(typeof(User));

            using (var memoryStream = new MemoryStream())
            using (var reader = new StreamReader(memoryStream))
            {
                xmlSerializer.WriteObject(memoryStream, userObj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
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

        public async Task DeSerializeFromStringAsync(string stringToDeSerialize)
        {
            if (IsStringXml(stringToDeSerialize))
                await DeSerializeXmlStringAsync(stringToDeSerialize);
            else if (IsStringJson(stringToDeSerialize))
                await DeSerializeJsonStringAsync(stringToDeSerialize);
            else
                await DeSerializeCsvStringAsync(stringToDeSerialize);
        }

        private static bool IsStringJson(string json)
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

        private async Task DeSerializeXmlStringAsync(string xml)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var xmlNodeList = xmlDocument.GetElementsByTagName("Firstname");
            int count = xmlNodeList.Count;
            if (count > 1)
            {
                var xmlSerializer = new DataContractSerializer(typeof(User[]));

                foreach (var user in (User[])xmlSerializer.ReadObject(new XmlNodeReader(xmlDocument)))
                    await CreateUserWithOrWithoutAddressAsync(user);
            }
            else
            {
                var xmlSerializer = new DataContractSerializer(typeof(User));
                var user = (User)xmlSerializer.ReadObject(new XmlNodeReader(xmlDocument));
                await CreateUserWithOrWithoutAddressAsync(user);
            }
        }

        private async Task DeSerializeJsonStringAsync(string json)
        {
            var token = JToken.Parse(json);

            if (token is JArray)
            {
                var userList = JsonConvert.DeserializeObject<List<User>>(json);
                foreach (var user in userList)
                    await CreateUserWithOrWithoutAddressAsync(user);
            }
            else if (token is JObject)
            {
                var user = JsonConvert.DeserializeObject<User>(json);
                await CreateUserWithOrWithoutAddressAsync(user);
            }
        }

        private async Task DeSerializeCsvStringAsync(string csv)
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

                    await CreateUserWithOrWithoutAddressAsync(user);
                }

                reader.Close();
            }
        }

        private async Task CreateUserWithOrWithoutAddressAsync(User user)
        {
            var originalPassword = user.Password;

            var pics = user.Picture;

            var hasPics = false;

            if (pics != null && pics.Count > 0)
            {
                user.Picture = null;
                hasPics = true;
            }

            await CreateTheUserAsync(user, user.Password);

            var createdUser = await GetUserAsync(user.Email);

            if (hasPics)
            {
                foreach (var pic in pics)
                    await _userRepository.AddUserPicturesAsync(createdUser, pic);
            }

            if (user.MustChangePassword)
                await _userRepository.ForgottenPasswordAsync(user.Id, originalPassword);
            else
                await _userRepository.ChangePasswordAsync(createdUser.Id, originalPassword);
        }
    }
}
