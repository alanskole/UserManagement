namespace ManageUsers.Model
{
    public class Address
    {
        private int _id;
        private string _street;
        private string _number;
        private string _zip;
        private string _area;
        private string _city;
        private string _country;

        public int Id { get => _id; set => _id = value; }
        public string Street { get => _street; set => _street = value; }
        public string Number { get => _number; set => _number = value; }
        public string Zip { get => _zip; set => _zip = value; }
        public string Area { get => _area; set => _area = value; }
        public string City { get => _city; set => _city = value; }
        public string Country { get => _country; set => _country = value; }

        public override string ToString()
        {
            return $"Address ID: {Id}\nAddress Street: {Street}\nAddress Number: {Number}" +
                   $"\nAddress Zip: {Zip}\nAddress Area: {Area}\nAddress City: {City}\nAddress Country: {Country}";
        }

        public Address()
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
