using System;

public class SuggestionPayload
{
    // Everything in markdown right now, can extend to more fields
    public required string SuggestionMarkdown { get; set; }
}
