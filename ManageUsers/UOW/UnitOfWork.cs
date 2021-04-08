using ManageUsers.Repository;
using System.Data.SQLite;

namespace ManageUsers.UOW
{
    internal class UnitOfWork
    {
        private SQLiteConnection _sQLiteConnection;

        public UserRepository UserRepository { get => new UserRepository(_sQLiteConnection); }
        public UsertypeRepository UsertypeRepository { get => new UsertypeRepository(_sQLiteConnection); }
        public AddressRepository AddressRepository { get => new AddressRepository(_sQLiteConnection); }
        public PasswordPolicyRepository PasswordPolicyRepository { get => new PasswordPolicyRepository(_sQLiteConnection); }

        public SQLiteConnection SQLiteConnection { get => _sQLiteConnection; }

        public UnitOfWork(string connectionString)
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
        }
    }
}
