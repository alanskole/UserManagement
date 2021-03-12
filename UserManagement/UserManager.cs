using System;
using System.Text;
using UserManagement.UOW;
using UserManagement.Model;
using UserManagement.CustomExceptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using static UserManagement.Helper.RegexChecker;
using static UserManagement.Helper.PasswordHelper;
using static UserManagement.Database.SetupTables;
using static UserManagement.Helper.AllCities;
using System.Text.RegularExpressions;

namespace UserManagement
{
    public class UserManager
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public async Task CreateUserAsync(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname)
        {
            if (password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            NullOrEmptyChecker(email, password, firstname, lastname);

            email = email.Trim();
            firstname = firstname.Trim();
            lastname = lastname.Trim();
            password = password.Trim();

            await ValidateEmailAsync(connectionString, email);

            await ValidatePasswordAsync(connectionString, password);

            password = HashThePassword(password, null, false);

            ValidateName(firstname, lastname);

            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, "User");

            User createdUser = await _unitOfWork.UserRepository.CreateAsync(connectionString, new User(email, password, firstname, lastname, type));

            if (createdUser.Id == 0)
                throw new ArgumentException("Creating user failed!");

            string accountActivationCode = RandomGenerator(10);

            await _unitOfWork.UserRepository.UploadAccountActivationCodeToDbAsync(connectionString, createdUser.Id, accountActivationCode);
        }

        public async Task CreateUserAsync(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country, string usertype)
        {
            if (password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            NullOrEmptyChecker(email, password, firstname, lastname, streetAdr, buildingNumber, zip, area, city, country, usertype);

            email = email.Trim();
            firstname = firstname.Trim();
            lastname = lastname.Trim();
            usertype = usertype.Trim();
            password = password.Trim();

            await ValidateEmailAsync(connectionString, email);

            await ValidatePasswordAsync(connectionString, password);

            password = HashThePassword(password, null, false);

            ValidateName(firstname, lastname);

            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, usertype);

            Address address = await CreateAddressAsync(connectionString, streetAdr, buildingNumber, zip, area, city, country);

            User createdUser = await _unitOfWork.UserRepository.CreateAsync(connectionString, new User(email, password, firstname, lastname, address, type));

            createdUser.Address = address;

            createdUser.Usertype = type;

            if (createdUser.Id == 0)
                throw new ArgumentException("Creating user failed!");

            string accountActivationCode = RandomGenerator(10);

            await _unitOfWork.UserRepository.UploadAccountActivationCodeToDbAsync(connectionString, createdUser.Id, accountActivationCode);
        }

        private static void NullOrEmptyChecker(string email, string password, string firstname, string lastname, string streetAdr, string buildingNumber, string zip, string area, string city, string country, string usertype)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(firstname) || String.IsNullOrEmpty(lastname) ||
                            String.IsNullOrEmpty(streetAdr) || String.IsNullOrEmpty(buildingNumber) || String.IsNullOrEmpty(zip) || String.IsNullOrEmpty(area) ||
                            String.IsNullOrEmpty(city) || String.IsNullOrEmpty(country) || String.IsNullOrEmpty(usertype))
                throw new ParameterException("Parameters", "empty or null");
        }

        private static void NullOrEmptyChecker(string email, string password, string firstname, string lastname)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(firstname) || String.IsNullOrEmpty(lastname))
                throw new ParameterException("Parameters", "empty or null");
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

        public async Task AddAddressToExisitingUserAsync(
            string connectionString, int userId, string streetAdr, string buildingNumber, string zip, string area,
            string city, string country)
        {
            Address address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddressAsync(connectionString, address);

            Address createdAddress = await _unitOfWork.AddressRepository.CreateAsync(connectionString, address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            await _unitOfWork.UserRepository.AddUserAddressAsync(connectionString, userId, createdAddress.Id);

            User user = await GetUserByIdAsync(connectionString, userId);

            if (user.Address == null)
                throw new ParameterException("Address could not be assigned to user!");

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

        public async Task ChangeNameOfUserAsync(string connectionString, int userId, string firstname, string lastname)
        {
            ValidateName(firstname, lastname);

            var user = new User { Id = userId, Firstname = firstname, Lastname = lastname };

            await _unitOfWork.UserRepository.UpdateNameAsync(connectionString, user);
        }

        public async Task ChangeEmailAsync(string connectionString, int userId, string email)
        {
            await ValidateEmailAsync(connectionString, email);

            await _unitOfWork.UserRepository.UpdateEmailAsync(connectionString, new User { Id = userId, Email = email });
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

        public async Task ActivateUserAsync(string connectionString, int userId, string activationCode)
        {
            await _unitOfWork.UserRepository.ActivateAccountAsync(connectionString, userId, activationCode);
        }

        public async Task ForgotPasswordAsync(string connectionString, int userId)
        {
            User user = await GetUserByIdAsync(connectionString, userId);

            string newPass = await GenerateRandomPasswordAsync(connectionString, 8);

            newPass = HashThePassword(newPass, null, false);

            await _unitOfWork.UserRepository.ForgottenPasswordAsync(connectionString, userId, newPass);
        }

        public async Task<User> GetUserByEmailAsync(string connectionString, string email)
        {
            User user = await _unitOfWork.UserRepository.GetByEmailAsync(connectionString, email);

            return user;
        }

        public async Task SetPasswordAfterGettingTemporaryPassword(string connectionString, string email, string temporaryPassword, string newPassword, string newPasswordConfirmed)
        {

            User user = await GetUserByEmailAsync(connectionString, email);
            
            if (!user.MustChangePassword)
                throw new PasswordChangeException("You have not requested a temporary password after forgetting your password!");

            await ChangePasswordAsync(connectionString, email, temporaryPassword, newPassword, newPasswordConfirmed);
            
            await _unitOfWork.UserRepository.ResetTempPasswordAsync(connectionString, user.Password, user.Id);
        }

        public async Task<User> GetUserByIdAsync(string connectionString, int userId)
        {
            User user = new User();

            user = await _unitOfWork.UserRepository.GetByIdAsync(connectionString, userId);

            if (user.Id == 0)
                throw new IdNotFoundException("User", userId);

            return user;
        }

        public async Task<List<User>> GetAllUsersAsync(string connectionString)
        {
            List<User> users = new List<User>();

            users = await _unitOfWork.UserRepository.GetAllAsync(connectionString);

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task<List<User>> GetAllUsersByUsertypeAsync(string connectionString, string usertype)
        {
            List<User> users = new List<User>();

            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(connectionString, usertype);

            users = await _unitOfWork.UserRepository.GetAllOfAGivenTypeAsync(connectionString, type.Id);

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task DeleteUserAsync(string connectionString, int userId)
        {
            var user = await GetUserByIdAsync(connectionString, userId);

            await _unitOfWork.UserRepository.DeleteAsync(connectionString, userId);

            await _unitOfWork.AddressRepository.DeleteAsync(connectionString, user.Address.Id);
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
            List<Usertype> types = await _unitOfWork.UsertypeRepository.CreateAsync(connectionString, userTypes);

            if (types.Count == 0)
                throw new FailedToCreateException("Usertype");
        }

        public async Task<List<Usertype>> GetAllUsertypesAsync(string connectionString)
        {
            List<Usertype> allTypes = new List<Usertype>();
            
            allTypes = await _unitOfWork.UsertypeRepository.GetAllAsync(connectionString);

            if (allTypes.Count == 0)
                throw new NoneFoundInDatabaseTableException("usertpes");

            return allTypes;
        }
    }
}
