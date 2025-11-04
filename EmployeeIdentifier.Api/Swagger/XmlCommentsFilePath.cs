using EmployeeIdentifier.Api.Models;
using System.Reflection;

namespace EmployeeIdentifier.Api.Swagger
{
    public static class XmlCommentsFilePath
    {
        public static string ControllerComments
        {
            get
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }

        public static string ApiModelsComments
        {
            get
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var fileName = typeof(ErrorModel).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }
    }
}
