using System;
using System.Collections.Generic;

namespace ManageUsers.Model
{
    public class User
    {
        private int _id;
        private string _email;
        private string _password;
        private string _firstname;
        private string _lastname;
        private Address _address;
        private bool _isActivated;
        private bool _mustChangePassword;
        private Usertype _usertype;
        private List<byte[]> _picture;

        public int Id { get => _id; set => _id = value; }
        public string Email { get => _email; set => _email = value; }
        public string Password { get => _password; set => _password = value; }
        public string Firstname { get => _firstname; set => _firstname = value; }
        public string Lastname { get => _lastname; set => _lastname = value; }
        public Address Address { get => _address; set => _address = value; }
        public bool IsActivated { get => _isActivated; set => _isActivated = value; }
        public bool MustChangePassword { get => _mustChangePassword; set => _mustChangePassword = value; }
        public Usertype Usertype { get => _usertype; set => _usertype = value; }
        public List<byte[]> Picture { get => _picture; set => _picture = value; }

        private string AddressWriter()
        {
            if (Address == null || String.IsNullOrEmpty(Address.Street))
                return "";

            return Address.ToString() + "\n";
        }

        private string PictureWriter()
        {
            if (Picture == null || Picture.Count == 0)
                return "";

            var str = "";

            foreach (var pic in Picture)
                str += "Picture: " + Convert.ToBase64String(pic) + "\n";

            return str.Substring(0, (str.Length - 1));
        }

        public override string ToString()
        {
            return $"User ID: {Id}\nUser Email: {Email}\nUser Firstname: {Firstname}\nUser Lastname{Lastname}\n{AddressWriter()}" +
                $"User Account Is Activated: {IsActivated}\nUser Must Change Password: {MustChangePassword}\n{Usertype.ToString()}" +
                $"\n{PictureWriter()}";
        }

        internal User()
        {

        }

        internal User(string email, string password, string firstname, string lastname, Address address, Usertype usertype)
        {
            _email = email;
            _password = password;
            _firstname = firstname;
            _lastname = lastname;
            _address = address;
            _usertype = usertype;
        }

        internal User(string email, string password, string firstname, string lastname, Usertype usertype)
        {
            _email = email;
            _password = password;
            _firstname = firstname;
            _lastname = lastname;
            _usertype = usertype;
        }
    }
}
