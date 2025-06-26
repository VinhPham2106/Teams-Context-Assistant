using WebSocketSharp;
using System.Text.Json;
using System.Diagnostics;



namespace TranscriptEmulator
{

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
                            Thread.Sleep(_delayPerWord * 10);
                        }
                    }
                    else
                    {
                        // Send the whole entry as it is within the limit
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
                // TODO: Make everything more configurable

                Console.WriteLine("Preparing to stream Transcript");

                using (var ws = new WebSocket("ws://localhost:9999/Transcript"))
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

                        Console.WriteLine("\n-------- processed transcript batch --------\n");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(suggestion.SuggestionMarkdown);
                        Console.ForegroundColor = ConsoleColor.White;
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