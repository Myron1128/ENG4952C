using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using Newtonsoft.Json;

namespace RasPiBtControl.UI
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogIn : ContentPage
    {
        private static String url_check_pass = "https://lamp.cse.fau.edu/~martinsanche2016/RiderOk/check_pass.php";
        private static String url_retrieve_contacts = "https://lamp.cse.fau.edu/~martinsanche2016/RiderOk/retrieve_contacts.php";
        private String username;
        private String password;
       // static HttpClient client;
        public LogIn()
        {
            InitializeComponent();
            //client = new HttpClient();
        }

        void LogInClicked(object sender, System.EventArgs e)
        {
            Command checkPassword = new Command(async () => await CheckPass());
            checkPassword.Execute(null);

            // var page = new LandingPage();
            //Navigation.PushAsync(page);
        }

        void SignUpClicked(object sender, System.EventArgs e)
        {
            var page = new Registration();
            Navigation.PushAsync(page);
        }

        public async Task CheckPass()
        {
            username = userEntry.Text;
            password = passEntry.Text;
            User logged;
            try
            {
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(url_check_pass);
                    var params1 = new List<KeyValuePair<string, string>>();
                    params1.Add(new KeyValuePair<string, string>("username", username));
                    params1.Add(new KeyValuePair<string, string>("password", password));
                    //var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
                   // var content = new StringContent(params1.ToString(), Encoding.UTF8, "application/json");
                   var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "username", username },
                            { "password", password }
                         });
                    var response = await client.PostAsync(url_check_pass, content);
                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        // resultLabel.Text = jsonString;
                        //dynamic data = JObject.Parse(jsonString);
                        Message data = new Message();
                        data = JsonConvert.DeserializeObject<Message>(jsonString);
                        int suc = data.success;
                        //resultLabel.Text = data.contacts.ToString();
                        
                        if (suc == 1)
                        {
                            //retrieve contacts api call
                            var content2 = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                { "username", username }
                            });
                            var response2 = await client.PostAsync(url_retrieve_contacts, content2);
                            if (response2 != null) //found contacts
                            {
                                var jsonString2 = await response2.Content.ReadAsStringAsync();
                                List<Contact> contacts = JsonConvert.DeserializeObject<List<Contact>>(jsonString2);
                                //resultLabel.Text = contacts[0].name;
                                logged = new User(username, data.first, data.last, contacts);
                            }
                            else //no contacts in db
                            {
                                List<Contact> noContacts = new List<Contact>();
                                logged = new User(username, data.first, data.last, noContacts);
                            }

                            //serialize User
                            String userJson = JsonConvert.SerializeObject(logged, Formatting.Indented);
                            //change page
                            
                            var page = new LandingPage(userJson);
                            await Navigation.PushAsync(page);
                        }
                        else
                        {
                            resultLabel.Text = data.message;
                        }
                        //JSON json1 =  JsonConvert.DeserializeObject<object>(jsonString);
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
            public string first { get; set; }
            public string last { get; set; }

        }
        /*
        public class Contact
        {
            public string name { get; set; }
            public string number { get; set; }
        }*/

    }
}