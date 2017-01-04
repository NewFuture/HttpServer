using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebServer webServer;
        public MainWindow()
        {
            InitializeComponent();
            this.FolderText.Text = System.IO.Directory.GetCurrentDirectory();
            webServer = new WebServer("0.0.0.0");
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();

            if (m_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.FolderText.Text = m_Dir;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var port = int.Parse(PortText.Text);
            var path = this.FolderText.Text.Trim();
            webServer.SetPort(port);
            webServer.SetRoot(path);
            webServer.Start();
        }

        private void directoryList_Checked(object sender, RoutedEventArgs e)
        {
            webServer.EnableList = directoryList.IsChecked == true;
        }
    }
}
