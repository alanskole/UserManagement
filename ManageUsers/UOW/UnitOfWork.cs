using ManageUsers.Interfaces.Repository;
using ManageUsers.Interfaces.UnitOfWork;
using ManageUsers.Repository;
using System.Data.SQLite;

namespace ManageUsers.UOW
{
    internal class UnitOfWork : IUnitOfWork
    {
        private SQLiteConnection _sQLiteConnection;

        public IUserRepository UserRepository { get => new UserRepository(_sQLiteConnection); }
        public IUsertypeRepository UsertypeRepository { get => new UsertypeRepository(_sQLiteConnection); }
        public IAddressRepository AddressRepository { get => new AddressRepository(_sQLiteConnection); }
        public IPasswordPolicyRepository PasswordPolicyRepository { get => new PasswordPolicyRepository(_sQLiteConnection); }

        public SQLiteConnection SQLiteConnection { get => _sQLiteConnection; }

        public UnitOfWork(string connectionString)
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
        }
    }
}
