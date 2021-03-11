using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UserManagement.Service;

namespace UserManagement
{
    public class UserManagementLibrary
    {
        public UserService userService = new UserService();
        public UsertypeService usertypeService = new UsertypeService();
        public AddressService addressService = new AddressService();
        public PasswordPolicyService policyService = new PasswordPolicyService();
    }
}
