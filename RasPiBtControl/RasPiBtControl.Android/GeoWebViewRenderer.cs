using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Webkit;
using RasPiBtControl.Droid;

[assembly: ExportRenderer(typeof(RasPiBtControl.GeoWebView), typeof(GeoWebViewRenderer))]
namespace RasPiBtControl.Droid
{
    public class GeoWebViewRenderer : WebViewRenderer
    {
        public GeoWebViewRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);
            Control.Settings.JavaScriptEnabled = true;
            Control.SetWebChromeClient(new MyWebClient());
        }
    }

    public class MyWebClient : WebChromeClient
    {
        public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback)
        {
            callback.Invoke(origin, true, false);
        }
    }
}