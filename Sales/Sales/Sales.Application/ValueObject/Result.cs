namespace Sales.Application.ValueObject;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public T Value { get; set; }
    
    public Result(bool isSuccess, string message, T value)
    {
        IsSuccess = isSuccess;
        Message = message;
        Value = value;
    }
    
    public static Result<T> Success(T value) => new Result<T>(true, string.Empty, value);
    public static Result<T> Fail(string message) => new Result<T>(false, message, default);
}