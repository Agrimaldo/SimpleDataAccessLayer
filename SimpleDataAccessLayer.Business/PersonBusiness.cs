using SimpleDataAccessLayer.Data;
using SimpleDataAccessLayer.Model;
using System;

namespace SimpleDataAccessLayer.Business
{
    public class PersonBusiness
    {
        public static void Create(Person person)
        {
            using (Repository _repository = new Repository())
            {
                _repository.Add<Person>(person);
            }  
        }

        public static void Create(Document personDoc)
        {
            using (Repository _repository = new Repository())
            {
                _repository.Add<Document>(personDoc);
            }
        }

        public static T Create<T>(T person)
        {
            using (Repository _repository = new Repository())
            {
                return _repository.Add<T>(person);
            }
        }
    }
}
