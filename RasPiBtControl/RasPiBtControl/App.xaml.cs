﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace RasPiBtControl
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

           // MainPage = new NavigationPage(new RasPiBtControl.UI.LandingPage());
            MainPage = new NavigationPage(new RasPiBtControl.UI.LogIn());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
