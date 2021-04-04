using UserManagement.Repository;

namespace UserManagement.UOW
{
    internal class UnitOfWork
    {
        private UserRepository _userRepository;
        private UsertypeRepository _usertypeRepository;
        private AddressRepository _addressRepository;
        private PasswordPolicyRepository _passwordPolicyRepository;

        public UserRepository UserRepository { get => _userRepository; set => _userRepository = value; }
        public UsertypeRepository UsertypeRepository { get => _usertypeRepository; set => _usertypeRepository = value; }
        public AddressRepository AddressRepository { get => _addressRepository; set => _addressRepository = value; }
        public PasswordPolicyRepository PasswordPolicyRepository { get => _passwordPolicyRepository; set => _passwordPolicyRepository = value; }

        public UnitOfWork()
        {
            _userRepository = new UserRepository();
            _usertypeRepository = new UsertypeRepository();
            _addressRepository = new AddressRepository();
            _passwordPolicyRepository = new PasswordPolicyRepository();
        }
        public UnitOfWork(UserRepository userRepository, UsertypeRepository usertypeRepository, AddressRepository addressRepository, PasswordPolicyRepository passwordPolicyRepository)
        {
            _userRepository = userRepository;
            _usertypeRepository = usertypeRepository;
            _addressRepository = addressRepository;
            _passwordPolicyRepository = passwordPolicyRepository;
        }

    }
}
