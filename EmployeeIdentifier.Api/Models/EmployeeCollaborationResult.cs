namespace EmployeeIdentifier.Api.Models
{
    /// <summary>
    /// Represents the result of the longest collaboration analysis
    /// </summary>
    public class EmployeeCollaborationResult
    {
        public int EmployeeFirstId { get; set; }

        public int EmployeeSecondId { get; set; }

        public int DaysWorkedTogether { get; set; }

        public List<ProjectCollaborationDetail> ProjectDetails { get; set; }
    }
}
