using System;
using System.Collections.Generic;

namespace BotIntelligence
{
	public static class Glossary
	{
		public static readonly IReadOnlyDictionary<string, string> Items =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
			
				["CSEO"] = "Microsoft Customer Security & Enterprise Organization (internal security engineering org)",
				["MTP"] = "Microsoft Threat Protection – unified XDR suite (precursor branding to Microsoft Defender XDR)",
				["MDE"] = "Microsoft Defender for Endpoint – EDR service for Windows, macOS, Linux, iOS & Android",
				["MDC"] = "Microsoft Defender for Cloud – CNAPP offering for Azure, AWS, and GCP",
				["KQL"] = "Kusto Query Language – syntax used in Azure Data Explorer, Log Analytics, and Sentinel",
				["PIM"] = "Privileged Identity Management – just-in-time / just-enough Azure AD role elevation",
				["CAEP"] = "Continuous Access Evaluation Protocol – near real-time Conditional Access enforcement",
				["JEA"] = "Just Enough Administration – PowerShell role-based access pattern limiting cmdlet exposure",
				["COB"] = "Close of Business – end of the working day (often 5 p.m. local)",
				["EOD"] = "End of Day – similar to COB but can imply later evening cut-off in DevOps teams",
				["PR"] = "Pull Request",
				["LLM"] = "Large Language Model",
				["COT"] = "Chain of Thought",
				["RAG"] = "Retrieval-Augmented Generation"
			};
	}
}