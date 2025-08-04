namespace LunkvayAPI.src.Models.Utils
{
    public record class ServiceResult<T>(bool IsSuccess, T? Result, string? Error, int StatusCode = 400)
    {
        public bool IsSuccess { get; } = IsSuccess;
        public T? Result { get; } = Result;
        public string? Error { get; } = Error;
        public int StatusCode { get; } = StatusCode;

        public static ServiceResult<T> Success(T result) 
            => new(true, result, null, 200);

        public static ServiceResult<T> Failure(string error, int statusCode = 400) 
            => new(false, default, error, statusCode);
    }
}
