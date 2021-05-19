# ManageUsers
A .net core 3.1 class library for managing users and authentication. 

You only need to extend the abstract class UserManagement and pass a connection string to an sqlite database and an email address and password for the email to send account activation code and forgotten passwords to the registered users to the constructor of the class that extends the UserManagement class.

Avaiable on nuget.org https://www.nuget.org/packages/ManageUsers/

Performing initial setup of the class library by creating all the tables with connection string to the SQLite database "YourSqlDb.db", setting the email to use when sending the registered users account activation codes and forgotten passwords "your@email.tld" and the password for the given email "yourEmailPassword":
```
using System.Threading.Tasks;
using ManageUsers;
namespace YourPackage
{
	public class YourClass : UserManagement
	{
		public YourClass(string connectionString, string senderEmailAddress, string senderEmailPassword) : base 
                    (connectionString, senderEmailAddress, senderEmailPassword)
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
Creating new users
```
await library.UserManager.CreateUserAsync("john@smith.com", "password", "password", "John", "Smith", "User");

await library.UserManager.CreateUserAsync("bla@bla.no", "password33", "password33", "Jan", "Olsen", "Abc street", "14B", 1234, "Remmen", "Halden", "Norway", "Admin");
```
1.2	Create a new user from a JSON string.
Create a new user from a JSON file with the path "user.json".
Create a new user from a CSV string.
Create a new user from a CSV file with the path "user.csv".

Create a new user from a XML string.

Create a new user from a XML file with the path "user.xml".

var jsonString = The text from Appendix E;
await library.UserManager.DeSerializeFromStringAsync(jsonString);
var jsonUser = "user.json";
await library.UserManager.DeSerializeFromFileAsync(jsonUser);
var csvString = The text from Appendix D;
await library.UserManager.DeSerializeFromStringAsync(csvString);
var csvUser = "user.csv";
await library.UserManager.DeSerializeFromFileAsync(csvUser);
var xmlString = The text from Appendix F;
await library.UserManager.DeSerializeFromStringAsync(xmlString);
var xmlUser = "user.xml";
await library.UserManager.DeSerializeFromFileAsync(xmlUser);

1.3	Create a new usertype called manager:

Create two new usertypes, one called employee and another called guest:

await library.UserManager.AddMoreUsertypesAsync("Manager");
await library.UserManager.AddMoreUsertypesAsync("Employee", "Guest");





2.1	Get a user from the database with ID 1.
Get a user from the database with email "aa@dd.cc".
Get a list of all the users in the database.
Get a list of all the usertypes in the database.
Get a list of all the admin users in the database:

var user1 = await library.UserManager.GetUserAsync(1);
var user2 = await library.UserManager.GetUserAsync("aa@dd.cc"); 
var allUsers = await library.UserManager.GetAllUsersAsync();
var allUsertypes = await library.UserManager.GetAllUsertypesAsync(); 
var allAdmins = await library.UserManager.GetAllUsersAsync("Admin");

2.2	Get an image with path "1.png" from user with ID 1.
Get an image with index 4 from user with ID 3.
Get an image with path "pic.jpeg" from user with email "em@ail.com".
Get an image the first picture from user with email "user@mail.com":

var img = await library.UserManager.GetUserPictureAsync(1, "1.png");
var img2 = await library.UserManager.GetUserPictureAsync(3, 4);
var img3 = await library.UserManager.GetUserPictureAsync("em@ail.com",   
                                                         "pic.jpeg");
var img4 = await library.UserManager.GetUserPictureAsync("user@mail.com", 1);

2.3	Get a list of all the images from user with ID 2.
Get a list of all the images from user with email "em@ail.com":

var allImg = await library.UserManager.GetAllUserPictureAsync(2);
var allImg2 = awaitlibrary.UserManager.GetAllUserPictureAsync("em@ail.com");

3.1	Delete an existing user with ID 100.

Delete an existing user with the email "aa@dd.cc":

await library.UserManager.DeleteUserAsync(100);
await library.UserManager.DeleteUserAsync("aa@dd.cc");

3.2	Update the address of user with ID 1, set the street name to "New street", number to 10A, zip to 4533, area to "Sellebakk", city to "Fredrikstad" and country to "Norway".
Update the address of user with "email ss@aa.com", set the street name to "Updated street", number to 15, zip to 3322, area to "Grålum", city to "Sarpsborg" and country to "Norway":

await library.UserManager.UpdateUserAsync(1, "New street", "10A", 4533,  
                                          "Sellebakk","Fredrikstad", "Norway");
await library.UserManager.UpdateUserAsync("ss@aa.com", "Updated street", "15", 
                                          3322, "Grålum", "Sarpsborg", 
                                          "Norway");






3.3	Update the name of user with ID 25, set first name to "John", last name to "Lewis".

Update the name of user with "em@ail.com", set first name to "James", last name to "Jameson":

await library.UserManager.UpdateUserAsync(25, "John", "Lewis");
await library.UserManager.UpdateUserAsync("em@ail.com", "James", "Jameson");

3.4	User with ID 50 changes their email from original@email.com to "new@email.com".

User with email some@email.com changes their email to "another@email.com":

await library.UserManager.UpdateUserAsync(50, "new@email.com");
await library.UserManager.UpdateUserAsync("some@email.com",  
                                          "another@email.com");

3.5	Changing usertype of user with ID 1 to Admin

Changing usertype of user with ID 10 to User.

Changing usertype of user with email "user@email.com" to Admin:

await library.UserManager.UpdateUsertypeOfUserAsync(1, "Admin");
await library.UserManager.UpdateUsertypeOfUserAsync(10, "User");
await library.UserManager.UpdateUsertypeOfUserAsync("some@email.com", "Admin");

3.6	User with ID 25 changes the password from "oldpassword" to "newpassword".

User with email "em@ail.com" changes the password from "blabla33" to a randomly generated password of length 10:

await library.UserManager.ChangePasswordAsync(25, "oldpassword", "newpassword", 
                                              "newpassword");
var randomPass = await library.UserManager.GenerateRandomPasswordAsync(10);
await library.UserManager.ChangePasswordAsync("em@ail.com", "blabla33", 
                                              "randomPass", "randomPass");

3.7	User with ID 1 uploads a new photo with filepath "myphoto.jpg" and "second.jpeg".

User with email "em@ail.com" uploads a new photo with filepath "nicepicture.png":

await library.UserManager.AddUserPictureAsync(1, "myphoto.jpg", "second.jpeg");
await library.UserManager.AddUserPictureAsync("em@ail.com", "nicepicture.png");

3.8	User with ID 1 deletes a photo with filepath "myphoto.png" and "secondphoto.jpeg".
User with ID 3 deletes their photo with index 4 and index 7.

User with email "aa@dd.cc" deletes their first and last image:

await library.UserManager.DeleteUserPictureAsync(1, "myphoto.png", 
                                                 "secondphoto.jpeg");
await library.UserManager.DeleteUserPictureAsync(3, 4, 7);

await library.UserManager.DeleteUserPictureAsync("em@ail.com", "pic.jpeg");
await library.UserManager.DeleteUserPictureAsync("aa@dd.cc",  
                                                 listOfUserImages[0], 
                                  listOfUserImages[listOfUserImages.Length-1]);
3.9	User with ID 1 deletes all their photos.

User with email "em@ail.com" deletes all their photos:

await library.UserManager.DeleteAllUserPicturesAsync(1);
await library.UserManager.DeleteAllUserPicturesAsync("em@ail.com");

4.1	Change the password policy so passwords must include at least 8 characters containing at least one number and letter.
Change the password policy to a stricter policy that requires at least 8 characters containing at least one number and one upper- and lowercase letter.
Change the password policy to enforce requirement of at least 8 characters containing minimum one letter, one number and one special character.
Change the password policy to require at least 8 characters containing at least one number, one special character and one upper- and lowercase letter.
Reset the password policy to the default policy that requires a password of at least 6 characters:

await library.PasswordPolicy.SetPolicyAsync(8, false, true, false); 
await library.PasswordPolicy.SetPolicyAsync(8, true, true, false); 
await library.PasswordPolicy.SetPolicyAsync(8, false, true, true); 
await library.PasswordPolicy.SetPolicyAsync(8, true, true, true); 
await library.PasswordPolicy.DefaultPolicyAsync();


4.2	User with email "email@email.com" has forgotten their password and requests a temporary password sent to their email.
User with ID 1 has forgotten their password and requests a temporary password sent to their email. User with email email "email@email.com" sets a new password after receiving the one-time password, "tempPassowrd", to "newPass":

await library.UserManager.ForgotPasswordAsync("email@email.com");
await library.UserManager.ForgotPasswordAsync(1);
await library.UserManager
      .SetPasswordAfterGettingTemporaryPasswordAsync("email@email.com",  
                                                     "tempPassowrd", "newPass", 
                                                     "newPass");

5.1	Login the user with email "email@email.com" and password "password123" and return the generated jwt:

var jwtTokenOfUser = await library.UserManager.LoginAsync("email@email.com",
                                                          "password123",
                                                          "Jwt secret key");

5.2	Logout user with ID 1
Logout user with email "em@ail.com":

await library.UserManager.LogoutAsync(1);
await library.UserManager.LogoutAsync("em@ail.com");



6.1	User with ID 1 activates their account with the activation code "activate".
User with email "user@email.com" activates their account with the activation code "accountActivator":

await library.UserManager.ActivateUserAsync(1, "activate");
await library.UserManager.ActivateUserAsync("email@email.com",  
                                            "accountActivator");


6.2	User with ID 1 requests a new account activation code sent to their email.

User with email "aa@dd.cc" requests a new account activation code sent to their email:

await library.UserManager.ResendAccountActivationCodeAsync(1);
await library.UserManager.ResendAccountActivationCodeAsync("aa@dd.cc");

7.1	Serialize user with ID 1 to a csv string.
Serialize user with ID 1 to a csv file with the path "user1.csv".

Serialize user with email "em@ail.com" to a json string.
Serialize user with email "em@ail.com" to a json file with the path "user2.json".

Serialize all users to xml string.


Serialize all users to xml file with the path "users.xml":

var user1 = await library.UserManager.GetUserAsync(1);
var userCsvString = library.UserManager.SerializeToCsvString(user1);
library.UserManager.SerializeToFile(user1, "user1.csv");
var user2 = await library.UserManager.GetUserAsync("em@ail.com");
var userJsonString = library.UserManager.SerializeToJsonString(user2);
library.UserManager.SerializeToFile(user2, "user2.json");
var allUsers = await library.UserManager.GetAllUsersAsync();
var userXmlString = library.UserManager.SerializeToXmlString(allUsers);
library.UserManager.SerializeToFile(allUsers, "users.xml");



















8.1	Check if a user is logged in by validating their JWT token.
Get the user ID from a JWT token.
Get the user email from a JWT token.
Get the usertype from a JWT token.
Check if user's usertype is User by examining JWT token.

Check if user is Admin by examining JWT token:

bool isUserLoggedIn = await library.UserManager
                            .ValidateJwtTokenAsync("the jwt token", 
                                                   "Jwt secret key");
var userId = library.UserManager.GetUserIdFromJwtToken("the jwt token",
                                                       "Jwt secret key");
var userEmail = library.UserManager.GetUserEmailFromJwtToken("the jwt token",  
                                                             "Jwt secret key");
var userType = await library.UserManager
                     .GetUsertypeFromJwtTokenAsync("the jwt token", 
                                                   "Jwt secret key");
bool isUser = await library.UserManager
                    .DoesUserHaveCorrectUsertypeAsync("the jwt token",                 
                                                      "Jwt secret key",  
                                                      "User");
bool isAdmin = await library.UserManager.IsAdminAsync("the jwt token",       
                                                      "Jwt secret key");
//The line above can also be done like:
bool isAdmin = await library.UserManager
                     .DoesUserHaveCorrectUsertypeAsync("the jwt token",          
                                                       "Jwt secret key", 
                                                       "Admin");
