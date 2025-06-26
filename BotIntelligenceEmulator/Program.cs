using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;
using WebSocketSharp;
using WebSocketSharp.Server;
using BotIntelligence;       

internal record TranscriptPayload(string Speaker, string Transcript);
internal record SuggestionPayload(string SuggestionMarkdown);

internal sealed class TranscriptReceiver : WebSocketBehavior
{
    private readonly Action<TranscriptPayload> _onChunk;
    public TranscriptReceiver(Action<TranscriptPayload> onChunk) => _onChunk = onChunk;

    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<TranscriptPayload>(e.Data);
            if (payload != null)
            {
                _onChunk(payload);
                Send(@"{""received"":1}");
            }
        }
        catch
        {
            Send(@"{""error"":""bad payload""}");
        }
    }
}

internal static class Program
{
    private const int BatchSize = 4;
    private static readonly Regex AcronymRegex = new(@"\b[A-Z]{2,5}\b");

    private static readonly ConcurrentQueue<TranscriptPayload> _queue = new();
    private static int _totalChunks;
    private static int _batchNo;
    private static WebSocketServer? _wss;

    private static async Task ProcessBatchAsync(IEnumerable<TranscriptPayload> batch)
    {
        string transcript = string.Join(' ', batch.Select(p => p.Transcript));
        if (string.IsNullOrWhiteSpace(transcript)) return;

        Console.WriteLine($"\n── Batch {++_batchNo} ({transcript.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length} words) ──\n");

        Env.Load();

        var glossary = new Dictionary<string, string>(Glossary.Items, StringComparer.OrdinalIgnoreCase);

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                     ?? throw new InvalidOperationException("OPENAI_API_KEY not found");

        using var openAi = new OpenAiService(apiKey);

        string topic = await openAi.ClassifyTopicAsync(MeetingTopics.List, transcript);

        var defs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cameFromGlossary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in AcronymRegex.Matches(transcript))
        {
            string term = m.Value;
            if (defs.ContainsKey(term)) continue;

            if (glossary.TryGetValue(term, out var meaning))
            {
                defs[term] = meaning;
                cameFromGlossary.Add(term);
            }
            else
            {
                defs[term] = await openAi.DefineAsync(term);
            }
        }

        foreach (var term in cameFromGlossary)
            defs[term] = await openAi.EnhanceAsync(term, defs[term]);

        string markdown = defs.Count == 0
            ? $"**Topic of Discussion:** {topic}"
            : $"**Topic of Discussion:** {topic}\n\n{$"**Term Definitions:**"}\n\n{await openAi.BeautifyAsync(defs)}";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(markdown + "\n");
        Console.ResetColor();

        var json = JsonSerializer.Serialize(new SuggestionPayload(markdown));
        _wss!.WebSocketServices["/Transcript"].Sessions.Broadcast(json);
    }

    private static void Main()
    {
        _wss = new WebSocketServer("ws://localhost:9999");
        _wss.AddWebSocketService(
            "/Transcript",
            () => new TranscriptReceiver(chunk =>
            {
                _queue.Enqueue(chunk);

                if (Interlocked.Increment(ref _totalChunks) % BatchSize == 0)
                {
                    var batch = new List<TranscriptPayload>(BatchSize);
                    for (int i = 0; i < BatchSize && _queue.TryDequeue(out var item); i++)
                        batch.Add(item);

                    _ = Task.Run(() => ProcessBatchAsync(batch));
                }
            }));

        _wss.Start();
        Console.WriteLine("Bot listening on ws://localhost:9999/Transcript  (press ENTER to quit)");
        Console.ReadLine();
        _wss.Stop();
    }
}