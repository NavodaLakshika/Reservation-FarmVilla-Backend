using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController : ControllerBase
    {
        private readonly ServiceTypeService _service;

        public ServiceTypeController(ServiceTypeService service)
        {
            _service = service;
        }
        //GET: api/servicetype/getNextCode
        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _service.GetNextServiceCode();
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
        //GET: api/servicetype
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return Ok(result);
        }

        // POST: api/servicetype[HttpPost]
        [HttpPost("add")]
        public IActionResult Create([FromBody] ServiceType serviceType)
        {
            try
            {
                bool success = _service.Create(serviceType);
                return Ok(new
                {
                    message = "Service Type created successfully.",
                    generatedCode = serviceType.ServiceCode
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

        // PUT: api/servicetype[HttpPut]
        [HttpPut("Update/{id}")]
        public IActionResult Update(long id, [FromBody] ServiceType serviceType)
        {
            try
            {
                serviceType.ServiceTypeID = id;

                bool updated = _service.Update(serviceType);
                if (updated)
                    return Ok("Service type updated successfully.");
                else
                    return NotFound("Service type not found.");
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

        [HttpDelete("Delete/{ServiceTypeID}")]
        public IActionResult Delete (long ServiceTypeID)
        {
            try
            {
                bool delete = _service.Delete(ServiceTypeID);
                if (delete)
                    return Ok("Service type delete successfully.");
                else
                    return NotFound("Service type not found.");
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
