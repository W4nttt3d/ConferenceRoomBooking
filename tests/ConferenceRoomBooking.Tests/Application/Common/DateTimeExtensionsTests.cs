using ConferenceRoomBooking.Application.Common;
using Xunit;

namespace ConferenceRoomBooking.Tests.Application.Common;

public class DateTimeExtensionsTests
{
    [Fact]
    public void AsUnspecifiedKind_ShouldConvertUtc_ToUnspecified()
    {
        var utc = DateTime.SpecifyKind(new DateTime(2026, 9, 15, 10, 0, 0), DateTimeKind.Utc);

        var result = utc.AsUnspecifiedKind();

        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
        Assert.Equal(new DateTime(2026, 9, 15, 10, 0, 0), result); // значення часу не змінюється, лише Kind
    }

    [Fact]
    public void AsUnspecifiedKind_ShouldConvertLocal_ToUnspecified()
    {
        var local = DateTime.SpecifyKind(new DateTime(2026, 9, 15, 10, 0, 0), DateTimeKind.Local);

        var result = local.AsUnspecifiedKind();

        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    [Fact]
    public void AsUnspecifiedKind_ShouldLeaveUnspecified_Unchanged()
    {
        var unspecified = new DateTime(2026, 9, 15, 10, 0, 0);

        var result = unspecified.AsUnspecifiedKind();

        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
        Assert.Equal(unspecified, result);
    }
}
