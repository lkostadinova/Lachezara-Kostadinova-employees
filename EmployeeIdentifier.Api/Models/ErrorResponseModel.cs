namespace EmployeeIdentifier.Api.Models
{
    public class ErrorResponseModel
    {
        public ErrorModel Error { get; set; }

        public ErrorResponseModel()
        {
        }

        public ErrorResponseModel(ErrorModel error)
        {
            Error = error;
        }
    }
}
