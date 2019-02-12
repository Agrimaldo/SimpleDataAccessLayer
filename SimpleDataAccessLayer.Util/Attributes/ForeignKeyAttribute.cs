using System;

namespace SimpleDataAccessLayer.Util.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
    }
}
