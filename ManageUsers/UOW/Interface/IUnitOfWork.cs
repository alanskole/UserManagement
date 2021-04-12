using ManageUsers.Repository.Interface;
using System.Data.SQLite;

namespace ManageUsers.UOW.Interface
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