using ManageUsers.CustomExceptions;
using ManageUsers.Model;
using ManageUsers.UOW;
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
using static ManageUsers.Helper.AllCities;
using static ManageUsers.Helper.Email;
using static ManageUsers.Helper.PasswordHelper;
using static ManageUsers.Helper.RegexChecker;

namespace ManageUsers
{
    /// <summary>
    /// A class containing all the methods that are used to manage users in the library.
    /// </summary>
    public class UserManager
    {
        private UnitOfWork _unitOfWork;

        internal UserManager(string connectionString)
        {
            _unitOfWork = new UnitOfWork(connectionString);
        }

        /// <summary>
        /// Creates a new user and inserts it into the database.
        /// </summary>
        /// <param name="user">The user object to create and insert into the database.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password do not pass regex validation</exception>
        /// <exception cref="ParameterException">Thrown if the email is already taken</exception>
        /// <exception cref="ParameterException">Thrown if the password confirmation fails</exception>
        /// <exception cref="FailedToCreateException">Thrown if the user is not inserted to the database</exception>
        /// <exception cref="FailedToCreateException">Thrown if an address must be created but the creation of fails</exception>
        /// <exception cref="GeographicalException">Thrown if an address must be created but the creation of fails because the validation of the city and/or coutry</exception>
        public async Task CreateUserAsync(User user, string passwordConfirmed)
        {
            ValidateName(user.Firstname, user.Lastname);

            await ValidateEmailAsync(user.Email);

            await ValidatePasswordAsync(user.Password);

            if (user.Password != passwordConfirmed)
                throw new ParameterException("The passwords don't match!");

            if (user.Address != null)
                user.Address = await CreateAddressAsync(user.Address.Street, user.Address.Number, user.Address.Zip, user.Address.Area, user.Address.City, user.Address.Country);

            user.Usertype = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(user.Usertype.Type);

            user.Password = HashThePassword(user.Password, null, false);

            var createdUser = await _unitOfWork.UserRepository.CreateAsync(user);

            if (createdUser.Id == 0)
                throw new FailedToCreateException("User");

            if (!user.IsActivated)
            {
                var accountActivationCodeUnhashed = RandomGenerator(10);

                var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

                await _unitOfWork.UserRepository.UploadAccountActivationCodeToDbAsync(createdUser.Id, accountActivationCodeHashed);

                EmailSender("aintbnb@outlook.com", "juSt@RandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");
            }
        }

        /// <summary>
        /// Creates a new user and inserts it into the database. The address of the user will be null. No usertype set, so the default usertype "User" is assigned to the user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <param name="firstname">The firstname of the user.</param>
        /// <param name="lastname">The lastname of the user.</param>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password do not pass regex validation</exception>
        /// <exception cref="ParameterException">Thrown if the email is already taken</exception>
        /// <exception cref="ParameterException">Thrown if the password confirmation fails</exception>
        /// <exception cref="FailedToCreateException">Thrown if the user is not inserted to the database</exception>
        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname)
        {
            await CreateUserAsync(email, password, passwordConfirmed, firstname, lastname, "User");
        }

