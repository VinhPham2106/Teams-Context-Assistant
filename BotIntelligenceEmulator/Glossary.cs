using System;
using System.Collections.Generic;

namespace BotIntelligence
{
    public static class Glossary
    {
        public static readonly IReadOnlyDictionary<string, string> Items =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SLA"] = "Service Level Agreement",
                ["KPI"] = "Key Performance Indicator",
                ["ROI"] = "Return On Investment",
                ["HR"]  = "Human Resources",
                ["PR"]  = "Pull Request"
            };
    }
}