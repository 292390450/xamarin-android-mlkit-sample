using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using AndroidX.Lifecycle;
using Google.Common.Util.Concurrent;
using Java.Lang;
using Xamarin.Essentials;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using Xamarin.Google.MLKit.Vision.BarCode.Internal;
using Exception = Java.Lang.Exception;
using Xamarin.Google.MLKit.Vision.Common;

namespace NetAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected async override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            FindViewById<Button>(Resource.Id.barcode_btn).Click += MainActivity_Click;

            FindViewById<Button>(Resource.Id.facedect_btn).Click += ((sender, args) =>
            {
                StartActivity(new Intent(this, typeof(FaceActivity)));
            });

            // cameraController.SetImageAnalysisAnalyzer(ContextCompat.GetMainExecutor(this));

            // Window.InsetsController.Show(WindowInsets.Type.SystemBars());
            //Window.InsetsController.Show(WindowInsets.Type.StatusBars());
            // Window.InsetsController.Hide(WindowInsets.Type.NavigationBars());
            // Window.InsetsController.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
            //if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            //{
            //    Window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
            //    Window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);
            //    Window.SetStatusBarColor(Color.Transparent);
            //}
            Window.InsetsController.Hide(WindowInsets.Type.NavigationBars());
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                var uiFlags = Android.Views.SystemUiFlags.LayoutStable;
                uiFlags |= Android.Views.SystemUiFlags.LayoutFullscreen;
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
                Window.SetStatusBarColor(Color.Transparent);
                Window.SetNavigationBarColor(Color.Transparent);
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

                Window.InsetsController.Hide(WindowInsets.Type.SystemBars());

                //Window.DecorView.SystemUiVisibility=(StatusBarVisibility)(uiFlags);
            }




            //if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            //{



            //   // 透明状态栏
            //    Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            //   // 透明导航栏
            //    Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            //}
        }

        private void MainActivity_Click(object? sender, System.EventArgs e)
        {
            StartActivity(new Intent(this, typeof(ActivityBarCode)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


    }

}




