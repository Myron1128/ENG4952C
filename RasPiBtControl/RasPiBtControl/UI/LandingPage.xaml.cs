using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RasPiBtControl.UI
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandingPage : ContentPage
    {
        public LandingPage(String userJson)
        {
            InitializeComponent();
            BindingContext = new LandingPageViewModel(Navigation, userJson);
        }
    }
}
