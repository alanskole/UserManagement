using ManageUsers.CustomExceptions;
using ManageUsers.Model;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ManageUsers.Helper.PasswordHelper;
using static ManageUsers.Helper.RegexChecker;

namespace Test
{
    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    internal class UserManagementTest
    {
        private static int? _testIdStatic;
        private int _testId;
        private string _connectionString;
        private UserManagementForTesting _userManagement;
        string email = "aa@aa.xx";
        string password = "aaaaaa";
        string firstname = "first";
        string lastname = "user";
        string defaultUsertype = "User";
        string adminUsertype = "Admin";
        string street = "fake";
        string number = "1a";
        string zip = "1111";
        string area = "blabla";
        string city = "Oslo";
        string country = "Norway";
        string userCsv = @"1,aa@aa.xx,uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==,first,user,0,,,,,,,False,False,2,User,";
        string userJson = "{\"Id\":1,\"Email\":\"aa@aa.xx\",\"Password\":\"uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==\",\"Firstname\":\"first\",\"Lastname\":\"user\",\"Address\":null,\"IsActivated\":false,\"MustChangePassword\":false,\"Usertype\":{\"Id\":2,\"Type\":\"User\"},\"Picture\":null}";
        string userXml =
            @"<?xml version=""1.0"" encoding=""utf-16""?>
<User xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Id>1</Id>
  <Email>aa@aa.xx</Email>
  <Password>uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==</Password>
  <Firstname>first</Firstname>
  <Lastname>user</Lastname>
  <IsActivated>false</IsActivated>
  <MustChangePassword>false</MustChangePassword>
  <Usertype>
    <Id>2</Id>
    <Type>User</Type>
  </Usertype>
</User>";
        string userAdrCsv = @"1,aa@aa.xx,uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==,first,user,1,fake,1a,1111,blabla,Oslo,Norway,False,False,2,User,";
        string userAdrJson = "{\"Id\":1,\"Email\":\"aa@aa.xx\",\"Password\":\"uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==\",\"Firstname\":\"first\",\"Lastname\":\"user\",\"Address\":{\"Id\":1,\"Street\":\"fake\",\"Number\":\"1a\",\"Zip\":\"1111\",\"Area\":\"blabla\",\"City\":\"Oslo\",\"Country\":\"Norway\"},\"IsActivated\":false,\"MustChangePassword\":false,\"Usertype\":{\"Id\":2,\"Type\":\"User\"},\"Picture\":null}";
        string userAdrXml =
            @"<?xml version=""1.0"" encoding=""utf-16""?>
<User xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Id>1</Id>
  <Email>aa@aa.xx</Email>
  <Password>uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==</Password>
  <Firstname>first</Firstname>
  <Lastname>user</Lastname>
  <Address>
    <Id>1</Id>
    <Street>fake</Street>
    <Number>1a</Number>
    <Zip>1111</Zip>
    <Area>blabla</Area>
    <City>Oslo</City>
    <Country>Norway</Country>
  </Address>
  <IsActivated>false</IsActivated>
  <MustChangePassword>false</MustChangePassword>
  <Usertype>
    <Id>2</Id>
    <Type>User</Type>
  </Usertype>
</User>";

        [SetUp]
        public async Task Setup()
        {
            if (_testIdStatic == null)
                _testIdStatic = 0;
            else
                _testIdStatic++;

            _testId = _testIdStatic.Value;

            _connectionString = $"Data Source=LibTest{_testId}.db;";
            _userManagement = new UserManagementForTesting(_connectionString, "aintbnb@outlook.com", "juSt@RandOmpassWordForSkewl");
            await _userManagement.SetupTables.CreateTablesAsync();
        }

        [TearDown]
        public void Teardown()
        {
            File.Delete($"LibTest{_testId}.db");
        }

        [Test]
        public async Task CreateUserAsync_ShouldCreateNewUser_WhenPassingUserObject_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            await _userManagement.UserManager.CreateUserAsync(user, password);
            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreEqual(createdUser.Email, user.Email);
            Assert.AreEqual(createdUser.Firstname, user.Firstname);
            Assert.AreEqual(createdUser.Lastname, user.Lastname);
            Assert.AreEqual(createdUser.Usertype.Type, user.Usertype.Type);
            Assert.AreEqual(createdUser.Address, user.Address);
        }

