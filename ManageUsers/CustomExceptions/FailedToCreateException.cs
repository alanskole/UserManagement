using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    internal class FailedToCreateException : Exception
    {
        public FailedToCreateException(string type) : base($"{type} could not be created!")
        {
        }
    }
}