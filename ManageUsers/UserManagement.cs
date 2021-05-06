using ManageUsers.BusinessLogic.Interface;
using ManageUsers.Helper;

namespace ManageUsers
{
    /// <summary>
    /// An abstract class containing all the methods that in the library.
    /// </summary>
    public abstract class UserManagement
    {
        private IUserManager _userManager;
        private ISetupTables _setupTables;
        private IPasswordPolicy _passwordPolicy;

        /// <summary>Getter for the UserManager class</summary>
        /// <returns>
        /// Gets the interface containing all the methods that are used to manage users in the library.
        /// </returns>
        public IUserManager UserManager { get => _userManager; }

        /// <summary>Getter for the SetupTables class</summary>
        /// <returns>
        /// Gets the interface with the method used to setup the tables of your database.
        /// </returns>
        public ISetupTables SetupTables { get => _setupTables; }

        /// <summary>Getter for the PasswordPolicy class</summary>
        /// <returns>
        /// Gets the interface containig methods to set the password policy for user passwords.
        /// </returns>
        public IPasswordPolicy PasswordPolicy { get => _passwordPolicy; }

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the SQLite database.</param>
        /// <param name="senderEmailAddress">The email address to send emails with activation codes and forgotten passwords to registered users from.</param>
        /// <param name="senderEmailPassword">The password of the email address to send emails with activation codes and forgotten passwords to registered users from.</param>
        public UserManagement(string connectionString, string senderEmailAddress, string senderEmailPassword)
        {
            Info.connectionString = connectionString;
            Info.senderEmail = senderEmailAddress;
            Info.emailPassword = senderEmailPassword;

            Setup(ServiceLocator.Current.Get<IUserManager>(),
            ServiceLocator.Current.Get<ISetupTables>(),
            ServiceLocator.Current.Get<IPasswordPolicy>());
        }

        private void Setup(IUserManager userManager, ISetupTables setupTables, IPasswordPolicy passwordPolicy)
        {
            _setupTables = setupTables;
            _userManager = userManager;
            _passwordPolicy = passwordPolicy;
        }
    }
}