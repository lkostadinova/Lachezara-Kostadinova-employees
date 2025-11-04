using EmployeeIdentifier.Api.Models;

namespace EmployeeIdentifier.Api.RequestHandlers.Responses
{
    public class GetEmployeeCollaborationResponse : BaseResponse
    {
        public EmployeeCollaborationResult Result { get; set; }

        public GetEmployeeCollaborationResponse(EmployeeCollaborationResult result)
        {
            Result = result;
            Success = true;
        }

        public GetEmployeeCollaborationResponse(ErrorModel error) : base(error)
        {
        }
    }
}
