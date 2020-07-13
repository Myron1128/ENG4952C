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
    public partial class Registration : ContentPage
    {
        private String firstName;
        private String lastName;
        private String username;
        private String password;
        private String confirmation;
        private static String url_create_user = "https://lamp.cse.fau.edu/~martinsanche2016/RiderOk/create_user.php";
        private static String url_check_username = "https://lamp.cse.fau.edu/~martinsanche2016/RiderOk/check_username.php";

        public Registration()
        {
            InitializeComponent();

        }

        void RegisterClicked(object sender, System.EventArgs e)
        {
            firstName = firstEntry.Text;
            lastName = lastEntry.Text;
            username = userEntry.Text;
            password = passEntry.Text;
            confirmation = confirmEntry.Text;

            //check that all fields have been entered
            if (firstName == null || lastName == null || username == null || password == null || confirmation == null)
            {
                messageLabel.Text = "Please enter all fields!";
                return;
            }

            //check that passwords match
            if (!password.Equals(confirmation))
            {
                messageLabel.Text = "Passwords do not match!";
                return;
            }

            //call async task that checks username and creates user
            Command createUser = new Command(async () => await CreateUser());
            createUser.Execute(null);


        }

        public async Task CreateUser()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "username", username }
                         });
                    var response = await client.PostAsync(url_check_username, content);
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
                            //username is not taken
                            var content2 = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                { "username", username },
                                { "password", password },
                                { "first_name", username },
                                { "last_name", username }
                            });
                            var response2 = await client.PostAsync(url_create_user, content2);
                            if (response2 != null)
                            {
                                var jsonString2 = await response2.Content.ReadAsStringAsync();
                                Message data2 = new Message();
                                data2 = JsonConvert.DeserializeObject<Message>(jsonString2);
                                int suc2 = data2.success;

                                if (suc2 == 1)
                                {
                                    //go back to login page
                                    await Navigation.PopAsync();
                                }
                                else
                                {
                                    messageLabel.Text = "User was not created, check connection and try again.";
                                }
                            }
                        }
                        else
                        {
                            messageLabel.Text = data.message;
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