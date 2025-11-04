namespace EmployeeIdentifier.Api.Models
{
    public class ProjectCollaborationDetail
    {
        public int ProjectId { get; set; }

        public int DaysWorkedTogether { get; set; }

        public string OverlapStart { get; set; }

        public string OverlapEnd { get; set; }
    }
}
