// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using dotNS;
using PoliticalityApi;
using PoliticalityApi.Ai;

Console.WriteLine("Should load? (1/0):");
var shouldLoad = Console.ReadLine()!.Contains('1');

string apiKey;
string username;
string password;
using var fs = File.Open("vars.json", FileMode.OpenOrCreate);

if (shouldLoad)
{
    var jsonElement = JsonSerializer.Deserialize<JsonElement>(fs);
    apiKey = jsonElement.GetProperty(nameof(apiKey)).GetString() ?? "";
    username = jsonElement.GetProperty(nameof(username)).GetString() ?? "";
    password = jsonElement.GetProperty(nameof(password)).GetString() ?? "";
}
else
{
    Console.WriteLine("Api key:");
    apiKey = Console.ReadLine()!;
    Console.WriteLine("Username:");
    username = Console.ReadLine()!;
    Console.WriteLine("Password:");
    password = Console.ReadLine()!;

    JsonSerializer.Serialize(fs, new
    {
        apiKey,
        username,
        password
    });
}

Trace.Assert(!Array.TrueForAll([apiKey, username, password], string.IsNullOrWhiteSpace));

var ai = new GoogleGemini(new(), apiKey);
var p = new Politicality(
    new(username, password, username),
    ai);

Console.WriteLine("Retrieving issues");
var issue = p.GetIssues()[0];
Console.WriteLine("Reasoning with AI and answering the nation");
var reason = p.AnswerIssue(issue, 0.825, 4, 0.9, 2000);
Console.WriteLine(reason);
p.WriteInFactBook($"Issue {issue.ID}: {issue.Title}", reason);
Console.WriteLine("Wrote in fact book");