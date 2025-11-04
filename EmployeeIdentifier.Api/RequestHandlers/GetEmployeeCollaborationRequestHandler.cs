using EmployeeIdentifier.Api.Models;
using EmployeeIdentifier.Api.Models.Enums;
using EmployeeIdentifier.Api.RequestHandlers.Abstract;
using EmployeeIdentifier.Api.RequestHandlers.Responses;
using EmployeeIdentifier.Services.Services.Abstract;
using Microsoft.AspNetCore.Http;

namespace EmployeeIdentifier.Api.RequestHandlers
{
    /// <summary>
    /// Request handler for analyzing employee collaboration data from CSV files
    /// </summary>
    public class GetEmployeeCollaborationRequestHandler(
        IEmployeeCollaborationService collaborationService,
        ILogger<GetEmployeeCollaborationRequestHandler> logger) : IGetEmployeeCollaborationRequestHandler
    {
        private readonly IEmployeeCollaborationService _collaborationService = collaborationService;
        private readonly ILogger<GetEmployeeCollaborationRequestHandler> _logger = logger;

        public async Task<GetEmployeeCollaborationResponse> HandleAsync(IFormFile file)
        {
            // Validate file presence
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning(ErrorMessages.NoFileUploaded);
                return new GetEmployeeCollaborationResponse(
                       new ErrorModel(ErrorCodes.VALIDATION_ERROR, ErrorMessages.NoFileUploaded));
            }

            // Validate file extension
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid file type uploaded: {FileName}", file.FileName);
                return new GetEmployeeCollaborationResponse(
                       new ErrorModel(ErrorCodes.VALIDATION_ERROR, ErrorMessages.FileMustBeCsv));
            }

            try
            {
                _logger.LogInformation("Processing employee collaboration file: {FileName}, Size: {Size} bytes",
                    file.FileName, file.Length);

                // Open and process the CSV stream
                using (var stream = file.OpenReadStream())
                {
                    var result = await _collaborationService.AnalyzeCollaborationsAsync(stream);

                    // Check if any collaborations were found
                    if (result == null)
                    {
                        //return validation error if no collaborations found. Can be return empty response based on requirement
                        _logger.LogWarning(ErrorMessages.NoCollaborationsFound);
                        return new GetEmployeeCollaborationResponse(
                               new ErrorModel(ErrorCodes.VALIDATION_ERROR,ErrorMessages.NoCollaborationsFound));
                    }

                    _logger.LogInformation("Successfully found longest collaboration: Employees {Emp1} and {Emp2} worked together for {Days} days",
                                            result.EmployeeFirstId, result.EmployeeSecondId, result.DaysWorkedTogether);

                    var employeeCollaborationResult = new EmployeeCollaborationResult
                    {
                        EmployeeFirstId = result.EmployeeFirstId,
                        EmployeeSecondId = result.EmployeeSecondId,
                        DaysWorkedTogether = result.DaysWorkedTogether,
                        ProjectDetails = result.ProjectDetails.Select(pd => new ProjectCollaborationDetail
                        {
                            ProjectId = pd.ProjectId,
                            DaysWorkedTogether = pd.DaysWorkedTogether,
                            OverlapEnd = pd.OverlapEnd.ToString("yyyy-MM-dd"),
                            OverlapStart = pd.OverlapStart.ToString("yyyy-MM-dd")
                        }).ToList()
                    };

                    return new GetEmployeeCollaborationResponse(employeeCollaborationResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing employee collaboration data from file: {FileName}",
                                 file.FileName);

                return new GetEmployeeCollaborationResponse(
                       new ErrorModel(ErrorCodes.UNKNOWN, ErrorMessages.ErrorProcessingFile, ex));
            }
        }
    }
}
