using ConferenceRoomBooking.Api.ExceptionHandling;
using ConferenceRoomBooking.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ConferenceRoomBooking.Tests.Api.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _sut = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Theory]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(BookingConflictException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(UnprocessableEntityException), StatusCodes.Status422UnprocessableEntity)]
    public async Task TryHandleAsync_ShouldMapKnownExceptions_ToCorrectStatusCode(Type exceptionType, int expectedStatusCode)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test message")!;
        var context = CreateHttpContext();

        var handled = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldMapUnknownException_To500_WithoutLeakingMessage()
    {
        // Свідомо "чутливе" повідомлення — перевіряємо, що воно НЕ
        // потрапляє в HTTP-відповідь клієнту для непередбачених винятків.
        var exception = new InvalidOperationException("password=hunter2; stacktrace internal detail");
        var context = CreateHttpContext();

        var handled = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("hunter2", body);
        Assert.DoesNotContain("stacktrace", body);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeExceptionMessage_ForKnownDomainExceptions()
    {
        // Для очікуваних доменних винятків повідомлення вже сформульоване
        // для користувача в сервісному шарі — воно безпечне і має дійти до клієнта.
        var exception = new NotFoundException("Зал з Id = 999 не знайдено.");
        var context = CreateHttpContext();

        await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        var body = await ReadBodyAsync(context);
        Assert.Contains("Зал з Id = 999 не знайдено.", body);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
