namespace TransportTrack.Infrastructure.Exceptions;

public class TrackingException : Exception
{
    public int StatusCode { get; }

    public TrackingException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class TrackingValidationException : TrackingException
{
    public TrackingValidationException(string message) : base(message, 400)
    {
    }
}

public class TrackingUnauthorizedException : TrackingException
{
    public TrackingUnauthorizedException(string message) : base(message, 401)
    {
    }
}

public class TrackingForbiddenException : TrackingException
{
    public TrackingForbiddenException(string message) : base(message, 403)
    {
    }
}

public class TrackingNotFoundException : TrackingException
{
    public TrackingNotFoundException(string message) : base(message, 404)
    {
    }
}