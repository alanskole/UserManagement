using ManageUsers.BusinessLogic.Imp;
using ManageUsers.BusinessLogic.Interface;
using ManageUsers.Repository.Imp;
using ManageUsers.Repository.Interface;
using Ninject;

namespace ManageUsers
{
    internal interface IServiceLocator
    {
        T Get<T>();
    }

    internal class ServiceLocator
    {
        private static IServiceLocator serviceLocator;

        static ServiceLocator()
        {
            serviceLocator = new DefaultServiceLocator();
        }

        public static IServiceLocator Current
        {
            get
            {
                return serviceLocator;
            }
        }

        private sealed class DefaultServiceLocator : IServiceLocator
        {
            private readonly IKernel kernel;

            public DefaultServiceLocator()
            {
                kernel = new StandardKernel();
                LoadBindings();
            }

            public T Get<T>()
            {
                return kernel.Get<T>();
            }

            private void LoadBindings()
            {
                kernel.Bind<IUserManager>().To<UserManager>().InTransientScope();
                kernel.Bind<ISetupTables>().To<SetupTables>().InTransientScope();
                kernel.Bind<IPasswordPolicy>().To<PasswordPolicy>().InTransientScope();
                kernel.Bind<IUserRepository>().To<UserRepository>().InTransientScope();
                kernel.Bind<IUsertypeRepository>().To<UsertypeRepository>().InTransientScope();
                kernel.Bind<IAddressRepository>().To<AddressRepository>().InTransientScope();
                kernel.Bind<IPasswordPolicyRepository>().To<PasswordPolicyRepository>().InTransientScope();
            }
        }
    }
}
