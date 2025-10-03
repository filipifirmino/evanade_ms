namespace APIGateway.ApplicationCore.Exceptions;

public class AuthenticationException : Exception
{
    public AuthenticationException() : base("Erro de autenticação")
    {
    }

    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}