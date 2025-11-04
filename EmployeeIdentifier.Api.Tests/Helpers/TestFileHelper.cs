using System.Text;

namespace EmployeeIdentifier.Api.Tests.Helpers
{
    public static class TestFileHelper
    {
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

        public static string ValidCsvContent => @"EmpID,ProjectID,DateFrom,DateTo
                                                143,12,2013-11-01,2014-01-05
                                                218,10,2012-05-16,NULL
                                                143,10,2009-01-01,2011-04-27
                                                218,12,2013-11-01,2014-01-05";
    }
}