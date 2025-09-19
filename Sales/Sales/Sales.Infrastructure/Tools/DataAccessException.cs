namespace Sales.Infrastructure.Tools;

public class DataAccessException(string message, Exception innerException) : Exception(message, innerException);