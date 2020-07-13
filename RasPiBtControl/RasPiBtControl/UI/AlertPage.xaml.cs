using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Plugin.Messaging;
using Android.Media;
using Android;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RasPiBtControl.UI
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertPage : ContentPage
    {
        String log;
        SensorSpeed speed = SensorSpeed.Fastest;
        List<float> xList;
        List<float> yList;
        List<float> zList;
        List<float> xyzList;
        List<float> gxList;
        List<float> gyList;
        List<float> gzList;
        List<String> numbers;
        List<Contact> contacts;
        String coordinates;
        protected MediaPlayer player;
        Boolean rearWarning;
        Boolean falseAlarm;
        Boolean sentSMS;
        User logged;

        public AlertPage(Boolean isRear, String userJson)
        {
            sentSMS = false;
            falseAlarm = false;
            rearWarning = isRear;
            String s = userJson;
            //WILL NEED NEW CONSTRUCTOR TO TAKE CONTACTS AND MAYBE MORE INFO
            logged = JsonConvert.DeserializeObject<User>(userJson);
            numbers = new List<String>(); //CHANGE THIS TO TAKE LIST FROM CONSTRUCTOR
            String messageText = "";
            String recipient = "5613811901"; //test number. DELETE LATER
            contacts = logged.getContacts();
            foreach (Contact c in contacts) {
                numbers.Add(c.number);
            }
            //contacts.Add(recipient); //DELETE THIS LATER
            //contacts.Add(recipient); //DELETE THIS LATER
            var smsMessenger = CrossMessaging.Current.SmsMessenger;
            Boolean aCrash = false;
            Boolean gCrash = false;
            //Accelerometer lists
            xList = new List<float>();
            yList = new List<float>();
            zList = new List<float>();
            xyzList = new List<float>();
            //Gyroscope Lists
            gxList = new List<float>();
            gyList = new List<float>();
            gzList = new List<float>();
            log = "Started: " + DateTime.Now.ToString() + "&#10"; //delete this later
            String crashType;
          // player = new MediaPlayer();
           // player = MediaPlayer.Create(this, RasPiBtControl.Droid.Resource.Raw.Alarm2);

            InitializeComponent();


            if (rearWarning)
            {
                alertLabel.Text = "REAR CRASH WARNING";
                crashType = "rear";
                //Execute Voice Warning
                Command warning = new Command(async () => await RearCrashWarning());
                warning.Execute(null);
            }
            else
            {
                alertLabel.Text = "FRONT CRASH WARNING";
                crashType = "front";
                Command warning = new Command(async () => await FrontCrashWarning());
                warning.Execute(null);
            }

            //Execute Blinking
            Command blinking = new Command(async () => await Blink());
            blinking.Execute(null);

            //Set accelerometer to get reading
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(speed);
            //Set gyroscope to get reading
            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
            Gyroscope.Start(speed);

            //get location
            Command gps = new Command(async () => await GetLocationAsync());
            gps.Execute(null);

            //Timer to stop accelerometer and gyroscope reading after X seconds and later analyze the data to detect crash
            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                //log data
                //AOutput.Text = log;
                //stop accelerometer
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                Accelerometer.Stop();
                //stop gyroscope
                Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
                Gyroscope.Stop();
                //analyze data here
                float xMax = xList.Max();
                float yMax = yList.Max();
                float zMax = zList.Max();
                float xyzMax = xyzList.Max();

                float xAve = xList.Average();
                float yAve = yList.Average();
                float zAve = zList.Average();
                float xyzAve = xyzList.Average();

                float gxMax = gxList.Max();
                float gyMax = gyList.Max();
                float gzMax = gzList.Max();

                //Button to ask for false alert and notify contacts
                Button falseButton = new Button { Text = "False Alert?" };
                falseButton.Clicked += RiderOk;

                //this is for testing
                GyrosLabel.Text = " Gyroscope Values: xMax = " + gxMax + " yMax = " + gyMax + " zMax = " + gzMax;

                AOutput.Text = " Accelerometer Values: xMax = " + xMax + " yMax = " + yMax + " zMax = " + zMax + " xyzMax = " + xyzMax + "&#10" + 
                                " xAve = " + xAve + " yAve = " + yAve + "zAve = " + zAve + " xyzAve = " + xyzAve;


                // this is part of the crash detection algorithm. WILL NEED MORE TESTING TO FIGURE OUT EXACT NUMBERS
                if (gxMax > 15 || gyMax > 15 || gzMax > 15)
                {
                    gCrash = true;
                }

                // this is part of the crash detection algorithm. WILL NEED MORE TESTING TO FIGURE OUT EXACT NUMBERS
                if (xMax > 5 || yMax > 5 || zMax > 5 || xyzMax > 10)
                {
                    aCrash = true;
                }

                if (aCrash && gCrash)
                {
                    //SERIOUS CRASH
                    Crashed.Text = "Accelerometer and Gyroscope detected a " + crashType + " Crash";
                    //ADD BUTTON TO ASK IF RIDER IS OK
                    stackLayout.Children.Add(falseButton);
                    
                    //Check if device was able to get location
                    if (!coordinates.Equals("Null"))
                    {
                        messageText = "Accelerometer and Gyroscope detected that " + logged.getFullName() + " suffered a " + crashType + " Crash at " + coordinates; //WILL NEED TO MODIFY THIS TO INCLUDE PERSONALIZED MESSAGE
                    }
                    else
                    {
                        messageText = "Accelerometer and Gyroscope detected that " + logged.getFullName() + " suffered a " + crashType + " Crash"; //WILL NEED TO MODIFY THIS TO INCLUDE PERSONALIZED MESSAGE
                    }
                    //command to wait for 5 seconds and send sms if not cancelled
                    Command wait2 = new Command(async () => await Wait(messageText));
                    wait2.Execute(null);
                    /*
                    //SEND TEXT MESSAGE TO LIST OF CONTACTS
                    if (smsMessenger.CanSendSms)
                    {
                        foreach (String c in contacts) //Send text to all contacts in list
                        {
                            smsMessenger.SendSmsInBackground(c, messageText);
                        }
                    }*/
                }
                else if (aCrash)
                {
                    //Evaluate Accelerometer values to determine seriousness
                    //Crash detected by Accelerometer only
                    Crashed.Text = "Accelerometer detected a " + crashType + " Crash";
                    //ADD BUTTON TO ASK IF RIDER IS OK
                    stackLayout.Children.Add(falseButton);
                    //Check if device was able to get location
                    if (!coordinates.Equals("Null"))
                    {
                        messageText = "Accelerometer detected that " + logged.getFullName() + " suffered a " + crashType + " Crash at " + coordinates; //WILL NEED TO MODIFY THIS TO INCLUDE PERSONALIZED MESSAGE
                    }
                    else
                    {
                        messageText = "Accelerometer detected that " + logged.getFullName() + " suffered a " + crashType + " Crash"; //WILL NEED TO MODIFY THIS TO INCLUDE PERSONALIZED MESSAGE
                    }

                    //command to wait for 5 seconds and send sms if not cancelled
                    Command wait2 = new Command(async () => await Wait(messageText));
                    wait2.Execute(null);
                    /*
                    //SEND TEXT MESSAGE TO LIST OF CONTACTS
                    if (smsMessenger.CanSendSms)
                    {
                        foreach (String c in contacts) //Send text to all contacts in list
                        {
                            smsMessenger.SendSmsInBackground(c, messageText);
                        }
                    }*/
                }
                else if (gCrash)
                {
                    //Can assume crash is not very serious if accelerometer was not triggered
                    //Crash detected by Gyroscope only
                    Crashed.Text = "Gyroscope detected a " + crashType + " Crash";
                    //ADD BUTTON TO ASK IF RIDER IS OK
                    stackLayout.Children.Add(falseButton);
                    //Check if device was able to get location
                    if (!coordinates.Equals("Null"))
                    {
                        messageText = "Gyroscope detected that " + logged.getFullName() + " suffered a " + crashType + " Crash at " + coordinates; //WILL NEED TO MODIFY THIS TO INCLUDE PERSONALIZED MESSAGE
                    }
                    else
                    {
                        messageText = "Gyroscope detected that " + logged.getFullName() + " suffered a " + crashType + " Crash"; //WILL NEED TO MODIFY THIS TO INCLUDE PERSONALIZED MESSAGE
                    }

                    //command to wait for 5 seconds and send sms if not cancelled
                    Command wait2 = new Command(async () => await Wait(messageText));
                    wait2.Execute(null);
                    /*
                    //SEND TEXT MESSAGE TO LIST OF CONTACTS
                    if (smsMessenger.CanSendSms)
                    {
                        foreach (String c in contacts) //Send text to all contacts in list
                        {
                            smsMessenger.SendSmsInBackground(c, messageText);
                        }
                    }*/
                }
                else
                {
                    Crashed.Text = "No Crash Detected!";
                    //command to wait and navigate to previous page
                    Command wait = new Command(async () => await WaitAndNavigate());
                    wait.Execute(null);
                }

                return false; // True = Repeat again, False = Stop the timer
            });
        }


        void RiderOk(object sender, System.EventArgs e)
        {
            if (sentSMS)
            {
                var smsMessenger = CrossMessaging.Current.SmsMessenger;
                String messageText = "False Alarm. Rider is Ok.";
                if (smsMessenger.CanSendSms)
                {
                    foreach (String c in numbers) //Send text to all contacts in list
                    {
                        smsMessenger.SendSmsInBackground(c, messageText);
                    }
                }
            }
            falseAlarm = true;
            //go back to main page
            Navigation.PopAsync();
        }

        public async Task RearCrashWarning()
        {
            await TextToSpeech.SpeakAsync("Attention, Rear Crash Warning!");

            // This method will block until utterance finishes.
        }

        public async Task FrontCrashWarning()
        {
            await TextToSpeech.SpeakAsync("Attention, Front Crash Warning!");

            // This method will block until utterance finishes.
        }

        private async Task Blink()
        {
            while (true)
            {
                await Task.Delay(500);
                // alertLabel.TextColor = alertLabel.TextColor == Color.Red ? Color.Black : Color.Red;
                if (rearWarning)
                {
                    stackLayout2.BackgroundColor = stackLayout2.BackgroundColor == Color.Red ? Color.Black : Color.Red;
                }
                else
                {
                    stackLayout2.BackgroundColor = stackLayout2.BackgroundColor == Color.Blue ? Color.Black : Color.Blue;
                }
            }
        }

        private async Task WaitAndNavigate()
        {
            //wait for 2 seconds
            await Task.Delay(2000);
            //go back to main page
            _ = Navigation.PopAsync();
        }

        private async Task Wait(String messageText)
        {
            var smsMessenger = CrossMessaging.Current.SmsMessenger;
            //wait for 5 seconds
            await Task.Delay(5000);
            //Send text message to list of contacts if not cancelled by False Alarm button
            if (smsMessenger.CanSendSms && !falseAlarm)
            {
                foreach (String c in numbers) //Send text to all contacts in list
                {
                    smsMessenger.SendSmsInBackground(c, messageText);
                }
            }
            sentSMS = true;

        }

        /*
        public void StartPlayer(String filePath)
        {
            if (player == null)
            {
                player = new MediaPlayer();
            }
            else
            {
                player.Reset();
                player.SetDataSource(filePath);
                player.Prepare();
                player.Start();
            }
        }*/

        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            // Console.WriteLine($"Reading: X: {data.Acceleration.X}, Y: {data.Acceleration.Y}, Z: {data.Acceleration.Z}");
            xList.Add(data.Acceleration.X);
            yList.Add(data.Acceleration.Y);
            zList.Add(data.Acceleration.Z);
            xyzList.Add(data.Acceleration.X + data.Acceleration.Y + data.Acceleration.Z);

            //log += DateTime.Now.ToString() + " Reading: X: " + data.Acceleration.X + ", Y: " + data.Acceleration.Y + ", Z: " + data.Acceleration.Z + "&#10";
            
        }

        public void ToggleAccelerometer()
        {
            try
            {
                if (Accelerometer.IsMonitoring)
                    Accelerometer.Stop();
                else
                    Accelerometer.Start(speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }

        void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            var data = e.Reading;
            // Process Angular Velocity X, Y, and Z reported in rad/s
            //Console.WriteLine($"Reading: X: {data.AngularVelocity.X}, Y: {data.AngularVelocity.Y}, Z: {data.AngularVelocity.Z}");
           // GyrosLabel.Text = "Gyros Reading: X: " + data.AngularVelocity.X + ", Y: " + data.AngularVelocity.Y + ", Z: " + data.AngularVelocity.Z;
            gxList.Add(data.AngularVelocity.X);
            gyList.Add(data.AngularVelocity.Y);
            gzList.Add(data.AngularVelocity.Z);
        }

        public void ToggleGyroscope()
        {
            try
            {
                if (Gyroscope.IsMonitoring)
                    Gyroscope.Stop();
                else
                    Gyroscope.Start(speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }

        private async Task GetLocationAsync() {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                   coordinates = "Latitude: " + location.Latitude + ", Longitude: " + location.Longitude;                    
                    return;  
                }
                else
                {
                    coordinates = "Null";
                    return;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                coordinates = "Null";
                return;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                coordinates = "Null";
                return;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                coordinates = "Null";
                return;
            }
            catch (Exception ex)
            {
                // Unable to get location
                coordinates = "Null";
                return;
            }
        }

    }
}