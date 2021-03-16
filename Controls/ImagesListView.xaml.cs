using Crusade.Data;
using Crusade.Windows;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crusade.Controls
{
    /// <summary>
    /// Interaction logic for ImagesListView.xaml
    /// </summary>
    public partial class ImagesListView : UserControl
    {
        public ImagesListView()
        {
            InitializeComponent();
            ImgListView.ItemsSource = ImageListData;                   
        }

        public ObservableCollection<ImageListItemData> ImageListData = new ObservableCollection<ImageListItemData>();

        public delegate void OnImageListCountChanged(object sender, int oldCount, int newCount);
        public event OnImageListCountChanged ImageListCountChanged;

        public void DealWithInput(string[] filenames, Action callbackUI)
        {
            int added = 0, listCount = ImageListData.Count;
            for (int i = 0, cnt = filenames.Length; i < cnt; i++)
            {
                if (Directory.Exists(filenames[i])) break;
                if (!File.Exists(filenames[i])) break;
                
                if (!Constants.PictureExtensions.Contains(Path.GetExtension(filenames[i]).ToLowerInvariant())) break;
                var path = filenames[i];
                int imgWidth = 0;
                int imgHeight = 0;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    imgWidth = bitmapFrame.PixelWidth;
                    imgHeight = bitmapFrame.PixelHeight;
                    stream.Close();
                }
                BitmapImage thumbnail = new BitmapImage();
                thumbnail.BeginInit();                
                thumbnail.UriSource = new Uri(path, UriKind.Absolute);                
                if (imgWidth > imgHeight)
                    thumbnail.DecodePixelWidth = 100;
                else
                    thumbnail.DecodePixelHeight = 100;
                try
                {
                    thumbnail.EndInit();
                }
                catch(ArgumentException)
                {
                    // try recreate thumbnail with IgnoreColorProfile option
                    thumbnail = new BitmapImage();
                    thumbnail.BeginInit();
                    thumbnail.UriSource = new Uri(path, UriKind.Absolute);
                    if (imgWidth > imgHeight)
                        thumbnail.DecodePixelWidth = 100;
                    else
                        thumbnail.DecodePixelHeight = 100;
                    thumbnail.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    thumbnail.EndInit();
                }
                thumbnail.Freeze();

                var item = new ImageListItemData(path, thumbnail);                
                Dispatcher.Invoke(() => ImageListData.Add(item));
                added++;
                Thread.Sleep(100);
            }           
            Dispatcher.Invoke(() =>
            {
                CollectionViewSource.GetDefaultView(ImageListData).Refresh();
                callbackUI.Invoke();
                ImageListCountChanged?.Invoke(this, listCount, listCount + added);
            });
        }

        #region Dependency Properties       
        public static DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(ImagesListView),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 238, 232, 170))));
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static DependencyProperty SelectBackgroundProperty =
            DependencyProperty.Register("SelectBackground", typeof(Brush), typeof(ImagesListView),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(200, 244, 164, 96))));
        public Brush SelectBackground
        {
            get => (Brush)GetValue(SelectBackgroundProperty);
            set => SetValue(SelectBackgroundProperty, value);
        }

        public static DependencyProperty HoverForegroundProperty =
           DependencyProperty.Register("HoverForeground", typeof(Brush), typeof(ImagesListView), new PropertyMetadata(Brushes.Orange));
        public Brush HoverForeground
        {
            get => (Brush)GetValue(HoverForegroundProperty);
            set => SetValue(HoverForegroundProperty, value);
        }

        public static DependencyProperty SelectForegroundProperty =
            DependencyProperty.Register("SelectForeground", typeof(Brush), typeof(ImagesListView), new PropertyMetadata(Brushes.White));
        public Brush SelectForeground
        {
            get => (Brush)GetValue(SelectForegroundProperty);
            set => SetValue(SelectForegroundProperty, value);
        }
        #endregion        

        #region ListViewItem UI Logic
        private bool RemoveButtonDown = false;
        private void RemoveItemButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            RemoveButtonDown = true;
            e.Handled = true;
        }

        private void RemoveItemButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RemoveButtonDown) //click
            {
                int count = ImageListData.Count;
                var data = ((sender as Button).DataContext as ListViewItem).DataContext as ImageListItemData;
                int index = ImgListView.Items.IndexOf(data);
                ImageListData.Remove(data);
                ImageListCountChanged?.Invoke(this, count, count - 1);
                Debug.WriteLine($"Removed item #{index}");
            }
        }

        bool EditButtonDown = false;
        private void EditItemButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            EditButtonDown = true;
            e.Handled = true;
        }

        private void EditItemButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(EditButtonDown)
            {
                var data = ((sender as Button).DataContext as ListViewItem).DataContext as ImageListItemData;
                int index = ImgListView.Items.IndexOf(data);
                var wnd = new EditItemWindow()
                {
                    ItemId=index,
                    ImageData = data
                };
                if(wnd.ShowDialog()==true)
                {
                    var result = wnd.Target;
                    LongTaskStart?.Invoke(this);
                    Task.Run(new Action(() =>
                    {
                        Temp.SaveEditedImage(result, ImageListData[index]);
                        Dispatcher.Invoke(() => CollectionViewSource.GetDefaultView(ImageListData).Refresh());
                        Dispatcher.Invoke(() => LongTaskEnd?.Invoke(this));
                    }));
                }
            }
        }

        public delegate void OnLongTaskStart(object o);
        public event OnLongTaskStart LongTaskStart;

        public delegate void OnLongTaskEnd(object o);
        public event OnLongTaskEnd LongTaskEnd;

        #endregion

        #region Drag'n'Drop Logic

        bool MouseDownOnItem = false;
        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDownOnItem = true;
            Debug.WriteLine("MsDown on item");
        }

        //private bool DropClosed = false;
        private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!MouseDownOnItem) return;
            Debug.WriteLine("Detecting drag start");
            var draggedItem = sender as ListViewItem;
            draggedItem.IsSelected = true;
            draggedItem.Tag = "Moving";
            var data = draggedItem.DataContext as ImageListItemData;                       
            
            DragDrop.DoDragDrop(draggedItem, data, DragDropEffects.Move);                     
            draggedItem.Tag = "";                
            MouseDownOnItem = false;            
        }

        private void ListViewItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseDownOnItem = false;
            Debug.WriteLine("MsUp on item");
        }

        private void ListViewItem_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {            
            // update the position of the visual feedback item
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);                                          
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e)
        {
            var droppedData = e.Data.GetData(typeof(ImageListItemData)) as ImageListItemData;
            var target = (sender as ListViewItem).DataContext as ImageListItemData;
            int targetIndex = ImgListView.Items.IndexOf(target);

            ImageListData.Remove(droppedData);
            ImageListData.Insert(targetIndex, droppedData);            
            Debug.WriteLine("Item dropped... huh");
        }

       
        #endregion
        
        #region GetCursorPos Import
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };
        #endregion      
    }
}
