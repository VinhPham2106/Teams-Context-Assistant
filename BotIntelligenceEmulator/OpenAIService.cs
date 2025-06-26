using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BotIntelligence
{
    public sealed class OpenAiService : IDisposable
    {
        private const string DefaultModel = "gpt-3.5-turbo";
        private readonly HttpClient _http;
        private readonly bool _disposeClient;

        public OpenAiService(string apiKey, HttpClient? httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("OpenAI key missing.", nameof(apiKey));

            _http = httpClient ?? new HttpClient();
            _disposeClient = httpClient is null;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> DefineAsync(
            string term,
            string model = DefaultModel,
            CancellationToken ct = default)
        {
            var body = new
            {
                model,
                temperature = 0,
                messages = new[]
                {
                    new { role = "system", content = "You are a concise technical dictionary." },
                    new { role = "user",   content = $"Explain in one sentence what \"{term}\" means." }
                }
            };

            using JsonDocument json = await PostJsonAsync(body, ct);
            return ExtractContent(json);
        }

        public async Task<string> EnhanceAsync(
            string term,
            string currentDef,
            string model = DefaultModel,
            CancellationToken ct = default)
        {
            var body = new
            {
                model,
                temperature = 0.2,
                messages = new[]
                {
                    new { role = "system",
                          content = "You are an expert technical editor. If the definition supplied is already clear and correct, return it unchanged. Otherwise return a better one." },
                    new { role = "user",
                          content = $"Term: {term}\nCurrent definition: {currentDef}\nReturn the best one-sentence definition." }
                }
            };

            using JsonDocument json = await PostJsonAsync(body, ct);
            return ExtractContent(json);
        }

        public async Task<string> BeautifyAsync(
            IDictionary<string, string> defs,
            string model = DefaultModel,
            CancellationToken ct = default)
        {
            var sb = new StringBuilder();
            foreach (var kv in defs)
                sb.AppendLine($"{kv.Key}: {kv.Value}");

            var body = new
            {
                model,
                temperature = 0.2,
                messages = new[]
                {
                    new { role = "system",
                          content = "You format chatbot responses for Microsoft Teams. Return a bulleted list where each line is **bold term** – definition." },
                    new { role = "user",
                          content = "Create the list from these term/definition pairs:\n" + sb }
                }
            };

            using JsonDocument json = await PostJsonAsync(body, ct);
            return ExtractContent(json);
        }

        public async Task<string> ClassifyTopicAsync(
            IEnumerable<string> topics,
            string transcript,
            string model = DefaultModel,
            CancellationToken ct = default)
        {
            string list = string.Join(", ", topics);

            var body = new
            {
                model,
                temperature = 0,
                messages = new[]
                {
                    new { role = "system",
                          content = "Classify the following text into one of these topics: " + list + ". Return only the topic word." },
                    new { role = "user",
                          content = transcript }
                }
            };

            using JsonDocument json = await PostJsonAsync(body, ct);
            return ExtractContent(json);
        }

        private async Task<JsonDocument> PostJsonAsync(
            object body,
            CancellationToken ct)
        {
            const string url = "https://api.openai.com/v1/chat/completions";

            using var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            using var resp = await _http.PostAsync(url, content, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var detail = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"OpenAI error {(int)resp.StatusCode} {resp.StatusCode}: {detail}");
            }

            return JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        }
        public async Task<IReadOnlyList<string>> RecommendTechLinksAsync(
    string transcript,
    int maxLinks = 2,
    string model = DefaultModel,
    CancellationToken ct = default)
        {
            var body = new
            {
                model,
                temperature = 0,
                messages = new[]
                {
            new {
                role    = "system",
                content = $"You are a knowledgeable assistant. " +
                          $"Return up to {maxLinks} authoritative TECHNICAL URLs " +
                          $"(one per line, no extra text) that help understand or implement " +
                          $"concepts appearing in the provided transcript. Only give the most relevant technical resource if it exists"
            },
            new { role = "user", content = transcript }
        }
            };

            using JsonDocument json = await PostJsonAsync(body, ct);
            string raw = ExtractContent(json);

            var links = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                           .Select(l => l.Trim())
                           .Where(l => l.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                           .Distinct()
                           .Take(maxLinks)
                           .ToArray();

            return links;
        }
        private static string ExtractContent(JsonDocument json) =>
            json.RootElement.GetProperty("choices")[0]
                .GetProperty("message").GetProperty("content")
                .GetString()!.Trim();

        public void Dispose()
        {
            if (_disposeClient)
                _http.Dispose();
        }
    }
}