        /// <summary>
        /// Creates a new user and inserts it into the database. The address of the user will be null.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="passwordConfirmed">The password must be confirmed before it can be set.</param>
        /// <param name="firstname">The firstname of the user.</param>
        /// <param name="lastname">The lastname of the user.</param>
        /// <param name="usertype">The usertype/role of the user.</param>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password do not pass regex validation</exception>
        /// <exception cref="ParameterException">Thrown if the email is already taken</exception>
        /// <exception cref="ParameterException">Thrown if the password confirmation fails</exception>
        /// <exception cref="FailedToCreateException">Thrown if the user is not inserted to the database</exception>
        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname, string usertype)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype(usertype) };

            await CreateUserAsync(user, passwordConfirmed);
        }

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
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password or address properties are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password or address properties do not pass regex validation</exception>
        /// <exception cref="ParameterException">Thrown if the email is already taken</exception>
        /// <exception cref="ParameterException">Thrown if the password confirmation fails</exception>
        /// <exception cref="FailedToCreateException">Thrown if the user is not inserted to the database</exception>
        /// <exception cref="FailedToCreateException">Thrown if the creation of the address fails</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            await CreateUserAsync(email, password, passwordConfirmed, firstname, lastname, streetAdr, buildingNumber, zip, area, city, country, "User");
        }

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
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password or address properties are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname, lastname, email, password or address properties do not pass regex validation</exception>
        /// <exception cref="ParameterException">Thrown if the email is already taken</exception>
        /// <exception cref="ParameterException">Thrown if the password confirmation fails</exception>
        /// <exception cref="FailedToCreateException">Thrown if the user is not inserted to the database</exception>
        /// <exception cref="FailedToCreateException">Thrown if the creation of the address fails</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task CreateUserAsync(string email, string password, string passwordConfirmed, string firstname, string lastname,
            string streetAdr, string buildingNumber, string zip, string area, string city, string country, string usertype)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype(usertype) };

            user.Address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await CreateUserAsync(user, passwordConfirmed);
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

            if (!await _unitOfWork.UserRepository.IsEmailAvailableAsync(email))
                throw new ParameterException("Email is not available!");
        }

        private async Task ValidatePasswordAsync(string password)
        {
            var policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicyAsync();

            if (String.IsNullOrEmpty(password))
                throw new ParameterException("Password", "null");

            if (password.Contains(" "))
                throw new ParameterException("Password can't contain space!");

            if (policy == "default")
            {
                if (password.Length < 6)
                    throw new ParameterException("Password", "shorter than 6");
            }
            else if (policy == "first")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetter.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number and letter!");
            }
            else if (policy == "second")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetterOneUpperAndLowerCase.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number andat least one upper- and one lowercase letter!");
            }
            else if (policy == "third")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacter.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number, letter and special character!");
            }
            else if (policy == "fourth")
            {
                if (!passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacterOneUpperAndLowerCase.IsMatch(password))
                    throw new ParameterException("Password must be at least 8 characters long with at least one number, at least one upper- and one lowercase letter and special character!");
            }
        }

        private async Task<Address> CreateAddressAsync(string streetAdr, string buildingNumber, string zip, string area, string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            await ValidateAddressAsync(address);

            var createdAddress = await _unitOfWork.AddressRepository.CreateAsync(address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            return createdAddress;
        }

        /// <summary>
        /// Sets the address of an existing user.
        /// </summary>
        /// <param name="user">The user object to add the address for. The address object of the user object will be the one set for the existing user.</param>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="FailedToCreateException">Thrown if the creation of the address fails</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task AddAddressToExisitingUserAsync(User user)
        {
            await AddAddressToExisitingUserAsync(user, user.Address);
        }

        /// <summary>
        /// Sets the address of an existing user.
        /// </summary>
        /// <param name="user">The user object to add the address for.</param>
        /// <param name="address">The address object to add to the existing user.</param>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="FailedToCreateException">Thrown if the creation of the address fails</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task AddAddressToExisitingUserAsync(User user, Address address)
        {
            await ValidateAddressAsync(address);

            var createdAddress = await _unitOfWork.AddressRepository.CreateAsync(address);

            if (createdAddress == null)
                throw new FailedToCreateException("Address");

            await _unitOfWork.UserRepository.AddUserAddressAsync(user.Id, createdAddress.Id);

            var usr = await GetUserAsync(user.Email);

            if (usr.Address == null)
                throw new ParameterException("Address could not be assigned to user!");
        }

        /// <summary>
        /// Sets the address of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the existing user to add the address for.</param>
        /// <param name="streetAdr">The street the user's address.</param>
        /// <param name="buildingNumber">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="FailedToCreateException">Thrown if the creation of the address fails</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task AddAddressToExisitingUserAsync(
            int userId, string streetAdr, string buildingNumber, string zip, string area,
            string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            var user = await GetUserAsync(userId);

            await AddAddressToExisitingUserAsync(user, address);
        }

        /// <summary>
        /// Sets the address of an existing user.
        /// </summary>
        /// <param name="email">The email of the existing user to add the address for.</param>
        /// <param name="streetAdr">The street the user's address.</param>
        /// <param name="buildingNumber">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="FailedToCreateException">Thrown if the creation of the address fails</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task AddAddressToExisitingUserAsync(
            string email, string streetAdr, string buildingNumber, string zip, string area,
            string city, string country)
        {
            var address = new Address(streetAdr, buildingNumber, zip, area, city, country);

            var user = await GetUserAsync(email);

            await AddAddressToExisitingUserAsync(user, address);
        }

        /// <summary>
        /// Changes the existing address of an existing user.
        /// </summary>
        /// <param name="userEmail">The email of the existing user to add the address for.</param>
        /// <param name="street">The street the user's address.</param>
        /// <param name="number">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="NoAddressException">Thrown if the user doesn't have an address</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task ChangeAddressOfUserAsync(string userEmail, string street, string number, string zip, string area, string city, string country)
        {
            var user = await GetUserAsync(userEmail);

            if (user.Address == null)
                throw new NoAddressException();

            var address = new Address(street, number, zip, area, city, country);

            address.Id = user.Address.Id;

            await ChangeAddressOfUserAsync(address);
        }

        /// <summary>
        /// Changes the existing address of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the existing user to add the address for.</param>
        /// <param name="street">The street the user's address.</param>
        /// <param name="number">The number of the user's residence.</param>
        /// <param name="zip">The zip code the user's address.</param>
        /// <param name="area">The area the user's address.</param>
        /// <param name="city">The city of the user's address.</param>
        /// <param name="country">The country of the user's address.</param>
        /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="NoAddressException">Thrown if the user doesn't have an address</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task ChangeAddressOfUserAsync(int userId, string street, string number, string zip, string area, string city, string country)
        {
            var user = await GetUserAsync(userId);

            if (user.Address == null)
                throw new NoAddressException();

            var address = new Address(street, number, zip, area, city, country);

            address.Id = user.Address.Id;

            await ChangeAddressOfUserAsync(address);
        }

        /// <summary>
        /// Changes the existing address of an existing user.
        /// </summary>
        /// <param name="address">An address object with the updated address.</param>
        /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
        /// <exception cref="ParameterException">Thrown if the address properties are null or fail regex validation</exception>
        /// <exception cref="NoAddressException">Thrown if the user doesn't have an address</exception>
        /// <exception cref="GeographicalException">Thrown if the address creation of fails because the validation of the city and/or coutry</exception>
        public async Task ChangeAddressOfUserAsync(Address address)
        {
            if (address == null)
                throw new NoAddressException();

            await ValidateAddressAsync(address);

            await _unitOfWork.AddressRepository.UpdateAsync(address.Id, address.Street, address.Number, address.Zip,
                address.Area, address.City, address.Country);
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
            await IsCountryAndCityCorrect(_unitOfWork.SQLiteConnection.ConnectionString, address.Country, address.City);
        }

        /// <summary>
        /// Adds a picture to an existing user.
        /// </summary>
        /// <param name="user">The user object of the user to upload the picture for.</param>
        /// <param name="picturePath">The file path of the picture to upload.</param>
        public async Task AddUserPictureAsync(User user, string picturePath)
        {
            var img = ConvertImageToBytes(Image.FromFile(picturePath));
            await _unitOfWork.UserRepository.AddUserPicturesAsync(user, img);
        }

        /// <summary>
        /// Adds a picture to an existing user.
        /// </summary>
        /// <param name="userEmail">The email of the user to upload the picture for.</param>
        /// <param name="picturePath">The file path of the picture to upload.</param>
        public async Task AddUserPictureAsync(string userEmail, string picturePath)
        {
            var user = await GetUserAsync(userEmail);

            await AddUserPictureAsync(user, picturePath);
        }

        /// <summary>
        /// Adds a picture to an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to upload the picture for.</param>
        /// <param name="picturePath">The file path of the picture to upload.</param>
        public async Task AddUserPictureAsync(int userId, string picturePath)
        {
            var user = await GetUserAsync(userId);

            await AddUserPictureAsync(user, picturePath);
        }

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<Image> GetUserPictureAsync(User user, string picturePath)
        {
            var pics = await _unitOfWork.UserRepository.GetPicturesOfUserAsync(user);

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

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userId">The ID of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index of the picture to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<Image> GetUserPictureAsync(int userId, int indexOfPicture)
        {
            var user = await GetUserAsync(userId);

            return await GetUserPictureAsync(user, indexOfPicture);
        }

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userEmail">The email of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index of the picture to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<Image> GetUserPictureAsync(string userEmail, int indexOfPicture)
        {
            var user = await GetUserAsync(userEmail);

            return await GetUserPictureAsync(user, indexOfPicture);
        }

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index of the picture to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<Image> GetUserPictureAsync(User user, int indexOfPicture)
        {
            var pics = await _unitOfWork.UserRepository.GetPicturesOfUserAsync(user);

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

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userId">The ID of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<Image> GetUserPictureAsync(int userId, string picturePath)
        {
            var user = await GetUserAsync(userId);

            return await GetUserPictureAsync(user, picturePath);
        }

        /// <summary>
        /// Fetches a image of a user.
        /// </summary>
        /// <returns>The image object of the requested picture.</returns>
        /// <param name="userEmail">The email of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<Image> GetUserPictureAsync(string userEmail, string picturePath)
        {
            var user = await GetUserAsync(userEmail);

            return await GetUserPictureAsync(user, picturePath);
        }

        /// <summary>
        /// Fetches all the images of a user.
        /// </summary>
        /// <returns>All the user's pictures in a list of image objects.</returns>
        /// <param name="user">The user object of the user to get the pictures of.</param>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        public async Task<List<Image>> GetAllUserPictureAsync(User user)
        {
            var pics = await _unitOfWork.UserRepository.GetPicturesOfUserAsync(user);

            if (pics == null || pics.Count == 0)
                throw new NotFoundException("User pictures");

            var images = new List<Image>();

            foreach (var pic in pics)
            {
                images.Add(ConvertBytesToImage(pic));
            }

            return images;
        }

        /// <summary>
        /// Fetches all the images of a user.
        /// </summary>
        /// <returns>All the user's pictures in a list of image objects.</returns>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<List<Image>> GetAllUserPictureAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            return await GetAllUserPictureAsync(user);
        }

        /// <summary>
        /// Fetches all the images of a user.
        /// </summary>
        /// <returns>All the user's pictures in a list of image objects.</returns>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="NotFoundException">Thrown if the user doesn't have any pictures</exception>
        /// <exception cref="NotFoundException">Thrown if the picture was not found</exception>
        public async Task<List<Image>> GetAllUserPictureAsync(string userEmail)
        {
            var user = await GetUserAsync(userEmail);

            return await GetAllUserPictureAsync(user);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="picturePath">The file path of the picture to delete.</param>       
        public async Task DeleteAUserPictureAsync(User user, string picturePath)
        {
            var img = ConvertImageToBytes(Image.FromFile(picturePath));

            await _unitOfWork.UserRepository.DeleteAPictureAsync(user, img);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <param name="picturePath">The file path of the picture to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAUserPictureAsync(int userId, string picturePath)
        {
            var user = await GetUserAsync(userId);

            await DeleteAUserPictureAsync(user, picturePath);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <param name="picturePath">The file path of the picture to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAUserPictureAsync(string userEmail, string picturePath)
        {
            var user = await GetUserAsync(userEmail);

            await DeleteAUserPictureAsync(user, picturePath);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="indexOfPicture">The index picture to delete.</param>
        public async Task DeleteAUserPictureAsync(User user, int indexOfPicture)
        {
            await _unitOfWork.UserRepository.DeleteAPictureAsync(user, indexOfPicture);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <param name="indexOfPicture">The index picture to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAUserPictureAsync(int userId, int indexOfPicture)
        {
            var user = await GetUserAsync(userId);

            await DeleteAUserPictureAsync(user, indexOfPicture);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <param name="indexOfPicture">The index picture to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAUserPictureAsync(string userEmail, int indexOfPicture)
        {
            var user = await GetUserAsync(userEmail);

            await DeleteAUserPictureAsync(user, indexOfPicture);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        /// <param name="imageToRemove">The image object of the picture to be deleted.</param> 
        public async Task DeleteAUserPictureAsync(User user, Image imageToRemove)
        {
            var img = ConvertImageToBytes(imageToRemove);

            await _unitOfWork.UserRepository.DeleteAPictureAsync(user, img);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <param name="imageToRemove">The image object of the picture to be deleted.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAUserPictureAsync(int userId, Image imageToRemove)
        {
            var user = await GetUserAsync(userId);

            await DeleteAUserPictureAsync(user, imageToRemove);
        }

        /// <summary>
        /// Delets a picture of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <param name="imageToRemove">The image object of the picture to be deleted.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAUserPictureAsync(string userEmail, Image imageToRemove)
        {
            var user = await GetUserAsync(userEmail);

            await DeleteAUserPictureAsync(user, imageToRemove);
        }

        /// <summary>
        /// Delets all the pictures of a user.
        /// </summary>
        /// <param name="user">The user object of the owner of the picture.</param>
        public async Task DeleteAllUserPicturesAsync(User user)
        {
            await _unitOfWork.UserRepository.DeleteAllPicturesAsync(user);
        }

        /// <summary>
        /// Delets all the pictures of a user.
        /// </summary>
        /// <param name="userId">The ID of the owner of the pictures.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteAllUserPicturesAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await DeleteAllUserPicturesAsync(user);
        }

        /// <summary>
        /// Delets all the pictures of a user.
        /// </summary>
        /// <param name="userEmail">The email of the owner of the pictures.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
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

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="user">The user object to updates the names of; the names set in the user object will be the updated values.</param>
        /// <exception cref="ParameterException">Thrown if the firstname or lastname are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname or lastname do not pass regex validation</exception>
        public async Task UpdateUserAsync(User user)
        {
            ValidateName(user.Firstname, user.Lastname);

            await _unitOfWork.UserRepository.UpdateNameAsync(user);
        }

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to update the name of.</param>
        /// <param name="updatedFirstname">The updated firstname of the user.</param>
        /// <param name="updatedLastname">The updated lastname of the user.</param>
        /// <exception cref="NotFoundException">Thrown if the user isn't found</exception>
        /// <exception cref="ParameterException">Thrown if the firstname or lastname are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname or lastname do not pass regex validation</exception>
        public async Task UpdateUserAsync(int userId, string updatedFirstname, string updatedLastname)
        {
            var user = await GetUserAsync(userId);

            user.Firstname = updatedFirstname;
            user.Lastname = updatedLastname;

            await UpdateUserAsync(user);
        }

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="email">The email of the user to update the name of.</param>
        /// <param name="updatedFirstname">The updated firstname of the user.</param>
        /// <param name="updatedLastname">The updated lastname of the user.</param>
        /// <exception cref="NotFoundException">Thrown if the user isn't found</exception>
        /// <exception cref="ParameterException">Thrown if the firstname or lastname are null</exception>
        /// <exception cref="ParameterException">Thrown if the firstname or lastname do not pass regex validation</exception>
        public async Task UpdateUserAsync(string email, string updatedFirstname, string updatedLastname)
        {
            var user = await GetUserAsync(email);

            user.Firstname = updatedFirstname;
            user.Lastname = updatedLastname;

            await UpdateUserAsync(user);
        }

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="user">The user object to updates the email of.</param>
        /// <param name="updatedEmail">The updated email of the user.</param>
        /// <exception cref="ParameterException">Thrown if the email is null</exception>
        /// <exception cref="ParameterException">Thrown if the email doesn't pass regex validation</exception>
        public async Task UpdateUserAsync(User user, string updatedEmail)
        {
            await ValidateEmailAsync(updatedEmail);

            user.Email = updatedEmail;

            await _unitOfWork.UserRepository.UpdateEmailAsync(user);
        }

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="originalEmail">The current email of the user to update the email of.</param>
        /// <param name="updatedEmail">The updated email of the user.</param>
        /// <exception cref="NotFoundException">Thrown if the user isn't found</exception>
        /// <exception cref="ParameterException">Thrown if the email is null</exception>
        /// <exception cref="ParameterException">Thrown if the email doesn't pass regex validation</exception>
        public async Task UpdateUserAsync(string originalEmail, string updatedEmail)
        {
            var user = await GetUserAsync(originalEmail);

            await UpdateUserAsync(user, updatedEmail);
        }

        /// <summary>
        /// Updates first and lastname of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to update the email of.</param>
        /// <param name="updatedEmail">The updated email of the user.</param>
        /// <exception cref="NotFoundException">Thrown if the user isn't found</exception>
        /// <exception cref="ParameterException">Thrown if the email is null</exception>
        /// <exception cref="ParameterException">Thrown if the email doesn't pass regex validation</exception>
        public async Task UpdateUserAsync(int userId, string updatedEmail)
        {
            var user = await GetUserAsync(userId);

            await UpdateUserAsync(user, updatedEmail);
        }

        /// <summary>
        /// Changes the password of an existing user.
        /// </summary>
        /// <param name="email">The email of the user to update the email of.</param>
        /// <param name="old">The current password of the user.</param>
        /// <param name="new1">The new password of the user.</param>
        /// <param name="new2">The new password of the user must be confirmed to be set.</param>
        /// <exception cref="NotFoundException">Thrown if the user isn't found</exception>
        /// <exception cref="PasswordChangeException">Thrown if the current and new passwords are equal</exception>
        /// <exception cref="PasswordChangeException">Thrown if the password confirmation for the new password fails</exception>
        /// <exception cref="PasswordChangeException">Thrown if the password is wrong</exception>
        /// <exception cref="ParameterException">Thrown if the password policy validation fails</exception>
        public async Task ChangePasswordAsync(string email, string old, string new1, string new2)
        {
            if (old == new1)
                throw new PasswordChangeException();

            if (new1 != new2)
                throw new PasswordChangeException("new");

            await ValidatePasswordAsync(new1);

            var user = await GetUserAsync(email);

            if (VerifyThePassword(old, user.Password))
            {
                user.Password = HashThePassword(new1, null, false);

                await _unitOfWork.UserRepository.ChangePasswordAsync(user.Id, user.Password);
            }
            else
                throw new PasswordChangeException("old");
        }

        /// <summary>
        /// Generates a random string that can be used as a password accepted by the current password policy.
        /// </summary>
        /// <returns>The randomly generated value as a string.</returns>
        /// <param name="length">The length of the randomly generated string.</param>
        /// <exception cref="ParameterException">Thrown if length parameter is smaller than the minimum password policy length</exception>
        public async Task<string> GenerateRandomPasswordAsync(int length)
        {
            var policy = await _unitOfWork.PasswordPolicyRepository.GetPasswordPolicyAsync();

            if (policy != "default")
            {
                if (length < 8)
                    throw new ParameterException("Random password length", "shorter than 8 characters");
            }
            var password = RandomGenerator(length);

            var currentPasswordRegex = new Regex("");

            if (policy == "default")
                if (length < 6)
                    throw new ParameterException("Random password length", "shorter than 6 characters");
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

        /// <summary>
        /// Activate the account of a user account by entering the activation code sent to the email of the user when creating the account.
        /// </summary>
        /// <param name="user">The user object to activate the account of.</param>
        /// <param name="activationCode">The activation code to activate the account with.</param>
        /// <exception cref="ParameterException">Thrown if account is already activated</exception>
        /// <exception cref="ParameterException">Thrown if the activation code is wrong</exception>
        public async Task ActivateUserAsync(User user, string activationCode)
        {
            if (user.IsActivated)
                throw new ParameterException("User is already activated!");

            if (VerifyThePassword(activationCode.Trim(), await _unitOfWork.UserRepository.GetActivationCodeAsync(user.Id)))
                await _unitOfWork.UserRepository.ActivateAccountAsync(user.Id);
            else
                throw new ParameterException("Activation code is incorrect!");
        }

        /// <summary>
        /// Activate the account of a user account by entering the activation code sent to the email of the user when creating the account.
        /// </summary>
        /// <param name="userId">The ID of the user to activate the account of.</param>
        /// <param name="activationCode">The activation code to activate the account with.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="ParameterException">Thrown if account is already activated</exception>
        /// <exception cref="ParameterException">Thrown if the activation code is wrong</exception>
        public async Task ActivateUserAsync(int userId, string activationCode)
        {
            var user = await GetUserAsync(userId);

            await ActivateUserAsync(user, activationCode);
        }

        /// <summary>
        /// Activate the account of a user account by entering the activation code sent to the email of the user when creating the account.
        /// </summary>
        /// <param name="email">The email of the user to activate the account of.</param>
        /// <param name="activationCode">The activation code to activate the account with.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="ParameterException">Thrown if account is already activated</exception>
        /// <exception cref="ParameterException">Thrown if the activation code is wrong</exception>
        public async Task ActivateUserAsync(string email, string activationCode)
        {
            var user = await GetUserAsync(email);

            await ActivateUserAsync(user, activationCode);
        }

        /// <summary>
        /// Sends a new activation code to the email of the user that must be used to activate the user's account.
        /// </summary>
        /// <param name="user">The user object of the user to send the activation to.</param>
        public async Task ResendAccountActivationCodeAsync(User user)
        {
            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            EmailSender("aintbnb@outlook.com", "juSt@RandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await _unitOfWork.UserRepository.ResendAccountActivationCodeAsync(user.Id, accountActivationCodeHashed);
        }

        /// <summary>
        /// Sends a new activation code to the email of the user that must be used to activate the user's account.
        /// </summary>
        /// <param name="userEmail">The email of the user to activate the account of.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task ResendAccountActivationCodeAsync(string userEmail)
        {
            var user = await GetUserAsync(userEmail);

            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            EmailSender("aintbnb@outlook.com", "juSt@RandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await _unitOfWork.UserRepository.ResendAccountActivationCodeAsync(user.Id, accountActivationCodeHashed);
        }

        /// <summary>
        /// Sends a new activation code to the email of the user that must be used to activate the user's account.
        /// </summary>
        /// <param name="userId">The ID of the user to activate the account of.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task ResendAccountActivationCodeAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            var accountActivationCodeUnhashed = RandomGenerator(10);

            var accountActivationCodeHashed = HashThePassword(accountActivationCodeUnhashed, null, false);

            EmailSender("aintbnb@outlook.com", "juSt@RandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Account activation code", "<h1>Your account activation code</h1> <p>Your account activation code is: </p> <p>" + accountActivationCodeUnhashed + "</p>");

            await _unitOfWork.UserRepository.ResendAccountActivationCodeAsync(user.Id, accountActivationCodeHashed);
        }

        /// <summary>
        /// Sets the user's password to a new randomly generated temporary password and sends it by email to the user.
        /// </summary>
        /// <param name="user">The user object of the user to send the new randomly generated password to.</param>
        public async Task ForgotPasswordAsync(User user)
        {
            var newPassUnhashed = await GenerateRandomPasswordAsync(8);

            var newPass = HashThePassword(newPassUnhashed, null, false);

            await _unitOfWork.UserRepository.ForgottenPasswordAsync(user.Id, newPass);

            EmailSender("aintbnb@outlook.com", "juSt@RandOmpassWordForSkewl", user.Email, "smtp.office365.com", 587, "Temporary password", "<h1>Your one-time password</h1> <p>Your temporary password is: </p> <p>" + newPassUnhashed + "</p>");
        }

        /// <summary>
        /// Sets the user's password to a new randomly generated temporary password and sends it by email to the user.
        /// </summary>
        /// <param name="email">The email of the user to send the new randomly generated password to.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await GetUserAsync(email);

            await ForgotPasswordAsync(user);
        }

        /// <summary>
        /// Sets the user's password to a new randomly generated temporary password and sends it by email to the user.
        /// </summary>
        /// <param name="userId">The ID of the user to send the new randomly generated password to.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task ForgotPasswordAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await ForgotPasswordAsync(user);
        }

        /// <summary>
        /// Fetches a user from the database.
        /// </summary>
        /// <returns>A user object of the requested user.</returns>
        /// <param name="email">The email of the user to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task<User> GetUserAsync(string email)
        {
            User user;

            try
            {
                user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            }
            catch (ArgumentOutOfRangeException)
            {
                user = await _unitOfWork.UserRepository.GetByEmailAddressNullAsync(email);
            }

            return user;
        }

        /// <summary>
        /// Fetches a user from the database.
        /// </summary>
        /// <returns>A user object of the requested user.</returns>
        /// <param name="userId">The ID of the user to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task<User> GetUserAsync(int userId)
        {
            User user;

            try
            {
                user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            }
            catch (ArgumentOutOfRangeException)
            {
                user = await _unitOfWork.UserRepository.GetByIdAddressNullAsync(userId);
            }

            return user;
        }

        /// <summary>
        /// Sets the new password after a user has forgotten their password and entered their temporary password that was sent to their email.
        /// </summary>
        /// <param name="email">The email of the user setting their new password.</param>
        /// <param name="temporaryPassword">The temporary password received on email.</param>
        /// <param name="newPassword">The new password of the user.</param>
        /// <param name="newPasswordConfirmed">The new password of the user must be confirmed to be set.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        /// <exception cref="PasswordChangeException">Thrown if the user must not change password</exception>
        /// <exception cref="PasswordChangeException">Thrown if the temporary and new passwords are equal</exception>
        /// <exception cref="PasswordChangeException">Thrown if the password confirmation for the new password fails</exception>
        /// <exception cref="PasswordChangeException">Thrown if the temporary password is wrong</exception>
        /// <exception cref="ParameterException">Thrown if the password policy validation fails</exception>
        public async Task SetPasswordAfterGettingTemporaryPassword(string email, string temporaryPassword, string newPassword, string newPasswordConfirmed)
        {
            var user = await GetUserAsync(email);

            if (!user.MustChangePassword)
                throw new PasswordChangeException("You have not requested a temporary password after forgetting your password!");

            await ChangePasswordAsync(email, temporaryPassword, newPassword, newPasswordConfirmed);

            await _unitOfWork.UserRepository.ResetTempPasswordAsync(user.Password, user.Id);
        }

        /// <summary>
        /// Fetch all the users in the database.
        /// </summary>
        /// <returns>All the users in the database in a list with user objects.</returns>
        /// <exception cref="NoneFoundInDatabaseTableException">Thrown if no users found</exception>
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            users = await _unitOfWork.UserRepository.GetAllAsync();

            users = users.Concat(await _unitOfWork.UserRepository.GetAllAddressNullAsync()).ToList();

            users.Sort((x, y) => x.Id.CompareTo(y.Id));

            if (users.Count == 0)
                throw new NoneFoundInDatabaseTableException("users");

            return users;
        }

        /// <summary>
        /// Fetch all the users in the database of a given usertype.
        /// </summary>
        /// <returns>All the users in the database of the given usertype in a list with user objects.</returns>
        /// <param name="usertype">The usertype of the users to fetch.</param>
        /// <exception cref="NotFoundException">Thrown if no users with the usertype found</exception>
        public async Task<List<User>> GetAllUsersByUsertypeAsync(string usertype)
        {
            var users = new List<User>();

            var type = await _unitOfWork.UsertypeRepository.GetUsertypeAsync(usertype);

            users = await _unitOfWork.UserRepository.GetAllOfAGivenTypeAsync(type.Id);

            users = users.Concat(await _unitOfWork.UserRepository.GetAllOfAGivenTypeAddressNullAsync(type.Id)).ToList();

            users.Sort((x, y) => x.Id.CompareTo(y.Id));

            if (users.Count == 0)
                throw new NotFoundException($"Users with type {type.Type}");

            return users;
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="user">The user object of the user to delete.</param>
        public async Task DeleteUserAsync(User user)
        {
            await _unitOfWork.UserRepository.DeleteAsync(user.Id);

            if (user.Address != null)
                await _unitOfWork.AddressRepository.DeleteAsync(user.Address.Id);
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteUserAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            await DeleteUserAsync(user);
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="email">The email of the user to delete.</param>
        /// <exception cref="NotFoundException">Thrown if the user wasn't found</exception>
        public async Task DeleteUserAsync(string email)
        {
            var user = await GetUserAsync(email);

            await DeleteUserAsync(user);
        }

        /// <summary>
        /// Checks if the user entered the correct credentials for login.
        /// </summary>
        /// <param name="email">The email of the user to login.</param>
        /// <param name="password">The password of the user to login.</param>
        /// <exception cref="NoneFoundInDatabaseTableException">Thrown if no users found</exception>
        /// <exception cref="LoginException">Thrown if the user hasn't activated their account</exception>
        /// <exception cref="LoginException">Thrown if the user must change their temporary password</exception>
        /// <exception cref="LoginException">Thrown if the email and/or password is wrong</exception>
        public async Task LoginAsync(string email, string password)
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

        /// <summary>
        /// Checks if the user entered the correct credentials for login and assigns a JWT token to the user if login is successful.
        /// </summary>
        /// <returns>A string conatining the generated JWT token.</returns>
        /// <param name="email">The email of the user to login.</param>
        /// <param name="password">The password of the user to login.</param>
        /// <param name="jwtSecretKey">The string with the secret key that will be used to validate the JWT token.</param>
        /// <exception cref="NoneFoundInDatabaseTableException">Thrown if no users found</exception>
        /// <exception cref="LoginException">Thrown if the user hasn't activated their account</exception>
        /// <exception cref="LoginException">Thrown if the user must change their temporary password</exception>
        /// <exception cref="LoginException">Thrown if the email and/or password is wrong</exception>
        public async Task<string> LoginAsync(string email, string password, string jwtSecretKey)
        {
            try
            {
                await LoginAsync(email, password);
                var user = await GetUserAsync(email);
                return generateJwtToken(user, jwtSecretKey);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string generateJwtToken(User user, string secret)
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

        /// <summary>
        /// Adds more usertypes to the already existing ones. By default the only usertypes are Admin and User.
        /// </summary>
        /// <param name="userTypes">A string that contains the names of the new usertypes. Seperate by comma if adding more than one new usertype.</param>
        /// <exception cref="FailedToCreateException">Thrown if no new usertypes were created</exception>
        public async Task AddMoreUsertypesAsync(params string[] userTypes)
        {
            var types = await _unitOfWork.UsertypeRepository.CreateAsync(userTypes);

            if (types.Count == 0)
                throw new FailedToCreateException("Usertype");
        }

        /// <summary>
        /// Fetches all the existing usertypes from the database.
        /// </summary>
        /// <returns>All the usertypes in a list with usertype objects.</returns>
        public async Task<List<Usertype>> GetAllUsertypesAsync()
        {
            var allTypes = await _unitOfWork.UsertypeRepository.GetAllAsync();

            return allTypes;
        }

        /// <summary>
        /// Serialize a user to csv, json or xml file.
        /// </summary>
        /// <param name="userToSerialize">The user object of the user to serlialize.</param>
        /// <param name="filePathToWriteTo">The file path of the file to write the serlialized object to. The file extension must be csv, json or xml.</param>
        public void SerializeToFile(User userToSerialize, string filePathToWriteTo)
        {
            SerliazeObjToFile(userToSerialize, filePathToWriteTo);
        }

        /// <summary>
        /// Serialize a list of users to csv, json or xml file.
        /// </summary>
        /// <param name="listOfUsersToSerialize">The list of user objects to serlialize.</param>
        /// <param name="filePathToWriteTo">The file path of the file to write the serlialized objects to. The file extension must be csv, json or xml.</param>
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


        /// <summary>
        /// Deserialize from csv, json or xml file and add the deserialized values to the database.
        /// </summary>
        /// <param name="filePathToReadFrom">The file path of the file to read and deserlialized objects from. The file extension must be csv, json or xml.</param>
        /// <exception cref="NotFoundException">Thrown if the file doesn't exist</exception>
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

        /// <summary>
        /// Serialize one or a list of users to xml string.
        /// </summary>
        /// <returns>Xml string with the serialized object.</returns>
        /// <param name="userObj">The user or list of users to serialize.</param>
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

        /// <summary>
        /// Serialize one or a list of users to json string.
        /// </summary>
        /// <returns>Json string with the serialized object.</returns>
        /// <param name="userObj">The user or list of users to serialize.</param>
        public string SerializeToJsonString(object userObj)
        {
            return JsonConvert.SerializeObject(userObj);
        }

        /// <summary>
        /// Serialize one or a list of users to csv string.
        /// </summary>
        /// <returns>Csv string with the serialized object.</returns>
        /// <param name="userObj">The user or list of users to serialize.</param>
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

        /// <summary>
        /// Deserialize from csv, json or xml formatted string and add the deserialized values to the database.
        /// </summary>
        /// <param name="stringToDeSerialize">The string to deserlialized objects from. The string must be formatted as csv, json or xml.</param>
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
                var xmlSerializer = new XmlSerializer(typeof(User[]));

                foreach (var user in (User[])xmlSerializer.Deserialize(new XmlNodeReader(xmlDocument)))
                    await CreateUserWithOrWithoutAddressAsync(user);
            }
            else
            {
                var xmlSerializer = new XmlSerializer(typeof(User));
                var user = (User)xmlSerializer.Deserialize(new XmlNodeReader(xmlDocument));
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

            await CreateUserAsync(user, user.Password);

            var createdUser = await GetUserAsync(user.Email);

            if (hasPics)
            {
                foreach (var pic in pics)
                    await _unitOfWork.UserRepository.AddUserPicturesAsync(createdUser, pic);
            }

            if (user.MustChangePassword)
                await _unitOfWork.UserRepository.ForgottenPasswordAsync(user.Id, originalPassword);
            else
                await _unitOfWork.UserRepository.ChangePasswordAsync(createdUser.Id, originalPassword);
        }
    }
}
