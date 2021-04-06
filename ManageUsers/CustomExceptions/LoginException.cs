using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    public class LoginException : Exception
    {
        public LoginException(string cause) : base(cause)
        {
        }
    }
}
