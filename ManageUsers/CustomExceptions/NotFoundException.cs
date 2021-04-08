using System;

namespace ManageUsers.CustomExceptions
{
    [Serializable]
    internal class NotFoundException : Exception
    {
        public NotFoundException(string type) : base($"{type} not found in the system!")
        {
        }
    }
}
