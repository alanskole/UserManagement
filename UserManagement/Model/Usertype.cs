using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Model
{
    public class Usertype
    {
        private int _id;
        private string _type;
        public int Id { get => _id; set => _id = value; }
        public string Type { get => _type; set => _type = value; }

        public Usertype()
        {

        }
        
        public Usertype(string type)
        {
            _type = type;
        }  
    }
}
