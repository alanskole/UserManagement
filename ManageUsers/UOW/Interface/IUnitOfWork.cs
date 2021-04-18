using ManageUsers.Repository.Interface;

namespace ManageUsers.UOW.Interface
{
    internal interface IUnitOfWork
    {
        IAddressRepository AddressRepository { get; }
        IPasswordPolicyRepository PasswordPolicyRepository { get; }
        IUserRepository UserRepository { get; }
        IUsertypeRepository UsertypeRepository { get; }
    }
}