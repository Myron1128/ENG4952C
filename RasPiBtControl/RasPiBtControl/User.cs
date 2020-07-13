using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasPiBtControl
{
    class User
    {
        public String username;
        public String first_name;
        public String last_name;
        public List<Contact> contacts;

        public User(String u, String f, String l, List<Contact> c) {
            username = u;
            first_name = f;
            last_name = l;
            contacts = c;
        }

        public String getUsername()
        {
            return username;
        }
        public String getFullName()
        {
            return first_name + " " + last_name;
        }
        public List<Contact> getContacts()
        {
            return contacts;
        }

        public void addContact(Contact c)
        {
            contacts.Add(c);
        }

        public void deleteContact(Contact c)
        {
            int index = 0;
            foreach (Contact contact in contacts) {
                if (c.name.Equals(contact.name) && c.number.Equals(contact.number))
                {
                    contacts.RemoveAt(index);
                    return;
                }
                index++;
            }
            return;
           // String test = "";
        }
    }
}
