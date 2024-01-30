using dotNS.Classes;

namespace PoliticalityApi.Ai;

public abstract class PoliticalAi
{
    public abstract PoliticalAiAnswer GetIssueAnswer(Issue issue, NationContext? context, double temperature, int topK, double topP,
        int maxOutputTokens);
}