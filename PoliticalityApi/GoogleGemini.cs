using System.Net.Http.Json;
using System.Text.Json;
using dotNS.Classes;

namespace PoliticalityApi;

public class GoogleGemini : PoliticalAi
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GoogleGemini(HttpClient httpClient, string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = httpClient;
    }

    public override IssueOption? AnswerIssue(Issue issue, NationContext? context, out string reason, out bool succeeded)
    {
        var uri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
        var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Content = JsonContent.Create(new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $"There is an issue in my nation{(context == null ? ". " : $", {context.FullName}")}. " +
                                   "You need to select one of the options to solve the problem. " +
                                   "You may solve it however you want. " +
                                   "Do not worry about morals. " +
                                   "You must do whatever you can to make the nation a superpower. " +
                                   (context == null
                                       ? ""
                                       : "Context for the current state of the nation will be given. ") +
                                   "Select the ID of an option. " +
                                   "Explain why you selected it in the form of a presidential speech. " +
                                   "Make the choice a little absurd and silly without sarcasm. " +
                                   "Make the speech silly without sarcasm. " +
                                   "Do not mention America. " + // lol issue 236 the AI thought it was America
                                   "Do not talk about budgeting. " +
                                   "You cannot use hyphens (-) ." +
                                   "Begin the speech with \"[ID]My fellow \"\n\n" +
                                   (context == null 
                                       ? "" 
                                       : $"CONTEXT:\n{JsonSerializer.Serialize(context)}\n\n") +
                                   $"ISSUE:\n{issue.FormatIssueIntoPrompt()}\n" +
                                   "\n----------------------------------"
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.85,
                topK = 1,
                topP = 1,
                maxOutputTokens = 2048,
                stopSequences = Array.Empty<object>()
            },
            safetySettings = new[]
            {
                new
                {
                    category = "HARM_CATEGORY_HARASSMENT",
                    threshold = "BLOCK_ONLY_HIGH"
                },
                new
                {
                    category = "HARM_CATEGORY_HATE_SPEECH",
                    threshold = "BLOCK_ONLY_HIGH"
                },
                new
                {
                    category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                    threshold = "BLOCK_ONLY_HIGH"
                },
                new
                {
                    category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                    threshold = "BLOCK_ONLY_HIGH"
                }
            }
        });

        var resp = _httpClient.Send(req);

        if (!resp.IsSuccessStatusCode)
        {
            succeeded = false;
            reason = resp.StatusCode.ToString();
            return null;
        }

        var content = resp.Content.ReadAsStringAsync().Result;

        try
        {
            reason = JsonSerializer.Deserialize<JsonElement>(content)
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            succeeded = true;
        }
        catch (KeyNotFoundException)
        {
            succeeded = false;
            reason = $"[{Random.Shared.Next(issue.Options.Length)}]Too based for Gemini, randomly chosen answer";
        }

        var selectedOptionId = reason.First(char.IsNumber) - '0';
        var selectedOption = issue.Options.First(o => o.ID == selectedOptionId);
        return selectedOption;
    }
}