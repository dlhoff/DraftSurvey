using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Java.Lang;
using Android.Views.Animations;
using Android.InputMethodServices;
using Android.Views.InputMethods;

namespace DraftSurveyTool.Droid
{
    [Activity(Label = "Enter Draft Value")]
    public class KBActivity : Activity
    {
        static public Android.InputMethodServices.Keyboard mKeyboard;
        static public CustomKeyboardView mKeyboardView;
        static public Android.Widget.TextView et;
        static public bool active = false;
        static public KBActivity kBActivity = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            active = true;
            kBActivity = this;

            // Load the UI defined in Second.axml
            // SetContentView(Resource.Layout.Main);
            HandleCustomEntry();
        }


        public void ShowKeyboardWithAnimation()
        {
            System.Diagnostics.Debug.WriteLine("keyboardState: " + mKeyboardView.Visibility.ToString());
            if (mKeyboardView.Visibility == ViewStates.Gone)
            {
                // System.Diagnostics.Debug.WriteLine("keyboardState: Gone is true");
                Android.Views.Animations.Animation animation = AnimationUtils.LoadAnimation(
                    MainActivity.mainActivity, Resource.Animation.slide_in_bottom);
                // System.Diagnostics.Debug.WriteLine("keyboardState, animation/code is: " + animation.ToString() + " " +
                //     Resource.Animation.slide_in_bottom);
                mKeyboardView.Visibility = ViewStates.Visible;
            }
        }

        public void HandleCustomEntry()
        {
            SetContentView(Resource.Layout.Main);   // xml specifies layout view 
            mKeyboard = new Android.InputMethodServices.Keyboard(this, Resource.Xml.keyboard2);   // KB xml
            et = (TextView)FindViewById(Resource.Id.target);    // User input is text view. Text entry caused standard KB to appear 
            var inputManager = (InputMethodManager)MainActivity.mainActivity.GetSystemService(Context.InputMethodService);   // Not necessary now 
            inputManager.HideSoftInputFromWindow(et.WindowToken, HideSoftInputFlags.None);
            inputManager.HideSoftInputFromInputMethod(et.WindowToken, HideSoftInputFlags.None);
            Window.DecorView.ClearFocus();
            et.ClearFocus();
            mKeyboardView = (CustomKeyboardView)this.FindViewById(Resource.Id.keyboard_view);
            mKeyboardView.Keyboard = mKeyboard;
            mKeyboardView.Visibility = ViewStates.Visible;
            // MainActivity.app.Unfocus();

            /*
            et.Touch += (sender1, e1) => {
                System.Diagnostics.Debug.WriteLine("onTouch - true");
                // mKeyboardView.Visibility = ViewStates.Visible;
                // ShowKeyboard(mTargetView);
                // ShowKeyboardWithAnimation();
                // MainActivity.app.Unfocus();
                e1.Handled = true;
            };
            */

            mKeyboardView.Key += (sender1, e1) =>
            {
                long eventTime = JavaSystem.CurrentTimeMillis();
                // System.Diagnostics.Debug.WriteLine("mKeyboardView.Key event, primarycode: " + e1.PrimaryCode);
                if (ProcessKeyCode(e1.PrimaryCode))
                {
                    // System.Diagnostics.Debug.WriteLine("mKeyboardView done with keyboard entry");
                    active = false;
                    Finish();
                }
            };
        }

        public bool ProcessKeyCode(Android.Views.Keycode keycode)
        {
            bool retCode = false;
            switch (keycode)
            {
                case Android.Views.Keycode.Num0:
                    et.Text += "0";
                    break;
                case Android.Views.Keycode.Num1:
                    et.Text += "1";
                    break;
                case Android.Views.Keycode.Num2:
                    et.Text += "2";
                    break;
                case Android.Views.Keycode.Num3:
                    et.Text += "3";
                    break;
                case Android.Views.Keycode.Num4:
                    et.Text += "4";
                    break;
                case Android.Views.Keycode.Num5:
                    et.Text += "5";
                    break;
                case Android.Views.Keycode.Num6:
                    et.Text += "6";
                    break;
                case Android.Views.Keycode.Num7:
                    et.Text += "7";
                    break;
                case Android.Views.Keycode.Num8:
                    et.Text += "8";
                    break;
                case Android.Views.Keycode.Num9:
                    et.Text += "9";
                    break;
                case Android.Views.Keycode.Period:
                    et.Text += ".";
                    break;
                case Android.Views.Keycode.NumpadAdd:
                    et.Text += "+";
                    break;
                case Android.Views.Keycode.NumpadSubtract:
                    et.Text += "-";
                    break;
                case Android.Views.Keycode.NumpadMultiply:
                    et.Text += "*";
                    break;
                case Android.Views.Keycode.NumpadDivide:
                    et.Text += "/";
                    break;
                case Android.Views.Keycode.Del:
                    if (et.Text.Length > 0)
                    {
                        et.Text = et.Text.Substring(0, et.Text.Length - 1);
                    }
                    break;
                case Android.Views.Keycode.Enter:
                    retCode = true;
                    MainActivity.app.UpdateValue(et.Text);
                    break;
                default:
                    break;
            }
            // et.SetSelection(et.Text.Length);
            return retCode;
        }

        public override void OnBackPressed()
        {
            // If you want to continue going back
            // base.OnBackPressed();
            // System.Diagnostics.Debug.WriteLine("***** onbackpressed called: Turn off activity/focus");
            active = false;
            MainActivity.app.Unfocus();
            Finish();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            // System.Diagnostics.Debug.WriteLine("****  KBActivity onsaveinstancestate called");
            base.OnSaveInstanceState(outState);
            outState.PutString("Entry", et.Text);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            // System.Diagnostics.Debug.WriteLine("****  KBActivity onrestoreinstancestate called");
            base.OnSaveInstanceState(savedInstanceState);
            et.Text = savedInstanceState.GetString("Entry");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.Dispose(); // Sever java binding.
        }
    }
}

