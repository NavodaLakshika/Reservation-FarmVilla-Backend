using Microsoft.AspNetCore.Mvc;
using OIT_Reservation.Models;
using OIT_Reservation.Services;
using OIT_Reservation.Helpers;
using OIT_Reservation.Interface;
using OIT_Reservation.Model;
using Microsoft.AspNetCore.Identity;

namespace OIT_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CRUDController : ControllerBase
    {
        private readonly IReservationService _service;
        private readonly IUserService _userService;
        private readonly JwtHelper _jwt;

        public CRUDController(IReservationService service, IUserService userService, IConfiguration config)
        {
            _service = service;
            _userService = userService;
            _jwt = new JwtHelper(config);
        }

        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("This is a test exception for logging");
        }



        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Username and Password are required");

            if (_userService.RegisterUser(user))
            {
                return Ok("User registered ");
            }

            return BadRequest("Registration failed");
            
        }


        // ✅ LOGIN endpoint
        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {

            if (_userService.ValidateUser(user))
            {
                var token = _jwt.GenerateToken(user.Username);
                return Ok(new { token });
                
                
                
            }

            return Unauthorized("Invalid credentials");
        }

        // ✅ RESERVATION endpoints
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] Reservation reservation)
        {
            if (_service.Create(reservation))
                return Ok("Created");
            return BadRequest("Failed");
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] Reservation reservation)
        {
            if (_service.Update(reservation))
                return Ok("Updated");
            return NotFound("Failed");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_service.Delete(id))
                return Ok("Deleted");
            return NotFound("Failed");
        }
    }
}
