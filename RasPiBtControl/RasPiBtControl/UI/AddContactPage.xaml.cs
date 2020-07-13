using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;

namespace RasPiBtControl.UI
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddContactPage : ContentPage
    {
        User logged;
        private static String url_add_contact = "https://lamp.cse.fau.edu/~martinsanche2016/RiderOk/add_contact.php";

        public AddContactPage(String userJson)
        {
            InitializeComponent();
            //deserialize user to modify it
            logged = JsonConvert.DeserializeObject<User>(userJson);
        }

        void CreateNewContactClicked(object sender, System.EventArgs e)
        {
            String name = nameEntry.Text;
            String number = numberEntry.Text;
            if (name == null || number == null)
            {
                MessageLabel.Text = "Please make sure all fields have been entered.";
                return;
            }
            else if(number.Length == 10 && IsNumeric(number))
            {
                Contact newContact = new Contact(name, number);
                logged.addContact(newContact);

                //Add to Database
                Command addContact = new Command(async () => await AddContact(logged.getUsername(), name, number));
                addContact.Execute(null);
            }
            else
            {
                MessageLabel.Text = "Please make sure number is 10 numeric digits.";
            }
            
        }

        //function to determine if a string is numeric
        public static bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public async Task AddContact(String username, String name, String number)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "username", username },
                            { "name", name },
                            { "number", number }
                         });
                    var response = await client.PostAsync(url_add_contact, content);
                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        Message data = new Message();
                        data = JsonConvert.DeserializeObject<Message>(jsonString);
                        int suc = data.success;

                        if (suc == 1)
                        {
                            //contact was successfully added
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