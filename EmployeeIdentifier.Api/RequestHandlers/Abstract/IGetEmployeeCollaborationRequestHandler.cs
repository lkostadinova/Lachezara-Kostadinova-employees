using EmployeeIdentifier.Api.RequestHandlers.Responses;
using Microsoft.AspNetCore.Http;

namespace EmployeeIdentifier.Api.RequestHandlers.Abstract
{
    public interface IGetEmployeeCollaborationRequestHandler
    {
        Task<GetEmployeeCollaborationResponse> HandleAsync(IFormFile file);
    }
}
