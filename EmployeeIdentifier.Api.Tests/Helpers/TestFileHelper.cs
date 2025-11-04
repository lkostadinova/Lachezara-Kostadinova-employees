using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace EmployeeIdentifier.Api.Tests.Helpers
{
    /// <summary>
    /// Helper class for creating test files
    /// </summary>
    public static class TestFileHelper
    {
        /// <summary>
        /// Creates a mock IFormFile with CSV content
        /// </summary>
        public static IFormFile CreateCsvFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var formFile = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            return formFile;
        }

        /// <summary>
        /// Creates a mock IFormFile with non-CSV content
        /// </summary>
        public static IFormFile CreateNonCsvFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var formFile = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            return formFile;
        }

        /// <summary>
        /// Creates an empty IFormFile
        /// </summary>
        public static IFormFile CreateEmptyFile(string fileName)
        {
            var stream = new MemoryStream();
            var formFile = new FormFile(stream, 0, 0, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            return formFile;
        }

        /// <summary>
        /// Sample CSV content with valid employee collaboration data
        /// </summary>
        public static string ValidCsvContent => @"EmpID,ProjectID,DateFrom,DateTo
                                                143,12,2013-11-01,2014-01-05
                                                218,10,2012-05-16,NULL
                                                143,10,2009-01-01,2011-04-27
                                                218,12,2013-11-01,2014-01-05";

        /// <summary>
        /// Sample CSV content with no overlapping dates
        /// </summary>
        public static string NoCollaborationCsvContent => @"EmpID,ProjectID,DateFrom,DateTo
                                                            143,12,2013-11-01,2014-01-05
                                                            218,10,2015-05-16,2016-06-20";

        /// <summary>
        /// Sample CSV with invalid format
        /// </summary>
        public static string InvalidCsvContent => @"Invalid,CSV,Format
                                                    123,456,789";
    }
}