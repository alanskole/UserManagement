# ManageUsers
A .net core 3.1 class library for managing users and authentication. 

You only need to extend the abstract class UserManagement and pass a connection string to an sqlite database and an email address and password for the email to send account activation code and forgotten passwords to the registered users to the constructor of the class that extends the UserManagement class.

Avaiable on nuget.org https://www.nuget.org/packages/ManageUsers/

Performing initial setup of the class library by creating all the tables with connection string to the SQLite database, setting the email to use when sending the registered users account activation codes and forgotten passwords and the password for the given email.
```
using System.Threading.Tasks;
using ManageUsers;
namespace YourPackage
{
	public class YourClass : UserManagement
	{
		public YourClass(string connectionString, string senderEmailAddress, string senderEmailPassword) 
                    		 : base (connectionString, senderEmailAddress, senderEmailPassword)
		{
		}
		static async Task Main(string[] args)
		{
			var connectionString = "Data Source=YourSqlDb.db;";
			var library = new YourClass(connectionString, "your@email.tld", "yourEmailPassword");
			await library.SetupTables.CreateTablesAsync();
		}
	}
}
```
Creating new users, the first user has usertype/role User without an address and the second is Admin and has an address
```
await library.UserManager.CreateUserAsync("john@smith.com", "password", "password", "John", "Smith", "User");

//usertype User is default and the last argument can be left out and the line above can be written like

await library.UserManager.CreateUserAsync("john@smith.com", "password", "password", "John", "Smith");

await library.UserManager.CreateUserAsync("bla@bla.no", "password33", "password33", "Jan", "Olsen", "Abc street", "14B", 1234, "Remmen", "Halden", "Norway", "Admin");
```

Create a new user or a list of users from a JSON/csv/xml string or file. The strings or files must be serialized by the library in order to be in the correct format!
```
string csvString = @"1,aa@aa.xx,uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==,first,user,0,,,,,,,False,False,2,User,";

string jsonUser = "{\"Id\":1,\"Email\":\"aa@aa.xx\",\"Password\":\"uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==\",\"Firstname\":\"first\",\"Lastname\":\"user\",\"Address\":null,\"IsActivated\":false,\"MustChangePassword\":false,\"Usertype\":{\"Id\":2,\"Type\":\"User\"},\"Picture\":null}";

string xmlUser = @"<User xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Id>1</Id><Email>aa@aa.xx</Email><Password>uOCnJ2IH3SMC4ksocSvnVseUvddcnluKSb7V7cpf8Xo=:MS4t5puCmSQ/IVnEnREoBQ==</Password><Firstname>first</Firstname><Lastname>user</Lastname><Address i:nil=""true""/><IsActivated>false</IsActivated><MustChangePassword>false</MustChangePassword><Usertype><Id>2</Id><Type>User</Type></Usertype><Picture i:nil=""true"" xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""/></User>";

await library.UserManager.DeSerializeFromStringAsync(jsonString);

var jsonUser = "path\\to\\user.json";
await library.UserManager.DeSerializeFromFileAsync(jsonUser);

await library.UserManager.DeSerializeFromStringAsync(csvString);

var csvUser = "path\\to\\user.csv";
await library.UserManager.DeSerializeFromFileAsync(csvUser);

await library.UserManager.DeSerializeFromStringAsync(xmlString);

var xmlUser = "path\\to\\user.xml";
await library.UserManager.DeSerializeFromFileAsync(xmlUser);
```

Adding more usertypes/roles other than Admin and User that are built in
```
await library.UserManager.AddMoreUsertypesAsync("Manager");
await library.UserManager.AddMoreUsertypesAsync("Employee", "Guest");
```

Getting a user from the database by id or email
```
var user1 = await library.UserManager.GetUserAsync(1);
var user2 = await library.UserManager.GetUserAsync("aa@dd.cc"); 
```

