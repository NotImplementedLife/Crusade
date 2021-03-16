using System.Windows;
using System.Windows.Input;


namespace Crusade.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();               
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

        private void TitleBarPreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            new DialogWindow() { Owner = this, Message = "Coming soon...", Title = "Preferences" }.ShowDialog();
        }

        private void TitleBarInfoButton_Click(object sender, RoutedEventArgs e)
        {
            new DialogWindow()
            {
                Owner = this,
                Message = $"Crusade {AssemblyData.VersionName}\n"+
                          "by NotImplementedLife",
                Title = "Info"
            }
            .ShowDialog();
        }

        #endregion       
    }
}
