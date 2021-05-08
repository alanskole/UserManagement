using ManageUsers.CustomExceptions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using static ManageUsers.Helper.PasswordHelper;
using static ManageUsers.Helper.RegexChecker;

namespace Test
{
    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    internal class UserManagementTest
    {
        private UserManagementForTesting _userManagement;
        private readonly string _connectionString = "LibTest.db";
        private readonly string jwtSecretKey = "super secret key for testing";
        private readonly string email = "aa@aa.xx";
        private readonly string password = "aaaaaa";
        private readonly string firstname = "first";
        private readonly string lastname = "user";
        private readonly string defaultUsertype = "User";
        private readonly string adminUsertype = "Admin";
        private readonly string street = "fake";
        private readonly string number = "1a";
        private readonly string zip = "1111";
        private readonly string area = "blabla";
        private readonly string city = "Oslo";
        private readonly string country = "Norway";
        private readonly string userCsv = @"1,aa@aa.xx,uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==,first,user,0,,,,,,,False,False,2,User,";
        private readonly string userJson = "{\"Id\":1,\"Email\":\"aa@aa.xx\",\"Password\":\"uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==\",\"Firstname\":\"first\",\"Lastname\":\"user\",\"Address\":null,\"IsActivated\":false,\"MustChangePassword\":false,\"Usertype\":{\"Id\":2,\"Type\":\"User\"},\"Picture\":null}";
        private readonly string userXml = @"<User xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Id>1</Id><Email>aa@aa.xx</Email><Password>uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==</Password><Firstname>first</Firstname><Lastname>user</Lastname><Address i:nil=""true""/><IsActivated>false</IsActivated><MustChangePassword>false</MustChangePassword><Usertype><Id>2</Id><Type>User</Type></Usertype><Picture i:nil=""true"" xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""/></User>";
        private readonly string userAdrCsv = @"1,aa@aa.xx,uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==,first,user,1,fake,1a,1111,blabla,Oslo,Norway,False,False,2,User,";
        private readonly string userAdrJson = "{\"Id\":1,\"Email\":\"aa@aa.xx\",\"Password\":\"uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==\",\"Firstname\":\"first\",\"Lastname\":\"user\",\"Address\":{\"Id\":1,\"Street\":\"fake\",\"Number\":\"1a\",\"Zip\":\"1111\",\"Area\":\"blabla\",\"City\":\"Oslo\",\"Country\":\"Norway\"},\"IsActivated\":false,\"MustChangePassword\":false,\"Usertype\":{\"Id\":2,\"Type\":\"User\"},\"Picture\":null}";
        private readonly string userAdrXml = @"<User xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Id>1</Id><Email>aa@aa.xx</Email><Password>uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==</Password><Firstname>first</Firstname><Lastname>user</Lastname><Address><Id>1</Id><Street>fake</Street><Number>1a</Number><Zip>1111</Zip><Area>blabla</Area><City>Oslo</City><Country>Norway</Country></Address><IsActivated>false</IsActivated><MustChangePassword>false</MustChangePassword><Usertype><Id>2</Id><Type>User</Type></Usertype><Picture i:nil=""true"" xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""/></User>";

        [SetUp]
        public async Task Setup()
        {
            _userManagement = new UserManagementForTesting($"Data Source={_connectionString};", "test@test.test", "test");
            await _userManagement.SetupTables.CreateTablesAsync();
        }

        [TearDown]
        public void Teardown()
        {
            File.Delete(_connectionString);
        }

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
        public async Task CreateUserAsync_ShouldCreateNewUser_WhenPassingPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
        }

