using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Crusade.Windows
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : CustomWindow
    {
        public DialogWindow()
        {
            InitializeComponent();            
        }

        private void TitleBarCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public string Message
        {
            get => MessageControl.Text;
            set => MessageControl.Text = value;
        }       
    }
}
