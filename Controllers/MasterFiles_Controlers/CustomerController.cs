 using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OIT_Reservation.Services;
using OIT_Reservation.Models;

namespace OIT_Reservation.Controllers
 {
    //     [Route("api/[controller]")]
    //     [ApiController]
    //     public class CustomerController : ControllerBase
    //     {
    //         private readonly CustomerService _customerService;
    //         private readonly ILogger<CustomerController> _logger;

    //         public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
    //         {
    //             _customerService = customerService;
    //             _logger = logger;
    //         }

    //         [HttpGet("getNextCode")]
    //         public IActionResult GetNextCode()
    //         {
    //             try
    //             {
    //                 var nextCode = _customerService.GetNextCustomerCode();
    //                 return Ok(new { nextCode });
    //             }
    //             catch (Exception ex)
    //             {
    //                 _logger.LogError(ex, "Error getting next customer code.");
    //                 return StatusCode(500, "An error occurred while retrieving the next customer code.");
    //             }
    //         }


    //         [HttpPost("save")]
    //         public IActionResult SaveCustomer([FromBody] Customer customer)
    //         {
    //             if (customer == null)
    //                 return BadRequest("Customer data is required.");

    //             try
    //             {
    //                 // Call service to save customer and get generated code
    //                 string customerCode = _customerService.CustomerSave(customer);

    //                 if (string.IsNullOrEmpty(customerCode))
    //                     return BadRequest("Failed to save customer.");

    //                 return Ok(new { CustomerCode = customerCode, message = "Customer saved successfully." });
    //             }
    //             catch (Exception ex)
    //             {
    //                 return StatusCode(500, "An error occurred while saving the customer. " + ex.Message);
    //             }
    //         }


    //         [HttpGet("getall")]
    //         public IActionResult GetAll()
    //         {
    //             try
    //             {
    //                 var list = _customerService.GetAllCustomers(); // NOTE: uses _service not _customerService
    //                 return Ok(list);
    //             }
    //             catch (Exception ex)
    //             {
    //                 return StatusCode(500, "An error occurred while retrieving customers." + ex.Message);
    //             }
    //         }

    //         [HttpPut("update/{customerCode}")]
    //         public IActionResult UpdateCustomerByCode(string customerCode, [FromBody] Customer customer)
    //         {
    //             try
    //             {
    //                 var updatedCode = _customerService.UpdateCustomerByCode(customerCode, customer);
    //                 return Ok(new { UpdatedCustomerCode = updatedCode });
    //             }
    //             catch (Exception ex)
    //             {
    //                 _logger.LogError(ex, "Error updating customer.");
    //                 return StatusCode(500, "An error occurred while updating the customer.");
    //             }
    //         }



    //     }
    // }
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet("getNextCode")]
        public IActionResult GetNextCode()
        {
            try
            {
                var nextCode = _customerService.GetNextCustomerCode();
                return Ok(new { nextCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next customer code.");
                return StatusCode(500, "An error occurred while retrieving the next customer code.");
            }
        }

        [HttpPost("save")]
        public IActionResult SaveCustomer([FromBody] Customer customer)
        {
            if (customer == null)
                return BadRequest("Customer data is required.");

            try
            {
                string customerCode = _customerService.CustomerSave(customer);

                if (string.IsNullOrEmpty(customerCode))
                    return BadRequest("Failed to save customer.");

                return Ok(new { CustomerCode = customerCode, message = "Customer saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving customer.");
                return StatusCode(500, "An error occurred while saving the customer: " + ex.Message);
            }
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                var list = _customerService.GetAllCustomers();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers.");
                return StatusCode(500, "An error occurred while retrieving customers: " + ex.Message);
            }
        }

        [HttpPut("update/{customerCode}")]
        public IActionResult UpdateCustomerByCode(string customerCode, [FromBody] Customer customer)
        {
            try
            {
                var updatedCode = _customerService.UpdateCustomerByCode(customerCode, customer);
                return Ok(new { UpdatedCustomerCode = updatedCode, message = "Customer updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer.");
                return StatusCode(500, "An error occurred while updating the customer: " + ex.Message);
            }
        }

        [HttpDelete("delete/{customerCode}")]
        public IActionResult Delete(string customerCode)
        {
            try
            {
                bool deleted = _customerService.Delete(customerCode); 

                if (deleted)
                    return Ok("Customer deleted successfully.");
                else
                    return NotFound("Customer not found.");
            }
            catch (SqlException ex)
            {
                return BadRequest($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}


