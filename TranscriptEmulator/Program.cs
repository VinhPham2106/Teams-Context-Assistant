using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;



namespace TranscriptEmulator
{
    public class SuggestionNotification : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            // Log the received message type
            try
            {
                SuggestionPayload suggestion = JsonSerializer.Deserialize<SuggestionPayload>(e.Data);
            } 
            catch (JsonException ex)
            {
                var err = JsonSerializer.Serialize(new
                {
                    error = "Invalid JSON payload",
                    details = ex.Message
                });
                Send(err);
                return;
            }
            var msg = JsonSerializer.Serialize(new
            {
                received = 1
            });

            Send(msg);

            // TODO: Process the suggestion
        }
    }

    public class TranscriptEmulator
    {
        private readonly WebSocket _webSocket;
        private int _maxWordsPerNotitification = 10;
        private int _delayPerWord = 600;
        private string _transcriptFileName = "defaultTranscript.json";
        public TranscriptEmulator(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }


        public void SendTranscript()
        {
            string jsonString = File.ReadAllText(_transcriptFileName);
            List<TranscriptPayload> entries = JsonSerializer.Deserialize<List<TranscriptPayload>>(jsonString);
            foreach (var entry in entries)
            {
                string[] words = entry.Transcript.Split(' ');
                if (words.Length > _maxWordsPerNotitification)
                {
                    // Split the transcript into chunks of max words
                    for (int i = 0; i < words.Length; i += _maxWordsPerNotitification)
                    {
                        var chunk = string.Join(" ", words.Skip(i).Take(_maxWordsPerNotitification));
                        var chunkEntry = new TranscriptPayload
                        {
                            Speaker = entry.Speaker,
                            Transcript = chunk
                        };
                        var json = JsonSerializer.Serialize(chunkEntry);
                        _webSocket.Send(json);
                        // Wait for a specified delay before sending the next chunk
                        System.Threading.Thread.Sleep(_delayPerWord * 10);
                    }
                }
                else
                {
                    // Send the whole entry as it is within the limit
                    var json = JsonSerializer.Serialize(entry);
                    _webSocket.Send(json);
                    System.Threading.Thread.Sleep(_delayPerWord * words.Length);
                }
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // TODO: Make everything more configurable

            var wssv = new WebSocketServer("ws://localhost:8888");
            wssv.AddWebSocketService<SuggestionNotification>("/Suggest");
            wssv.Start();
            Console.WriteLine("Suggestion Receiver started on ws://localhost:8888");

            Console.WriteLine("Preparing to stream Transcript");

            using (var ws = new WebSocket("ws://localhost:9999/"))
            {
                //ws.OnMessage += (sender, e) =>
                //    Console.WriteLine("Receive status " + e.Data);

                ws.Connect();
                TranscriptEmulator transcriptEmulator = new TranscriptEmulator(ws);
                transcriptEmulator.SendTranscript();
                Console.WriteLine("Transcript sent successfully.");
            }

            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}