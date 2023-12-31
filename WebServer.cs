using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServer
{
    public static class Server{
        private static HttpListener listener;

        private static List<IPAddress> GetLocalHostIPs(){
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ret;
        }

        private static HttpListener InitializeListener(List<IPAddress> localhostIPs){
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            localhostIPs.ForEach(ip => {Console.WriteLine("Listening on IP" + "http://" + ip.ToString() + "/"); listener.Prefixes.Add("http://" + ip.ToString() + "/");});
            return listener;
        }

        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

        private static void Start(HttpListener listener){
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        private static void RunServer(HttpListener listener){
            while (true){
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        private static async void StartConnectionListener(HttpListener listener){
            HttpListenerContext context = await listener.GetContextAsync();

            sem.Release();

            string response = "Hello Browser!";
            byte[] encoded = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = encoded.Length;
            context.Response.OutputStream.Write(encoded, 0, encoded.Length);
            context.Response.OutputStream.Close();
        }

        public static void Start(){
            List<IPAddress> localHostIPs = GetLocalHostIPs();
            HttpListener listener = InitializeListener(localHostIPs);
            Start(listener);
        }
    }
}

namespace ConsoleWebServer{
    class Program{
        static void Main(string[] args){
            WebServer.Server.Start();
            Console.ReadLine();
        }
    }
}

