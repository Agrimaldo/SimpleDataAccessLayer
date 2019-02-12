using SimpleDataAccessLayer.Util.Attributes;
using System;

namespace SimpleDataAccessLayer.Model
{
    [Table("Document", "IdPerson")]
    public class Document : Person
    {
        [TableColumn("Description")]
        public string Description { get; set; }
        [TableColumn("Value")]
        public string Value { get; set; }
    }
}
