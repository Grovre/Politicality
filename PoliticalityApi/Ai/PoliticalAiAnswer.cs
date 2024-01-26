using dotNS.Classes;

namespace PoliticalityApi.Ai;

public record PoliticalAiAnswer(IssueOption SelectedOption, string Reason)
{
}