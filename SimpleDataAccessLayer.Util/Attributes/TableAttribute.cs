using System;

namespace SimpleDataAccessLayer.Util.Attributes
{
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="foreignKey">In case of inheritance. </param>
        public TableAttribute(string name, string foreignKey)
        {
            this.Name = name;
            this.ForeignKey = foreignKey;
        }

        public string Name { get; set; }

        public string ForeignKey { get; set; }

    }
}
