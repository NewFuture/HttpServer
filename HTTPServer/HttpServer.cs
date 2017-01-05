using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace HTTPServer
{
    public class HttpServer : IServer
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string ServerIP { get; private set; }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int ServerPort { get; private set; }

        /// <summary>
        /// 服务器目录
        /// </summary>
        public string ServerRoot { get; private set; }

        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsRunning { get; private set; }


        /// <summary>
        /// 服务端Socet
        /// </summary>
        //private Socket serverSocket;
        public TcpListener serverListener { get; private set; }



        /// <summary>
        /// SSL证书
        /// </summary>
        private X509Certificate serverCertificate = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="root">根目录</param>
        private HttpServer(IPAddress ipAddress, int port, string root)
        {
            this.ServerIP = ipAddress.ToString();
            this.ServerPort = port;

            //如果指定目录不存在则采用默认目录
            if (!Directory.Exists(root))
                this.ServerRoot = AppDomain.CurrentDomain.BaseDirectory;

            this.ServerRoot = root;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="root">根目录</param>
        public HttpServer(string ipAddress, int port, string root) :
            this(IPAddress.Parse(ipAddress), port, root)
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        public HttpServer(string ipAddress, int port) :
            this(IPAddress.Parse(ipAddress), port, AppDomain.CurrentDomain.BaseDirectory)
        { }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="root">根目录</param>
        public HttpServer(int port, string root) :
            this(IPAddress.Loopback, port, root)
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">端口号</param>
        public HttpServer(int port) :
            this(IPAddress.Loopback, port, AppDomain.CurrentDomain.BaseDirectory)
        { }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        public HttpServer(string ip) :
            this(IPAddress.Parse(ip), 80, AppDomain.CurrentDomain.BaseDirectory)
        { }

        #region 公开方法 

        /// <summary>
        /// 开启服务器
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                return;

            //创建服务端Socket
            serverListener = new TcpListener(IPAddress.Parse(ServerIP), ServerPort);
            IsRunning = true;
            this.Log(String.Format("Sever is running at http://{0}:{1}/.", ServerIP, ServerPort));
            serverListener.Start();
            while (IsRunning)
            {
                TcpClient client = serverListener.AcceptTcpClient();
                Thread requestThread = new Thread(() => { ProcessRequest(client); });
                requestThread.Start();
            }

            //new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort));
            //serverSocket.Listen(10);
            //IsRunning = true;

            //输出服务器状态


            ////连接客户端
            //while (IsRunning)
            //{
            //    Socket clientSocket = serverSocket.Accept();
            //    Thread requestThread = new Thread(() => { ProcessRequest(clientSocket); });
            //    requestThread.Start();
            //}
        }




        public void SetSSL(string certificate)
        {
            var serverCertificate = X509Certificate.CreateFromCertFile(certificate);

        }

        public void SetSSL(X509Certificate certifiate)
        {
            this.serverCertificate = certifiate;
        }
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            serverListener.Stop();
        }

        /// <summary>
        /// 设置服务器目录
        /// </summary>
        /// <param name="root"></param>
        public HttpServer SetRoot(string root)
        {
            if (!Directory.Exists(root))
                this.ServerRoot = AppDomain.CurrentDomain.BaseDirectory;

            this.ServerRoot = root;
            return this;
        }


        /// <summary>
        /// 设置端口
        /// </summary>
        /// <param name="port">端口号int</param>
        /// <returns></returns>
        public HttpServer SetPort(int port)
        {
            if (port > 0)
            {
                this.ServerPort = port;
            }

            return this;
        }
        #endregion

        #region 内部方法

        /// <summary>
        /// 处理客户端请求
        /// </summary>
        /// <param name="handler">客户端Socket</param>
        private void ProcessRequest(TcpClient handler)
        {
            //构造请求报文
            Stream clientStream = handler.GetStream();
            HttpRequest request = null;
            //if (serverCertificate != null)
            //{
            //    SslStream clientStream = new SslStream(clientStream, false);
            //    request = new HttpRequest(sslStream);
            //}
            //else
            //{
            //    request = new HttpRequest(clientStream);
            //}
            request = new HttpRequest(clientStream);
            HttpResponse response = new HttpResponse(clientStream);
            //根据请求类型进行处理
            if (request.Method == "GET")
            {
                OnGet(request, response);
            }
            else if (request.Method == "POST")
            {
                OnPost(request, response);
            }
            else
            {
                OnDefault(request, response);
            }
        }


        private byte[] ProcessSSL(SslStream sslStream)
        {
            byte[] message = null;
            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, true);
                // Display the properties and settings for the authenticated stream.
                //DisplaySecurityLevel(sslStream);
                //DisplaySecurityServices(sslStream);
                //DisplayCertificateInformation(sslStream);
                //DisplayStreamProperties(sslStream);

                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 10000;
                // Read a message from the client.   
                //Console.WriteLine("Waiting for client message...");
                //string messageData = ReadMessage(sslStream);

                //byte[] buffer = new byte[2048];
                //StringBuilder messageData = new StringBuilder();
                //int bytes = -1;
                //do
                //{
                //    // Read the client's test message.
                //    bytes = sslStream.Read(buffer, 0, buffer.Length);

                //    // Use Decoder class to convert from bytes to UTF8
                //    // in case a character spans two buffers.
                //    Decoder decoder = Encoding.UTF8.GetDecoder();
                //    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                //    decoder.GetChars(buffer, 0, bytes, chars, 0);
                //    messageData.Append(chars);
                //    // Check for EOF or an empty message.
                //    if (messageData.ToString().IndexOf("<EOF>") != -1)
                //    {
                //        break;
                //    }
                //} while (bytes != 0);

                //return messageData.ToString();

                //Console.WriteLine("Received: {0}", messageData);

                //// Write a message to the client.
                //message = Encoding.UTF8.GetBytes("Hello from the server.<EOF>");
                //Console.WriteLine("Sending hello message.");
                //sslStream.Write(message);
            }
            catch (AuthenticationException e)
            {
                Log("Authentication failed - closing the connection: " + e.Message);
                if (e.InnerException != null)
                {
                    Log("Inner exception: " + e.InnerException.Message);
                }
                message = null;
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                sslStream.Close();
            }
            return message;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected HttpServer Log(string msg)
        {
            Console.WriteLine(msg);
            return this;
        }

        /// <summary>
        /// 处理服务端响应
        /// </summary>
        /// <param name="handler">客户端Socket</param>
        /// <param name="response">响应报文</param>
        //protected void ProcessResponse(HttpResponse response)
        //{
        //    //构建响应头
        //    byte[] header = response.Encoding.GetBytes(response.BuildHeader());

        //    //发送响应头
        //    handler.Send(header);

        //    //发送空行
        //    handler.Send(response.Encoding.GetBytes(System.Environment.NewLine));

        //    //发送消息体
        //    handler.Send(response.Content);

        //    //结束会话
        //    handler.Close();
        //}

        /// <summary>
        /// 根据文件扩展名获取内容类型
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns></returns>
        protected string GetContentType(string extension)
        {
            string reval = string.Empty;

            if (string.IsNullOrEmpty(extension))
                return null;

            switch (extension)
            {
                case ".htm":
                    reval = "text/html";
                    break;
                case ".html":
                    reval = "text/html";
                    break;
                case ".txt":
                    reval = "text/plain";
                    break;
                case ".css":
                    reval = "text/css";
                    break;
                case ".png":
                    reval = "image/png";
                    break;
                case ".gif":
                    reval = "image/gif";
                    break;
                case ".jpg":
                    reval = "image/jpg";
                    break;
                case ".jpeg":
                    reval = "image/jgeg";
                    break;
                case ".zip":
                    reval = "application/zip";
                    break;
            }
            return reval;
        }

        #endregion

        #region 虚方法

        /// <summary>
        /// 响应Get请求
        /// </summary>
        /// <param name="request">请求报文</param>
        public virtual void OnGet(HttpRequest request, HttpResponse response)
        {

        }

        /// <summary>
        /// 响应Post请求
        /// </summary>
        /// <param name="request"></param>
        public virtual void OnPost(HttpRequest request, HttpResponse response)
        {

        }

        /// <summary>
        /// 响应默认请求
        /// </summary>

        public virtual void OnDefault(HttpRequest request, HttpResponse response)
        {

        }

        #endregion


    }
}
