﻿// See https://aka.ms/new-console-template for more information

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

var ai = new GoogleGemini(new(), apiKey, "The response must be attractive and interesting to the reader");
var p = new Politicality(
    new(username, password, username), ai);

Console.WriteLine("Retrieving issues");
var issues = p.GetIssues();
var issue = issues[0];
Console.WriteLine("Reasoning with AI and answering the nation");
var answer = p.GetAiAnswer(issue, 0.825, 4, 0.9, 2000);
p.AnswerIssue(answer);
Console.WriteLine(answer.Reason);
p.WriteInFactBook($"Issue {issue.ID}: {issue.Title}", answer.Reason);
Console.WriteLine("Wrote in fact book");
Console.WriteLine($"{issues.Length - 1} issues left");