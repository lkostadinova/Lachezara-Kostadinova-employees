using EmployeeIdentifier.Api.Models;
using EmployeeIdentifier.Api.Models.Enums;
using EmployeeIdentifier.Api.RequestHandlers;
using EmployeeIdentifier.Api.RequestHandlers.Abstract;
using EmployeeIdentifier.Api.Tests.Helpers;
using EmployeeIdentifier.Services.Dtos;
using EmployeeIdentifier.Services.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace EmployeeIdentifier.Api.Tests
{
    [Collection("Tests")]
    public class EmployeesTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public EmployeesTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region Controller Tests - Positive Scenarios

        [Fact]
        public async Task GetEmployees_WithValidCsvFile_ReturnsOk()
        {
            // Arrange
            var csvContent = TestFileHelper.ValidCsvContent;
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(csvContent));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
            content.Add(fileContent, "file", "employees.csv");

            // Act
            var response = await _client.PostAsync(Routes.GET_EMPLOYEES, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region Controller Tests - Negative Scenarios

        [Fact]
        public async Task GetEmployees_WithoutFile_ReturnsBadRequest()
        {
            // Arrange
            using var content = new MultipartFormDataContent();

            // Act
            var response = await _client.PostAsync(Routes.GET_EMPLOYEES, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployees_WithEmptyFile_ReturnsBadRequest()
        {
            // Arrange
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Array.Empty<byte>());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
            content.Add(fileContent, "file", "empty.csv");

            // Act
            var response = await _client.PostAsync(Routes.GET_EMPLOYEES, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployees_WithNonCsvFile_ReturnsBadRequest()
        {
            // Arrange
            var textContent = "This is not a CSV file";
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(textContent));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            content.Add(fileContent, "file", "document.txt");

            // Act
            var response = await _client.PostAsync(Routes.GET_EMPLOYEES, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Handler Unit Tests - Positive Scenarios

        [Fact]
        public async Task Handler_WithValidCsvFile_ReturnsSuccessResponse()
        {
            // Arrange
            var mockService = new Mock<IEmployeeCollaborationService>();
            var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

            var expectedDto = new EmployeeCollaborationDto
            {
                EmployeeFirstId = 143,
                EmployeeSecondId = 218,
                DaysWorkedTogether = 65,
                ProjectDetails = new List<ProjectCollaborationDetailDto>
                {
                    new ProjectCollaborationDetailDto
                    {
                        ProjectId = 12,
                        DaysWorkedTogether = 65,
                        OverlapStart = new DateTime(2013, 11, 1),
                        OverlapEnd = new DateTime(2014, 1, 5)
                    }
                }
            };

            mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
                       .ReturnsAsync(expectedDto);

            var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
            var file = TestFileHelper.CreateCsvFile("test.csv", TestFileHelper.ValidCsvContent);

            // Act
            var result = await handler.HandleAsync(file);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Result);
            Assert.Equal(143, result.Result.EmployeeFirstId);
            Assert.Equal(218, result.Result.EmployeeSecondId);
            Assert.Equal(65, result.Result.DaysWorkedTogether);
            Assert.Single(result.Result.ProjectDetails);
        }

        [Fact]
        public async Task Handler_WithValidCsvFile_CallsServiceOnce()
        {
            // Arrange
            var mockService = new Mock<IEmployeeCollaborationService>();
            var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

            var expectedDto = new EmployeeCollaborationDto
            {
                EmployeeFirstId = 100,
                EmployeeSecondId = 200,
                DaysWorkedTogether = 30,
                ProjectDetails = new List<ProjectCollaborationDetailDto>()
            };

            mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
                       .ReturnsAsync(expectedDto);

            var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
            var file = TestFileHelper.CreateCsvFile("test.csv", TestFileHelper.ValidCsvContent);

            // Act
            await handler.HandleAsync(file);

            // Assert
            mockService.Verify(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()), Times.Once);
        }

        #endregion

        #region Handler Unit Tests - Negative Scenarios

        //[Fact]
        //public async Task Handler_WithNullFile_ReturnsValidationError()
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();
        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);

        //    var projectDetails = new List<ProjectCollaborationDetail>
        //    {
        //        new ProjectCollaborationDetail
        //        {
        //            ProjectId = 101,
        //            DaysWorkedTogether = 10,
        //            OverlapStart = new DateTime(2020, 1, 1).ToString("yyyy-MM-dd"),
        //            OverlapEnd = new DateTime(2020, 1, 11).ToString("yyyy-MM-dd")
        //        }
        //    };

        //    var employee = new EmployeeCollaborationResult
        //    {
        //        EmployeeFirstId = 1,
        //        EmployeeSecondId = 2,
        //        DaysWorkedTogether = 10,
        //        ProjectDetails = projectDetails
        //    };

        //    // Act
        //    var result = await handler.HandleAsync(null);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.Success);
        //    Assert.NotNull(result.Error);

        //    var updateContent = TestB.GetStringContent(payload);

        //    var responseContent = await result.Content.ReadAsStringAsync();
        //    var errorResponse = JsonConvert.DeserializeObject<ErrorResponseModel>(responseContent);
        //    Assert.Equal(ErrorCodes.VALIDATION_ERROR, errorResponse.Error.Code);
        //    Assert.Equal("TipsPoolConfigurationId must be provided when action is 'Assign'.", errorResponse.Error.Messages[0]);
        //    Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.Error.Code);
        //    Assert.Contains(ErrorMessages.NoFileUploaded, result.Error.Messages);
        //}

        [Fact]
        public async Task Handler_WithEmptyFile_ReturnsValidationError()
        {
            // Arrange
            var mockService = new Mock<IEmployeeCollaborationService>();
            var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();
            var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
            var file = TestFileHelper.CreateEmptyFile("empty.csv");

            // Act
            var result = await handler.HandleAsync(file);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.Error.Error.Code);
            Assert.Contains(ErrorMessages.NoFileUploaded, result.Error.Error.Messages[0]);
        }

        //[Fact]
        //public async Task Handler_WithNonCsvFile_ReturnsValidationError()
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();
        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateNonCsvFile("document.txt", "Some text content");

        //    // Act
        //    var result = await handler.HandleAsync(file);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.Success);
        //    Assert.NotNull(result.Error);
        //    Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.Error.Code);
        //    Assert.Contains(ErrorMessages.FileMustBeCsv, result.Error.Messages);
        //}

        //[Fact]
        //public async Task Handler_WithNoCollaborationsFound_ReturnsValidationError()
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

        //    mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
        //               .ReturnsAsync((EmployeeCollaborationDto)null);

        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateCsvFile("test.csv", TestFileHelper.NoCollaborationCsvContent);

        //    // Act
        //    var result = await handler.HandleAsync(file);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.Success);
        //    Assert.NotNull(result.Error);
        //    Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.Error.Code);
        //    Assert.Contains(ErrorMessages.NoCollaborationsFound, result.Error.Messages);
        //}

        //[Fact]
        //public async Task Handler_WhenServiceThrowsException_ReturnsUnknownError()
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

        //    mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
        //               .ThrowsAsync(new Exception("Database connection failed"));

        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateCsvFile("test.csv", TestFileHelper.ValidCsvContent);

        //    // Act
        //    var result = await handler.HandleAsync(file);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.Success);
        //    Assert.NotNull(result.Error);
        //    Assert.Equal(ErrorCodes.UNKNOWN, result.Error.Code);
        //    Assert.Contains(ErrorMessages.ErrorProcessingFile, result.Error.Messages);
        //}

        //[Fact]
        //public async Task Handler_WhenServiceThrowsException_LogsError()
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

        //    var expectedException = new Exception("Database connection failed");
        //    mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
        //               .ThrowsAsync(expectedException);

        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateCsvFile("test.csv", TestFileHelper.ValidCsvContent);

        //    // Act
        //    await handler.HandleAsync(file);

        //    // Assert
        //    mockLogger.Verify(
        //        x => x.Log(
        //            LogLevel.Error,
        //            It.IsAny<EventId>(),
        //            It.Is<It.IsAnyType>((v, t) => true),
        //            It.IsAny<Exception>(),
        //            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
        //        Times.Once);
        //}

        //#endregion

        //#region Handler Unit Tests - Edge Cases

        //[Fact]
        //public async Task Handler_WithMultipleProjects_ReturnsCorrectAggregation()
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

        //    var expectedDto = new EmployeeCollaborationDto
        //    {
        //        EmployeeFirstId = 143,
        //        EmployeeSecondId = 218,
        //        DaysWorkedTogether = 130,
        //        ProjectDetails = new List<ProjectCollaborationDetailDto>
        //        {
        //            new ProjectCollaborationDetailDto
        //            {
        //                ProjectId = 10,
        //                DaysWorkedTogether = 65,
        //                OverlapStart = new DateTime(2013, 1, 1),
        //                OverlapEnd = new DateTime(2013, 3, 7)
        //            },
        //            new ProjectCollaborationDetailDto
        //            {
        //                ProjectId = 12,
        //                DaysWorkedTogether = 65,
        //                OverlapStart = new DateTime(2013, 11, 1),
        //                OverlapEnd = new DateTime(2014, 1, 5)
        //            }
        //        }
        //    };

        //    mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
        //               .ReturnsAsync(expectedDto);

        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateCsvFile("test.csv", TestFileHelper.ValidCsvContent);

        //    // Act
        //    var result = await handler.HandleAsync(file);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.Success);
        //    Assert.Equal(130, result.Result.DaysWorkedTogether);
        //    Assert.Equal(2, result.Result.ProjectDetails.Count);
        //}

        //[Theory]
        //[InlineData("test.csv")]
        //[InlineData("TEST.CSV")]
        //[InlineData("Test.CsV")]
        //[InlineData("data.CSV")]
        //public async Task Handler_WithCsvFileExtension_AcceptsFileCaseInsensitive(string fileName)
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();

        //    var expectedDto = new EmployeeCollaborationDto
        //    {
        //        EmployeeFirstId = 1,
        //        EmployeeSecondId = 2,
        //        DaysWorkedTogether = 10,
        //        ProjectDetails = new List<ProjectCollaborationDetailDto>()
        //    };

        //    mockService.Setup(s => s.AnalyzeCollaborationsAsync(It.IsAny<Stream>()))
        //               .ReturnsAsync(expectedDto);

        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateCsvFile(fileName, TestFileHelper.ValidCsvContent);

        //    // Act
        //    var result = await handler.HandleAsync(file);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.Success);
        //}

        //[Theory]
        //[InlineData("document.txt")]
        //[InlineData("spreadsheet.xlsx")]
        //[InlineData("data.json")]
        //[InlineData("file.xml")]
        //public async Task Handler_WithNonCsvExtension_ReturnsValidationError(string fileName)
        //{
        //    // Arrange
        //    var mockService = new Mock<IEmployeeCollaborationService>();
        //    var mockLogger = new Mock<ILogger<GetEmployeeCollaborationRequestHandler>>();
        //    var handler = new GetEmployeeCollaborationRequestHandler(mockService.Object, mockLogger.Object);
        //    var file = TestFileHelper.CreateNonCsvFile(fileName, "Some content");

        //    // Act
        //    var result = await handler.HandleAsync(file);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.Success);
        //    Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.Error.Code);
        //    Assert.Contains(ErrorMessages.FileMustBeCsv, result.Error.Messages);
        //}

        #endregion
    }
}