using System;

namespace BotIntelligence
{
    /// <summary>Central list of topics the OpenAI model can classify.</summary>
    public static class MeetingTopics
    {
        public static readonly string[] List =
        {
            "Sprint Status",
            "Action Items",
            "Blockers",
            "ROI Planning",
            "Grafana Dashboard",
            "Traefik Ingress",
            "CI/CD Pipeline",
            "Code Review / PRs",
            "Intern Onboarding",
            "Project Timeline",
            "Product Roadmap"
        };
    }
}