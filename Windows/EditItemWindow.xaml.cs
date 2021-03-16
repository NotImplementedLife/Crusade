using Crusade.Data;
using Crusade.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Crusade.Controls;

namespace Crusade.Windows
{
    /// <summary>
    /// Interaction logic for EditItemWindow.xaml
    /// </summary>
    public partial class EditItemWindow : CustomWindow
    {
        public EditItemWindow()
        {
            InitializeComponent();
            DataContext = this;
            ImageTool = CropperTool;            
        }
        

        private int _ItemId = -1;
        public int ItemId
        {
            get => _ItemId;
            set
            {
                _ItemId = value;
                Title = $"Image {_ItemId} edit options | EXPERIMENTAL";
            }
        }        

        private ImageListItemData _ImageData;
        public ImageListItemData ImageData
        {
            get => _ImageData;
            set
            {
                _ImageData = value;
                if (_ImageData.TempPath == "")
                    Target = new BitmapImage(new Uri(_ImageData.Path, UriKind.Absolute));
                else
                    Target = new BitmapImage(new Uri(System.IO.Path.Combine(App.Dir, _ImageData.TempPath), UriKind.Absolute));
            }
        }

        #region CropLogic        

        private void Crop_TabItem_Selected(object sender, RoutedEventArgs e)
        {
            ImageTool = CropperTool;
            SetSource();
        }

        private void CropButton_Click(object sender, RoutedEventArgs e)
        {
            CropButton.Tag = "Loading";
            BitmapImage bmp = null;
            TabControl.IsEnabled = false;
            Task.Run(new Action(() =>
            {
                bmp = CropperTool.GetCroppedImage(out Int32Rect bounds);
                Dispatcher.Invoke(() =>
                {
                    Target = bmp;
                    CropButton.Tag = null;
                    TabControl.IsEnabled = true;
                });                         
            }));            
        }

        #endregion

        #region Resize Logic

        private void Resize_TabItem_Selected(object sender, RoutedEventArgs e)
        {
            ImageTool = ResizerTool;
            SetSource();
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResizerTool.GetResizeDimensions(out int w, out int h))
            {
                ResizeButton.Tag = "Loading";
                BitmapImage bmp = null;
                TabControl.IsEnabled = false;
                int w0 = Target.PixelWidth, h0 = Target.PixelHeight;
                Task.Run(new Action(() =>
                {
                    BitmapImage src = null;
                    Dispatcher.Invoke(() =>
                    {
                        src = Target;
                    });
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    var ms = new MemoryStream();
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(src, null, null, null));
                    encoder.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    bmp.StreamSource = ms;
                    bmp.DecodePixelWidth = w;
                    bmp.DecodePixelHeight = h;                    
                    bmp.EndInit();
                    bmp.Freeze();                    
                    Dispatcher.Invoke(() =>
                    {                        
                        Target = bmp;                        
                        ResizeButton.Tag = null;
                        TabControl.IsEnabled = true;
                    });                    
                }));
            }
        }

        #endregion

        private BitmapImage _Target = null;
        public BitmapImage Target
        {
            get => _Target;
            set
            {
                _Target = value;
                SetSource();
            }
        }        

        private object ImageTool = null;       
        private void SetSource()
        {
            if (ImageTool == null)
                return;            
            var prop = ImageTool.GetType().GetProperty("Source");
            prop.SetValue(ImageTool, Target, null);
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            Target.Freeze();
            DialogResult = true;
            Close();
        }

        #region TitleBar        

        private void TitleBarMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBarCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

    }
}
