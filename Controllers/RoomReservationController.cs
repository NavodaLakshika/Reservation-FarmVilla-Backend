using Microsoft.AspNetCore.Mvc;
using OIT_Reservation;

//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;

[ApiController]
[Route("api/[controller]")]
public class RoomReservationController : ControllerBase
{
    private readonly IRoomReservationService _reservationService;
    private readonly ILogger<RoomReservationController> _logger;

    public RoomReservationController(
        IRoomReservationService reservationService,
        ILogger<RoomReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveReservation([FromBody] ReservationDto reservationDto)
    {
        if (reservationDto == null)
        {
            _logger.LogWarning("Invalid reservation data received");
            return BadRequest(new { message = "Invalid reservation data" });
        }

        // Manual validation for Customer object
        if (reservationDto.Customer == null)
        {
            _logger.LogWarning("Customer object is null in reservation");
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "One or more validation errors occurred.",
                status = 400,
                errors = new { Customer = new[] { "The Customer field is required." } },
                traceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            _logger.LogInformation("Saving reservation for customer: {CustomerCode}", reservationDto.CustomerCode);
            _logger.LogDebug("Received Customer data: {@Customer}", reservationDto.Customer);

            // Save the reservation
            var reservationNo = await _reservationService.SaveOrUpdateReservationAsync(reservationDto);

            // ✅ Get the invoice number (will be null if not finalized)
            var invoiceNo = await _reservationService.GetInvoiceNumberAsync(reservationNo);

            _logger.LogInformation("Reservation saved successfully: {ReservationNo}, Invoice: {InvoiceNo}",
                reservationNo, invoiceNo ?? "NOT GENERATED");

            return Ok(new
            {
                ReservationNo = reservationNo,
                InvoiceNo = invoiceNo, // ✅ Add invoice number
                Message = invoiceNo != null
                    ? $"Reservation finalized with Invoice: {invoiceNo}"
                    : "Reservation saved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving reservation for customer: {CustomerCode}", reservationDto.CustomerCode);
            return StatusCode(500, new
            {
                message = "An error occurred while saving the reservation",
                error = ex.Message
            });
        }
    }


    // NEW: GET ALL
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] int? top = null)
    {
        try
        {
            var data = await _reservationService.GetAllAsync(top);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all reservations.");
            return StatusCode(500, "An error occurred while retrieving reservations: " + ex.Message);
        }

    }

    [HttpGet("byStatus/{statusId}")]
    public async Task<IActionResult> GetByStatus(int statusId, [FromQuery] string? fromDate = null, [FromQuery] string? toDate = null)
    {
        try
        {
            var reservations = await _reservationService.GetByStatusAsync(statusId, fromDate, toDate);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reservations by status {statusId}", statusId);
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }




}
