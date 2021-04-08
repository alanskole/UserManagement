using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    internal class ParameterException : Exception
    {

        public ParameterException(string message) : base(message)
        {
        }

        public ParameterException(string parameter, string value) : base($"{parameter} cannot be {value}!")
        {
        }
    }
}
