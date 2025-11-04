using EmployeeIdentifier.Services.Dtos;

namespace EmployeeIdentifier.Services.Services.Abstract
{
    /// <summary>
  /// Service interface for analyzing employee collaborations
    /// </summary>
    public interface IEmployeeCollaborationService
    {
        Task<EmployeeCollaborationDto?> AnalyzeCollaborationsAsync(Stream csvStream);
    }
}
