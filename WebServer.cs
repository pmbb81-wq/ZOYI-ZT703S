using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZOYI
{
    internal class WebServer
    {
        private TcpListener? server;
        private IPAddress? IPaddr;
        private int port = 0;
        private string URI = "";
        private bool bRunning = false;

        FrameDecoder frame_decoder;

        public bool IsRunning => bRunning;

        public WebServer(FrameDecoder frm_decoder)
        {
            frame_decoder = frm_decoder;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPaddr = ipHostInfo.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }

        public string getURI() => URI;

        public void Start(int port = 8080)
        {
            if (!bRunning)
            {
                this.port = port;
                URI = $"http://{IPaddr}:{port}/";
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                bRunning = true;
                Task.Run(() => AcceptClients());
            }
        }

        public void Stop()
        {
            if (bRunning)
            {
                bRunning = false;
                server!.Stop();
                server.Dispose();
            }
        }

        private async void AcceptClients()
        {
            try
            {
                while (bRunning)
                {
                    var client = await server!.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException) { }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using var stream = client.GetStream();
                var buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return;

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string firstLine = request.Split('\r', '\n')[0];
                Console.WriteLine(">> " + firstLine);

                if (firstLine.Contains("GET") && firstLine.Contains("/measure"))
                {
                    await ServeSSE(stream);
                }
                else if (firstLine.Contains("GET") && firstLine.Contains("/zoyi.png"))
                {
                    await ServeFile(stream, "html\\zoyi.png", "image/png");
                }
                else if (firstLine.Contains("GET") && firstLine.Contains("/favicon.ico"))
                {
                    await ServeFile(stream, "html\\favicon.ico", "image/x-icon");
                }
                else if (firstLine.Contains("GET") && firstLine.Contains("t6.html"))
                {
                    await ServeFile(stream, "html\\t6.html", "text/html; charset=UTF-8");
                }
                else if (firstLine.Contains("GET") && firstLine.Contains("/images/"))
                {
                    string[] parts = firstLine.Split(' ');
                    string reqPath = parts.Length > 1 ? parts[1].TrimStart('/') : "";
                    string localPath = System.IO.Path.Combine("html", reqPath);
                    string ext = System.IO.Path.GetExtension(localPath).ToLower();
                    string mime = ext switch
                    {
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        ".svg" => "image/svg+xml",
                        ".ico" => "image/x-icon",
                        _ => "application/octet-stream"
                    };
                    await ServeFile(stream, localPath, mime);
                }
                else
                {
                    await ServeFile(stream, "html\\index.htm", "text/html; charset=UTF-8");
                }
            }
            catch (IOException) { }
            catch (Exception ex) { Console.WriteLine("ERR: " + ex.Message); }
        }

        private async Task ServeSSE(NetworkStream stream)
        {
            string header =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: text/event-stream\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Connection: keep-alive\r\n\r\n";

            byte[] hdr = Encoding.UTF8.GetBytes(header);
            await stream.WriteAsync(hdr, 0, hdr.Length);
            await stream.FlushAsync();

            int id = 0;
            try
            {
                while (bRunning)
                {
                    id++;
                    string json = frame_decoder.JsonSerialize();
                    string msg = $"id: {id}\r\ndata: {json}\r\n\r\n";
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                    await Task.Delay(200);
                }
            }
            catch (IOException) { Console.WriteLine("SSE: closed"); }
        }

        private async Task ServeFile(NetworkStream stream, string path, string contentType)
        {
            try
            {
                byte[] content = System.IO.File.ReadAllBytes(path);
                string header =
                    "HTTP/1.1 200 OK\r\n" +
                    $"Content-Type: {contentType}\r\n" +
                    $"Content-Length: {content.Length}\r\n" +
                    "Connection: close\r\n\r\n";

                byte[] hdr = Encoding.ASCII.GetBytes(header);
                await stream.WriteAsync(hdr, 0, hdr.Length);
                await stream.WriteAsync(content, 0, content.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FILE ERR: " + ex.Message);
                string body = "404 Not Found";
                byte[] b = Encoding.UTF8.GetBytes(body);
                string h = "HTTP/1.1 404 Not Found\r\nContent-Length: " + b.Length + "\r\nConnection: close\r\n\r\n";
                byte[] hdr = Encoding.ASCII.GetBytes(h);
                await stream.WriteAsync(hdr, 0, hdr.Length);
                await stream.WriteAsync(b, 0, b.Length);
            }
        }
    }
}
