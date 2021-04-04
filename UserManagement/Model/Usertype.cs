namespace UserManagement.Model
{
    public class Usertype
    {
        private int _id;
        private string _type;
        public int Id { get => _id; internal set => _id = value; }
        public string Type { get => _type; internal set => _type = value; }

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
