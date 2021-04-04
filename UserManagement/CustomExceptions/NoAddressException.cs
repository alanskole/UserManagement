using System;

namespace UserManagement.CustomExceptions
{
    [Serializable]
    public class NoAddressException : Exception
    {
        public NoAddressException() : base("The user doesn't have an address registered!")
        {
        }
    }
}
