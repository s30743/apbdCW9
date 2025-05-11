namespace APBDcw9.Exceptions;

public class NotFoundEx : Exception
{
    public NotFoundEx()
    {
    }

    public NotFoundEx(string? message) : base(message)
    {
    }

    public NotFoundEx(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}