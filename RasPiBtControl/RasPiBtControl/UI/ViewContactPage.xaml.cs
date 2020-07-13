using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RasPiBtControl.UI
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewContactPage : ContentPage
    {
        User logged;
        Contact selectedContact;
        private static String url_remove_contact = "https://lamp.cse.fau.edu/~martinsanche2016/RiderOk/remove_contact.php";
        public ViewContactPage(String userJson, String contactJson)
        {
            InitializeComponent();

            logged = JsonConvert.DeserializeObject<User>(userJson);
            selectedContact = JsonConvert.DeserializeObject<Contact>(contactJson);
            nameLabel.Text = logged.getUsername();
            numberLabel.Text = selectedContact.number;

        }

        void DeleteContactClicked(object sender, System.EventArgs e)
        {
            //try to remove from database
            //Add to Database
            Command deleteContact = new Command(async () => await DeleteContact());
            deleteContact.Execute(null);
        }

        public async Task DeleteContact()
        {

            try
            {
                String username = logged.getUsername();
                String number = selectedContact.number;
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "username", username },
                            { "number", number }
                         });
                    var response = await client.PostAsync(url_remove_contact, content);
                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        Message data = new Message();
                        data = JsonConvert.DeserializeObject<Message>(jsonString);
                        int suc = data.success;

                        if (suc == 1)
                        {
                            //contact was successfully deleted
                            //now delete it from object
                            logged.deleteContact(selectedContact);
                            //serialize logged again since it has been changed
                            String userJsonString = JsonConvert.SerializeObject(logged, Formatting.Indented);
                            var page = new LandingPage(userJsonString);
                            await Navigation.PushAsync(page);
                        }
                        else
                        {
                            MessageLabel.Text = data.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // myCustomLogger.LogException(ex);
            }
        }

        public class Message
        {
            public int success { get; set; }
            public string message { get; set; }
        }
    }
}