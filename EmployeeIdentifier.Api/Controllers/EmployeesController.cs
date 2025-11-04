using EmployeeIdentifier.Api.Models;
using EmployeeIdentifier.Api.RequestHandlers.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeIdentifier.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeesController : ControllerBase
    {
        [HttpGet]
        [Route("/", Name = "HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok("Employee Identifier API is running.");
        }


        /// <summary>
        /// Analyzes employee project data file and returns the pair of employees who worked together. 
        /// </summary>
        /// <param name="file">CSV file with columns: EmpID, ProjectID, DateFrom, DateTo</param>
        /// <param name="handler">The request handler service</param>
        /// <returns>The employee pair with the longest collaboration</returns>
        [HttpPost]
        [Route(Routes.GET_EMPLOYEES, Name = nameof(Routes.GET_EMPLOYEES))]
        [ProducesResponseType(typeof(EmployeeCollaborationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEmployees(IFormFile file, [FromServices] IGetEmployeeCollaborationRequestHandler handler)
        {
            var response = await handler.HandleAsync(file);

            if (!response.Success)
            {
                return BadRequest(response.Error);
            }

            return Ok(response.Result);
        }
    }
}
