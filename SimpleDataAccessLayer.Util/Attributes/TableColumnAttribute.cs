using System;

namespace SimpleDataAccessLayer.Util.Attributes
{
    public class TableColumnAttribute : Attribute
    {
        public TableColumnAttribute(string name, bool primaryKey = false, bool update = true)
        {
            this.Name = name;
            this.PrimaryKey = primaryKey;
            this.Update = update;
        }
        public string Name { get; set; }

        public bool PrimaryKey { get; set; }

        public bool Update { get; set; }
    }
}
