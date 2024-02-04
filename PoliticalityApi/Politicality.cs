using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using System.Xml;
using dotNS;
using dotNS.Classes;
using PoliticalityApi.Ai;
using PoliticalityApi.Exceptions;

namespace PoliticalityApi;

public class Politicality
{
    private readonly NationStatesConfiguration _nsConfig;
    private readonly PoliticalAi _ai;
    public DotNS Api { get; }

    public Politicality(NationStatesConfiguration nsConfig, PoliticalAi aiConfig)
    {
        _nsConfig = nsConfig;
        _ai = aiConfig;
        
        Api = new(_nsConfig.Username, _nsConfig.Password, _nsConfig.UserAgent)
        {
            RateLimit = false
        };
    }

    public Issue[] GetIssues()
    {
        var issues = Api.GetPrivateNation().Issues;
        
        if (issues == null)
            throw new NoIssuesException("dotNS returned null issue array");

        return issues;
    }

    public PoliticalAiAnswer GetAiAnswer(Issue issue, double temperature, int topK, double topP, int maxOutputTokens)
    {
        var context = new NationContext(Api.GetNationInfo(_nsConfig.Username));
        var answer = _ai.GetIssueAnswer(issue, context, temperature, topK, topP, maxOutputTokens);
        return answer;
    }

    public bool AnswerIssue(PoliticalAiAnswer answer)
    {
        // initialized local for debugging purposes
        var nodeList = Api.AddressIssue(answer.Issue, answer.SelectedOption);
        return true;
    }

    private static readonly HttpClient _httpClient = new();
    public void WriteInFactBook(string title, string text)
    {
        title = HttpUtility.UrlEncode(title);
        text = HttpUtility.UrlEncode(text);
        
        var req = new HttpRequestMessage(HttpMethod.Get, $"https://www.nationstates.net/cgi-bin/api.cgi?nation={_nsConfig.Username}&c=dispatch&dispatch=add&title={title}&text={text}&category=5&subcategory=545&mode=prepare");
        if (Api.Pin == "-1")
            Api.UpdatePin(_nsConfig.Username, _nsConfig.Password);
        req.Headers.Add("X-Pin", Api.Pin);
        req.Headers.UserAgent.Add(new(_nsConfig.Username, "v1"));

        var resp = _httpClient.Send(req);
        var content = resp.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(content);
        var token = xmlDoc.GetElementsByTagName("SUCCESS")[0]!.InnerText;

        req = new(HttpMethod.Get,
            req.RequestUri!.AbsoluteUri.Replace("mode=prepare", "mode=execute") + $"&token={token}");
        req.Headers.UserAgent.Add(new(_nsConfig.Username, "v1"));
        req.Headers.Add("X-Pin", Api.Pin);
        resp = _httpClient.Send(req);
        // For debugging purposes to view content
        content = resp.Content.ReadAsStringAsync().Result;
        resp.EnsureSuccessStatusCode();

        // "nation=testlandia&c=dispatch&dispatch=add&title=Test%20Dispatch&text=Hello%20there.&category=1&subcategory=105&mode=execute&token=1234567890abcdefg"
    }
}