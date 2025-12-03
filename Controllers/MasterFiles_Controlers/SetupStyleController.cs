using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SetupStyleController : ControllerBase
    {
        private readonly setupStyleService _service;

        public SetupStyleController(setupStyleService service)
        {
            _service = service;
        }

        //GET: api/setupstyle/getNextCode
        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _service.GetNextSetupStyleCode();
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

        //GET: api/setupstyle
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return Ok(result);
        }

        // POST: api/setupstyle[HttpPost]
        [HttpPost("add")]
        public IActionResult Create([FromBody] SetupStyle setupStyle)
        {
            try
            {
                bool success = _service.Create(setupStyle);
                return Ok(new
                {
                    message = "Setup style type created successfully.",
                    generatedCode = setupStyle.SetupStyleCode
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

        // PUT: api/setupstyle[HttpPut]
        [HttpPut("Update/{id}")]
        public IActionResult Update(long id, [FromBody] SetupStyle setupStyle)
        {
            try
            {
                setupStyle.SetupStyleTypeID = id;

                bool updated = _service.Update(setupStyle);
                if (updated)
                    return Ok("Setup style type updated successfully.");
                else
                    return NotFound("Setup style type not found.");
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

        [HttpDelete("Delete/{SetupStyleTypeID}")]
        public IActionResult Delete(long SetupStyleTypeID)
        {
            try
            {
                bool delete = _service.Delete(SetupStyleTypeID);
                if (delete)
                    return Ok("Setup style type delete successfully.");
                else
                    return NotFound("Setup style type not found.");


            }catch (SqlException ex)
            {
                return BadRequest($"SQL Error: server error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server error: {ex.Message}");
            }
        }

    }

}
