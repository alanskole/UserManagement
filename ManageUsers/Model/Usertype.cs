using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ManageUsers.Model
{
    /// <summary>
    /// A class representing a usertype/role.
    /// </summary>
    [DataContract(Name = "Usertype", Namespace = "")]
    public class Usertype
    {
        private int _id;
        private string _type;

        /// <summary>
        /// Gets the ID of the usertype.
        /// </summary>
        [DataMember(Name = "Id", Order = 0)]
        [JsonProperty]
        public int Id { get => _id; internal set => _id = value; }

        /// <summary>
        /// Gets the type of the usertype as a string.
        /// </summary>
        [DataMember(Name = "Type", Order = 1)]
        [JsonProperty]
        public string Type { get => _type; internal set => _type = value; }

        /// <summary>
        /// ToString method that prints out the usertype.
        /// </summary>
        /// <returns>A string of the usertype property values.</returns>
        public override string ToString()
        {
            return $"Usertype ID: {Id}\nUsertype Type: {Type}";
        }

        internal Usertype()
        {

        }

        internal Usertype(string type)
        {
            _type = type;
        }
    }
}
