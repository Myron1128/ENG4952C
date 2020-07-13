using System;
using RasPiBtControl.Model;
using RasPiBtControl.Services;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Xamarin.Forms;
using System.Reactive;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms.Maps;
using Xamarin.Essentials;
using Newtonsoft.Json;
//using Android.Webkit;

namespace RasPiBtControl.UI
{

    public class LandingPageViewModel : ReactiveObject
    {
        private IProgressDialogService _progressDialogService;
        private IBtDiscovery _btDiscovery;
        private IBtClient _btClient;
        private Boolean isLocationReady;
        private String userJsonString;

       // public Android.Webkit.WebView webvw { get; private set; }
        public Xamarin.Forms.Maps.Map Map { get; private set; }

        //Changed this to accept navigation
        public INavigation Navigation { get; set; }

        private List<BtDeviceInfo> _pairedDevices;
        /// <summary>
        /// List of paired devices
        /// </summary>
        public List<BtDeviceInfo> PairedDevices
        {
            get { return _pairedDevices; }
            set { this.RaiseAndSetIfChanged(ref _pairedDevices, value); }
        }

        /*
        private BtDeviceInfo _selectedDevice;
        /// <summary>
        /// Current selected device
        /// </summary>
        public BtDeviceInfo SelectedDevice
        {
            get { return _selectedDevice; }
            set { this.RaiseAndSetIfChanged(ref _selectedDevice, value); }
        }*/

        private string _message;
        /// <summary>
        /// Last message from server
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }

        private string _velocity;
        /// <summary>
        /// Velocity from Geolocation
        /// </summary>
        public string Velocity
        {
            get { return _velocity; }
            set { this.RaiseAndSetIfChanged(ref _velocity, value); }
        }

        //Can comment this
        private string _speed;
        /// <summary>
        /// Speed that the user sets on Entry object
        /// </summary>

        //Can comment this
        public string Speed
        {
            get { return _speed; }
            set { this.RaiseAndSetIfChanged(ref _speed, value); }
        }

        /// <summary>
        /// List of supported operations by the server
        /// </summary>
        //public ObservableCollection<string> Operations { get; set; }

        /// <summary>
        /// Refreshes the list of paired devices
        /// </summary>
        public ReactiveCommand<Unit, Unit> Refresh { get; set; }

        /// <summary>
        /// Executes an operation from the available list
        /// </summary>
        //public ReactiveCommand<string, Unit> Execute { get; set; }//commented from original code

        /// <summary>
        /// Executes the operation SendSpeed
        /// </summary>
        public ReactiveCommand<string, Unit> SendSpeed { get; set; }
        public ReactiveCommand<Unit, Unit> GoRearAlert { get; set; }
        public ReactiveCommand<Unit, Unit> GoFrontAlert { get; set; }
        public ReactiveCommand<Unit, Unit> Shutdown { get; set; }
        public ReactiveCommand<Unit, Unit> SeeContacts { get; set; }



