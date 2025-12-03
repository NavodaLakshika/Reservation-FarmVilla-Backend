using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventTypeController : ControllerBase
    {
        private readonly EventTypeService _service;

        public EventTypeController(EventTypeService service)
        {
            _service = service;
        }

        //GET: api/eventtype/getNextCode
        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _service.GetNextEventCode();
                return Ok(new { nextCode = nextCode });
            }
            catch (SqlException ex)
            {
                return BadRequest($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //GET: api/eventtype
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return Ok(result);
        }

        // POST: api/eventtype[HttpPost]
        [HttpPost("add")]
        public IActionResult Create([FromBody] EventType eventType)
        {
            try
            {
                bool success = _service.Create(eventType);
                return Ok(new
                {
                    message = "Event type created successfully.",
                    generatedCode = eventType.EventCode
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // PUT: api/eventtype[HttpPut]
        [HttpPut("Update/{id}")]
        public IActionResult Update(long id, [FromBody] EventType eventType)
        {
            try
            {
                eventType.EventTypeID = id;

                bool updated = _service.Update(eventType);
                if (updated)
                    return Ok("Event type updated successfully.");
                else
                    return NotFound("Event type not found.");
            }
            catch (SqlException ex)
            {
                return BadRequest($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{eventTypeId}")]
        public IActionResult Delete(string eventTypeId)
        {
            try
            {
                bool delete = _service.Delete(eventTypeId);
                if (delete)
                    return Ok("Event type delete successfully.");
                else
                    return NotFound("Event type not found.");
            }
            catch (SqlException ex)
            {
                return BadRequest($"SQL Error: {ex.Message}");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
