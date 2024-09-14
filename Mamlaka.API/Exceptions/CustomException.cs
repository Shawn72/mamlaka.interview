using System.Net;
using Mamlaka.API.Extensions;

namespace Mamlaka.API.Exceptions;
public class CustomException : Exception
{
    public string ErrorCode { get; set; } = null!;
    public HttpStatusCode StatusCode { get; set; }
    public string? UserMessage { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow.ToEastAfricanTime();

    public CustomException() : base() { }

    //use 3 constructor classes: coding standard
    public CustomException(string friendlyMessage, string errorCode, HttpStatusCode statusCode = HttpStatusCode.OK) : base(friendlyMessage)
    {
        UserMessage = friendlyMessage;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public CustomException(string technicalMessage, string friendlyMessage, string errorCode, HttpStatusCode statusCode = HttpStatusCode.OK) : base(technicalMessage)
    {
        UserMessage = friendlyMessage;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public CustomException(Exception exception, string friendlyMessage, string errorCode, HttpStatusCode statusCode = HttpStatusCode.OK) : base(friendlyMessage, exception)
    {
        UserMessage = friendlyMessage;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
