using System;
using System.Text;
using UserManagement.UOW;
using UserManagement.Model;
using UserManagement.CustomExceptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using static UserManagement.Helper.RegexChecker;
using static UserManagement.Helper.Email;
using static UserManagement.Helper.PasswordHelper;
using static UserManagement.Helper.AllCities;
using System.Text.RegularExpressions;

namespace UserManagement
{
    public class UserManager
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public async Task CreateUserAsync(string connectionString, User user, string passwordConfirmed)
        {
            NullOrEmptyChecker(user);

            await ValidateEmailAsync(connectionString, user.Email);

            await ValidatePasswordAsync(connectionString, user.Password);

            if (user.Password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            user.Password = HashThePassword(user.Password, null, false);

            ValidateName(user.Firstname, user.Lastname);

            var createdUser = await _unitOfWork.UserRepository.CreateAsync(connectionString, user);

            if (createdUser.Id == 0)
                throw new ArgumentException("Creating user failed!");

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
            User user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname };

            user.Usertype = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, usertype);

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
            User user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname };

            user.Usertype = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, usertype);

            user.Address = await CreateAddressAsync(connectionString, streetAdr, buildingNumber, zip, area, city, country);

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
            string policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicyAsync(connectionString);

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
            Address address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddressAsync(connectionString, address);

            Address createdAddress = await _unitOfWork.AddressRepository.CreateAsync(connectionString, address);

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
            var address = new Address(street, number, zip, area, city, country);

            var user = await GetUserAsync(connectionString, userEmail);

            address.Id = user.Address.Id;

            await ChangeAddressOfUserAsync(connectionString, address);
        }

        public async Task ChangeAddressOfUserAsync(string connectionString, int userId, string street, string number, string zip, string area, string city, string country)
        {
            Address address = new Address(street, number, zip, area, city, country);

            User user = await GetUserAsync(connectionString, userId);

            address.Id = user.Address.Id;

            await ChangeAddressOfUserAsync(connectionString, address);
        }

        public async Task ChangeAddressOfUserAsync(string connectionString, Address address)
        {
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

            User user = await _unitOfWork.UserRepository.GetByEmailAsync(connectionString, email);

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

            Regex currentPasswordRegex = new Regex("");

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
            activationCode = HashThePassword(activationCode, null, false);

            await _unitOfWork.UserRepository.ActivateAccountAsync(connectionString, user.Id, activationCode);
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
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(connectionString, email);

            return user;
        }

        public async Task<User> GetUserAsync(string connectionString, int userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(connectionString, userId);

            if (user.Id == 0)
                throw new IdNotFoundException("User", userId);

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

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task<List<User>> GetAllUsersByUsertypeAsync(string connectionString, string usertype)
        {
            var users = new List<User>();

            var type = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, usertype);

            users = await _unitOfWork.UserRepository.GetAllOfAGivenTypeAsync(connectionString, type.Id);

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task DeleteUserAsync(string connectionString, User user)
        {
            await _unitOfWork.UserRepository.DeleteAsync(connectionString, user.Id);

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

                    if (VerifyThePassword(password.Trim(), user.Password))
                        return;
                }
            }
            throw new LoginException("Username and/or password not correct!");
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
    }
}
