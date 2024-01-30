using System.Runtime.Serialization;

namespace PoliticalityApi.Exceptions;

public class PromptBlockedException : Exception
{
    public PromptBlockedException()
    {
    }

    public PromptBlockedException(string? message) : base(message)
    {
    }

    public PromptBlockedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}