using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ManageUsers.Model
{
    /// <summary>
    /// A class representing a user.
    /// </summary>
    [DataContract(Name = "User", Namespace = "")]
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

        /// <summary>
        /// Gets the Id of the user.
        /// </summary>
        [DataMember(Name = "Id", Order = 0)]
        [JsonProperty]
        public int Id { get => _id; internal set => _id = value; }

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        [DataMember(Name = "Email", Order = 1)]
        [JsonProperty]
        public string Email { get => _email; internal set => _email = value; }

        /// <summary>
        /// Gets the password of the user.
        /// </summary>
        [DataMember(Name = "Password", Order = 2)]
        [JsonProperty]
        public string Password { get => _password; internal set => _password = value; }

        /// <summary>
        /// Gets the firstname of the user.
        /// </summary>
        [DataMember(Name = "Firstname", Order = 3)]
        [JsonProperty]
        public string Firstname { get => _firstname; internal set => _firstname = value; }

        /// <summary>
        /// Gets the lastname of the user.
        /// </summary>
        [DataMember(Name = "Lastname", Order = 4)]
        [JsonProperty]
        public string Lastname { get => _lastname; internal set => _lastname = value; }

        /// <summary>
        /// Gets the address of the user.
        /// </summary>
        [DataMember(Name = "Address", Order = 5)]
        [JsonProperty]
        public Address Address { get => _address; internal set => _address = value; }

        /// <summary>
        /// Gets the bool value that represents if the account is activated or not.
        /// </summary>
        [DataMember(Name = "IsActivated", Order = 6)]
        [JsonProperty]
        public bool IsActivated { get => _isActivated; internal set => _isActivated = value; }

        /// <summary>
        /// Gets the bool value that represents if the user must change their password or not.
        /// </summary>
        [DataMember(Name = "MustChangePassword", Order = 7)]
        [JsonProperty]
        public bool MustChangePassword { get => _mustChangePassword; internal set => _mustChangePassword = value; }

        /// <summary>
        /// Gets the usertype of the user.
        /// </summary>
        [DataMember(Name = "Usertype", Order = 8)]
        [JsonProperty]
        public Usertype Usertype { get => _usertype; internal set => _usertype = value; }

        /// <summary>
        /// Gets the pictures of the user.
        /// </summary>
        [DataMember(Name = "Picture", Order = 9)]
        [JsonProperty]
        public List<byte[]> Picture { get => _picture; internal set => _picture = value; }

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

        /// <summary>
        /// ToString method that prints out the user.
        /// </summary>
        /// <returns>A string of the user property values.</returns>
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
