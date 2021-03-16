using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crusade.Controls
{    
    using Pixel = System.Drawing.Point;
    /// <summary>
    /// Control which contains an UI helping the user to select a picture area and crop it into a new image.
    /// </summary>
    public partial class ImageCropControl : UserControl
    {
        public ImageCropControl()
        {
            InitializeComponent();
            Loaded += ImageCropControl_Loaded;
            SizeChanged += ImageCropControl_SizeChanged;            
            Image.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            
        }       

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapImage), typeof(ImageCropControl), new PropertyMetadata());
        public BitmapImage Source
        {
            get => GetValue(SourceProperty) as BitmapImage;
            set
            {                                
                SetValue(SourceProperty, value);
                if (value == null) return;
                Frame.Width = swidth = value.PixelWidth;
                Frame.Height = sheight = value.PixelHeight;
                Cover.Rect = new Rect(0, 0, swidth, sheight);
                P1 = new Point();
                P3 = new Point(swidth, sheight);                
            }
        }
        private double swidth  = 1;
        private double sheight = 1;

        private void ImageCropControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Source == null) return;                        
            ScaleTransform scale = (VisualTreeHelper.GetChild(ViewBox, 0) as ContainerVisual).Transform as ScaleTransform;            
            ComputedThickness = 3 / Math.Min(scale.ScaleX, scale.ScaleY);
            ComputedRadius = 5 / Math.Min(scale.ScaleX, scale.ScaleY);
        }

        private void ImageCropControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Source == null) return;
            ScaleTransform scale = (VisualTreeHelper.GetChild(ViewBox, 0) as ContainerVisual).Transform as ScaleTransform;
            ComputedThickness = 3 / Math.Min(scale.ScaleX, scale.ScaleY);
            ComputedRadius = 5 / Math.Min(scale.ScaleX, scale.ScaleY);
        }

        #region Dependency Properties
        public static DependencyProperty ComputedThicknessProperty =
            DependencyProperty.Register("ComputedThickness", typeof(double), typeof(ImageCropControl), new PropertyMetadata());
        public double ComputedThickness
        {
            get => (double)GetValue(ComputedThicknessProperty);
            set => SetValue(ComputedThicknessProperty, value);
        }

        public static DependencyProperty ComputedRadiusProperty =
            DependencyProperty.Register("ComputedRadius", typeof(double), typeof(ImageCropControl), new PropertyMetadata());
        public double ComputedRadius
        {
            get => (double)GetValue(ComputedRadiusProperty);
            set => SetValue(ComputedRadiusProperty, value);
        }

        public static DependencyProperty P1Property = DependencyProperty.Register("P1", typeof(Point), typeof(ImageCropControl), new PropertyMetadata());
        public static DependencyProperty P2Property = DependencyProperty.Register("P2", typeof(Point), typeof(ImageCropControl), new PropertyMetadata());
        public static DependencyProperty P3Property = DependencyProperty.Register("P3", typeof(Point), typeof(ImageCropControl), new PropertyMetadata());
        public static DependencyProperty P4Property = DependencyProperty.Register("P4", typeof(Point), typeof(ImageCropControl), new PropertyMetadata());

        int __Pid = 0;
        public Point P1
        {
            get => (Point)GetValue(P1Property);
            set
            {                
                SetValue(P1Property, value);
                if (__Pid != 0) return;
                __Pid = 1;
                P2 = new Point(P2.X, P1.Y);
                P4 = new Point(P1.X, P4.Y);
                __Pid = 0;
            }
        }
        public Point P2
        {
            get => (Point)GetValue(P2Property);
            set
            {
                SetValue(P2Property, value);
                if (__Pid != 0) return;
                __Pid = 2;
                P1 = new Point(P1.X, P2.Y);
                P3 = new Point(P2.X, P3.Y);
                __Pid = 0;
            }
        }
        public Point P3
        {
            get => (Point)GetValue(P3Property);
            set
            {
                SetValue(P3Property, value);
                if (__Pid != 0) return;
                __Pid = 3;
                P2 = new Point(P3.X, P2.Y);
                P4 = new Point(P4.X, P3.Y);
                __Pid = 0;
            }
        }
        public Point P4
        {
            get => (Point)GetValue(P4Property);
            set
            {
                SetValue(P4Property, value);
                if (__Pid != 0) return;
                __Pid = 4;
                P1 = new Point(P4.X, P1.Y);
                P3 = new Point(P3.X, P4.Y);
                __Pid = 0;
            }
        }

        public static DependencyProperty StrokeColorProperty =
            DependencyProperty.Register("StrokeColor", typeof(Brush), typeof(ImageCropControl), new PropertyMetadata(Brushes.DodgerBlue));
        public Brush StrokeColor
        {
            get => GetValue(StrokeColorProperty) as Brush;
            set
            {
                if (StrokeColor != value)
                    SetValue(StrokeColorProperty, value);
            }
        }
        #endregion

        private int movedCorner = 0;
        private Point relPt;
        private void CutLayer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(CutLayer);
            if(CheckMsDown(pos,P1,ComputedRadius))
            {                
                movedCorner = 1;
                relPt = GetRelativePos(P1, pos);
            }
            else if (CheckMsDown(pos, P2, ComputedRadius))
            {               
                movedCorner = 2;
                relPt = GetRelativePos(P2, pos);
            }
            else if (CheckMsDown(pos, P3, ComputedRadius))
            {               
                movedCorner = 3;
                relPt = GetRelativePos(P3, pos);
            }
            else if (CheckMsDown(pos, P4, ComputedRadius))
            {                
                movedCorner = 4;
                relPt = GetRelativePos(P4, pos);
            }
        }

        private void CutLayer_PreviewMouseMove(object sender, MouseEventArgs e)
        {            
            if (movedCorner == 0 || e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(CutLayer);
            if (movedCorner == 1) 
            {
                P1 = GetNewPos(pos);
            }
            else if (movedCorner == 2) 
            {
                P2 = GetNewPos(pos);
            }
            else if (movedCorner == 3)
            {
                P3 = GetNewPos(pos);
            }
            else if (movedCorner == 4)
            {
                P4 = GetNewPos(pos);
            }            
        }

        private void CutLayer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            movedCorner = 0;
        }

        private Point GetRelativePos(Point o, Point p) => new Point(p.X - o.X, p.Y - o.Y);
        private double GetDistance(Point p1, Point p2) => Point.Subtract(p2, p1).Length;

        private bool CheckMsDown(Point cur, Point center, double radius)
            => GetDistance(cur, center) <= radius;

        private Point GetNewPos(Point pos)
        {
            var x = pos.X - relPt.X;
            var y = pos.Y - relPt.Y;            
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= swidth) x = swidth;
            if (y >= sheight) y = sheight;            
            return new Point(x, y);
        }

        public BitmapImage GetCroppedImage(out Int32Rect bounds)
        {
            Debug.WriteLine("Start");
            BitmapSource src = null;
            Dispatcher.Invoke(() =>
            {
                src = (Source.Format != PixelFormats.Bgra32) ?
                        new FormatConvertedBitmap(Source, PixelFormats.Bgra32, null, 0) as BitmapSource : Source;
                src.Freeze();
            });                       
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            int stride = width * 4;
            int size = height * stride;
            byte[] pixels = new byte[size];
            src.CopyPixels(pixels, stride, 0);
            int cropX = 0, cropY = 0, cropW = 0, cropH = 0;
            Dispatcher.Invoke(() =>
            {
                cropX = Min((int)P1.X, (int)P2.X, (int)P3.X, (int)P4.X);
                cropY = Min((int)P1.Y, (int)P2.Y, (int)P3.Y, (int)P4.Y);
                cropW = Max((int)P1.X, (int)P2.X, (int)P3.X, (int)P4.X) - cropX;
                cropH = Max((int)P1.Y, (int)P2.Y, (int)P3.Y, (int)P4.Y) - cropY;
            });

            WriteableBitmap result = new WriteableBitmap(cropW, cropH, 96, 96, PixelFormats.Bgra32, null);
            bounds = new Int32Rect(cropX, cropY, cropW, cropH);
            int cropStride = 4 * cropW;
            byte[] rpixels = new byte[cropH * cropStride];
            int srcit = 0, it = 0, x = 0, y = 0;
            try
            {
                for (x = 0; x < cropW; x++)
                    for (y = 0; y < cropH; y++)
                    {
                        srcit = (cropY + y) * stride + 4 * (cropX + x);
                        it = y * cropStride + 4 * x;
                        rpixels[it + 0] = pixels[srcit + 0];
                        rpixels[it + 1] = pixels[srcit + 1];
                        rpixels[it + 2] = pixels[srcit + 2];
                        rpixels[it + 3] = pixels[srcit + 3];
                    }
            }
            catch (Exception e)
            {
                MessageBox.Show($"n error occured:\n\n {e.Message}");
                return null;
            }
            result.WritePixels(new Int32Rect(0, 0, cropW, cropH), rpixels, cropStride, 0);

            Debug.WriteLine("Finished result");
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(result));
                encoder.Save(stream);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = stream;
                bmp.EndInit();
                bmp.Freeze();
            }
            Debug.WriteLine("Finished bmp");
            return bmp;
        }              
        
        private static int Max(params int[] values) => values.Max();
        private static int Min(params int[] values) => values.Min();

        private void Frame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScaleTransform scale = (VisualTreeHelper.GetChild(ViewBox, 0) as ContainerVisual).Transform as ScaleTransform;
            ComputedThickness = 3 / Math.Min(scale.ScaleX, scale.ScaleY);
            ComputedRadius = 5 / Math.Min(scale.ScaleX, scale.ScaleY);
        }
    }
}