        [Test, NonParallelizable]
        public async Task CreateUserAsync_ShouldCreateNewUserWithSpecifiedUsertype_WhenPassingPropertiesAsParameters_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, adminUsertype);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, adminUsertype);
        }

        [Test, NonParallelizable]
        public async Task CreateUserAsync_ShouldHashThePassword_WhenCreatingANewUser_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(createdUser.Email, password);
            Assert.True(VerifyThePassword(password, createdUser.Password));
        }

        [Test, NonParallelizable]
        [TestCase(null, "last")]
        [TestCase("first", null)]
        [TestCase(null, null)]
        public void CreateUserAsync_ShouldFail_WhenNameOrLastnameNull(string firstname, string lastname)
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Names cannot be null!", ex.Message);
        }

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
        public void CreateUserAsync_ShouldFail_WhenEmailIsNull()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(null, password, password, firstname, lastname));

            Assert.AreEqual("Email cannot be null!", ex.Message);
        }

        [Test, NonParallelizable]
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


        [Test, NonParallelizable]
        public async Task CreateUserAsync_ShouldFail_WhenEmailAddressAlreadyTaken_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Email is not available!", ex.Message);
        }

        [Test, NonParallelizable]
        public void CreateUserAsync_ShouldFail_WhenPasswordConfirmationIsWrong()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password + "a", firstname, lastname));

            Assert.AreEqual("The passwords don't match!", ex.Message);
        }

        [Test, NonParallelizable]
        public void CreateUserAsync_ShouldFail_WhenPasswordIsNull()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, null, password, firstname, lastname));

            Assert.AreEqual("Password cannot be null!", ex.Message);
        }

        [Test, NonParallelizable]
        [TestCase("aaa aaa")]
        [TestCase(" aaaaaa")]
        [TestCase("aaaaaa ")]
        public void CreateUserAsync_ShouldFail_WhenPasswordContainsSpaces(string password)
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual("Password can't contain space!", ex.Message);
        }


        [Test, NonParallelizable]
        [TestCase("firstt")]
        [TestCase("second12")]
        [TestCase("Third123")]
        [TestCase("f0urth#)")]
        [TestCase("F1fith%!")]
        public async Task CreateUserAsync_ShouldPass_WhenUserPasswordApprovedByPasswordPolicy_Async(string password)
        {
            if (password == "second12")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, true, false);
            }
            else if (password == "Third123")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, true, true, false);
            }
            else if (password == "f0urth#)")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, true, true);
            }
            else if (password == "F1fith%!")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, true, true, true);
            }

            Assert.DoesNotThrowAsync(async () => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));
        }

        [Test, NonParallelizable]
        [TestCase("first")]
        [TestCase("secondpassword")]
        [TestCase("thirdpassw0rd")]
        [TestCase("fourthpassw0rD")]
        [TestCase("aaaaaaaa")]
        public async Task CreateUserAsync_ShouldFail_WhenUserPasswordNotApprovedByPasswordPolicy_Async(string password)
        {
            var exceptionText = "";
            if (password == "first")
            {
                await _userManagement.PasswordPolicy.DefaultPolicyAsync();
                exceptionText = "Password cannot be shorter than 6 characters!";
            }
            else if (password == "secondpassword")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, true, false);
                exceptionText = "Password must contain at least one number!";
            }
            else if (password == "thirdpassw0rd")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, true, true, false);
                exceptionText = "Password must contain at least one upper case letter!";
            }
            else if (password == "fourthpassw0rD")
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, true, true);
                exceptionText = "Password must contain at least one special character!";
            }
            else
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(9, false, false, false);
                exceptionText = "Password must be at least 9 characters long!";
            }
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname));

            Assert.AreEqual(exceptionText, ex.Message);
        }


        [Test, NonParallelizable]
        public void CreateUserAsync_ShouldFail_WhenUsertypeIsInvalid()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, adminUsertype + "a"));

            Assert.AreEqual("Invalid usertype!", ex.Message);
        }

        [Test, NonParallelizable]
        [TestCase("ooslo", "norway")]
        [TestCase("oslo", "noorway")]
        [TestCase("oslo", "sweden")]
        public void CreateUserAsync_ShouldFail_WhenTheCityOrCountryInTheAddressAreNotValid(string city, string country)
        {
            var ex = Assert.ThrowsAsync<GeographicalException>(async ()
                => await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname, street, number, zip, area, city, country));
        }

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserIdAndUsertypeNameAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(adminUsertype, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser.Id, adminUsertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(adminUsertype, createdUser.Usertype.Type);
        }

        [Test, NonParallelizable]
        public async Task UpdateUserTypeAsync_ShouldUpdateUsertypeOfUser_WhenPassingUserEmailAndUsertypeNameAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreNotEqual(adminUsertype, createdUser.Usertype.Type);

            await _userManagement.UserManager.UpdateUsertypeOfUserAsync(createdUser.Email, adminUsertype);
            createdUser = await _userManagement.UserManager.GetUserAsync(createdUser.Id);

            Assert.AreEqual(adminUsertype, createdUser.Usertype.Type);
        }

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
        public async Task ChangePasswordAsync_ShouldFail_WhenCurrentUserPasswordIsIncorrect_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newpass = password + "a";

            var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                => await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, newpass, password, password));

            Assert.AreEqual("The old passwords don't match!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task ChangePasswordAsync_ShouldFail_WhenCurrentAndNewPasswordsAreEqual_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                => await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, password, password, password));

            Assert.AreEqual("The new and old password must be different!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task ChangePasswordAsync_ShouldFail_WhenNewPasswordConfirmationIsWrong_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            var newpass = password + "a";

            var ex = Assert.ThrowsAsync<PasswordChangeException>(async ()
                => await _userManagement.UserManager.ChangePasswordAsync(createdUser.Email, password, newpass, newpass + "s"));

            Assert.AreEqual("The new passwords don't match!", ex.Message);
        }

        [Test, NonParallelizable]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public async Task GenerateRandomPasswordAsync_ShouldReturnARandomStringThatSatisfiesPasswordPolicy_WhenCorrectLengthSentAsParameter_Async(int i)
        {
            if (i == 1)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(10, true, true, true);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(10);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 10));
                Assert.True(ContainsUpperCase(password));
                Assert.True(ContainsNumber(password));
                Assert.True(ContainsSpecialCharacter(password));
            }
            else if (i == 2)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, false, false);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(8);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 8));
            }
            else if (i == 3)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, true, false, false);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(8);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 8));
                Assert.True(ContainsUpperCase(password));
            }
            else if (i == 4)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, true, false);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(8);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 8));
                Assert.True(ContainsNumber(password));
            }
            else if (i == 5)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, false, true);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(8);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 8));
                Assert.True(ContainsSpecialCharacter(password));
            }
            else if (i == 6)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(10, true, true, false);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(10);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 10));
                Assert.True(ContainsUpperCase(password));
                Assert.True(ContainsNumber(password));
            }
            else if (i == 7)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(10, true, false, true);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(10);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 10));
                Assert.True(ContainsUpperCase(password));
                Assert.True(ContainsSpecialCharacter(password));
            }
            else if (i == 8)
            {
                await _userManagement.PasswordPolicy.SetPolicyAsync(10, false, true, true);
                var password = await _userManagement.UserManager.GenerateRandomPasswordAsync(10);
                Assert.True(ContainsLowerCase(password));
                Assert.True(ContainsMinimumAmountOfCharacters(password, 10));
                Assert.True(ContainsSpecialCharacter(password));
                Assert.True(ContainsNumber(password));
            }
        }

        [Test, NonParallelizable]
        public async Task GenerateRandomPasswordAsync_ShouldFail_WhenInCorrectLengthSentAsParameter_Async()
        {
            var ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.GenerateRandomPasswordAsync(5));

            Assert.AreEqual("Random password length cannot be shorter than 6 characters!", ex.Message);

            await _userManagement.PasswordPolicy.SetPolicyAsync(8, false, false, false);

            ex = Assert.ThrowsAsync<ParameterException>(async ()
                => await _userManagement.UserManager.GenerateRandomPasswordAsync(7));

            Assert.AreEqual("Random password length cannot be shorter than 8 characters!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task GetUserAsync_ShouldReturnCorrectUser_WhenPassingUserIdAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(1);

            Assert.AreEqual(createdUser.Id, 1);
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
            Assert.IsNull(createdUser.Address);
        }

        [Test, NonParallelizable]
        public async Task GetUserAsync_ShouldReturnCorrectUser_WhenPassingUserEmailAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Id, 1);
            Assert.AreEqual(createdUser.Email, email);
            Assert.AreEqual(createdUser.Firstname, firstname);
            Assert.AreEqual(createdUser.Lastname, lastname);
            Assert.AreEqual(createdUser.Usertype.Type, defaultUsertype);
            Assert.IsNull(createdUser.Address);
        }

        [Test, NonParallelizable]
        public async Task GetUserAsync_ShouldFail_WhenPassingWrongUserId_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(0));

            Assert.AreEqual("User with ID 0 not found in the system!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task GetUserAsync_ShouldFail_WhenPassingWrongUserEmail_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var wrongemail = email + "a";

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(wrongemail));

            Assert.AreEqual($"User with email {wrongemail} not found in the system!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var email2 = email + "a";

            await _userManagement.UserManager.CreateUserAsync(email2, password, password, firstname, lastname, street, number, zip, area, city, country);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);
            var createdUser2 = await _userManagement.UserManager.GetUserAsync(email2);

            var all = await _userManagement.UserManager.GetAllUsersAsync();

            Assert.AreEqual(all.Count, 2);
            Assert.AreEqual(all[0].ToString(), createdUser.ToString());
            Assert.AreEqual(all[1].ToString(), createdUser2.ToString());
        }

        [Test, NonParallelizable]
        public void GetAllUsersAsync_ShouldFail_WhenNoUsersExist()
        {
            var ex = Assert.ThrowsAsync<NoneFoundInDatabaseTableException>(async ()
                => await _userManagement.UserManager.GetAllUsersAsync());

            Assert.AreEqual("No users exist in the database!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task GetAllUsersAsync_ShouldReturnAllUsersWithTheSpecifiedUsertype_WhenPassingUsertypeAsParameter_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var email2 = email + "a";
            var email3 = email2 + "a";

            await _userManagement.UserManager.CreateUserAsync(email2, password, password, firstname, lastname, street, number, zip, area, city, country, adminUsertype);

            await _userManagement.UserManager.CreateUserAsync(email3, password, password, firstname, lastname, adminUsertype);

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

        [Test, NonParallelizable]
        public async Task GetAllUsersAsync_ShouldFail_WhenPassingUsertypeAsParameterButNoUsersWithGivenUsertypeExist_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetAllUsersAsync(adminUsertype));

            Assert.AreEqual($"Users with type {adminUsertype} not found in the system!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserIdPassedAsParamater_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);

            await _userManagement.UserManager.DeleteUserAsync(createdUser.Id);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(email));

            Assert.AreEqual($"User with email {email} not found in the system!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserEmailPassedAsParamater_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            Assert.AreEqual(createdUser.Email, email);

            await _userManagement.UserManager.DeleteUserAsync(createdUser.Email);

            var ex = Assert.ThrowsAsync<NotFoundException>(async ()
                => await _userManagement.UserManager.GetUserAsync(email));

            Assert.AreEqual($"User with email {email} not found in the system!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task LoginAsync_ShouldFail_WhenUserAccountIsNotActivated_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var ex = Assert.ThrowsAsync<LoginException>(async ()
                => await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey));

            Assert.AreEqual("Verify your account with the code you received in your email first!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task LoginAsync_ShouldFail_WhenUserMustChangePassword_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            await _userManagement.UserManager.ForgotPasswordAsync(1);

            var ex = Assert.ThrowsAsync<LoginException>(async ()
                => await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey));

            Assert.AreEqual("Change your password first!", ex.Message);
        }

        [Test, NonParallelizable]
        [TestCase("a@a.cc", "aaaaaa")]
        [TestCase("aa@aa.cc", "aaaaaaaa")]
        [TestCase("a@a.cc", "aaaaaaaaa")]
        public async Task LoginAsync_ShouldFail_WhenEmailAndOrPasswordAreWorng_Async(string email, string password)
        {
            await _userManagement.UserManager.CreateUserAsync(this.email, this.password, this.password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(this.email);

            createdUser.IsActivated = true;

            var ex = Assert.ThrowsAsync<LoginException>(async ()
                => await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey));

            Assert.AreEqual("Username and/or password not correct!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task LoginAsync_ShouldPass_WhenEmailAndPasswordAreCorrectAndUserAccountIsActivatedAndUserMustNotChangePassword_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            Assert.DoesNotThrowAsync(async () => await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey));
        }

        [Test, NonParallelizable]
        public async Task AddMoreUsertypesAsync_ShouldCreateNewUsertypes_WhenPassingAnyAmountOfNewUsertypes_Async()
        {
            var new1 = "Newtype";
            var new2 = "Newtype1";
            var new3 = "Newtype2";

            await _userManagement.UserManager.AddMoreUsertypesAsync(new1);

            var all = await _userManagement.UserManager.GetAllUsertypesObjectAsync();

            Assert.AreEqual(all[2].Type, new1);

            await _userManagement.UserManager.AddMoreUsertypesAsync(new2, new3);

            all = await _userManagement.UserManager.GetAllUsertypesObjectAsync();

            Assert.AreEqual(all[3].Type, new2);
            Assert.AreEqual(all[4].Type, new3);
        }

        [Test, NonParallelizable]
        public void AddMoreUsertypesAsync_ShouldFail_WhenPassingAlreadyExistingUsertypes()
        {
            var ex = Assert.ThrowsAsync<FailedToCreateException>(async ()
                => await _userManagement.UserManager.AddMoreUsertypesAsync(adminUsertype, defaultUsertype));

            Assert.AreEqual("Usertype could not be created!", ex.Message);
        }

        [Test, NonParallelizable]
        public async Task GetAllUsertypesObjectAsync_ShouldReturnAllTheUsertypes_Async()
        {
            var all = await _userManagement.UserManager.GetAllUsertypesObjectAsync();

            Assert.AreEqual(all[0].Type, adminUsertype);
            Assert.AreEqual(all[1].Type, defaultUsertype);
        }

        [Test, NonParallelizable]
        public async Task GetAllUsertypesAsync_ShouldReturnAllTheUsertypes_Async()
        {
            var all = await _userManagement.UserManager.GetAllUsertypesAsync();

            Assert.AreEqual(all[0], adminUsertype);
            Assert.AreEqual(all[1], defaultUsertype);
        }

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
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

        [Test, NonParallelizable]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public async Task SerializeToFile_ShouldWriteIdenticalStringToFile_AsTheDeserializedUserFromAString(int i)
        {
            if (i == 1)
            {
                await _userManagement.UserManager.DeSerializeFromStringAsync(userXml);
                _userManagement.UserManager.SerializeToFile(await _userManagement.UserManager.GetUserAsync(1), "user1.xml");
                Assert.AreEqual(userXml, File.ReadAllText("user1.xml"));
                File.Delete("user1.xml");
            }
            else if (i == 2)
            {
                await _userManagement.UserManager.DeSerializeFromStringAsync(userCsv);
                _userManagement.UserManager.SerializeToFile(await _userManagement.UserManager.GetUserAsync(1), "user1.csv");
                Assert.AreEqual(userCsv, File.ReadAllText("user1.csv"));
                File.Delete("user1.csv");
            }
            else if (i == 3)
            {
                await _userManagement.UserManager.DeSerializeFromStringAsync(userJson);
                _userManagement.UserManager.SerializeToFile(await _userManagement.UserManager.GetUserAsync(1), "user1.json");
                Assert.AreEqual(userJson, File.ReadAllText("user1.json"));
                File.Delete("user1.json");
            }
            else if (i == 4)
            {
                await _userManagement.UserManager.DeSerializeFromStringAsync(userAdrXml);
                _userManagement.UserManager.SerializeToFile(await _userManagement.UserManager.GetUserAsync(1), "user2.xml");
                Assert.AreEqual(userAdrXml, File.ReadAllText("user2.xml"));
                File.Delete("user2.xml");
            }
            else if (i == 5)
            {
                await _userManagement.UserManager.DeSerializeFromStringAsync(userAdrCsv);
                _userManagement.UserManager.SerializeToFile(await _userManagement.UserManager.GetUserAsync(1), "user2.csv");
                Assert.AreEqual(userAdrCsv, File.ReadAllText("user2.csv"));
                File.Delete("user2.csv");
            }
            else
            {
                await _userManagement.UserManager.DeSerializeFromStringAsync(userAdrJson);
                _userManagement.UserManager.SerializeToFile(await _userManagement.UserManager.GetUserAsync(1), "user2.json");
                Assert.AreEqual(userAdrJson, File.ReadAllText("user2.json"));
                File.Delete("user2.json");
            }
        }

        [Test, NonParallelizable]
        public async Task GetUserEmailFromJwtTokenAsync_ShouldReturnCorrectEmail_FromJwtToken_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.AreEqual(email, _userManagement.UserManager.GetUserEmailFromJwtToken(token));
        }

        [Test, NonParallelizable]
        public async Task GetUserIdFromJwtTokenAsync_ShouldReturnCorrectId_FromJwtToken_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.AreEqual(1, _userManagement.UserManager.GetUserIdFromJwtToken(token));
        }

        [Test, NonParallelizable]
        public async Task GetUsertypeFromJwtTokenAsync_ShouldReturnCorrectUsertype_FromJwtToken_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            var usertype = await _userManagement.UserManager.GetUsertypeFromJwtTokenAsync(token);

            Assert.AreEqual(defaultUsertype, usertype);
        }

        [Test, NonParallelizable]
        public async Task GetUsertypeObjectFromJwtTokenAsync_ShouldReturnCorrectUsertype_FromJwtToken_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            var usertype = await _userManagement.UserManager.GetUsertypeObjectFromJwtTokenAsync(token);

            Assert.AreEqual(defaultUsertype, usertype.Type);
        }

        [Test, NonParallelizable]
        public async Task ValidateJwtToken_ShouldReturnTrue_IfSecretKeyIsCorrectAndUserHasNotLoggedOut_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.True(await _userManagement.UserManager.ValidateJwtTokenAsync(token, jwtSecretKey));
        }

        [Test, NonParallelizable]
        public async Task ValidateJwtToken_ShouldReturnFalse_IfSecretKeyIsWrong_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.False(await _userManagement.UserManager.ValidateJwtTokenAsync(token, jwtSecretKey + "s"));
        }

        [Test, NonParallelizable]
        public async Task ValidateJwtToken_ShouldReturnFalseIfUserHasLoggedOut_WhenLoggingOutWithUserEmail_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.True(await _userManagement.UserManager.ValidateJwtTokenAsync(token, jwtSecretKey));

            await _userManagement.UserManager.LogoutAsync(email);

            Assert.False(await _userManagement.UserManager.ValidateJwtTokenAsync(token, jwtSecretKey));
        }

        [Test, NonParallelizable]
        public async Task ValidateJwtToken_ShouldReturnFalseIfUserHasLoggedOut_WhenLoggingOutWithUserId_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            var createdUser = await _userManagement.UserManager.GetUserAsync(email);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.True(await _userManagement.UserManager.ValidateJwtTokenAsync(token, jwtSecretKey));

            await _userManagement.UserManager.LogoutAsync(createdUser.Id);

            Assert.False(await _userManagement.UserManager.ValidateJwtTokenAsync(token, jwtSecretKey));
        }

        [Test, NonParallelizable]
        public async Task DoesUserHaveCorrectUsertypeAsync_ShouldReturnTrueIfJwtHasCorrectUsertype_WhenPassingUsertypeAsString_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.True(await _userManagement.UserManager.DoesUserHaveCorrectUsertypeAsync(token, defaultUsertype));
        }

        [Test, NonParallelizable]
        public async Task DoesUserHaveCorrectUsertypeAsync_ShouldReturnFalseIfJwtDoesNotHaveCorrectUsertype_WhenPassingUsertypeAsString_Async()
        {
            await _userManagement.UserManager.CreateUserAsync(email, password, password, firstname, lastname);

            await _userManagement.UserManager.ActivateUserAsync(email, "");

            var token = await _userManagement.UserManager.LoginAsync(email, password, jwtSecretKey);

            Assert.False(await _userManagement.UserManager.DoesUserHaveCorrectUsertypeAsync(token, adminUsertype));
        }
    }
}
