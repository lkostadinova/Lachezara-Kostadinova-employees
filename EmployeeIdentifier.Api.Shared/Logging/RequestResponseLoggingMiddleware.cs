using EmployeeIdentifier.Logging.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace EmployeeIdentifier.Api.Shared.Logging
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiLoggingSettings _settings;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly List<string> _sensitiveDataJsonPaths = [];

        public RequestResponseLoggingMiddleware(RequestDelegate next, ApiLoggingSettings settings, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _settings = settings;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            LoggerThreadContext.Properties["trackingId"] = Guid.NewGuid().ToString();

            //if setting is set to false => continue without logging
            if (!_settings.LogRequestsAndResponses)
            {
                await _next(context);
                return;
            }

            //First, get the incoming request
            var formattedRequestBody = await FormatRequestBody(context.Request);
            AddMetadataToLogs(context.Request, formattedRequestBody);
            _logger.LogInformation($"HTTP request received: {context.Request.Method} {context.Request.GetEncodedPathAndQuery()}; " +
                                   $"Request body: {formattedRequestBody}");

            //We keep a reference to the original response body stream
            var originalBodyStream = context.Response.Body;

            //Create a new memory stream...
            using (var responseBody = new MemoryStream())
            {
                //...and use that for the temporary response body
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                Stopwatch stopwatch = Stopwatch.StartNew();
                await _next(context);
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed;

                //Format the response from the server
                var formattedResponseBody = await FormatResponseBody(context.Response);
                _logger.LogInformation($"HTTP response returned: HttpStatus {context.Response.StatusCode}; " +
                                       $"ResponseTime {elapsedTime.Milliseconds}ms; " +
                                       $"Request: {context.Request.Method} {context.Request.GetEncodedPathAndQuery()}. " +
                                       $"Response body: {formattedResponseBody}");


                //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private void AddMetadataToLogs(HttpRequest request, string requestBody)
        {
            try
            {
                if (request.Headers.TryGetValue("user-agent", out Microsoft.Extensions.Primitives.StringValues value))
                {
                    LoggerThreadContext.Properties["user-agent"] = value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while trying to get meta data from request. Exception details: {ex.Message}.");
                // If meta data can not be parsed, we will catch and request will be logged so we can find out what is wrong with it
            }

        }

        private async Task<string> FormatRequestBody(HttpRequest request)
        {
            var requestBody = "NA";

            if (request.ContentLength == 0)
            {
                return requestBody;
            }

            // Check if the request is multipart/form-data (in our case is file upload)
            if (request.HasFormContentType)
            {
                try
                {
                    var form = await request.ReadFormAsync();
                    if (form.Files.Count > 0)
                    {
                        var fileInfo = string.Join(", ", form.Files.Select(f => $"{f.Name}: {f.FileName} ({f.Length} bytes)"));
                        return $"[File Upload] {fileInfo}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading form data");
                    return "[File Upload - Unable to read form details]";
                }
            }

            //This line allows us to set the reader for the request back at the beginning of its stream and read it again.
            request.EnableBuffering();
            var bodyAsText = await ReadStream(request.Body, request.ContentLength);

            return FormatBody(bodyAsText);
        }

        private async Task<string> FormatResponseBody(HttpResponse response)
        {
            var responseBody = "NA";

            var skipLoggingResponse = LoggerThreadContext.Properties.TryGetValue("skipLoggingResponse");
            bool logResponseBody = skipLoggingResponse == null || (bool)skipLoggingResponse != true;

            response.Body.Position = 0;

            if (response.ContentLength == 0 || !logResponseBody)
            {
                return responseBody;
            }

            var bodyAsText = await ReadStream(response.Body, response.ContentLength);

            return FormatBody(bodyAsText);
        }

        private static async Task<string> ReadStream(Stream stream, long? contentLength)
        {
            var bodyAsText = string.Empty;
            var bufferSize = Convert.ToInt32(contentLength ?? 1024);

            // Leave the body open so the next middleware can read it.
            using (var reader = new StreamReader(
                stream,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: bufferSize,
                leaveOpen: true))
            {
                bodyAsText = await reader.ReadToEndAsync();

                // Reset the request body stream position so the next middleware can read it
                stream.Position = 0;
            }

            return bodyAsText;
        }

        private string FormatBody(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    var token = JToken.Parse(text);
                    text = token.ToString(Formatting.Indented);
                }
                catch (JsonReaderException)
                {
                    // If it's not valid JSON, return the text as-is (truncated if too long)
                    if (text.Length > 1000)
                    {
                        return text.Substring(0, 1000) + "... [truncated]";
                    }
                    return text;
                }
            }

            return text;
        }
    }
}
