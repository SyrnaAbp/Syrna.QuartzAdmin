using System;
namespace Syrna.QuartzAdmin
{
    public class Key
    {
        public string Name { get; set; }
        public string Group { get; set; }

        public bool Equals(string name, string group)
        {
            return Name == name && Group == group;
        }

        public static Key Create(string name, string group)
        {
            return new Key() { Name = name, Group = group };
        }
    }
}

