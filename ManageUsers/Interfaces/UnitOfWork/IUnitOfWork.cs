using ManageUsers.Interfaces.Repository;
using System.Data.SQLite;

namespace ManageUsers.Interfaces.UnitOfWork
{
    internal interface IUnitOfWork
    {
        IAddressRepository AddressRepository { get; }
        IPasswordPolicyRepository PasswordPolicyRepository { get; }
        SQLiteConnection SQLiteConnection { get; }
        IUserRepository UserRepository { get; }
        IUsertypeRepository UsertypeRepository { get; }
    }
}