using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Example
{
    public class Laputa : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = e.Data == "BALUS"
                      ? "I've been balused already..."
                      : "I'm not available now.";

            Send(msg);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var wssv = new WebSocketServer("ws://localhost:8888");
            wssv.AddWebSocketService<Laputa>("/Laputa");
            wssv.Start();
            Console.WriteLine("WebSocket server started on ws://localhost:8888\n");
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}