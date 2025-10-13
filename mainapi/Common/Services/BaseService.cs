using LunkvayAPI.Common.Results;

namespace LunkvayAPI.Common.Services
{
    public abstract class BaseService
    {
        protected ServiceResult<T>? ValidateId<T>(Guid id, string idName = "Id")
        {
            if (id == Guid.Empty)
                return ServiceResult<T>.Failure($"{idName} не может быть пустым");

            return null; // null означает - ошибок нет
        }

        protected ServiceResult<T>? ValidateNotNull<T>(object obj, string paramName)
        {
            if (obj is null)
                return ServiceResult<T>.Failure($"{paramName} не может быть null");

            return null;
        }
    }
}
