namespace PoliticalityApi;

public class NationStatesConfiguration
{
    public string Username { get; }
    public string Password { get; }
    public string UserAgent { get; }

    public NationStatesConfiguration(string username, string password, string userAgent)
    {
        Username = username;
        Password = password;
        UserAgent = userAgent;
    }
}