using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// <summary>
    /// Interaction logic for ImageResizeControl.xaml
    /// </summary>
    public partial class ImageResizeControl : UserControl
    {
        public ImageResizeControl()
        {
            InitializeComponent();
        }
        
        private BitmapImage _Source = null;
        public BitmapImage Source 
        {
            get => _Source; 
            set
            {                
                _Source = value;
                if (_Source == null)
                    return;
                isChangeTrusted = true;
                CurrentWidthValue.Text = NewWidthValue.Text = _Source.PixelWidth.ToString();
                CurrentHeightValue.Text = NewHeightValue.Text = _Source.PixelHeight.ToString();
                NewWidthValue.BorderBrush = NewHeightValue.Background = InputBrushDefault;
                isChangeTrusted = false;
            }
        }
        
        private static readonly Brush InputBrushDefault = new SolidColorBrush(Colors.Transparent);
        private static readonly Brush InputBrushError = new SolidColorBrush(Color.FromArgb(58, 255, 0, 0));

        bool isChangeTrusted = false;       

        private void CbKeepAspectRatio_Checked(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NewWidthValue.Text, out int iwidth))
            {
                isChangeTrusted = true;
                int h = (int)(Math.Round(1f * iwidth * Source.Height / Source.Width));
                if (h == 0) h = 1;
                NewHeightValue.Text = h.ToString();
                NewHeightValue.Background = InputBrushDefault;
                isChangeTrusted = false;
            }
            else                           
                NewHeightValue.Text = "!~INVALID";                            
        }

        private void NewWidthValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isChangeTrusted) return;
            if (int.TryParse(NewWidthValue.Text, out int iwidth) && iwidth > 0) 
            {
                if (iwidth == 0) iwidth = 1;
                NewWidthValue.Background = InputBrushDefault;
                if (cbKeepAspectRatio.IsChecked == true)
                {
                    isChangeTrusted = true;
                    int h = (int)Math.Round(1f * iwidth * Source.Height / Source.Width);
                    if (h == 0) h = 1;
                    NewHeightValue.Text = h.ToString();
                    NewHeightValue.Background = InputBrushDefault;
                    isChangeTrusted = false;
                }                             
            }
            else
            {
                NewWidthValue.Background = InputBrushError;
                if (cbKeepAspectRatio.IsChecked == true) 
                {
                    NewHeightValue.Text = "!~INVALID";
                }
            }
        }

        private void NewHeightValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isChangeTrusted) return;
            if (int.TryParse(NewHeightValue.Text, out int iheight) && iheight > 0) 
            {                         
                NewHeightValue.Background = InputBrushDefault;
                if (cbKeepAspectRatio.IsChecked == true)
                {
                    isChangeTrusted = true;
                    int w = (int)Math.Round(1f * iheight * Source.Width / Source.Height);
                    if (w == 0) w = 1;
                    NewWidthValue.Text = w.ToString();
                    NewWidthValue.Background = InputBrushDefault;
                    isChangeTrusted = false;
                }                
            }    
            else
            {
                NewHeightValue.Background = InputBrushError;
                if (cbKeepAspectRatio.IsChecked == true)
                {
                    NewWidthValue.Text = "!~INVALID";
                }
            }
        }

        public bool GetResizeDimensions(out int width, out int height)
        {
            width = 0; height = 0;
            return int.TryParse(NewWidthValue.Text, out width) && int.TryParse(NewHeightValue.Text, out height);                
        }
    }
}
