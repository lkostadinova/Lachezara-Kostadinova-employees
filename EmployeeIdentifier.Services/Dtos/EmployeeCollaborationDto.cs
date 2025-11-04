using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeIdentifier.Services.Dtos
{
    public class EmployeeCollaborationDto
    {
        public int EmployeeFirstId { get; set; }

        public int EmployeeSecondId { get; set; }

        public int DaysWorkedTogether { get; set; }

        public List<ProjectCollaborationDetailDto> ProjectDetails { get; set; }
    }
}
