using System.Net.Http.Json;
using System.Text.Json;
using dotNS.Classes;

namespace PoliticalityApi.Ai;

public class GoogleGemini : PoliticalAi
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    public List<string> PromptAdditions { get; }

    public GoogleGemini(HttpClient httpClient, string apiKey, params string[]? promptAdditions)
    {
        _apiKey = apiKey;
        _httpClient = httpClient;
        PromptAdditions = promptAdditions?.Select(s => s.Trim()).ToList() ?? [];
    }

    public GoogleGemini(HttpClient httpClient, string apiKey, IEnumerable<string> promptAdditions) : this(httpClient, apiKey)
    {
        PromptAdditions = promptAdditions.Select(s => s.Trim()).ToList();
    }

    private HttpRequestMessage CreateRequest(Issue issue, NationContext? context, double temperature, int topK, double topP,
        int maxOutputTokens)
    {
        var uri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
        var req = new HttpRequestMessage(HttpMethod.Post, uri);
        var text = $"There is an issue in your nation{(context == null ? ". " : $", {context.FullName}")}. " +
                   "You are the president of your nation. " +
                   "You need to select one of the options to solve the problem. " +
                   "You may solve it however you want. " +
                   "You must do whatever you can to make the nation a superpower. " +
                   (context == null
                       ? ""
                       : "Context for the current state of the nation will be given. " +
                         "You may use the context to base your decisions off of. ") +
                   "Select the ID of an option. " +
                   "Start by explaining what the issue is. " +
                   "Explain what each option was, including the one you chose. " +
                   "Explain last in a separate paragraph why you chose the option you did. " +
                   "Make the choice a little absurd and silly. " +
                   "Do not mention America. " + // lol issue 236 the AI thought it was America
                   "You cannot use hyphens (-). " +
                   string.Join(". ", PromptAdditions) + ". " +
                   "Begin the speech with \"[ID] My fellow \"\n\n" +
                   (context == null
                       ? ""
                       : $"CONTEXT:\n{JsonSerializer.Serialize(context)}\n\n") +
                   $"ISSUE:\n{issue.FormatIssueIntoPrompt()}\n" +
                   "\n----------------------------------";
        
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
                            text
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature,
                topK,
                topP,
                maxOutputTokens,
                stopSequences = Array.Empty<object>()
            },
            safetySettings = new[]
            {
                new
                {
                    category = "HARM_CATEGORY_HARASSMENT",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_HATE_SPEECH",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                    threshold = "BLOCK_NONE"
                }
            }
        });

        return req;
    }

    public override PoliticalAiAnswer GetIssueAnswer(Issue issue, NationContext? context, double temperature, int topK, double topP,
        int maxOutputTokens)
    {
        var req = CreateRequest(issue, context, temperature, topK, topP, maxOutputTokens);
        var resp = _httpClient.Send(req);
        var content = resp.Content.ReadAsStringAsync().Result;
        resp.EnsureSuccessStatusCode();
        
        var reason = JsonSerializer.Deserialize<JsonElement>(content)
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()?
            .Trim() ?? string.Empty;

        var selectedOptionId = reason.First(char.IsNumber) - '0';
        var selectedOption = issue.Options.First(o => o.ID == selectedOptionId);

        return new PoliticalAiAnswer(issue, selectedOption, reason);
    }
}