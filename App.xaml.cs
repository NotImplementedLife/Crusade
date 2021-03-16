using Crusade.Controls;
using Crusade.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crusade
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {      
        public App()
        {           
        }

        private void App_Startup(object sender,StartupEventArgs e)
        {
            if(e.Args.Length==0)
            {
                new MainWindow().Show();
            }
            else
            {
                int k = 0;
                while(k<e.Args.Length)
                {                   
                    k++;
                }
            }
        }

        public static readonly string Dir = AppDomain.CurrentDomain.BaseDirectory;
    }
}
