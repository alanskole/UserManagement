using System;

namespace UserManagement.CustomExceptions
{
    [Serializable]
    public class FailedToCreateException : Exception
    {
        public FailedToCreateException(string type) : base($"{type} could not be created!")
        {
        }
    }
}