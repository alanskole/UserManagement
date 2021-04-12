using ManageUsers.Repository.Imp;
using ManageUsers.Repository.Interface;
using ManageUsers.UOW.Interface;
using System.Data.SQLite;

namespace ManageUsers.UOW.Imp
{
    internal class UnitOfWork : IUnitOfWork
    {
        private SQLiteConnection _sQLiteConnection;
        private IUserRepository _userRepository;
        private IUsertypeRepository _usertypeRepository;
        private IAddressRepository _addressRepository;
        private IPasswordPolicyRepository _passwordPolicyRepository;

        public IUserRepository UserRepository { get => _userRepository; }
        public IUsertypeRepository UsertypeRepository { get => _usertypeRepository; }
        public IAddressRepository AddressRepository { get => _addressRepository; }
        public IPasswordPolicyRepository PasswordPolicyRepository { get => _passwordPolicyRepository; }

        public SQLiteConnection SQLiteConnection { get => _sQLiteConnection; }

        public UnitOfWork(string connectionString)
        {
            _sQLiteConnection = new SQLiteConnection(connectionString);
            _userRepository = new UserRepository(_sQLiteConnection);
            _usertypeRepository = new UsertypeRepository(_sQLiteConnection);
            _addressRepository = new AddressRepository(_sQLiteConnection);
            _passwordPolicyRepository = new PasswordPolicyRepository(_sQLiteConnection);
        }
    }
}
