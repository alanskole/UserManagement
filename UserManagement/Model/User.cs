using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Model
{
    public class User
    {
        private int _id;
        private string _email;
        private string _password;
        private string _firstname;
        private string _lastname;
        private Address _address;
        private int _isActivated;
        private int _mustChangePassword;
        private Usertype _usertype;

        public int Id { get => _id; set => _id = value; }
        public string Email { get => _email; set => _email = value; }
        public string Password { get => _password; set => _password = value; }
        public string Firstname { get => _firstname; set => _firstname = value; }
        public string Lastname { get => _lastname; set => _lastname = value; }
        public Address Address { get => _address; set => _address = value; }
        public int IsActivated { get => _isActivated; set => _isActivated = value; }
        public int MustChangePassword { get => _mustChangePassword; set => _mustChangePassword = value; }
        public Usertype Usertype { get => _usertype; set => _usertype = value; }

        public User()
        {

        }

        public User(string email, string password, string firstname, string lastname, Address address, Usertype usertype)
        {
            _email = email;
            _password = password;
            _firstname = firstname;
            _lastname = lastname;
            _address = address;
            _usertype = usertype;
        }

        public User(string email, string password, string firstname, string lastname, Usertype usertype)
        {
            _email = email;
            _password = password;
            _firstname = firstname;
            _lastname = lastname;
            _usertype = usertype;
        }
    }
}
