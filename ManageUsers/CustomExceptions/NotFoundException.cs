using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(string type) : base($"{type} not found in the system!")
        {
        }
    }
}
