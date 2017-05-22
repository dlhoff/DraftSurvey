using System;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Preferences;
using Dropbox.CoreApi.Android;
using Dropbox.CoreApi.Android.Session;
using Java.Lang;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Android.Provider;
using DraftSurveyTool;
using Xamarin.Forms.Platform.Android;
using DraftSurveyTool.Droid;
using Xamarin.Forms;
using Android.Text.Method;
using System.ComponentModel;
using Android.Content.Res;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
[assembly: ExportRenderer(typeof(CustomEntryStd), typeof(CustomEntryStdRenderer))]
[assembly: ExportRenderer(typeof(CustomLabel), typeof(CustomLabelRenderer))]
[assembly: ExportRenderer(typeof(CustomLabelRed), typeof(CustomLabelRedRenderer))]

namespace DraftSurveyTool.Droid
{
    // Used for all white text. Gives "3D Shadow" effect 
    public class CustomLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            var label = (TextView)Control;
            if (label != null)
            {
                // Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Decalotype-Black.ttf");  // font name specified here
                // label.Typeface = font;
                label.SetTextColor(Android.Graphics.Color.White);     // Character is white 
                label.SetShadowLayer(2.5f, 2.5f, 2.5f, Android.Graphics.Color.Black);   // Applies the "3D Shadow" effect in black
            }
        }
    }
    // Used for all red (error) text. Gives "3D Shadow" effect 
    public class CustomLabelRedRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            var label = (TextView)Control;
            if (label != null)
            {
                // Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Decalotype-Black.ttf");  // font name specified here
                // label.Typeface = font;
                label.SetTextColor(Android.Graphics.Color.Red);
                label.SetShadowLayer(2.5f, 2.5f, 2.5f, Android.Graphics.Color.Black);
            }
        }
    }

    // Uses the standard keyboard. CustomEntryStd only provides the "3D Shadow" effect
    public class CustomEntryStdRenderer : EntryRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            var label = (TextView)Control;
            if (label != null)
            {
                label.SetTextColor(Android.Graphics.Color.White);
                label.SetShadowLayer(2.5f, 2.5f, 2.5f, Android.Graphics.Color.Black);
            }
        }
        private void Control_KeyPress(object sender, KeyEventArgs e)
        {
            // throw new NotImplementedException();
        }
    }
    
    // Provides both "3D Shadow" effect and custom KB   
    public class CustomEntryRenderer : EntryRenderer
    {
        public static EntryEditText control = null;
        public static Activity activity;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            MainActivity.mainActivity.Window.DecorView.ClearFocus();

            if (Control == null)    // Make sure that it is an actual user event
            {
                return;
            }

            if (!Control.HasFocus)   // Only process focus events 
            {
                return;
            }
            var label = (TextView)Control;
            if (label != null)
            {
                label.SetTextColor(Android.Graphics.Color.White);
                label.SetShadowLayer(2.5f, 2.5f, 2.5f, Android.Graphics.Color.Black);
            }

            var i = new Intent(MainActivity.mainActivity, typeof(KBActivity));
            MainActivity.mainActivity.StartActivity(i);   // Starts custom KB activity 
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                return;
            }

            var label = (TextView)Control;
            if (label != null)
            {
                label.SetTextColor(Android.Graphics.Color.White);
                label.SetShadowLayer(2.5f, 2.5f, 2.5f, Android.Graphics.Color.Black);
            }
        }

        private void Control_KeyPress(object sender, KeyEventArgs e)
        {
            // throw new NotImplementedException();
        }
    }


    [Activity(Label = "DraftSurveyTool", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        static public Context mainContext;
        static public int screenWidth;
        static public int screenHeight;
        static public float density;
        static public int screenWidthDp = 0;
        static public int screenHeightDp = 0;
        static public bool isCamera;
        static public string[] aL = new string[20];
        static public MainActivity mainActivity;
        static public Android.InputMethodServices.Keyboard mKeyboard;
        static public CustomKeyboardView mKeyboardView;
        public Android.Widget.EditText et;
        public Bundle bundleSave;
        static public Activity activitySave;

        static public App app;
        // public LayoutInflater inflater;
        // public EditText et; 

        protected override void OnCreate(Bundle bundle)
        {
            bundleSave = bundle;
            // System.Diagnostics.Debug.WriteLine("****  MainActivity onCreate called");
            mainActivity = this;
            base.OnCreate(bundle);
            isCamera = IsThereAnAppToTakePictures();

            AssetManager assetMgr = this.Assets;
            aL = assetMgr.List("DemoShips");

            var metrics = Resources.DisplayMetrics;
            screenWidth = metrics.WidthPixels;
            screenHeight = metrics.HeightPixels;
            // screenHeightDp = Resources.Configuration.ScreenHeightDp;  // not correct, but good place for resources 
            density = metrics.Density;

            mainContext = this;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(app = new App());
        }

        // static public string AppKey = "82x7847skg6q31j";
        // static public string AppSecret = "5v2pfu07wqpyu7z";

        protected override void OnStart()
        {
            base.OnStart();
            mainContext = this;
            // System.Diagnostics.Debug.WriteLine("****  MainActivity onStart called");
            isCamera = IsThereAnAppToTakePictures();
        }

        protected override void OnResume()
        {
            // System.Diagnostics.Debug.WriteLine("****  MainActivity onResume called");
            mainContext = this;

            var metrics = Resources.DisplayMetrics;
            screenWidth = metrics.WidthPixels;
            screenHeight = metrics.HeightPixels;
            density = metrics.Density;
            // System.Diagnostics.Debug.WriteLine("****  MainActivity sw,sh,sd,swdp,shdp " + screenWidth + " " + screenHeight + " " +
            //     density + " " + screenWidthDp + " " + screenHeightDp);
            base.OnResume();

            // This block of code must be the last of OnResume() because of the return
            if ((SendEmail_Android.DBAuthStart) && (!SendEmail_Android.DBAuthResumed))
            {

                // After you allowed to link the app with Dropbox,
                // you need to finish the Authentication process
                var session = SendEmail_Android.dropboxApi.Session as AndroidAuthSession;
                if (!session.AuthenticationSuccessful())
                    return;

                try
                {
                    // Call this method to finish the authentication process
                    // Will bind the user's access token to the session.
                    session.FinishAuthentication();

                    // Save the Access Token somewhere
                    var accessToken = session.OAuth2AccessToken;
                }
                catch (IllegalStateException ex)
                {
                    Toast.MakeText(this, ex.LocalizedMessage, ToastLength.Short).Show();
                }
                SendEmail_Android.DBAuthResumed = true;
                ThreadPool.QueueUserWorkItem(o => SendEmail_Android.writeDB());
            }
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            var metrics = Resources.DisplayMetrics;
            screenWidth = metrics.WidthPixels;
            screenHeight = metrics.HeightPixels;
            density = metrics.Density;
            // System.Diagnostics.Debug.WriteLine("****  MainActivity sw,sh,sd,swdp,shdp " + screenWidth + " " + screenHeight + " " +
            //     density);

            if (newConfig.Orientation == Android.Content.Res.Orientation.Portrait)
            {
                // System.Diagnostics.Debug.WriteLine("****  Orientation:   portrait");
                if (screenWidth > screenHeight)
                {
                    int temp = screenWidth;
                    screenWidth = screenHeight;
                    screenHeight = temp;
                }
            }
            else if (newConfig.Orientation == Android.Content.Res.Orientation.Landscape)
            {
                // System.Diagnostics.Debug.WriteLine("****  Orientation:   landscape");
                if (screenWidth < screenHeight)
                {
                    int temp = screenWidth;
                    screenWidth = screenHeight;
                    screenHeight = temp;
                }
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            System.Diagnostics.Debug.WriteLine("****  MainActivity onsaveinstancestate called");
            base.OnSaveInstanceState(outState);

            var metrics = Resources.DisplayMetrics;
            screenWidth = metrics.WidthPixels;
            screenHeight = metrics.HeightPixels;
            density = metrics.Density;
        }

        public override void OnBackPressed()
        {
            // If you want to continue going back
            // base.OnBackPressed();
            // System.Diagnostics.Debug.WriteLine("***** onbackpressed called");
            app.NotifyBackChg();
        }

        public void AndroidBack()
        {
            base.OnBackPressed();
        }
    }
}

