using WebSocketSharp;
using System.Text.Json;
using System.Diagnostics;

namespace TranscriptEmulator
{
    public class TranscriptEmulator
    {
        private readonly WebSocket _webSocket;
        private int _maxWordsPerNotification = 10;
        private int _delayPerWord = 600;
        private string _transcriptFileName = "defaultTranscript.json";
        private static int _batchNo;

        public TranscriptEmulator(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public void SendTranscript()
        {
            string jsonString = File.ReadAllText(_transcriptFileName);

            try
            {
                List<TranscriptPayload> entries = JsonSerializer.Deserialize<List<TranscriptPayload>>(jsonString);

                if (entries == null || entries.Count == 0)
                {
                    Console.WriteLine("No transcript entries found or deserialization failed.");
                    return;
                }

                foreach (var entry in entries)
                {
                    string[] words = entry.Transcript.Split(' ');

                    if (words.Length > _maxWordsPerNotification)
                    {
                        for (int i = 0; i < words.Length; i += _maxWordsPerNotification)
                        {
                            var chunk = string.Join(" ", words.Skip(i).Take(_maxWordsPerNotification));
                            var chunkEntry = new TranscriptPayload
                            {
                                Speaker = entry.Speaker,
                                Transcript = chunk
                            };

                            var json = JsonSerializer.Serialize(chunkEntry);
                            _webSocket.Send(json);

                            var jsonElement = JsonSerializer.Deserialize<TranscriptPayload>(json);
                            string prettyJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });

                            Console.WriteLine($"\n── Sending Batch {++_batchNo} to WebSocket ──\n");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(prettyJson);
                            Console.ForegroundColor = ConsoleColor.White;

                            Thread.Sleep(_delayPerWord * 10);
                        }
                    }
                    else
                    {
                        var json = JsonSerializer.Serialize(entry);
                        _webSocket.Send(json);
                        Thread.Sleep(_delayPerWord * words.Length);
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Error deserializing JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred while sending the transcript: " + ex.Message);
            }
        }

        public class Program
        {
            public static void Main(string[] args)
            {
                Console.WriteLine("Preparing to stream Transcript");

                // Define TranscriptEndpoint from environment variable or default value
                string transcriptEndpoint = Environment.GetEnvironmentVariable("TRANSCRIPT_ENDPOINT") ?? "ws://localhost:9999/Transcript";
                //transcriptEndpoint = "ws://host.docker.internal:9999/Transcript";
                Console.WriteLine("Using Transcript Endpoint: " + transcriptEndpoint);
                using (var ws = new WebSocket(transcriptEndpoint))
                {
                    ws.OnMessage += (sender, e) =>
                    {
                        SuggestionPayload suggestion;

                        try
                        {
                            suggestion = JsonSerializer.Deserialize<SuggestionPayload>(e.Data);
                        }
                        catch (JsonException ex)
                        {
                            var err = JsonSerializer.Serialize(new
                            {
                                error = "Invalid JSON payload",
                                details = ex.Message
                            });

                            ws.Send(err);
                            return;
                        }

                        Debug.Assert(suggestion != null, "SuggestionPayload should not be null after deserialization.");
                    };

                    ws.Connect();
                    TranscriptEmulator transcriptEmulator = new TranscriptEmulator(ws);
                    transcriptEmulator.SendTranscript();

                    Console.WriteLine("Transcript sent successfully.");
                }

                Console.WriteLine("Streaming complete. Press any key to exit.");
                Console.ReadKey(true);
            }
        }
    }
}
