using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeIdentifier.Services.Dtos
{
    public class ProjectCollaborationDetailDto
    {
        public int ProjectId { get; set; }

        public int DaysWorkedTogether { get; set; }

        public DateTime OverlapStart { get; set; }

        public DateTime OverlapEnd { get; set; }
    }
}