        public LandingPageViewModel(INavigation navigation, String userJson)
        {
            Message = "";
            this.Navigation = navigation;
            userJsonString = userJson;
            //logged = JsonConvert.DeserializeObject<User>(userJson);
            _progressDialogService = Locator.Current.GetService<IProgressDialogService>();
            _btDiscovery = Locator.Current.GetService<IBtDiscovery>();
            _btClient = Locator.Current.GetService<IBtClient>();

            _btClient.ReceivedData += _btClient_ReceivedData;
            isLocationReady = true;

            Command audio = new Command(async () => await AudioCheck());
            audio.Execute(null);

            // Command gps = new Command(async () => await UpdateVelocity());
            // gps.Execute(null);


            // Operations = new ObservableCollection<string>(); //commented from original code

            //GET SPEED ON A TIMER. MOVE THIS CODE FURTHER DOWN LATER ON.
            /*
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                //get location speed
                Command gps = new Command(async () => await UpdateVelocity());
                gps.Execute(null);
                return true; //repeat cycle
            });*/

            Position position = new Position(36.9628066, -122.0194722);
            MapSpan mapSpan = new MapSpan(position, 0.01, 0.01);
            Map = new Xamarin.Forms.Maps.Map(mapSpan);

            //Set webview
            /*
            webvw = new Android.Webkit.WebView(Context);
            webvw.SetWebViewClient(new GeoWebViewClient());
            webvw.SetWebChromeClient(new GeoWebChromeClient());
            webvw.Settings.JavaScriptCanOpenWindowsAutomatically = true;
            webvw.Settings.DisplayZoomControls = true;
            webvw.Settings.JavaScriptEnabled = true;
            webvw.Settings.SetGeolocationEnabled(true);
            webvw.LoadUrl("https://www.google.com/maps");*/

            // Subscribe to the update of paired devices property
            this.WhenAnyValue(vm => vm._btDiscovery.PairedDevices)
                .Subscribe((pairedDevices) =>
                {
                    PairedDevices = pairedDevices;
                    /*
                    //traverse list of paired devices looking for the one with the Raspberry Pi UUID
                    foreach (BtDeviceInfo dev in PairedDevices)
                    {
                        String devName = dev.Name;
                        bool hasId = dev.HasRequiredServiceID;
                        //if (dev != null && dev.HasRequiredServiceID)
                        if (dev != null && hasId)
                        {
                            
                            if (_btClient.Connect(dev.Address))
                            {
                                // Fetch supported operations from the server
                                // _btClient.SendData("getop"); //commented this from original code
                                // Ping the device to ensure good communcation
                                _btClient.SendData("ping");
                                return;
                            }
                            else
                            {
                                Message = "Unable to connect to device";
                                return;
                            }
                        }
                    }*/
                });

          

            // Display a progress dialog when the bluetooth discovery takes place
            this.WhenAnyValue(vm => vm._btDiscovery.IsBusy)
                .DistinctUntilChanged()
                .Subscribe((isBusy) =>
                {
                    if(isBusy)
                    {
                        _progressDialogService.Show("Please wait...");
                    }
                    else
                    {
                        //traverse list of paired devices looking for the one with the Raspberry Pi UUID
                        foreach (BtDeviceInfo dev in PairedDevices)
                        {
                            String devName = dev.Name;
                            bool hasId = dev.HasRequiredServiceID;
                            //if (dev != null && dev.HasRequiredServiceID)
                            if (dev != null && hasId)
                            {
                                
                                if (_btClient.Connect(dev.Address))
                                {
                                    // Fetch supported operations from the server
                                    // _btClient.SendData("getop"); //commented this from original code
                                    // Ping the device to ensure good communcation
                                    _btClient.SendData("ping");
                                    _progressDialogService.Hide();
                                    if (Message.Equals("Java.IO.IOException"))
                                    {
                                        Message = "Connection Failed! Please check Raspberry Pi is turned on and try again.";
                                    }
                                    return;
                                }
                                else
                                {
                                    Message = "Unable to connect to device";
                                    _progressDialogService.Hide();
                                    return;
                                }
                            }
                        }
                        _progressDialogService.Hide();
                    }
                });

            // The refresh command
            Refresh = ReactiveCommand.Create(() =>
            {
                _btDiscovery.Refresh();
            });

            //commented from original code
            /*
            // Handle the selection of a device from the list
            this.WhenAnyValue(vm => vm.SelectedDevice)                
                .Subscribe((device) =>
                {
                    if (device == null)
                    {
                        return;
                    }

                    if(!device.HasRequiredServiceID)
                    {
                        Message = "Device not supported or service not running";
                        return;
                    }

                    if(_btClient.Connect(device.Address))
                    {
                        // Fetch supported operations from the server
                        // _btClient.SendData("getop"); //commented this from original code
                        // Ping the device to ensure good communcation
                        _btClient.SendData("ping");
                    }
                    else
                    {
                        Message = "Unable to connect to device";
                        return;
                    }
                });*/

            //commented from original code
            /*
            // Execute an operation
            this.Execute = ReactiveCommand.Create<string>((operation) =>
            {
                _btClient.SendData(operation);
            });*/


            // Execute GoAlert Operation
            this.GoRearAlert = ReactiveCommand.Create(() =>
            {
                Command navigate = new Command(async () => await GotoAlertPage(true, userJsonString));
                navigate.Execute(null);
            });

            this.GoFrontAlert = ReactiveCommand.Create(() =>
            {
                Command navigate = new Command(async () => await GotoAlertPage(false, userJsonString));
                navigate.Execute(null);
            });

            // Execute Shutdown Operation
            this.Shutdown = ReactiveCommand.Create(() =>
            {
                _btClient.SendData("shutdown");
            });

            //Execute GoContactListPage Operation
            this.SeeContacts = ReactiveCommand.Create(() =>
            {
                Command navigate = new Command(async () => await GotoContactListPage(userJsonString));
                navigate.Execute(null);
            });


            // Execute SendSpeed Operation
            this.SendSpeed = ReactiveCommand.Create<string>((_speed) =>
            {
                _btClient.SendData("speed:" + _speed); //Use speed heading so that the Raspberry knows it is sending speed
            });

            Refresh.Execute().Subscribe();
        }

