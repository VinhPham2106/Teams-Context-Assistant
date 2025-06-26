using System;

namespace BotIntelligence
{
    /// <summary>Central list of topics the OpenAI model can classify.</summary>
    public static class MeetingTopics
    {
        public static readonly string[] List =
        {
            "Status Updates",      
            "Blockers & Risks",
            "Action Items & Next Steps",
            "Dependencies",
            "Announcements",

            "Security & Identity",          
            "DevOps & Infrastructure",       
            "Observability & Monitoring",  
            "Quality & Testing",             

       
            "Metrics & KPIs",               
            "Business Impact & ROI",
            "People & Onboarding",          
            "Planning & Roadmap"        
        };
    }
}