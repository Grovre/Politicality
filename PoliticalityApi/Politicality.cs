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

namespace PoliticalityApi;

public class Politicality
{
    private NationStatesConfiguration _nsConfig;
    private PoliticalAi _ai;
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
            throw new NullReferenceException("dotNS returned null issue array");

        return issues;
    }

    public string AnswerIssue(Issue issue, bool continueWithRandomForBlockedPrompts, out bool succeeded)
    {
        var context = new NationContext(Api.GetNationInfo(_nsConfig.Username));
        var option = _ai.AnswerIssue(issue, context, out var reason, out succeeded);

        if (!succeeded && (option == null || reason.Contains("StatusCode")))
        {
            if (option == null 
                || !continueWithRandomForBlockedPrompts
                || reason.Contains("StatusCode"))
                return "Did not answer any issues.\n" + reason;
        }
        
        var nodeList = Api.AddressIssue(issue, option);
        return reason;
    }

    // TODO: PR DotNS and make command api public
    private static readonly HttpClient _httpClient = new();
    public void WriteInFactBook(string title, string text)
    {
        title = HttpUtility.UrlEncode(title);
        text = HttpUtility.UrlEncode(text);
        
        var req = new HttpRequestMessage(HttpMethod.Get, $"https://www.nationstates.net/cgi-bin/api.cgi?nation={_nsConfig.Username}&c=dispatch&dispatch=add&title={title}&text={text}&category=1&subcategory=105&mode=prepare");
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
        _httpClient.Send(req).EnsureSuccessStatusCode();

        // "nation=testlandia&c=dispatch&dispatch=add&title=Test%20Dispatch&text=Hello%20there.&category=1&subcategory=105&mode=execute&token=1234567890abcdefg"
    }
}