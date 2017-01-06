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

        public WebServer(string ip) : base(ip)
        {
        }

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
        public bool ListEnable { get; set; }


        public override void OnGet(HttpRequest request, HttpResponse response)
        {
            string requestURL = request.URL.Trim().TrimStart('/').Replace("/", @"\").Replace("\\..", "");
            string requestFile = Path.Combine(ServerRoot, requestURL);
            Log(requestFile);

            if (Directory.Exists(requestFile))//文件夹
            {
                requestFile = requestFile.TrimEnd('\\') + '\\';
                if (ListEnable && !File.Exists(requestFile + index))
                {
                    //列出目录
                    response.SetContent(ListFiles(requestFile, requestURL), Encoding.UTF8);
                    response.Content_Type = "text/html";
                    return;
                }
                else
                {
                    requestFile += Path.Combine(requestFile, index);
                }
            }

            ResponseWithFile(requestFile, response);

        }


        /// <summary>
        /// POST
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public override void OnPost(HttpRequest request, HttpResponse response)
        {
            string s = string.Join(";", request.Params.Select(x => x.Key + "=" + x.Value).ToArray());
            response.SetContent(s, Encoding.UTF8);
        }

        /// <summary>
        /// 其他操作
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public override void OnDefault(HttpRequest request, HttpResponse response)
        {
            if (request.Method == "HEAD")
            {
                response.SetContent("");
            }
            else if (request.Method == "DELE")
            {
                response.SetContent("DELE: " + request.URL);
            }
            else
            {
                response.StatusCode = "405";
                response.SetContent("405 Method Not Allowed:" + request.Method);
            }
        }


        /// <summary>
        /// 拼接字符串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
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
        private string ListFiles(string path, string h1)
        {
            string[] folders = { "../" };
            folders = folders.Concat(Directory.GetDirectories(path)).ToArray();
            var files = Directory.GetFiles(path);

            var listFolders = ListString(folders);

            var listFiles = ListString(files);

            var responseText = String.Format(
                "<html><head><title>{0}</title></head><body><h1>{1}[目录]</h1><h2>文件列表</h2><hr/><ul>{2}</ul><br/><h2>目录列表</h2><hr/><ul>{3}</ul></body></html>",
                path,
                 h1,
                listFiles,
                listFolders
             );
            //var response = new HttpResponse(responseText, Encoding.UTF8);
            //return response;
            return responseText;
        }

        /// <summary>
        /// 使用文件来提供HTTP响应
        /// </summary>
        /// <param name="fileName">文件名</param>
        private HttpResponse ResponseWithFile(string fileName, HttpResponse response)
        {
            //准备HTTP响应报文
            //HttpResponse response;

            //如果文件不存在则返回404否则读取文件内容
            if (!File.Exists(fileName))
            {
                response.SetContent("<html><body><h1>404 - Not Found</h1></body></html>", Encoding.UTF8);
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
                response = response.SetContent(File.ReadAllBytes(fileName), Encoding.UTF8);
                response.StatusCode = "200";
                response.Content_Type = contentType;
            }

            //返回数据
            return response;
        }
    }
}
