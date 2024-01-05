using System.Text;
using System.Text.Json;
using dotNS.Classes;

namespace PoliticalityApi;

public static class IssueExtensions
{
    public static string FormatIssueIntoPromptJson(this Issue issue)
    {
        var preserializerOptions = issue.Options
            .Select(o => new { o.ID, OptionText = o.Text });
        var preserializerIssue = new { issue.Title, issue.Text, Options = preserializerOptions };
        var json = JsonSerializer.Serialize(preserializerIssue,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        return json;
    }
    
    public static string FormatIssueIntoPrompt(this Issue issue)
    {
        var sb = new StringBuilder();
        sb.Append("Title: ").AppendLine(issue.Title)
            .Append("Description: ").AppendLine(issue.Text)
            .AppendLine()
            .Append("Options:");

        foreach (var o in issue.Options)
        {
            sb.AppendLine()
                .Append("Option ").Append(o.ID).AppendLine(":")
                .AppendLine(o.Text);
        }

        return sb.ToString();
    }
}