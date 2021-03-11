using System;

namespace UserManagement.CustomExceptions
{
    [Serializable]
    public class LoginException : Exception
    {
        public LoginException(string cause) : base(cause)
        {
        }
    }
}
