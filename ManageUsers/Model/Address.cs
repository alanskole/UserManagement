using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ManageUsers.Model
{
    /// <summary>
    /// A class representing an address.
    /// </summary>
    [DataContract(Name = "Address", Namespace = "")]
    public class Address
    {
        private int _id;
        private string _street;
        private string _number;
        private string _zip;
        private string _area;
        private string _city;
        private string _country;

        /// <summary>
        /// Gets the Id of the address.
        /// </summary>
        [DataMember(Name = "Id", Order = 0)]
        [JsonProperty]
        public int Id { get => _id; internal set => _id = value; }

        /// <summary>
        /// Gets the street of the address.
        /// </summary>
        [DataMember(Name = "Street", Order = 1)]
        [JsonProperty]
        public string Street { get => _street; internal set => _street = value; }

        /// <summary>
        /// Gets the building number of the address.
        /// </summary>
        [DataMember(Name = "Number", Order = 2)]
        [JsonProperty]
        public string Number { get => _number; internal set => _number = value; }

        /// <summary>
        /// Gets the zip of the address.
        /// </summary>
        [DataMember(Name = "Zip", Order = 3)]
        [JsonProperty]
        public string Zip { get => _zip; internal set => _zip = value; }

        /// <summary>
        /// Gets the area of the address.
        /// </summary>
        [DataMember(Name = "Area", Order = 4)]
        [JsonProperty]
        public string Area { get => _area; internal set => _area = value; }

        /// <summary>
        /// Gets the city of the address.
        /// </summary>
        [DataMember(Name = "City", Order = 5)]
        [JsonProperty]
        public string City { get => _city; internal set => _city = value; }

        /// <summary>
        /// Gets the country of the address.
        /// </summary>
        [DataMember(Name = "Country", Order = 6)]
        [JsonProperty]
        public string Country { get => _country; internal set => _country = value; }

        /// <summary>
        /// ToString method that prints out the address.
        /// </summary>
        /// <returns>A string of the address property values.</returns>
        public override string ToString()
        {
            return $"Address ID: {Id}\nAddress Street: {Street}\nAddress Number: {Number}" +
                   $"\nAddress Zip: {Zip}\nAddress Area: {Area}\nAddress City: {City}\nAddress Country: {Country}";
        }

        internal Address()
        {

        }

        internal Address(string street, string number, string zip, string area, string city, string country)
        {
            _street = street;
            _number = number;
            _zip = zip;
            _area = area;
            _city = city;
            _country = country;
        }
    }
}
