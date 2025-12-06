using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReservationCalendarController : ControllerBase
{
    private readonly IReservationCalendarService _calendarService;

    public ReservationCalendarController(IReservationCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetReservationCalendar([FromQuery] DateTime startDate,
                                                            [FromQuery] DateTime endDate,
                                                            [FromQuery] int calendarType = 1,
                                                            [FromQuery] int? statusId = null)
    {
        var data = await _calendarService.GetReservationCalendarDataAsync(startDate, endDate, calendarType, statusId);
        return Ok(data);
    }
}
