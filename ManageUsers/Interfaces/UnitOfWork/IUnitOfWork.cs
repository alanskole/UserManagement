using ManageUsers.Repository;
using System.Data.SQLite;

namespace ManageUsers.Interfaces.UnitOfWork
{
    internal interface IUnitOfWork
    {
        AddressRepository AddressRepository { get; }
        PasswordPolicyRepository PasswordPolicyRepository { get; }
        SQLiteConnection SQLiteConnection { get; }
        UserRepository UserRepository { get; }
        UsertypeRepository UsertypeRepository { get; }
    }
}