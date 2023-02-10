using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using Java.Lang;
using static Android.Content.Res.Resources;
using System.Reflection.Emit;
using Android.App;
using Android.Gms.Extensions;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Google.Common.Util.Concurrent;
using Xamarin.Essentials;
using AndroidX.Lifecycle;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;

namespace NetAndroid;

[Activity(Label = "@string/app_name", Theme = "@style/MyTheme.NoActionBar")]
public class ActivityBarCode : AppCompatActivity, IRunnable, ImageAnalysis.IAnalyzer
{
    private IListenableFuture _cameraProviderFuture;
    private AndroidX.Camera.View.PreviewView previewView;
    EditText _barcode;
    protected async override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_barcode);
        previewView = FindViewById<PreviewView>(Resource.Id.previewView);
        _barcode = FindViewById<EditText>(Resource.Id.content);
        await Xamarin.Essentials.Permissions.RequestAsync<Permissions.Camera>();


        _cameraProviderFuture = ProcessCameraProvider.GetInstance(this);
        _cameraProviderFuture.AddListener(this, ContextCompat.GetMainExecutor(this));

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


        // Create your application here
    }
    public void Run()
    {
        try
        {
            ProcessCameraProvider cameraProvider = _cameraProviderFuture.Get() as ProcessCameraProvider;
            bindPreview(cameraProvider);
        }
        catch (Exception e)
        {
            // No errors need to be handled for this Future.
            // This should never be reached.
        }
    }

    private IBarcodeScanner scanner = null;
    void bindPreview(ProcessCameraProvider cameraProvider)
    {
        Preview preview = new Preview.Builder().Build();

        CameraSelector cameraSelector = new CameraSelector.Builder()
            .RequireLensFacing(CameraSelector.LensFacingBack)
            .Build();

        preview.SetSurfaceProvider(previewView.SurfaceProvider);

        //var camera = cameraProvider.BindToLifecycle((ILifecycleOwner)this, cameraSelector, preview);
        BarcodeScannerOptions options = new BarcodeScannerOptions.Builder()
            .SetBarcodeFormats(Barcode.FormatQrCode)
            .Build();

        scanner = BarcodeScanning.GetClient(options);
        ImageAnalysis imageAnalysis =
            new ImageAnalysis.Builder()
                // enable the following line if RGBA output is needed.
                //.setOutputImageFormat(ImageAnalysis.OUTPUT_IMAGE_FORMAT_RGBA_8888)
                .SetDefaultResolution(new Size(1280, 720))
                .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                .Build();
        imageAnalysis.SetAnalyzer(ContextCompat.GetMainExecutor(this), this);
        var camera = cameraProvider.BindToLifecycle((ILifecycleOwner)this, cameraSelector, imageAnalysis, preview);

    }
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
        Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    public async void Analyze(IImageProxy p0)
    {
        var image = p0.Image;
        if (image != null)
        {
            InputImage ee = InputImage.FromMediaImage(image, p0.ImageInfo.RotationDegrees);
            var res = (await scanner.Process(ee)) as JavaList;
            foreach (var re in res)
            {
                var baroce = re as Barcode;
                if (baroce != null)
                {

                    _barcode.Text = baroce.DisplayValue;
                }
            }
        }
        p0?.Close();
    }
}