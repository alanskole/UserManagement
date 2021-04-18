using ManageUsers.Repository.Imp;
using ManageUsers.Repository.Interface;
using ManageUsers.UOW.Interface;
using System.Data.SQLite;

namespace ManageUsers.UOW.Imp
{
    internal class UnitOfWork : IUnitOfWork
    {
        private IUserRepository _userRepository;
        private IUsertypeRepository _usertypeRepository;
        private IAddressRepository _addressRepository;
        private IPasswordPolicyRepository _passwordPolicyRepository;

        public IUserRepository UserRepository { get => _userRepository; }
        public IUsertypeRepository UsertypeRepository { get => _usertypeRepository; }
        public IAddressRepository AddressRepository { get => _addressRepository; }
        public IPasswordPolicyRepository PasswordPolicyRepository { get => _passwordPolicyRepository; }

        public UnitOfWork(string connectionString)
        {
            var sqliteConnection = new SQLiteConnection(connectionString);
            _userRepository = new UserRepository(sqliteConnection);
            _usertypeRepository = new UsertypeRepository(sqliteConnection);
            _addressRepository = new AddressRepository(sqliteConnection);
            _passwordPolicyRepository = new PasswordPolicyRepository(sqliteConnection);
        }
    }
}
