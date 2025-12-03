using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Models;
using OIT_Reservation.Services;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly RoomService _service;

        public RoomController(RoomService service)
        {
            _service = service;
        }

        // ✅ Get next room/banquet code
        [HttpGet("getNextRoomCode")]
        public IActionResult GetNextRoomCode(bool isRoom, bool isBanquet)
        {
            try
            {
                var code = _service.GetNextRoomCode(isRoom, isBanquet);
                return Ok(new { nextCode = code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // ✅ Get all rooms
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                var rooms = _service.GetAll();
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // ✅ Save new room
        [HttpPost("add")]
        public IActionResult Create([FromBody] Room room)
        {
            try
            {
                string generatedCode = _service.Save(room, isNew: true);
                return Ok(new
                {
                    message = "Room created successfully.",
                    generatedCode
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

        // ✅ Update existing room
        [HttpPut("update/{roomCode}")]
        public IActionResult Update(string roomCode, [FromBody] Room room)
        {
            try
            {
                room.RoomCode = roomCode;
                string updatedCode = _service.Save(room, isNew: false);

                if (!string.IsNullOrEmpty(updatedCode))
                    return Ok("Room updated successfully.");
                else
                    return NotFound("Room not found.");
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

        // Add this to your RoomController.cs
        [HttpDelete("delete/{roomCode}")]
        public IActionResult Delete(string roomCode)
        {
            try
            {
                bool isDeleted = _service.Delete(roomCode);

                if (isDeleted)
                    return Ok("Room deleted successfully.");
                else
                    return NotFound("Room not found.");
            }
            catch (SqlException ex) when (ex.Number == 547) // Foreign key constraint
            {
                return BadRequest("Cannot delete room because it is referenced in other records.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
