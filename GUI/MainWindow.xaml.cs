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
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Net;

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
            webServer = new WebServer("0.0.0.0");
            InitializeComponent();
            this.FolderText.Text = System.IO.Directory.GetCurrentDirectory();
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
            String keyPath = keyText.Text;
            this.LogText.Text = String.Format("Web Server is running on port {0}.\nThe root path is {1}\n", port, path);
            Task.Run(() => this.Run(port, path, keyPath));

        }

        private void directoryList_Checked(object sender, RoutedEventArgs e)
        {
            webServer.ListEnable = directoryList.IsChecked == true;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="port"></param>
        /// <param name="path"></param>
        private void Run(int port, string path, string keyPath = null )
        {
            webServer.SetPort(port)
              .SetRoot(path)
              .SetListener(LogMsg)
              //.SetSSL(keyPath)
              .Start();
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private DispatcherOperation LogMsg(string msg)
        {
            return this.Dispatcher.BeginInvoke(new Action(() => LogText.Text += "\n" + msg));
        }

        private void LogText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog m_Dialog = new OpenFileDialog();

            if (m_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            //string m_Dir = m_Dialog.SelectedPath.Trim();
            string m_Dir = m_Dialog.FileName.Trim();
            this.keyText.Text = m_Dir;
        }

        private void https_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
