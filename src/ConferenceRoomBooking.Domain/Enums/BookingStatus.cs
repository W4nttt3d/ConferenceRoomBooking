namespace ConferenceRoomBooking.Domain.Enums;

/// <summary>
/// Статус бронювання.
/// Мінімальний набір за планом: Confirmed / Cancelled.
/// Pending/Completed можна додати пізніше, якщо з'явиться потреба
/// (наприклад, підтвердження бронювання адміністратором).
/// </summary>
public enum BookingStatus
{
    Confirmed = 0,
    Cancelled = 1
}
