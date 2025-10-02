using System.Net;

namespace LunkvayFriendsService.Models.Utils
{
    public record class ServiceResult<T>(bool IsSuccess, T? Result, string? Error, HttpStatusCode StatusCode = HttpStatusCode.BadRequest)
    {
        public bool IsSuccess { get; } = IsSuccess;
        public T? Result { get; } = Result;
        public string? Error { get; } = Error;
        public HttpStatusCode StatusCode { get; } = StatusCode;

        public static ServiceResult<T> Success(T result)
            => new (true, result, null, HttpStatusCode.OK);

        public static ServiceResult<T> Failure(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new(false, default, error, statusCode);
    }
}
