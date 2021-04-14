using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    internal class NoAddressException : Exception
    {
        public NoAddressException() : base("The user doesn't have an address registered!")
        {
        }
    }
}
