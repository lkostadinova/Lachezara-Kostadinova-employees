using EmployeeIdentifier.Api.Models;
using EmployeeIdentifier.Api.Models.Enums;

namespace EmployeeIdentifier.Api.RequestHandlers.Responses
{
    public abstract class BaseResponse
    {
        public bool Success { get; set; }

        public ErrorResponseModel Error { get; }

        public ErrorCodes ErrorCode
        {
            get
            {
                return Error.Error.Code;
            }
        }

        protected BaseResponse()
        {
            Success = true;
        }

        protected BaseResponse(ErrorModel error)
        {
            Success = false;
            Error = new ErrorResponseModel(error);
        }
    }
}