        /// <summary>
        /// Handles data received from server
        /// </summary>
        /// <param name="sender">Client instance</param>
        /// <param name="data">The data</param>
        private void _btClient_ReceivedData(object sender, string data)
        {
            if(data.StartsWith("msg"))
            {
                var parts = data.Split(':');

                Message = parts[1];
                if (Message.Equals("Pong")) //if there is a response to sent ping
                {
                    Message += ". Successful connection!";
                    var count = 0; //variable to test speed sending. REPLACE WITH GPS SPEED FROM PHONE
                    //start timer to send speed
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        count++;
                        Task.Run(async () =>
                        {
                            if (isLocationReady)
                            {
                                isLocationReady = false;
                                Command gps = new Command(async () => await UpdateVelocity());
                                gps.Execute(null);
                            }
                            //_btClient.SendData("speed:" + count); //Use speed heading so that the Raspberry knows it is sending speed
                           // _btClient.SendData("speed:" + Velocity);
                        });
                        return true;
                    });
                }
            }
            //commented from original code
            /*else if(data.StartsWith("op"))
            {
                var parts = data.Split(':');
                var operations = parts[1].Split(',');

                Operations.Clear();
                foreach(var op in operations)
                {
                    Operations.Add(op);
                }
            }*/
            // Insert more handlers here
            //Alert Handler added to navigate to Alert page. ADD CODE TO DIFFERENTIATE FRONT THREAT FROM REAR THREAT
            else if (data.StartsWith("rear"))
            {
                Command navigate =  new Command(async () => await GotoAlertPage(true, userJsonString));
                navigate.Execute(null);          
            }
            else if (data.StartsWith("front"))
            {
                Command navigate = new Command(async () => await GotoAlertPage(false, userJsonString));
                navigate.Execute(null);
            }
            else
            {
                Message = $"Unknown response {data}";
            }
        }

        public async Task AudioCheck()
        {
            await TextToSpeech.SpeakAsync("Audio Check. Make sure volume is high enough for warning.");

            // This method will block until utterance finishes.
        }

        /// <summary>
        /// Async Task to navigate to Alert page
        /// </summary>
        public async Task GotoAlertPage(Boolean isRear, String userJson)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new AlertPage(isRear, userJson));
            });
        }

        /// <summary>
        /// Async Task to navigate to ContactList page
        /// </summary>
        public async Task GotoContactListPage(String userJson)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ContactListPage(userJson));
            });
        }

        private async Task UpdateVelocity() {
            double vel = 0; //let it be zero by default
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    vel = (double)location.Speed;
                    Velocity = vel.ToString();
                    _btClient.SendData("speed:" + Velocity);
                    isLocationReady = true;
                    return;
                }
                else
                {
                    Velocity = "Null";
                    return;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                return;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                return;
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                return;
            }
            catch (Exception ex)
            {
                // Unable to get location
                isLocationReady = true;
                return;
            }
        }
        /*
        public async Task OpenMap()
        {
            var location = new Location(47.645160, -122.1306032);
            var options = new MapLaunchOptions { NavigationMode = NavigationMode.Driving };

            await Map.OpenAsync(location, options);
        }*/
        /*
        public class GeoWebChromeClient : WebChromeClient
        {
            public void onGeolocationPermissionsShowPrompt(String origin, GeolocationPermissions.ICallback callback)
            {
                // Always grant permission since the app itself requires location
                // permission and the user has therefore already granted it
                callback.Invoke(origin, false, false);
            }
        }


        public class GeoWebViewClient : WebViewClient
        {
            public bool shouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
            {
                // When user clicks a hyperlink, load in the existing WebView
                view.LoadUrl(url);
                return true;
            }
        }*/

    }
}