Getting all the users from the database, and getting all the users that have the usertype/role Admin.
```
var allUsers = await library.UserManager.GetAllUsersAsync();
var allAdmins = await library.UserManager.GetAllUsersAsync("Admin");
```
Get a list of all the images from user by ID or email.
```
var allImg = await library.UserManager.GetAllUserPictureAsync(2);
var allImg2 = awaitlibrary.UserManager.GetAllUserPictureAsync("em@ail.com");
```
Delete an existing user by ID or email.
```
await library.UserManager.DeleteUserAsync(100);
await library.UserManager.DeleteUserAsync("aa@dd.cc");
```
Updating a user's address by ID or email.
```
await library.UserManager.UpdateUserAsync(1, "New street", "10A", 4533, "Sellebakk","Fredrikstad", "Norway");
await library.UserManager.UpdateUserAsync("ss@aa.com", "Updated street", "15",  3322, "Grålum", "Sarpsborg", "Norway");
```
Updating a user's first- and last names by ID or email.
```
await library.UserManager.UpdateUserAsync(25, "John", "Lewis");
await library.UserManager.UpdateUserAsync("em@ail.com", "James", "Jameson");
```
Changing the email of user by ID or email.
```
await library.UserManager.UpdateUserAsync(50, "new@email.com");
await library.UserManager.UpdateUserAsync("some@email.com", "another@email.com");
```
Changing the usertype/role of a user.
```
await library.UserManager.UpdateUsertypeOfUserAsync(10, "User");
await library.UserManager.UpdateUsertypeOfUserAsync("some@email.com", "Admin");
```
Changing password of a user.
```
await library.UserManager.ChangePasswordAsync(25, "oldpassword", "newpassword",  "newpassword");
```
Random generation of passwords of length 10 or length set to be the minimum allowed length of the password policy.
```
var randomPass = await library.UserManager.GenerateRandomPasswordAsync(10);
var randomPass2 = await library.UserManager.GenerateRandomPasswordAsync();
```

Uploading a new photo.
```
await library.UserManager.AddUserPictureAsync(1, "myphoto.jpg", "second.jpeg");
await library.UserManager.AddUserPictureAsync("em@ail.com", "nicepicture.png");
```
Deleting all the photos of a user.
```
await library.UserManager.DeleteAllUserPicturesAsync(1);
await library.UserManager.DeleteAllUserPicturesAsync("em@ail.com");
```



Change the password policy to require at least 8 characters containing at least one number, one special character and one upper- and lowercase letter.

```
await library.PasswordPolicy.SetPolicyAsync(8, true, true, true); 
```
Forgotten password; getting a new temporary password by email.
```
await library.UserManager.ForgotPasswordAsync("email@email.com");
await library.UserManager.ForgotPasswordAsync(1);
```
Resetting the password after receving a temporary password when forgetting the original one.
```
await library.UserManager.SetPasswordAfterGettingTemporaryPasswordAsync("email@email.com", "tempPassowrd", "newPass", "newPass");
```
Login and get the generated jwt token.
```
var jwtTokenOfUser = await library.UserManager.LoginAsync("email@email.com", "password123", "Jwt secret key that must be used to decode the token");
```
Logout user.
```
await library.UserManager.LogoutAsync(1);
await library.UserManager.LogoutAsync("em@ail.com");
```
Account activation by entering the activation code received by email after signup.
```
await library.UserManager.ActivateUserAsync(1, "activate");
await library.UserManager.ActivateUserAsync("email@email.com", "accountActivator");
```
Resending the account activation code by email.
```
await library.UserManager.ResendAccountActivationCodeAsync(1);
await library.UserManager.ResendAccountActivationCodeAsync("aa@dd.cc");
```
Serialize user/list of users to JSON/csv/xml strings or files.
```
var user1 = await library.UserManager.GetUserAsync(1);
var userCsvString = library.UserManager.SerializeToCsvString(user1);
library.UserManager.SerializeToFile(user1, "user1.csv");

var user2 = await library.UserManager.GetUserAsync("em@ail.com");
var userJsonString = library.UserManager.SerializeToJsonString(user2);
library.UserManager.SerializeToFile(user2, "user2.json");

var allUsers = await library.UserManager.GetAllUsersAsync();
var userXmlString = library.UserManager.SerializeToXmlString(allUsers);
library.UserManager.SerializeToFile(allUsers, "users.xml");
```
Check if a user is logged in by validating their JWT token.
```
bool isUserLoggedIn = await library.UserManager.ValidateJwtTokenAsync("the jwt token", "Jwt secret key to decode the token");
```
Get the user ID from a JWT token.
```
var userId = library.UserManager.GetUserIdFromJwtToken("the jwt token", "Jwt secret key to decode the token");
```
Get the user email from a JWT token.
```
var userEmail = library.UserManager.GetUserEmailFromJwtToken("the jwt token", "Jwt secret key to decode the token");
```
Get the usertype from a JWT token.
```
var userType = library.UserManager.GetUsertypeFromJwtTokenAsync("the jwt token", "Jwt secret key to decode the token");
```
Check if user's usertype is User by examining JWT token.
```
bool isUser = await library.UserManager.DoesUserHaveCorrectUsertypeAsync("the jwt token", "Jwt secret key to decode the token", "User");
```
Check if user is Admin by examining JWT token.
```
bool isAdmin = await library.UserManager.IsAdminAsync("the jwt token", "Jwt secret key to decode the token");
```
