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
using System.IO;

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
            this.directoryList.Checked += DirectoryList_Changed;
            this.directoryList.Unchecked += DirectoryList_Changed;

            this.log.Checked += LogCheck_Changed;
            this.log.Unchecked += LogCheck_Changed;

            this.FolderText.Text = System.IO.Directory.GetCurrentDirectory();

            this.log.IsChecked = true;
            this.directoryList.IsChecked = true;

        }

        /// <summary>
        /// log复选框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (log.IsChecked == true)
            {
                webServer.SetListener(LogMsg);
            }
            else
            {
                this.webServer.SetListener(null);
            }
        }

        /// <summary>
        /// 列举目录复选框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectoryList_Changed(object sender, RoutedEventArgs e)
        {
            webServer.ListEnable = directoryList.IsChecked == true;
        }

        /// <summary>
        /// 选择文件夹按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// RUN 按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (webServer.IsRunning)
            {
                webServer.Stop();
                RunButton.Content = "启动";
                Setting.IsEnabled = true;
            }
            else
            {
                Setting.IsEnabled = false;
                Run();
                RunButton.Content = "停止";
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="port"></param>
        /// <param name="path"></param>
        private void Run()
        {
            var port = int.Parse(PortText.Text);
            var path = this.FolderText.Text.Trim();
            this.LogText.Text = String.Format("Web Server is running on port {0}.\nThe root path is {1}\n", port, path);
            webServer.SetPort(port).SetRoot(path);
            if (log.IsChecked == true)
            {
                webServer.SetListener(LogMsg);
            }
            else
            {
                this.webServer.SetListener(null);
            }
            if (HTTPS.IsChecked == true)
            {
                String keyPath = keyText.Text;
                webServer.SetSSL(keyPath);
            }

            Task.Run(() => webServer.Start());

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

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog m_Dialog = new OpenFileDialog();
            m_Dialog.Title = "选择证书";
            m_Dialog.Multiselect = false;
            if (m_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.FileName.Trim();
            this.keyText.Text = m_Dir;
        }

        private void https_Checked(object sender, RoutedEventArgs e)
        {
            keyButton.IsEnabled = true;
            keyText.IsEnabled = true;
            if (PortText.Text.Trim() == "80")
            {
                PortText.Text = "443";
            }

            var key = keyText.Text;
            if (String.IsNullOrWhiteSpace(key) || !File.Exists(key))
            {
                //选择证书
                KeyButton_Click(null, null);
            }
        }

        private void https_Unchecked(object sender, RoutedEventArgs e)
        {
            keyButton.IsEnabled = false;
            keyText.IsEnabled = false;
            webServer.DelSSL();
            if (PortText.Text.Trim() == "443")
            {
                PortText.Text = "80";
            }
        }

        private void keyText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var path = this.keyText.Text.Trim();
            if (String.IsNullOrWhiteSpace(path))
            {
                //webServer.DelSSL();
                https_Unchecked(null, null);
            }
            else
            {
                webServer.SetSSL(path);
            }
        }



        private void directoryList_Click(object sender, RoutedEventArgs e)
        {
            webServer.ListEnable = directoryList.IsChecked == true;
        }
    }
}
