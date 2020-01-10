using SkiaSharp;
using SkiaSharp.Views.Forms;
using SolixTest.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchTracking;
using Xamarin.Forms;

namespace SolixTest.Views
{
    public partial class MainPage : ContentPage
    {
        SKBitmap bitmap;
        public List<InspectionPoint> InspectionPoints { get; set; } = new List<InspectionPoint>
        {
            new InspectionPoint(0.1f, 0.1f, "xxxx-001"),
            new InspectionPoint(0.2f, 0.1f, "xxxx-002"),
            new InspectionPoint(0.1f, 0.2f, "xxxx-003"),
            new InspectionPoint(0.2f, 0.2f, "xxxx-004"),
        };

        SKPaint paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill,
            Color = Color.Black.ToSKColor(),
            StrokeWidth = 5
        };
        SKPaint thinLinePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 2
        };
        SKPaint textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Black,
            TextSize = 30,
            TextAlign = SKTextAlign.Left
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
            var offsetX = - matrix.TransX;
            var offsetY = - matrix.TransY;
            //var srcRect = new SKRect(offsetX, offsetY, offsetX + info.Width, offsetY + info.Height);
            //var destRect = new SKRect(0, 0, info.Width, info.Height);

            canvas.SetMatrix(matrix);
            //canvas.DrawBitmap(bitmap, srcRect, destRect);
            canvas.DrawBitmap(bitmap, 0, 0);

            int rectangleWidth = 140;
            int rectangleHeight = 40;
            foreach (InspectionPoint element in InspectionPoints)
            {
                float _x = element.x * bitmap.Width - offsetX;
                float _y = element.y * bitmap.Height - offsetY;
                float lineEndX = _x + 50;
                float lineEndY = _y - 50;
                float textStartX = lineEndX + 40;
                float textStartY = lineEndY - 10;
                canvas.DrawCircle(_x, _y, 5, paint);
                canvas.DrawLine(_x, _y, lineEndX, lineEndY, thinLinePaint);
                canvas.DrawLine(lineEndX, lineEndY, lineEndX + 30, lineEndY, thinLinePaint);
                canvas.DrawText(element.code, textStartX, textStartY, textPaint);
                canvas.DrawRect(lineEndX + 30, lineEndY - rectangleHeight, rectangleWidth, rectangleHeight, thinLinePaint);
            }

        }

        long touchId = -1;
        SKPoint previousPoint;
        SKMatrix matrix = SKMatrix.MakeIdentity();

        Dictionary<long, SKPoint> touchDictionary = new Dictionary<long, SKPoint>();

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            // Convert Xamarin.Forms point to pixels
            var pt = args.Location;
            SKPoint point = new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                                        (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Find transformed bitmap rectangle
                    SKRect rect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
                    rect = matrix.MapRect(rect);

                    // Determine if the touch was within that rectangle
                    //if (rect.Contains(point))
                    //{
                    //    // touchId = args.Id;
                    //    // previousPoint = point;
                    //    touchDictionary.Add(args.Id, point);
                    //}
                    if (rect.Contains(point) && !touchDictionary.ContainsKey(args.Id))
                    {
                        touchDictionary.Add(args.Id, point);
                    }
                    break;

                case TouchActionType.Moved:
                    if (touchDictionary.ContainsKey(args.Id))
                    {
                        //if (touchId == args.Id)
                        if (touchDictionary.Count == 1)
                        {
                            // Adjust the matrix for the new position
                            //matrix.TransX += point.X - previousPoint.X;
                            //matrix.TransY += point.Y - previousPoint.Y;
                            //previousPoint = point;
                            SKPoint prevPoint = touchDictionary[args.Id];
                            matrix.TransX += point.X - prevPoint.X;
                            matrix.TransY += point.Y - prevPoint.Y;
                            canvasView.InvalidateSurface();
                        }
                        else if (touchDictionary.Count >= 2)
                        {
                            // Copy two dictionary keys into array
                            long[] keys = new long[touchDictionary.Count];
                            touchDictionary.Keys.CopyTo(keys, 0);

                            // Find index of non-moving (pivot) finger
                            int pivotIndex = (keys[0] == args.Id) ? 1 : 0;

                            // Get the three points involved in the transform
                            SKPoint pivotPoint = touchDictionary[keys[pivotIndex]];
                            SKPoint prevPoint = touchDictionary[args.Id];
                            SKPoint newPoint = point;

                            // Calculate two vectors
                            SKPoint oldVector = prevPoint - pivotPoint;
                            SKPoint newVector = newPoint - pivotPoint;

                            // Scaling factors are ratios of those
                            float scaleX = newVector.X / oldVector.X;
                            //float scaleY = newVector.Y / oldVector.Y;

                            //if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX) &&
                            //    !float.IsNaN(scaleY) && !float.IsInfinity(scaleY))
                            if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX))
                                {
                                // If something bad hasn't happened, calculate a scale and translation matrix
                                SKMatrix scaleMatrix =
                                    SKMatrix.MakeScale(scaleX, scaleX, pivotPoint.X, pivotPoint.Y);

                                SKMatrix.PostConcat(ref matrix, scaleMatrix);
                                canvasView.InvalidateSurface();
                            }
                        }
                        // Store the new point in the dictionary
                        touchDictionary[args.Id] = point;
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    // touchId = -1;
                    if (touchDictionary.ContainsKey(args.Id))
                    {
                        touchDictionary.Remove(args.Id);
                    }
                    break;
            }
        }
    }
}