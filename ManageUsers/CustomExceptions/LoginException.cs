using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    internal class LoginException : Exception
    {
        public LoginException(string cause) : base(cause)
        {
        }
    }
}
