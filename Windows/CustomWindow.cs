using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Crusade.Windows
{
    [ContentProperty("InnerContent")]
    public class CustomWindow : Window
    {
        public CustomWindow()
        {
            Style = (Style)FindResource("WindowStyle");
            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            Body = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Body.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
            Body.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });


            TitleBarContentControl = new ContentControl();
            TitleBarContentControl.SetBinding(ContentControl.ContentProperty,
                new Binding("TitleBarContent")
                {
                    RelativeSource = new RelativeSource
                    {
                        Mode = RelativeSourceMode.FindAncestor,
                        AncestorType = typeof(CustomWindow)
                    }
                });
            TitleBarContentControl.MouseDown += TitleBarContentControl_MouseDown;

            Grid.SetRow(TitleBarContentControl, 0);
            Body.Children.Add(TitleBarContentControl);

            WindowContent = new ContentPresenter();
            WindowContent.SetBinding(ContentPresenter.ContentProperty,
                new Binding("InnerContent")
                {
                    RelativeSource = new RelativeSource
                    {
                        Mode = RelativeSourceMode.FindAncestor,
                        AncestorType = typeof(CustomWindow)
                    }
                });
            Grid.SetRow(WindowContent, 1);
            Body.Children.Add(WindowContent);

            AddChild(Body);
        }

        private void TitleBarContentControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        public static readonly DependencyProperty TitleBarContentProperty =
            DependencyProperty.Register("TitleBarContent", typeof(FrameworkElement), typeof(CustomWindow), new UIPropertyMetadata(null));

        public FrameworkElement TitleBarContent
        {
            get => GetValue(TitleBarContentProperty) as FrameworkElement;
            set => SetValue(TitleBarContentProperty, value);
        }

        public static readonly DependencyProperty InnerContentProperty =
           DependencyProperty.Register("InnerContent", typeof(FrameworkElement), typeof(CustomWindow), new UIPropertyMetadata(null));

        public FrameworkElement InnerContent
        {
            get => GetValue(InnerContentProperty) as FrameworkElement;
            set => SetValue(InnerContentProperty, value);
        }

        private Grid Body;
        private ContentControl TitleBarContentControl;
        private ContentPresenter WindowContent;
    }
}
