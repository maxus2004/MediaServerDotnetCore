using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaServerDotnetCore {
    class Program {
        static Dictionary<string, string[][]> mediaList = new Dictionary<string, string[][]>();
        static void Main(string[] args) {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://media.loc/");
            listener.Start();

            Console.WriteLine("Server started");

            while (true) {
                HttpListenerContext context = listener.GetContext();

                if (context.Request.RawUrl.StartsWith("/update")) {
                    string pcName = context.Request.QueryString["pc_name"];
                    string dataStr = new StreamReader(context.Request.InputStream).ReadToEnd();
                    context.Response.OutputStream.Close();

                    Console.WriteLine(dataStr);

                    string[][] media = dataStr.Split('\n').Select(a => { return a.Split('\t'); }).ToArray();
                    mediaList[pcName] = media;
                } else if (context.Request.RawUrl.Equals("/")) {
                    byte[] mediaListHtml = getMediaListHtml();
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.OutputStream.Write(mediaListHtml, 0, mediaListHtml.Length);
                    context.Response.OutputStream.Close();
                } else if (context.Request.RawUrl.Equals("/style.css")) {
                    string test = "" +
                        ".mediaList{\n" +
                        "border: 2px solid black;\n" +
                        "}\n" +
                        ".pcName{\n" +
                        "font: 16pt sans-serif;\n" +
                        "}\n" +
                        ".mediaLink{\n" +
                        "font: 14pt sans-serif;\n" +
                        "margin: 0;\n" +
                        "}\n";
                    byte[] bytes = Encoding.UTF8.GetBytes(test);
                    context.Response.ContentType = "text/css; charset=UTF-8";
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    context.Response.OutputStream.Close();
                }
            }
        }

        static byte[] getMediaListHtml() {
            string mediaHtml = "";
            mediaHtml += "<!DOCTYPE html>\n";
            mediaHtml += "<html>\n";
            mediaHtml += "<head>\n";
            mediaHtml += "<title>МедиаСервер</title>\n";
            mediaHtml += "<link rel='stylesheet' href='style.css'>\n";
            mediaHtml += "</head>\n";
            mediaHtml += "<body>\n";
            foreach (KeyValuePair<string, string[][]> media in mediaList) {
                string pcName = media.Key;
                string[][] list = media.Value;

                mediaHtml += "<div class='mediaList'>\n";
                mediaHtml += "<p class='pcName'>" + pcName + "</p>\n";
                foreach (string[] movie in list) {
                    mediaHtml += "<p class='mediaLink'>\r\n<a href = ";
                    mediaHtml += movie[0];
                    mediaHtml += ">\n";
                    mediaHtml += movie[1];
                    mediaHtml += "</a>\n</p>\n";
                }
                mediaHtml += "</div>\n";
            }
            mediaHtml += "</body>\n";
            mediaHtml += "</html>\n";

            return Encoding.UTF8.GetBytes(mediaHtml);
        }
    }
}
