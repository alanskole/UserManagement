using ManageUsers;

namespace Test
{
    internal class UserManagementForTesting : UserManagement
    {
        internal UserManagementForTesting(string connectionString) : base(connectionString)
        {

        }
    }
}
