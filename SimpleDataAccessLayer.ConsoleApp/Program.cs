using SimpleDataAccessLayer.Business;
using SimpleDataAccessLayer.Model;
using SimpleDataAccessLayer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccessLayer.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            // simple example
            Person _person = new Person();
            _person.Name = "Agrimaldo";
            _person.City = "São Paulo";
            _person.BirthDate = DateTime.Parse("14-06-1990");

            _person.MailAddress = "agrimaltr00@hotmail.com";
            _person.Active = true;
            _person.RegisterDate = DateTime.Now;

            _person = PersonBusiness.Create<Person>(_person);

            Document _personDoc = ExtensionMethod.ConvertBaseToSubType<Document>(_person);
            _personDoc.Description = "RG";
            _personDoc.Value = "00.000.000-0";
            _personDoc = PersonBusiness.Create<Document>(_personDoc);

            Console.WriteLine("Finish");
            Console.ReadKey(); 
        }
    }
}
