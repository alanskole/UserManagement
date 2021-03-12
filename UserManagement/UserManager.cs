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

        public async Task CreateUser(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname)
        {
            if (password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            NullOrEmptyChecker(email, password, firstname, lastname);

            email = email.Trim();
            firstname = firstname.Trim();
            lastname = lastname.Trim();
            password = password.Trim();

            await ValidateEmail(connectionString, email);

            await ValidatePassword(connectionString, password);

            password = HashThePassword(password, null, false);

            ValidateName(firstname, lastname);

            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertype(connectionString, "User");

            User createdUser = await _unitOfWork.UserRepository.Create(connectionString, new User(email, password, firstname, lastname, type));

            if (createdUser.Id == 0)
                throw new ArgumentException("Creating user failed!");

            string accountActivationCode = RandomGenerator(10);

            await _unitOfWork.UserRepository.UploadAccountActivationCodeToDb(connectionString, createdUser.Id, accountActivationCode);
        }

        public async Task CreateUser(string connectionString, string email, string password, string passwordConfirmed, string firstname, string lastname,
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

            await ValidateEmail(connectionString, email);

            await ValidatePassword(connectionString, password);

            password = HashThePassword(password, null, false);

            ValidateName(firstname, lastname);

            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertype(connectionString, usertype);

            Address address = await CreateAddress(connectionString, streetAdr, buildingNumber, zip, area, city, country);

            User createdUser = await _unitOfWork.UserRepository.Create(connectionString, new User(email, password, firstname, lastname, address, type));

            createdUser.Address = address;

            createdUser.Usertype = type;

            if (createdUser.Id == 0)
                throw new ArgumentException("Creating user failed!");

            string accountActivationCode = RandomGenerator(10);

            await _unitOfWork.UserRepository.UploadAccountActivationCodeToDb(connectionString, createdUser.Id, accountActivationCode);
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

        private async Task ValidateEmail(string connectionString, string email)
        {
            if (!isEmailValidFormat.IsMatch(email))
                throw new ParameterException("Email not formatted correctly!");

            if (!await _unitOfWork.UserRepository.IsEmailAvailable(connectionString, email))
                throw new ParameterException("Email is not available!");
        }

        private async Task ValidatePassword(string connectionString, string password)
        {
            string policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicy(connectionString);

            if (policy == "none")
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

        private async Task<Address> CreateAddress(string connectionString, string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            Address address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddress(connectionString, address);

            Address createdAddress = await _unitOfWork.AddressRepository.Create(connectionString, address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            return createdAddress;
        }

        public async Task AddAddressToExisitingUser(
            string connectionString, int userId, string streetAdr, string buildingNumber, string zip, string area,
            string city, string country)
        {
            Address address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddress(connectionString, address);

            Address createdAddress = await _unitOfWork.AddressRepository.Create(connectionString, address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            await _unitOfWork.UserRepository.AddUserAddress(connectionString, userId, createdAddress.Id);

            User user = await GetUserById(connectionString, userId);

            if (user.Address == null)
                throw new ParameterException("Address could not be assigned to user!");

        }

        public async Task ChangeAddressOfUser(string connectionString, Address address)
        {
            await ValidateAddress(connectionString, address);

            await _unitOfWork.AddressRepository.Update(connectionString, address.Id, address.Street, address.Number, address.Zip,
                address.Area, address.City, address.Country);
        }

        private async Task ValidateAddress(string connectionString, Address address)
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

        public async Task ChangeNameOfUser(string connectionString, int userId, string firstname, string lastname)
        {
            ValidateName(firstname, lastname);

            var user = new User { Id = userId, Firstname = firstname, Lastname = lastname };

            await _unitOfWork.UserRepository.UpdateName(connectionString, user);
        }

        public async Task ChangeEmail(string connectionString, int userId, string email)
        {
            await ValidateEmail(connectionString, email);

            await _unitOfWork.UserRepository.UpdateEmail(connectionString, new User { Id = userId, Email = email });
        }

        public async Task ChangePassword(string connectionString, string email, string old, string new1, string new2)
        {
            if (old == new1)
                throw new PasswordChangeException();

            if (new1 != new2)
                throw new PasswordChangeException("new");

            await ValidatePassword(connectionString, new1);

            User user = await _unitOfWork.UserRepository.GetByEmail(connectionString, email);

            if (VerifyThePassword(old, user.Password))
            {
                user.Password = HashThePassword(new1, null, false);

                await _unitOfWork.UserRepository.ChangePassword(connectionString, user.Id, user.Password);
            }
            else
                throw new PasswordChangeException("old");
        }

        public async Task<string> GenerateRandomPassword(string connectionString, int length)
        {
            string policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicy(connectionString);

            if (policy != "none")
            {
                if (length < 8)
                    throw new ParameterException("Password length", "shorter than 8 characters");
            }
            string password = RandomGenerator(length);

            Regex currentPasswordRegex = new Regex("");

            if (policy == "none")
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

        public async Task ActivateUser(string connectionString, int userId, string activationCode)
        {
            await _unitOfWork.UserRepository.ActivateAccount(connectionString, userId, activationCode);
        }

        public async Task ForgotPassword(string connectionString, int userId)
        {
            User user = await GetUserById(connectionString, userId);

            string newPass = await GenerateRandomPassword(connectionString, 8);

            newPass = HashThePassword(newPass, null, false);

            await _unitOfWork.UserRepository.ForgottenPassword(connectionString, userId, newPass);
        }

        public async Task<User> GetUserByEmail(string connectionString, string email)
        {
            User user = await _unitOfWork.UserRepository.GetByEmail(connectionString, email);

            return user;
        }

        public async Task SetPasswordAfterGettingTemporaryPassword(string connectionString, string email, string temporaryPassword, string newPassword, string newPasswordConfirmed)
        {

            User user = await GetUserByEmail(connectionString, email);
            
            if (!user.MustChangePassword)
                throw new PasswordChangeException("You have not requested a temporary password after forgetting your password!");

            await ChangePassword(connectionString, email, temporaryPassword, newPassword, newPasswordConfirmed);
            
            await _unitOfWork.UserRepository.ResetTempPassword(connectionString, user.Password, user.Id);
        }

        public async Task<User> GetUserById(string connectionString, int userId)
        {
            User user = new User();

            user = await _unitOfWork.UserRepository.GetById(connectionString, userId);

            if (user.Id == 0)
                throw new IdNotFoundException("User", userId);

            return user;
        }

        public async Task<List<User>> GetAllUsers(string connectionString)
        {
            List<User> users = new List<User>();

            users = await _unitOfWork.UserRepository.GetAll(connectionString);

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task<List<User>> GetAllUsersWithSameUsertype(string connectionString, string usertype)
        {
            List<User> users = new List<User>();

            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertype(connectionString, usertype);

            users = await _unitOfWork.UserRepository.GetAllOfAGivenType(connectionString, type.Id);

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        public async Task DeleteUser(string connectionString, int userId)
        {
            var user = await GetUserById(connectionString, userId);

            await _unitOfWork.UserRepository.Delete(connectionString, userId);

            await _unitOfWork.AddressRepository.Delete(connectionString, user.Address.Id);
        }

        public async Task Login(string connectionString, string email, string password)
        {
            foreach (var user in await GetAllUsers(connectionString))
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

        public async Task AddMoreUsertypes(string connectionString, params string[] userTypes)
        {
            List<Usertype> types = await _unitOfWork.UsertypeRepository.Create(connectionString, userTypes);

            if (types.Count == 0)
                throw new FailedToCreateException("Usertype");
        }

        public async Task<List<Usertype>> GetAllUsertypes(string connectionString)
        {
            List<Usertype> allTypes = new List<Usertype>();
            
            allTypes = await _unitOfWork.UsertypeRepository.GetAll(connectionString);

            if (allTypes.Count == 0)
                throw new NoneFoundInDatabaseTableException("usertpes");

            return allTypes;
        }
    }
}
