using System.Runtime.Serialization;

namespace PoliticalityApi.Exceptions;

public class NoIssuesException : Exception
{
    public NoIssuesException()
    {
    }

    public NoIssuesException(string? message) : base(message)
    {
    }

    public NoIssuesException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}