using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Google.Common.Util.Concurrent;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Xamarin.Essentials;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using static Android.Content.Res.Resources;
using Exception = Java.Lang.Exception;
using Android.Runtime;
using Android.Util;
using AndroidX.Lifecycle;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;
using Android.Gms.Extensions;
using Android.Media;
using Xamarin.Google.MLKit.Vision.Face;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace NetAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme.NoActionBar")]
    internal class FaceActivity : AppCompatActivity, IRunnable, ImageAnalysis.IAnalyzer
    {
        private IListenableFuture _cameraProviderFuture;
        private AndroidX.Camera.View.PreviewView previewView;
        private SkiaSharp.Views.Android.SKCanvasView _canvasView;
        protected async override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_face);
            previewView = FindViewById<PreviewView>(Resource.Id.previewView);
            _canvasView = FindViewById<SkiaSharp.Views.Android.SKCanvasView>(Resource.Id.face_over);
            _canvasView.PaintSurface += _canvasView_PaintSurface;
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

        private List<Face> _face=new List<Face>();
        private void _canvasView_PaintSurface(object? sender, SkiaSharp.Views.Android.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
           
            if (_face.Any())
            {
                SKPaint paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = Color.Red.ToSKColor(),
                    StrokeWidth = 2
                };
                var xRatio = width / info.Width;
                var YRatio = height / info.Height;
                foreach (var face in _face)
                {
                    canvas.DrawRect((int)(face.BoundingBox.Left / xRatio), (int)(face.BoundingBox.Top / YRatio), (int)(face.BoundingBox.Width() / xRatio), (int)(face.BoundingBox.Height() / YRatio), paint);
                    foreach (var faceAllContour in face.AllContours)
                    {
                        canvas.DrawPoints(SKPointMode.Points, faceAllContour.Points.Select(x => new SKPoint(x.X, x.X)).ToArray(), paint);
                    }
                }
            }
           
            //canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
            //paint.Style = SKPaintStyle.Fill;
            //paint.Color = SKColors.Blue;
            //canvas.DrawCircle(e.Info.Width / 2, e.Info.Height / 2, 100, paint);
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
        private IFaceDetector scanner = null;
        void bindPreview(ProcessCameraProvider cameraProvider)
        {
            Preview preview = new Preview.Builder().Build();
           
            CameraSelector cameraSelector = new CameraSelector.Builder()
                .RequireLensFacing(CameraSelector.LensFacingBack)
                .Build();

            preview.SetSurfaceProvider(previewView.SurfaceProvider);
            previewView.SetScaleType(PreviewView.ScaleType.FillCenter);
            //var camera = cameraProvider.BindToLifecycle((ILifecycleOwner)this, cameraSelector, preview);
            FaceDetectorOptions options = new FaceDetectorOptions.Builder()
                
                .Build();
            
            scanner = FaceDetection.GetClient(options);
            ImageAnalysis imageAnalysis =
                new ImageAnalysis.Builder()
                    // enable the following line if RGBA output is needed.
                    //.setOutputImageFormat(ImageAnalysis.OUTPUT_IMAGE_FORMAT_RGBA_8888)
                    //.SetDefaultResolution(new Size(1280, 720))
                   // .SetTargetResolution(new Size(540, 540))
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

        private double height, width ;
        public async void Analyze(IImageProxy p0)
        {
            _face.Clear();
            var image = p0.Image;
            if (image != null)
            {
               
                InputImage ee = InputImage.FromMediaImage(image, p0.ImageInfo.RotationDegrees);
                if (p0.ImageInfo.RotationDegrees==90)
                {
                    height = ee.Width;
                    width = ee.Height;
                }
                else
                {
                    height = ee.Height;
                    width = ee.Width;
                }
              
                var res = (await scanner.Process(ee)) as JavaList;
                if (res.Count>0)
                {
                    foreach (var re in res)
                    {
                        var face = re as Face;
                        if (face != null)
                        {
                           _face.Add(face);
                           
                        }
                    }
                    _canvasView.Invalidate();
                }
                else
                {
                    _canvasView.Invalidate();
                }
             
            }
            p0?.Close();
        }
    }

}
