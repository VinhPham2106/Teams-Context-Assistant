using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetEnv;
using BotIntelligence;

internal static class Program
{
    private static async Task Main()
    {
        Env.Load();

        const string transcript = @"
During today's meeting we reviewed the SLA for our cloud offering,
looked at the latest KPI dashboard and predicted ROI.  The HR team also
reminded everyone about pending GDPR training.";

        var glossary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SLA"] = "Service Level Agreement",
            ["KPI"] = "Key Performance Indicator",
            ["ROI"] = "Return On Investment",
            ["HR"] = "Human Resources"
        };

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    ?? throw new InvalidOperationException("OPENAI_API_KEY not found. Check your .env file.");

        var openAi = new OpenAiService(apiKey);
        var regex = new Regex(@"\b[A-Z]{2,5}\b");
        var defs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cameFromGlossary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match m in regex.Matches(transcript))
        {
            var term = m.Value;
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
        {
            defs[term] = await openAi.EnhanceAsync(term, defs[term]);
        }
  
        var prettyMessage = await openAi.BeautifyAsync(defs);

        Console.WriteLine("\n—- Message sent to Teams —-\n");
        Console.WriteLine(prettyMessage);
    }
}