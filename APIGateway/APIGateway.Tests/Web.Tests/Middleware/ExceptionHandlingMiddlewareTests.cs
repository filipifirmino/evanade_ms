using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using APIGateway.Web.Middleware;
using Xunit;

namespace APIGateway.Tests.Web.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _environmentMock = new Mock<IHostEnvironment>();
        _middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object, _environmentMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_ShouldCallNext()
    {
        var context = CreateHttpContext();

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
    }

   



    

    private HttpContext CreateHttpContext()
    {
        var context = new Mock<HttpContext>();
        var response = new Mock<HttpResponse>();
        var responseBody = new MemoryStream();

        response.Setup(x => x.Body).Returns(responseBody);
        response.SetupProperty(x => x.StatusCode);
        response.SetupProperty(x => x.ContentType);

        context.Setup(x => x.Response).Returns(response.Object);

        return context.Object;
    }
}
