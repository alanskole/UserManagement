using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    public class NoneFoundInDatabaseTableException : Exception
    {
        public NoneFoundInDatabaseTableException(string type) : base($"No {type} exist in the database!")
        {
        }
    }
}
