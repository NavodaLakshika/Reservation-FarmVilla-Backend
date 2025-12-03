using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackageInfoController : ControllerBase
    {
        private readonly PackageInfoService _service;

        public PackageInfoController(PackageInfoService service)
        {
            _service = service;
        }

        //GET: api/packageinfo/getNextCode
        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _service.GetNextPackageCode();
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


        

        //GET: api/packageinfo
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return Ok(result);
        }

        // POST: api/packageinfo[HttpPost]
        [HttpPost("add")]
        public IActionResult Create([FromBody] PackageInfo packageInfo)
        {
            try
            {
                bool success = _service.Create(packageInfo);
                return Ok(new
                {
                    message = "Package Information created successfully.",
                    generatedCode = packageInfo.PackageCode
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

        // PUT: api/packageinfo[HttpPut]
        [HttpPut("Update/{id}")]
        public IActionResult Update(long id, [FromBody] PackageInfo packageInfo)
        {
            try
            {
                packageInfo.PackageID = id;

                bool updated = _service.Update(packageInfo);
                if (updated)
                    return Ok("Package information updated successfully.");
                else
                    return NotFound("Package information not found.");
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

        [HttpDelete("Delete/{PackageID}")]
        public IActionResult Delete(long PackageID)
        {
            try
            {
                bool delete = _service.Delete(PackageID);
                if (delete)
                    return Ok("Package information delete successfully.");
                else
                    return NotFound("Package information not found.");

            }
            catch(SqlException ex)
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