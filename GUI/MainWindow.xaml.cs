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
            this.LogText.Text = String.Format("Web Server is running on port {0}.\nThe root path is {1}\n", port, path);
            Task.Run(() => this.Run(port, path));

            //HttpListener listener = new HttpListener();
            //listener.Prefixes.Add("http://127.0.0.1:8080/");
            //listener.Start();
            //Console.WriteLine("Listening...");
            //// Note: The GetContext method blocks while waiting for a request. 
            //HttpListenerContext context = listener.GetContext();
            //HttpListenerRequest request = context.Request;
            //// Obtain a response object.
            //HttpListenerResponse response = context.Response;
            //// Construct a response.
            //string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            //// Get a response stream and write the response to it.
            //response.ContentLength64 = buffer.Length;
            //System.IO.Stream output = response.OutputStream;
            //output.Write(buffer, 0, buffer.Length);
            //// You must close the output stream.
            //output.Close();
            //listener.Stop();

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
        private void Run(int port, string path)
        {
            webServer.SetPort(port)
              .SetRoot(path)
              .SetListener(LogMsg)
              .SetSSL(@"D:\code\HttpServer\ssl\ssl.pfx")
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
    }
}
