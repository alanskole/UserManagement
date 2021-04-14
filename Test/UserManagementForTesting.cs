using ManageUsers;

namespace Test
{
    internal class UserManagementForTesting : UserManagement
    {
        public UserManagementForTesting(string connectionString, string senderEmailAddress, string senderEmailPassword) : base(connectionString, senderEmailAddress, senderEmailPassword)
        {
        }
    }
}
