using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HTTPServer;

namespace GUI
{
    class WebServer : HttpServer
    {
        private string index = "index.html";
        /// <summary>
        /// 默认文件
        /// </summary>
        public string Index
        {
            get
            {
                return index;
            }
            private set
            {
                this.index = value;

            }
        }

        /// <summary>
        /// 是否开启目录列表
        /// </summary>
        public bool EnableList { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        public WebServer(string ipAddress, int port)
            : base(ipAddress, port)
        {

        }

        public override void OnGet(HttpRequest request)
        {
            string requestURL = request.URL.Trim().TrimStart('/').Replace("/", @"\").Replace("\\..", "");
            string requestFile = Path.Combine(ServerRoot, requestURL);
            Console.WriteLine(requestFile);

            //构造HTTP响应
            HttpResponse response;

            if (Directory.Exists(requestFile))//文件夹
            {
                requestFile = requestFile.TrimEnd('\\') + '\\';
                if (!File.Exists(requestFile + index))
                {
                    //列出目录
                    response = ListFiles(requestFile);
                }
                else
                {
                    requestFile += Path.Combine(requestFile, index);
                    response = ResponseWithFile(requestFile);

                }
            }
            else
            {
                response = ResponseWithFile(requestFile);
            }

            //构造HTTP响应
            response.Server = "FutureHTTP";

            //发送响应
            ProcessResponse(request.Handler, response);
        }

        private string ListString(string[] list)
        {
            // Enumerable.Aggregate(files, (pre, file) => String.Format("{0}<li><a href=\"{1}\">{1}</a></li>", pre, file.Trim(Path.PathSeparator)));
            string text = "";
            foreach (var l in list)
            {
                text += String.Format("<li><a href=\"{0}\">{0}</a></li>", l.Trim('\\'));
            }
            return text;

        }

        /// <summary>
        /// 列出目录
        /// </summary>
        /// <param name="path"></param>
        public HttpResponse ListFiles(string path)
        {
            string[] folders = { "../" };
            folders = folders.Concat(Directory.GetDirectories(path)).ToArray();
            var files = Directory.GetFiles(path);

            var listFolders = ListString(folders);

            var listFiles = ListString(files);

            var responseText = String.Format(
                "<html><head><title>{0}</title></head><body><h1>{1}[目录]</h1><h2>文件列表</h2><hr/><ul>{2}</ul><br/><h2>目录列表</h2><hr/><ul>{3}</ul></body></html>",
                path,
                Path.GetDirectoryName(path),
                listFiles,
                listFolders
             );
            var response = new HttpResponse(responseText, Encoding.UTF8);
            response.StatusCode = "200";
            response.Content_Type = "text/html";
            return response;
        }

        /// <summary>
        /// 使用文件来提供HTTP响应
        /// </summary>
        /// <param name="fileName">文件名</param>
        private HttpResponse ResponseWithFile(string fileName)
        {
            //准备HTTP响应报文
            HttpResponse response;

            //如果文件不存在则返回404否则读取文件内容
            if (!File.Exists(fileName))
            {
                response = new HttpResponse("<html><body><h1>404 - Not Found</h1></body></html>", Encoding.UTF8);
                response.StatusCode = "404";
                response.Content_Type = "text/html";
                response.Server = "ExampleServer";
            }
            else
            {
                //获取文件扩展名以判断内容类型
                string extension = Path.GetExtension(fileName);

                //获取当前内容类型
                string contentType = GetContentType(extension);

                response = new HttpResponse(File.ReadAllBytes(fileName), Encoding.UTF8);
                response.StatusCode = "200";
                response.Content_Type = contentType;
            }

            //返回数据
            return response;
        }
    }
}
