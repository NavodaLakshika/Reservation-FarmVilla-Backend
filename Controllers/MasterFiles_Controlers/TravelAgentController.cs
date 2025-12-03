using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelAgentController : ControllerBase
    {
        private readonly TravelAgentService _service;

        public TravelAgentController(TravelAgentService service)
        {
            _service = service;
        }

        //GET: api/travelagent/getNextCode
        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _service.GetNextTravelAgentCode();
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

        //GET: api/travelagent
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return Ok(result);
        }

        // POST: api/travelagent[HttpPost]
        [HttpPost("add")]
        public IActionResult Create([FromBody] TravelAgent travelAgent)
        {
            try
            {
                bool success = _service.Create(travelAgent);
                return Ok(new
                {
                    message = "Travel agent created successfully.",
                    generatedCode = travelAgent.TravelAgentCode
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

        [HttpPut("Update/{id}")]
        public IActionResult Update(long id, [FromBody] TravelAgent travelAgent)
        {
            try
            {
                travelAgent.TravelAgentID = id;

                bool updated = _service.Update(travelAgent);
                if (updated)
                    return Ok("Travel Agent updated successfully.");
                else
                    return NotFound("Travel Agent not found.");
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

        [HttpDelete("Delete/{TravelAgentID}")]

        public IActionResult Delete(long TravelAgentID)
        {
            try
            {
                bool delete = _service.Delete(TravelAgentID);
                if (delete)
                    return Ok("Travel Agent delete successfully.");
                else
                    return NotFound("Trvel Agent not found");
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
