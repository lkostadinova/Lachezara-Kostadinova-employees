using EmployeeIdentifier.Services.Dtos;

namespace EmployeeIdentifier.Services.Services.Abstract
{
    public interface IEmployeeCollaborationService
    {
        Task<EmployeeCollaborationDto?> AnalyzeCollaborationsAsync(Stream csvStream);
    }
}
