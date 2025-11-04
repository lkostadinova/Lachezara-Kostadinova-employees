using EmployeeIdentifier.Services.Dtos;
using EmployeeIdentifier.Services.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace EmployeeIdentifier.Services.Services
{
    /// <summary>
    /// Service for analyzing employee collaborations from CSV data
    /// </summary>
    public class EmployeeCollaborationService : IEmployeeCollaborationService
    {
        private readonly ILogger<EmployeeCollaborationService> _logger;

        public EmployeeCollaborationService(ILogger<EmployeeCollaborationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Analyzes CSV data to find the pair of employees who worked together the longest
        /// </summary>
        public async Task<EmployeeCollaborationDto?> AnalyzeCollaborationsAsync(Stream csvStream)
        {
            try
            {
                var records = await ParseCsvAsync(csvStream);

                if (records.Count == 0)
                {
                    _logger.LogWarning("No valid records found in CSV");
                    return null;
                }

                return FindLongestCollaboration(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing collaborations");
                throw;
            }
        }

        /// <summary>
        /// Parses CSV stream into employee project records
        /// </summary>
        private async Task<List<EmployeeProjectRecordDto>> ParseCsvAsync(Stream csvStream)
        {
            var records = new List<EmployeeProjectRecordDto>();

            using (var reader = new StreamReader(csvStream))
            {
                string? line;
                int lineNumber = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var parts = line.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length < 4)
                        {
                            _logger.LogWarning("Line {LineNumber} has insufficient columns: {Line}", lineNumber, line);
                            continue;
                        }

                        if (!int.TryParse(parts[0], out int empId))
                        {
                            _logger.LogWarning("Line {LineNumber} has invalid EmpID: {Line}", lineNumber, line);
                            continue;
                        }

                        if (!int.TryParse(parts[1], out int projectId))
                        {
                            _logger.LogWarning("Line {LineNumber} has invalid ProjectID: {Line}", lineNumber, line);
                            continue;
                        }

                        if (!DateTime.TryParse(parts[2], out DateTime dateFrom))
                        {
                            _logger.LogWarning("Line {LineNumber} has invalid DateFrom: {Line}", lineNumber, line);
                            continue;
                        }

                        DateTime? dateTo = null;
                        if (!string.IsNullOrEmpty(parts[3]) &&
                    !parts[3].Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            if (DateTime.TryParse(parts[3], out DateTime parsedDateTo))
                            {
                                dateTo = parsedDateTo;
                            }
                            else
                            {
                                _logger.LogWarning("Line {LineNumber} has invalid DateTo: {Line}", lineNumber, line);
                                continue;
                            }
                        }

                        records.Add(new EmployeeProjectRecordDto
                        {
                            EmpId = empId,
                            ProjectId = projectId,
                            DateFrom = dateFrom,
                            DateTo = dateTo
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing line {LineNumber}: {Line}", lineNumber, line);
                    }
                }
            }

            return records;
        }

        /// <summary>
        /// Finds the pair of employees with the longest collaboration across all projects
        /// </summary>
        private EmployeeCollaborationDto? FindLongestCollaboration(List<EmployeeProjectRecordDto> records)
        {
            // Group records by project
            var projectGroups = records.GroupBy(r => r.ProjectId).ToList();

            // Dictionary to store total days worked together for each employee pair
            var collaborations = new Dictionary<(int, int), List<ProjectCollaborationDetailDto>>();

            // For each project, find overlapping periods between employee pairs
            foreach (var projectGroup in projectGroups)
            {
                var projectRecords = projectGroup.ToList();
                var projectId = projectGroup.Key;

                // Compare each pair of employees on this project
                for (int i = 0; i < projectRecords.Count; i++)
                {
                    for (int j = i + 1; j < projectRecords.Count; j++)
                    {
                        var emp1Record = projectRecords[i];
                        var emp2Record = projectRecords[j];

                        // Calculate overlap period
                        var overlap = CalculateOverlap(emp1Record, emp2Record);

                        if (overlap.HasValue && overlap.Value.days > 0)
                        {
                            // Create a normalized pair key (smaller ID first)
                            var pairKey = emp1Record.EmpId < emp2Record.EmpId
                                        ? (emp1Record.EmpId, emp2Record.EmpId)
                                        : (emp2Record.EmpId, emp1Record.EmpId);

                            if (!collaborations.ContainsKey(pairKey))
                            {
                                collaborations[pairKey] = new List<ProjectCollaborationDetailDto>();
                            }

                            collaborations[pairKey].Add(new ProjectCollaborationDetailDto
                            {
                                ProjectId = projectId,
                                DaysWorkedTogether = overlap.Value.days,
                                OverlapStart = overlap.Value.start,
                                OverlapEnd = overlap.Value.end
                            });
                        }
                    }
                }
            }

            // Find the pair with the maximum total days
            if (collaborations.Count == 0)
            {
                _logger.LogInformation("No collaborations found");
                return null;
            }

            var longestCollaboration = collaborations
                .Select(kvp => new
                {
                    Pair = kvp.Key,
                    Details = kvp.Value,
                    TotalDays = kvp.Value.Sum(d => d.DaysWorkedTogether)
                })
      .OrderByDescending(x => x.TotalDays)
     .First();

            _logger.LogInformation(
           "Found longest collaboration: Employees {Emp1} and {Emp2} worked together for {Days} days",
            longestCollaboration.Pair.Item1,
            longestCollaboration.Pair.Item2,
            longestCollaboration.TotalDays);

            return new EmployeeCollaborationDto
            {
                EmployeeFirstId = longestCollaboration.Pair.Item1,
                EmployeeSecondId = longestCollaboration.Pair.Item2,
                DaysWorkedTogether = longestCollaboration.TotalDays,
                ProjectDetails = longestCollaboration.Details
            };
        }

        /// <summary>
        /// Calculates the overlap period between two employee records
        /// </summary>
        private (DateTime start, DateTime end, int days)? CalculateOverlap(
                                                          EmployeeProjectRecordDto record1,
                                                          EmployeeProjectRecordDto record2)
        {
            // Use today's date if DateTo is null
            var end1 = record1.DateTo ?? DateTime.Today;
            var end2 = record2.DateTo ?? DateTime.Today;

            // Find overlap start (later of the two start dates)
            var overlapStart = record1.DateFrom > record2.DateFrom
                             ? record1.DateFrom
                             : record2.DateFrom;

            // Find overlap end (earlier of the two end dates)
            var overlapEnd = end1 < end2 ? end1 : end2;

            // Check if there's an actual overlap
            if (overlapStart > overlapEnd)
            {
                return null;
            }

            // Calculate days
            var days = (int)(overlapEnd - overlapStart).TotalDays + 1;

            return (overlapStart, overlapEnd, days);
        }
    }
}
