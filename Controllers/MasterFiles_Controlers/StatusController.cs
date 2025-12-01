//using System.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;

//namespace OIT_Reservation.Controllers.MasterFiles_Controlers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class StatusController : Controller
//    {
//        private readonly IConfiguration _config;

//        public StatusController(IConfiguration config)
//        {
//            _config = config;
//        }

//        [HttpGet("list")]
//        public IActionResult GetStatusList()
//        {
//            try
//            {
//                string conn = _config.GetConnectionString("DefaultConnection");

//                if (string.IsNullOrEmpty(conn))
//                {
//                    return BadRequest("Connection string not found!");
//                }

//                using (SqlConnection con = new SqlConnection(conn))
//                {
//                    string query = "SELECT statusid, statusname FROM Reservation_Status";

//                    SqlDataAdapter da = new SqlDataAdapter(query, con);
//                    DataTable dt = new DataTable();
//                    da.Fill(dt);

//                    return Ok(dt);
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest("API ERROR: " + ex.Message);
//            }
//        }
//    }
//}
