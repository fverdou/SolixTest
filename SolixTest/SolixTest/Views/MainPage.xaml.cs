using SkiaSharp;
using SkiaSharp.Views.Forms;
using SolixTest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SolixTest.Views
{
    public partial class MainPage : ContentPage
    {
        SKBitmap bitmap;
        public List<InspectionPoint> InspectionPoints { get; set; } = new List<InspectionPoint>
        {
            new InspectionPoint(100, 100, "xxxx-001"),
            new InspectionPoint(100, 200, "xxxx-002"),
            new InspectionPoint(200, 100, "xxxx-003"),
            new InspectionPoint(200, 200, "xxxx-004"),
        };
        public MainPage()
        {
            InitializeComponent();

            string resourceID = "SolixTest.Resources.image.png";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                bitmap = SKBitmap.Decode(stream);
            }
            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            var Content = canvasView;
        }
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.DrawBitmap(bitmap, 1, 1);

            SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.Black.ToSKColor(),
                StrokeWidth = 5
            };
            foreach (InspectionPoint element in InspectionPoints)
            {
                canvas.DrawCircle(element.x, element.y, 5, paint);
            }
        }
    }
}