using System;

namespace UserManagement.CustomExceptions
{
    [Serializable]
    public class NoneFoundInDatabaseTableException : Exception
    {
        public NoneFoundInDatabaseTableException(string type) : base($"No {type} exist in the database!")
        {
        }
    }
}
