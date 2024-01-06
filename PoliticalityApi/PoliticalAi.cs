using dotNS.Classes;

namespace PoliticalityApi;

public abstract class PoliticalAi
{
    public abstract IssueOption AnswerIssue(Issue issue, NationContext? context, out string reason, out bool succeeded);
}