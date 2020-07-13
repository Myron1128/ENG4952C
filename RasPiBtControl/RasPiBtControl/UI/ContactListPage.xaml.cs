using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RasPiBtControl.UI
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContactListPage : ContentPage
    {
        String userJsonString;
        public ContactListPage(String userJson)
        {
            userJsonString = userJson;
            InitializeComponent();
            User logged = JsonConvert.DeserializeObject<User>(userJson);
            List < String > names = new List<String>();
            List<Contact> contacts = logged.getContacts();
            //get all contacts names
            foreach (Contact c in contacts)
            {
                names.Add(c.name);
            }
            contactList.ItemsSource = names;

            contactList.ItemSelected += (object sender, SelectedItemChangedEventArgs e) =>
            {
                String item = (String)e.SelectedItem; //selected name
                int index = (contactList.ItemsSource as List<String>).IndexOf(e.SelectedItem as String);
                //MessageLabel.Text = "Selected contact is " + contacts[index].name + " " + contacts[index].number;
                String contactJsonString = JsonConvert.SerializeObject(contacts[index], Formatting.Indented);
                var page = new ViewContactPage(userJsonString, contactJsonString);
                Navigation.PushAsync(page);
            };

        }
        
        void AddNewContactClicked(object sender, System.EventArgs e)
        {
            var page = new AddContactPage(userJsonString);
            Navigation.PushAsync(page);
        }


       
    }
}