using SimpleDataAccessLayer.Util.Attributes;
using System;

namespace SimpleDataAccessLayer.Model
{
    [Table("Person")]
    public class Person
    {
        [TableColumn("Id", primaryKey:true)]
        public Guid Id { get; set; }
        [TableColumn("Name")]
        public string Name { get; set; }
        [TableColumn("BirthDate")]
        public DateTime BirthDate { get; set; }
        [TableColumn("MailAddress")]
        public string MailAddress { get; set; }
        [TableColumn("City")]
        public string City { get; set; }
        [TableColumn("RegisterDate")]
        public DateTime RegisterDate { get; set; }
        [TableColumn("Active")]
        public bool Active { get; set; }
    }
}
