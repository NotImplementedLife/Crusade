using Crusade.Data;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Crusade.Controls
{
    /// <summary>
    /// Interaction logic for ImgContainer.xaml
    /// </summary>
    public partial class ImgContainer : UserControl
    {        
        public ImgContainer()
        {
            InitializeComponent();

            ImagesListView.LongTaskStart += ImagesListView_LongTaskStart;
            ImagesListView.LongTaskEnd += ImagesListView_LongTaskEnd;
            CurrentVisible = OpenFilesUI;             
        }

        #region UI
        private FrameworkElement CurrentVisible;
        public void SetCurrentVisibleUI(FrameworkElement element)
        {
            if (CurrentVisible != null)
                CurrentVisible.Visibility = Visibility.Collapsed;
            CurrentVisible = element;
            CurrentVisible.Visibility = Visibility.Visible;
        }

        public void SetDragModeInterface() => SetCurrentVisibleUI(DropFilesUI);
        public void SetOpenModeInterface() => SetCurrentVisibleUI(OpenFilesUI);
        public void SetListViewInterface() => SetCurrentVisibleUI(ListViewUI);
        private void ImagesListView_ImageListCountChanged(object sender, int oldCount, int newCount)
            => SetCurrentVisibleUI(newCount == 0 ? (FrameworkElement)OpenFilesUI : (FrameworkElement)ListViewUI);

        private void ImagesListView_LongTaskEnd(object o)
        {
            LoadingStatusIcon.Visibility = Visibility.Hidden;
        }

        private void ImagesListView_LongTaskStart(object o)
        {
            LoadingStatusIcon.Visibility = Visibility.Visible;
        }
        #endregion

        #region Drag'n'Drop        

        private void ImgContainer_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {                
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                SetListViewInterface();
                Task.Run(new Action(() => ImagesListView.DealWithInput(files,
                     () => { LoadingStatusIcon.Visibility = Visibility.Hidden; })));
            }
        }

        private void ImgContainter_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                SetDragModeInterface();
        }

        private void ImgContainer_PreviewDragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                SetOpenModeInterface();
        }
        #endregion

        #region Open Files + Dialog
        private OpenFileDialog OpenFileDialog = new OpenFileDialog()
        {
            Multiselect=true,
            Filter = $"Picture files|{string.Join(";", Constants.PictureExtensions.Select(e => "*" + e).ToList())}|All files (*.*)|*.*"
        };

        private void OpenFilesLink_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFileDialog.ShowDialog() == true)
            {
                SetListViewInterface();
                string[] files = OpenFileDialog.FileNames;
                LoadingStatusIcon.Visibility = Visibility.Visible;               
                Task.Run(new Action(() => ImagesListView.DealWithInput(files, 
                    () => { LoadingStatusIcon.Visibility = Visibility.Hidden; })));
            }
        }
        #endregion

        #region Save File + Dialog
        private SaveFileDialog SaveFileDialog = new SaveFileDialog();

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            var convData = ConvertComboBox.SelectedValue as Converter.ConvertData;            
            SaveFileDialog.Filter = convData.Filter;
            if(SaveFileDialog.ShowDialog()==true)
            {
                LoadingStatusIcon.Visibility = Visibility.Visible;
                string[] files = ImagesListView.ImageListData.Select(item => item.TempPath != "" ? item.TempPath : item.Path).ToArray();
                string outputhPath = SaveFileDialog.FileName;
                Task.Run(new Action(() =>
                {
                    convData.Action(files,outputhPath);
                    Dispatcher.Invoke(() => LoadingStatusIcon.Visibility = Visibility.Hidden);
                }));
            }           
        }
        #endregion
    }
}
