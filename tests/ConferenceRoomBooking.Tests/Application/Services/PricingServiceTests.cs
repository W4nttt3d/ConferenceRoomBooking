using ConferenceRoomBooking.Application.Services;
using Xunit;

namespace ConferenceRoomBooking.Tests.Application.Services;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    [Fact]
    public void StandardPrice_ShouldReturnBasePrice()
    {
        // 10:00–11:00 повністю в межах Standard (09:00–12:00) — без коефіцієнта.
        var start = new DateTime(2026, 9, 15, 10, 0, 0);
        var end = new DateTime(2026, 9, 15, 11, 0, 0);

        var price = _sut.CalculateRoomPrice(baseHourlyRate: 2000m, start, end);

        Assert.Equal(2000m, price);
    }

    [Fact]
    public void MorningPrice_ShouldApply10PercentDiscount()
    {
        // 06:00–07:00 повністю в межах Morning (06:00–09:00) — знижка 10%.
        var start = new DateTime(2026, 9, 15, 6, 0, 0);
        var end = new DateTime(2026, 9, 15, 7, 0, 0);

        var price = _sut.CalculateRoomPrice(baseHourlyRate: 1000m, start, end);

        Assert.Equal(900m, price);
    }

    [Fact]
    public void EveningPrice_ShouldApply20PercentDiscount()
    {
        // 19:00–20:00 повністю в межах Evening (18:00–23:00) — знижка 20%.
        var start = new DateTime(2026, 9, 15, 19, 0, 0);
        var end = new DateTime(2026, 9, 15, 20, 0, 0);

        var price = _sut.CalculateRoomPrice(baseHourlyRate: 1000m, start, end);

        Assert.Equal(800m, price);
    }

    [Fact]
    public void PeakPrice_ShouldApply15PercentSurcharge()
    {
        // 12:00–13:00 повністю в межах Peak (12:00–14:00) — надбавка 15%.
        var start = new DateTime(2026, 9, 15, 12, 0, 0);
        var end = new DateTime(2026, 9, 15, 13, 0, 0);

        var price = _sut.CalculateRoomPrice(baseHourlyRate: 1000m, start, end);

        Assert.Equal(1150m, price);
    }

    [Fact]
    public void MixedPeriod_ShouldCalculateCorrectPrice()
    {
        // 11:00–15:00 перетинає три зони:
        // 11:00–12:00 Standard (1г × 1.00 = 2000)
        // 12:00–14:00 Peak     (2г × 1.15 = 4600)
        // 14:00–15:00 Standard (1г × 1.00 = 2000)
        // Разом: 8600
        var start = new DateTime(2026, 9, 15, 11, 0, 0);
        var end = new DateTime(2026, 9, 15, 15, 0, 0);

        var price = _sut.CalculateRoomPrice(baseHourlyRate: 2000m, start, end);

        Assert.Equal(8600m, price);
    }

    [Fact]
    public void PartialHour_ShouldBeProratedCorrectly()
    {
        // 08:30–09:30 перетинає межу Morning/Standard рівно посередині:
        // 08:30–09:00 Morning  (0.5г × 0.90 = 900 при ставці 2000)
        // 09:00–09:30 Standard (0.5г × 1.00 = 1000)
        // Разом: 1900
        var start = new DateTime(2026, 9, 15, 8, 30, 0);
        var end = new DateTime(2026, 9, 15, 9, 30, 0);

        var price = _sut.CalculateRoomPrice(baseHourlyRate: 2000m, start, end);

        Assert.Equal(1900m, price);
    }

    [Fact]
    public void EndTimeNotAfterStartTime_ShouldThrowArgumentException()
    {
        var start = new DateTime(2026, 9, 15, 10, 0, 0);
        var end = new DateTime(2026, 9, 15, 10, 0, 0); // рівні часи — невалідний інтервал

        Assert.Throws<ArgumentException>(() => _sut.CalculateRoomPrice(1000m, start, end));
    }
}
