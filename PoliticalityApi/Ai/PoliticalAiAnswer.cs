using dotNS.Classes;

namespace PoliticalityApi.Ai;

public record PoliticalAiAnswer(Issue Issue, IssueOption SelectedOption, string Reason)
{
}