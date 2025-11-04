using EmployeeIdentifier.Api.Models;
using EmployeeIdentifier.Api.Models.Enums;
using EmployeeIdentifier.Api.Shared.ExceptionHandling;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EmployeeIdentifier.Api.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly JsonSerializerSettings _jsonSerializerSetting;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        public ErrorHandlerMiddleware(RequestDelegate next, IOptions<ErrorHandlingSettings> settings, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            if (settings.Value.ShowException)
            {
                jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
            else
            {
                var ignores = new Dictionary<Type, HashSet<string>>();
                var errorModelHashSet = new HashSet<string>
                {
                    nameof(ErrorModel.Details).ToLower()
                };
                ignores.Add(typeof(ErrorModel), errorModelHashSet);

                jsonSerializerSettings.ContractResolver = new PropertyIgnoreSerializerContractResolver(ignores);
            }

            _jsonSerializerSetting = jsonSerializerSettings;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                var errorModel = new ErrorModel(ErrorCodes.VALIDATION_ERROR, ex.Message, ex);
                await ReturnErrorResponse(ex, context, HttpStatusCode.BadRequest, errorModel);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorModel(ErrorCodes.UNKNOWN, "Unhandled exception", ex);
                await ReturnErrorResponse(ex, context, HttpStatusCode.InternalServerError, errorModel);
            }
        }

        private async Task ReturnErrorResponse(Exception ex, HttpContext context, HttpStatusCode statusCode, ErrorModel errorModel = null)
        {
            ex = GetInnerException(ex);

            var errorMessage = ex.Message;
            if (context.Request.RouteValues["controller"] != null && context.Request?.RouteValues["action"] != null)
            {
                errorMessage = $"Action {context.Request.RouteValues["action"]} on controller {context.Request.RouteValues["controller"]} failed with: {ex.Message}";
            }

            _logger.LogError(ex, errorMessage);

            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;

            if (errorModel != null)
            {
                var result = JsonConvert.SerializeObject(new ErrorResponseModel(errorModel), _jsonSerializerSetting);
                await response.WriteAsync(result);
            }
        }
        private static Exception GetInnerException(Exception ex)
        {
            var inner = ex;
            while (inner?.InnerException != null)
            {
                inner = inner.InnerException;
            }
            return inner;
        }
    }
}