        [Test]
        public async Task CreateUserAsync_ShouldCreateNewUserWithAddress_WhenPassingUserObjectWithAddress_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            user.Address = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };
            await _userManagement.UserManager.CreateUserAsync(user, password);
            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreEqual(createdUser.Email, user.Email);
            Assert.AreEqual(createdUser.Firstname, user.Firstname);
            Assert.AreEqual(createdUser.Lastname, user.Lastname);
            Assert.AreEqual(createdUser.Usertype.Type, user.Usertype.Type);
            Assert.AreEqual(createdUser.Address.Street, user.Address.Street);
            Assert.AreEqual(createdUser.Address.Number, user.Address.Number);
            Assert.AreEqual(createdUser.Address.Zip, user.Address.Zip);
            Assert.AreEqual(createdUser.Address.Area, user.Address.Area);
            Assert.AreEqual(createdUser.Address.City, user.Address.City);
            Assert.AreEqual(createdUser.Address.Country, user.Address.Country);
        }

        [Test]
        public async Task CreateUserAsync_ShouldCreateNewUserWithAddress_WhenPassingPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, street, number, zip, area, city, country);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
            Assert.AreEqual(createdUser.Address.Street, street);
            Assert.AreEqual(createdUser.Address.Number, number);
            Assert.AreEqual(createdUser.Address.Zip, zip);
            Assert.AreEqual(createdUser.Address.Area, area);
            Assert.AreEqual(createdUser.Address.City, city);
            Assert.AreEqual(createdUser.Address.Country, country);
        }

        [Test]
        public async Task CreateUserAsync_ShouldCreateNewUserWithAddressAndSpecifiedUsertpye_WhenPassingPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, street, number, zip, area, city, country, adminUsertype);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, adminUsertype);
            Assert.AreEqual(createdUser.Address.Street, street);
            Assert.AreEqual(createdUser.Address.Number, number);
            Assert.AreEqual(createdUser.Address.Zip, zip);
            Assert.AreEqual(createdUser.Address.Area, area);
            Assert.AreEqual(createdUser.Address.City, city);
            Assert.AreEqual(createdUser.Address.Country, country);
        }

        [Test]
        public async Task CreateUserAsync_ShouldCreateNewUser_WhenPassingPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
        }

        [Test]
        public async Task CreateUserAsync_ShouldCreateNewUserWithSpecifiedUsertype_WhenPassingPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, adminUsertype);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, adminUsertype);
        }

        [Test]
        public async Task CreateUserAsync_ShouldHashThePassword_WhenCreatingANewUser_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            await _userManagement.UserManager.CreateUserAsync(user, password);
            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreNotEqual(createdUser.Email, password);
            Assert.True(VerifyThePassword(password, createdUser.Password));
        }

        [Test]
        [TestCase(null, "last")]
        [TestCase("first", null)]
        [TestCase(null, null)]
        public void CreateUserAsync_ShouldFail_WhenNameOrLastnameNull(string firstname, string lastname)
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Names cannot be null!", ex.Message);
        }

        [Test]
        [TestCase("a1", "last")]
        [TestCase("a", "last")]
        [TestCase("first", "a--")]
        [TestCase("11", "as")]
        public void CreateUserAsync_ShouldFail_WhenNameOrLastnameLengthLessThanTwoAndContainingLessOtherCharactersThanLettersOrOneSpaceOrOneDashBetweenLetters(string firstname, string lastname)
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Names must be at least two letters and cannot be containing any other than letters and one space or dash between names!", ex.Message);
        }

        [Test]
        public void CreateUserAsync_ShouldFail_WhenEmailIsNull()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(null, password, password, firstname, lastname));

            Assert.AreEqual("Email cannot be null!", ex.Message);
        }

        [Test]
        [TestCase("a@a.a")]
        [TestCase("aaa@a.")]
        [TestCase("@.cc")]
        [TestCase("aaaa.com")]
        [TestCase("")]
        public void CreateUserAsync_ShouldFail_WhenEmailAddressFormatIsWrong(string email)
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Email not formatted correctly!", ex.Message);
        }


        [Test]
        public async Task CreateUserAsync_ShouldFail_WhenEmailAddressAlreadyTaken_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Email is not available!", ex.Message);
        }

        [Test]
        public void CreateUserAsync_ShouldFail_WhenPasswordConfirmationIsWrong()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password + "a", firstname, lastname));

            Assert.AreEqual("The passwords don't match!", ex.Message);
        }

        [Test]
        public void CreateUserAsync_ShouldFail_WhenPasswordIsNull()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, null, password, firstname, lastname));

            Assert.AreEqual("Password cannot be null!", ex.Message);
        }

        [Test]
        [TestCase("aaa aaa")]
        [TestCase(" aaaaaa")]
        [TestCase("aaaaaa ")]
        public void CreateUserAsync_ShouldFail_WhenPasswordContainsSpaces(string password)
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Password can't contain space!", ex.Message);
        }


        [Test]
        [TestCase("firstt")]
        [TestCase("second12")]
        [TestCase("Third123")]
        [TestCase("f0urth#)")]
        [TestCase("F1fith%!")]
        public async Task CreateUserAsync_ShouldPass_WhenUserPasswordApprovedByPasswordPolicy_Async(string password)
        {
            if (password == "second12")
            {
                await _userManagement.PasswordPolicy.LevelOneAsync();
            }
            else if (password == "Third123")
            {
                await _userManagement.PasswordPolicy.LevelTwoAsync();
            }
            else if (password == "f0urth#)")
            {
                await _userManagement.PasswordPolicy.LevelThreeAsync();
            }
            else if (password == "F1fith%!")
            {
                await _userManagement.PasswordPolicy.LevelFourAsync();
            }

            Assert.DoesNotThrowAsync(async () => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));
        }

        [Test]
        [TestCase("first")]
        [TestCase("secondpassword")]
        [TestCase("thirdpassw0rd")]
        [TestCase("fourthpassw0rD")]
        [TestCase("fifthpassw0rd!")]
        public async Task CreateUserAsync_ShouldFail_WhenUserPasswordNotApprovedByPasswordPolicy_Async(string password)
        {
            var exceptionText = "";
            if (password == "first")
                exceptionText = "Password cannot be shorter than 6!";
            else if (password == "secondpassword")
            {
                await _userManagement.PasswordPolicy.LevelOneAsync();
                exceptionText = "Password must be at least 8 characters long with at least one number and letter!";
            }
            else if (password == "thirdpassw0rd")
            {
                await _userManagement.PasswordPolicy.LevelTwoAsync();
                exceptionText = "Password must be at least 8 characters long with at least one number andat least one upper- and one lowercase letter!";
            }
            else if (password == "fourthpassw0rD")
            {
                await _userManagement.PasswordPolicy.LevelThreeAsync();

                exceptionText = "Password must be at least 8 characters long with at least one number, letter and special character!";
            }
            else
            {
                await _userManagement.PasswordPolicy.LevelFourAsync();

                exceptionText = "Password must be at least 8 characters long with at least one number, at least one upper- and one lowercase letter and special character!";
            }
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual(exceptionText, ex.Message);
        }


        [Test]
        public void CreateUserAsync_ShouldFail_WhenUsertypeIsInvalid()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, adminUsertype + "a"));

            Assert.AreEqual("Invalid usertype!", ex.Message);
        }

        [Test]
        [TestCase("ooslo", "norway")]
        [TestCase("oslo", "noorway")]
        [TestCase("oslo", "sweden")]
        public void CreateUserAsync_ShouldFail_WhenTheCityOrCountryInTheAddressAreNotValid(string city, string country)
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            user.Address = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };

            var ex = Assert.ThrowsAsync<GeographicalException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(user, password));
        }

        [Test]
        public async Task UpdateUserUserAsync_ShouldAddAddressToExistingUserWithoutAddress_WhenPassingUserWithAddressObject_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.IsNull(createdUser.Address);

            var address = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };

            createdUser.Address = address;

            await _userManagement.UserManager.UpdateUserAsync(createdUser);

            Assert.AreEqual(address.Street, createdUser.Address.Street);
            Assert.AreEqual(address.Number, createdUser.Address.Number);
            Assert.AreEqual(address.Zip, createdUser.Address.Zip);
            Assert.AreEqual(address.Area, createdUser.Address.Area);
            Assert.AreEqual(address.City, createdUser.Address.City);
            Assert.AreEqual(address.Country, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldAddAddressToExistingUserWithoutAddress_WhenPassingUserObjectAndAddressObject_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.IsNull(createdUser.Address);

            var address = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };

            await _userManagement.UserManager.UpdateUserAsync(createdUser, address);

            createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(address.Street, createdUser.Address.Street);
            Assert.AreEqual(address.Number, createdUser.Address.Number);
            Assert.AreEqual(address.Zip, createdUser.Address.Zip);
            Assert.AreEqual(address.Area, createdUser.Address.Area);
            Assert.AreEqual(address.City, createdUser.Address.City);
            Assert.AreEqual(address.Country, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldAddAddressToExistingUserWithoutAddress_WhenPassingUserIdAndAddressPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.IsNull(createdUser.Address);

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Id, street, number, zip, area, city, country);

            createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(street, createdUser.Address.Street);
            Assert.AreEqual(number, createdUser.Address.Number);
            Assert.AreEqual(zip, createdUser.Address.Zip);
            Assert.AreEqual(area, createdUser.Address.Area);
            Assert.AreEqual(city, createdUser.Address.City);
            Assert.AreEqual(country, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldAddAddressToExistingUserWithoutAddress_WhenPassingUserEmailAndAddressPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.IsNull(createdUser.Address);

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Email, street, number, zip, area, city, country);

            createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(street, createdUser.Address.Street);
            Assert.AreEqual(number, createdUser.Address.Number);
            Assert.AreEqual(zip, createdUser.Address.Zip);
            Assert.AreEqual(area, createdUser.Address.Area);
            Assert.AreEqual(city, createdUser.Address.City);
            Assert.AreEqual(country, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldChangeAddressOfExistingUserWithAddress_WhenPassingAddressObjectWithUpdatedValues_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, street, number, zip, area, city, country);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var address = createdUser.Address;

            var newstr = street + "a";
            var newnum = "10000a";
            var newzip = zip + "111";
            var newarea = area + "aaa";
            var newcity = "Paris";
            var newcountry = "France";

            address.Street = newstr;
            address.Number = newnum;
            address.Zip = newzip;
            address.Area = newarea;
            address.City = newcity;
            address.Country = newcountry;

            await _userManagement.UserManager.UpdateUserAsync(createdUser, address);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(newstr, createdUser.Address.Street);
            Assert.AreEqual(newnum, createdUser.Address.Number);
            Assert.AreEqual(newzip, createdUser.Address.Zip);
            Assert.AreEqual(newarea, createdUser.Address.Area);
            Assert.AreEqual(newcity, createdUser.Address.City);
            Assert.AreEqual(newcountry, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldChangeAddressOfExistingUserWithAddress_WhenPassingUserIdAndAddressPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, street, number, zip, area, city, country);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newstr = street + "a";
            var newnum = "10000a";
            var newzip = zip + "111";
            var newarea = area + "aaa";
            var newcity = "Paris";
            var newcountry = "France";

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Id, newstr, newnum, newzip, newarea, newcity, newcountry);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(newstr, createdUser.Address.Street);
            Assert.AreEqual(newnum, createdUser.Address.Number);
            Assert.AreEqual(newzip, createdUser.Address.Zip);
            Assert.AreEqual(newarea, createdUser.Address.Area);
            Assert.AreEqual(newcity, createdUser.Address.City);
            Assert.AreEqual(newcountry, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldChangeAddressOfExistingUserWithAddress_WhenPassingUserEmailAndAddressPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, street, number, zip, area, city, country);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newstr = street + "a";
            var newnum = "10000a";
            var newzip = zip + "111";
            var newarea = area + "aaa";
            var newcity = "Paris";
            var newcountry = "France";

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Email, newstr, newnum, newzip, newarea, newcity, newcountry);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(newstr, createdUser.Address.Street);
            Assert.AreEqual(newnum, createdUser.Address.Number);
            Assert.AreEqual(newzip, createdUser.Address.Zip);
            Assert.AreEqual(newarea, createdUser.Address.Area);
            Assert.AreEqual(newcity, createdUser.Address.City);
            Assert.AreEqual(newcountry, createdUser.Address.Country);
        }

        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserObjectWithUpdatedUsertypeAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var usertype = new Usertype(adminUsertype);

            Assert.AreNotEqual(usertype.Type, createdUser.Usertype.Type);

            createdUser.Usertype = usertype;

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(usertype.Type, createdUser.Usertype.Type);
        }

        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserObjectAndUsertypeNameAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(adminUsertype, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser, adminUsertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(adminUsertype, createdUser.Usertype.Type);
        }

        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserIdAndUsertypeNameAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(adminUsertype, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser.Id, adminUsertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(adminUsertype, createdUser.Usertype.Type);
        }

        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserEmailAndUsertypeNameAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(adminUsertype, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser.Email, adminUsertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(adminUsertype, createdUser.Usertype.Type);
        }

        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserObjectAndUsertypeObjectAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var usertype = new Usertype(adminUsertype);

            Assert.AreNotEqual(usertype.Type, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser, usertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(usertype.Type, createdUser.Usertype.Type);
        }

        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserIdAndUsertypeObjectAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var usertype = new Usertype(adminUsertype);

            Assert.AreNotEqual(usertype.Type, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser.Id, usertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(usertype.Type, createdUser.Usertype.Type);
        }


        [Test]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserEmailAndUsertypeObjectAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var usertype = new Usertype(adminUsertype);

            Assert.AreNotEqual(usertype.Type, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser.Email, usertype);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Email);

            Assert.AreEqual(usertype.Type, createdUser.Usertype.Type);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateFirstAndLastname_WhenPassingUserObjectWithUpdatedNames_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);

            var newfirst = firstname + "a";
            var newlast = lastname + "a";

            var userUpdatedNames = new User
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                Password = createdUser.Password,
                Firstname = newfirst,
                Lastname = newlast
            };

            await _userManagement.UserManager.UpdateUserAsync(userUpdatedNames);

            createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(createdUser.Firstname, firstname);
            Assert.AreNotEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Firstname, newfirst);
            Assert.AreEqual(createdUser.Lastname, newlast);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateFirstAndLastname_WhenPassingUserIdWithNewNamesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);

            var newfirst = firstname + "a";
            var newlast = lastname + "a";

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Id, newfirst, newlast);

            createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(createdUser.Firstname, firstname);
            Assert.AreNotEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Firstname, newfirst);
            Assert.AreEqual(createdUser.Lastname, newlast);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateFirstAndLastname_WhenPassingUserEmailWithNewNamesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);

            var newfirst = firstname + "a";
            var newlast = lastname + "a";

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Email, newfirst, newlast);

            createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(createdUser.Firstname, firstname);
            Assert.AreNotEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Firstname, newfirst);
            Assert.AreEqual(createdUser.Lastname, newlast);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateEmail_WhenPassingUserObjectWithUpdatedEmail_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);

            var newemail = email + "a";

            await _userManagement.UserManager.UpdateUserAsync(createdUser, newemail);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreNotEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Email, newemail);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateEmail_WhenPassingUserIdWithNewEmailAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);

            var newemail = email + "a";

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Id, newemail);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreNotEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Email, newemail);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateEmail_WhenPassingUserEmailWithNewEmailAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);

            var newemail = email + "a";

            await _userManagement.UserManager.UpdateUserAsync(createdUser.Email, newemail);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreNotEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Email, newemail);
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldChangePassword_WhenCorrectPasswordEnteredAndNewPasswordIsDifferentFromCurrent_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);
            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newpass = password + "a";

            Assert.False(VerifyThePassword(newpass, createdUser.Password));

            await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, password, newpass, newpass);

            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Email);

            Assert.True(VerifyThePassword(newpass, createdUser.Password));
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldFail_WhenCurrentUserPasswordIsIncorrect_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newpass = password + "a";

            var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                => await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, newpass, password, password));

            Assert.AreEqual("The old passwords don't match!", ex.Message);
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldFail_WhenCurrentAndNewPasswordsAreEqual_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                => await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, password, password, password));

            Assert.AreEqual("The new and old password must be different!", ex.Message);
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldFail_WhenNewPasswordConfirmationIsWrong_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newpass = password + "a";

            var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                => await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, password, newpass, newpass + "s"));

            Assert.AreEqual("The new passwords don't match!", ex.Message);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public async Task CreateUserAsync_ShouldReturnARandomStringThatSatisfiesPasswordPolicy_WhenCorrectLengthSentAsParameter_Async(int i)
        {
            var currentPasswordRegex = new Regex("");

            if (i == 2)
            {
                await _userManagement.PasswordPolicy.LevelOneAsync();
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetter;
            }
            else if (i == 3)
            {
                await _userManagement.PasswordPolicy.LevelTwoAsync();
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetterOneUpperAndLowerCase;
            }
            else if (i == 4)
            {
                await _userManagement.PasswordPolicy.LevelThreeAsync();
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacter;
            }
            else if (i == 5)
            {
                await _userManagement.PasswordPolicy.LevelFourAsync();
                currentPasswordRegex = passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacterOneUpperAndLowerCase;
            }

            Assert.True(currentPasswordRegex.IsMatch(await _userManagement.UserManager.GenerateRandomPasswordAsync(8)));
        }

        [Test]
        public async Task CreateUserAsync_ShouldFail_WhenInCorrectLengthSentAsParameter_Async()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.GenerateRandomPasswordAsync(5));

            Assert.AreEqual("Random password length cannot be shorter than 6 characters!", ex.Message);

            await _userManagement.PasswordPolicy.LevelOneAsync();

            ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.GenerateRandomPasswordAsync(5));

            Assert.AreEqual("Random password length cannot be shorter than 8 characters!", ex.Message);
        }

        [Test]
        public async Task GetUserAsync_ShouldReturnCorrectUser_WhenPassingUserIdAsParameter_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            await _userManagement.UserManager.CreateUserAsync(user, password);

            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(createdUser.Id, 1);
            Assert.AreEqual(createdUser.Email, user.Email);
            Assert.AreEqual(createdUser.Firstname, user.Firstname);
            Assert.AreEqual(createdUser.Lastname, user.Lastname);
            Assert.AreEqual(createdUser.Usertype.Type, user.Usertype.Type);
            Assert.AreEqual(createdUser.Address, user.Address);
        }

        [Test]
        public async Task GetUserAsync_ShouldReturnCorrectUser_WhenPassingUserEmailAsParameter_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            await _userManagement.UserManager.CreateUserAsync(user, password);

            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreEqual(createdUser.Id, 1);
            Assert.AreEqual(createdUser.Email, user.Email);
            Assert.AreEqual(createdUser.Firstname, user.Firstname);
            Assert.AreEqual(createdUser.Lastname, user.Lastname);
            Assert.AreEqual(createdUser.Usertype.Type, user.Usertype.Type);
            Assert.AreEqual(createdUser.Address, user.Address);
        }

        [Test]
        public async Task GetUserAsync_ShouldFail_WhenPassingWrongUserId_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            await _userManagement.UserManager.CreateUserAsync(user, password);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(0));

            Assert.AreEqual("User with ID 0 not found in the system!", ex.Message);
        }

        [Test]
        public async Task GetUserAsync_ShouldFail_WhenPassingWrongUserEmail_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            await _userManagement.UserManager.CreateUserAsync(user, password);

            var wrongemail = email + "a";

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(wrongemail));

            Assert.AreEqual($"User with email {wrongemail} not found in the system!", ex.Message);
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var email2 = email + "a";

            var user2 = new User { Email = email2, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = adminUsertype } };
            user2.Address = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };

            await _userManagement.UserManager.CreateUserAsync(user2, password);


            var createdUser = await _userManagement.UserManager.GetUserAsync(email);
            var createdUser2 = await _userManagement.UserManager.GetUserAsync(email2);

            var all = await _userManagement.UserManager.GetAllUsersAsync();

            Assert.AreEqual(all.Count, 2);
            Assert.AreEqual(all[0].ToString(), createdUser.ToString());
            Assert.AreEqual(all[1].ToString(), createdUser2.ToString());
        }

        [Test]
        public void GetAllUsersAsync_ShouldFail_WhenNoUsersExist()
        {
            var ex = Assert.ThrowsAsync<NoneFoundInDatabaseTableException>(async ()
                => await _userManagement.UserManager.GetAllUsersAsync());

            Assert.AreEqual("No users exist in the database!", ex.Message);
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnAllUsersWithTheSpecifiedUsertype_WhenPassingUsertypeAsParameter_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var email2 = email + "a";
            var email3 = email2 + "a";

            var user2 = new User { Email = email2, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = adminUsertype } };
            user2.Address = new Address { Street = street, Number = number, Zip = zip, Area = area, City = city, Country = country };

            await _userManagement.UserManager.CreateUserAsync(user2, password);

            var user3 = new User { Email = email3, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = adminUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user3, password);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);
            var createdUser2 = await _userManagement.UserManager.GetUserAsync(email2);
            var createdUser3 = await _userManagement.UserManager.GetUserAsync(email3);

            var all = await _userManagement.UserManager.GetAllUsersAsync(createdUser2.Usertype.Type);

            Assert.AreEqual(all.Count, 2);
            Assert.AreNotEqual(all[0].ToString(), createdUser.ToString());
            Assert.AreNotEqual(all[1].ToString(), createdUser.ToString());
            Assert.AreEqual(all[0].ToString(), createdUser2.ToString());
            Assert.AreEqual(all[1].ToString(), createdUser3.ToString());
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldFail_WhenPassingUsertypeAsParameterButNoUsersWithGivenUsertypeExist_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetAllUsersAsync(adminUsertype));

            Assert.AreEqual($"Users with type {adminUsertype} not found in the system!", ex.Message);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserObjectPassedAsParamater_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreEqual(createdUser.Email, email);

            await _userManagement.UserManager.DeleteUserAsync(createdUser);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(email));

            Assert.AreEqual($"User with email {email} not found in the system!", ex.Message);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserIdPassedAsParamater_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreEqual(createdUser.Email, email);

            await _userManagement.UserManager.DeleteUserAsync(createdUser.Id);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(email));

            Assert.AreEqual($"User with email {email} not found in the system!", ex.Message);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserEmailPassedAsParamater_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var createdUser = await _userManagement.UserManager.GetUserAsync(user.Email);

            Assert.AreEqual(createdUser.Email, email);

            await _userManagement.UserManager.DeleteUserAsync(createdUser.Email);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(email));

            Assert.AreEqual($"User with email {email} not found in the system!", ex.Message);
        }

        [Test]
        public async Task LoginAsync_ShouldFail_WhenUserAccountIsNotActivated_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            await _userManagement.UserManager.CreateUserAsync(user, password);

            var ex = Assert.ThrowsAsync<LoginException>(async ()
                => await _userManagement.UserManager.LoginAsync(email, password));

            Assert.AreEqual("Verify your account with the code you received in your email first!", ex.Message);
        }

        [Test]
        public async Task LoginAsync_ShouldFail_WhenUserMustChangePassword_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };

            user.IsActivated = true;

            await _userManagement.UserManager.CreateUserAsync(user, password);

            await _userManagement.UserManager.ForgotPasswordAsync(1);

            var ex = Assert.ThrowsAsync<LoginException>(async ()
                => await _userManagement.UserManager.LoginAsync(email, password));

            Assert.AreEqual("Change your password first!", ex.Message);
        }

        [Test]
        [TestCase("a@a.cc", "aaaaaa")]
        [TestCase("aa@aa.cc", "aaaaaaaa")]
        [TestCase("a@a.cc", "aaaaaaaaa")]
        public async Task LoginAsync_ShouldFail_WhenEmailAndOrPasswordAreWorng_Async(string email, string password)
        {
            var user = new User { Email = this.email, Password = this.password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            user.IsActivated = true;

            await _userManagement.UserManager.CreateUserAsync(user, this.password);

            var ex = Assert.ThrowsAsync<LoginException>(async ()
                => await _userManagement.UserManager.LoginAsync(email, password));

            Assert.AreEqual("Username and/or password not correct!", ex.Message);
        }

        [Test]
        public async Task LoginAsync_ShouldPass_WhenEmailAndPasswordAreCorrectAndUserAccountIsActivatedAndUserMustNotChangePassword_Async()
        {
            var user = new User { Email = email, Password = password, Firstname = firstname, Lastname = lastname, Usertype = new Usertype { Type = defaultUsertype } };
            user.IsActivated = true;

            await _userManagement.UserManager.CreateUserAsync(user, password);

            Assert.DoesNotThrowAsync(async () => await _userManagement.UserManager.LoginAsync(email, password));
        }

        [Test]
        public async Task AddMoreUsertypesAsync_ShouldCreateNewUsertypes_WhenPassingAnyAmountOfNewUsertypes_Async()
        {
            var new1 = "Newtype";
            var new2 = "Newtype1";
            var new3 = "Newtype2";

            await _userManagement.UserManager.AddMoreUsertypesAsync(new1);

            var all = await _userManagement.UserManager.GetAllUsertypesAsync();

            Assert.AreEqual(all[2].Type, new1);

            await _userManagement.UserManager.AddMoreUsertypesAsync(new2, new3);

            all = await _userManagement.UserManager.GetAllUsertypesAsync();

            Assert.AreEqual(all[3].Type, new2);
            Assert.AreEqual(all[4].Type, new3);
        }

        [Test]
        public void AddMoreUsertypesAsync_ShouldFail_WhenPassingAlreadyExistingUsertypes()
        {
            var ex = Assert.ThrowsAsync<FailedToCreateException>(async ()
                => await _userManagement.UserManager.AddMoreUsertypesAsync(adminUsertype, defaultUsertype));

            Assert.AreEqual("Usertype could not be created!", ex.Message);
        }

        [Test]
        public async Task GetAllUsertypesAsync_ShouldReturnAllTheUsertypes_Async()
        {
            var all = await _userManagement.UserManager.GetAllUsertypesAsync();

            Assert.AreEqual(all[0].Type, adminUsertype);
            Assert.AreEqual(all[1].Type, defaultUsertype);
        }

        [Test]
        public async Task DeSerializeFromStringAsync_ShouldCreateNewUser_WhenPassingCsvStringWithUser_Async()
        {
            await _userManagement.UserManager.DeSerializeFromStringAsync(userCsv);
            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(userCsv, _userManagement.UserManager.SerializeToCsvString(createdUser));
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
        }

        [Test]
        public async Task DeSerializeFromStringAsync_ShouldCreateNewUser_WhenPassingJsonStringWithUser_Async()
        {
            await _userManagement.UserManager.DeSerializeFromStringAsync(userJson);
            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(userJson, _userManagement.UserManager.SerializeToJsonString(createdUser));
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
        }

        [Test]
        public async Task DeSerializeFromStringAsync_ShouldCreateNewUser_WhenPassingXmlStringWithUser_Async()
        {
            await _userManagement.UserManager.DeSerializeFromStringAsync(userXml);
            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(userXml, _userManagement.UserManager.SerializeToXmlString(createdUser));
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
        }

        [Test]
        public async Task DeSerializeFromStringAsync_ShouldCreateNewUserWithAddress_WhenPassingCsvStringWithUserWithAddress_Async()
        {
            await _userManagement.UserManager.DeSerializeFromStringAsync(userAdrCsv);
            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(userAdrCsv, _userManagement.UserManager.SerializeToCsvString(createdUser));
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
            Assert.AreEqual(createdUser.Address.Street, street);
            Assert.AreEqual(createdUser.Address.Number, number);
            Assert.AreEqual(createdUser.Address.Zip, zip);
            Assert.AreEqual(createdUser.Address.Area, area);
            Assert.AreEqual(createdUser.Address.City, city);
            Assert.AreEqual(createdUser.Address.Country, country);
        }

        [Test]
        public async Task DeSerializeFromStringAsync_ShouldCreateNewUserWithAddress_WhenPassingJsonStringWithUserWithAddress_Async()
        {
            await _userManagement.UserManager.DeSerializeFromStringAsync(userAdrJson);
            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(userAdrJson, _userManagement.UserManager.SerializeToJsonString(createdUser));
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
            Assert.AreEqual(createdUser.Address.Street, street);
            Assert.AreEqual(createdUser.Address.Number, number);
            Assert.AreEqual(createdUser.Address.Zip, zip);
            Assert.AreEqual(createdUser.Address.Area, area);
            Assert.AreEqual(createdUser.Address.City, city);
            Assert.AreEqual(createdUser.Address.Country, country);
        }

        [Test]
        public async Task DeSerializeFromStringAsync_ShouldCreateNewUserWithAddress_WhenPassingXmlStringWithUserWithAddress_Async()
        {
            await _userManagement.UserManager.DeSerializeFromStringAsync(userAdrXml);
            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(userAdrXml, _userManagement.UserManager.SerializeToXmlString(createdUser));
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
            Assert.AreEqual(createdUser.Address.Street, street);
            Assert.AreEqual(createdUser.Address.Number, number);
            Assert.AreEqual(createdUser.Address.Zip, zip);
            Assert.AreEqual(createdUser.Address.Area, area);
            Assert.AreEqual(createdUser.Address.City, city);
            Assert.AreEqual(createdUser.Address.Country, country);
        }
    }
}
