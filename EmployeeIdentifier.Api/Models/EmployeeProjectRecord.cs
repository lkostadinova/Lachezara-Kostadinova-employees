namespace EmployeeIdentifier.Api.Models
{
    /// <summary>
    /// Represents an employee's work period on a project
    /// </summary>
    public class EmployeeProjectRecord
    {
        public int EmpId { get; set; }

        public int ProjectId { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
