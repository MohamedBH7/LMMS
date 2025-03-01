using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LMMS.Controllers
{
    // Use ApiController to make this an API controller
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorA : ControllerBase
    {
        private readonly string _connectionString;

        // Inject the configuration in the constructor to fetch the connection string
        public InstructorA(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // API to Reject a Book Request
        [HttpPut("bookrequests/reject/{id}")]
        [Authorize(Roles = "Instructor")]
        public IActionResult RejectRequest(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Books_Request_To_Add SET State = 'Rejected' WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return Ok(new { message = "Request Rejected" });
                    }
                }

                return BadRequest(new { message = "Error Rejecting Request" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // API to Delete a Book Request
        [HttpDelete("bookrequests/delete/{id}")]
        [Authorize(Roles = "Instructor")]
        public IActionResult DeleteRequest(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Books_Request_To_Add WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return Ok(new { message = "Request Deleted" });
                    }
                }

                return BadRequest(new { message = "Error Deleting Request" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // add also Function for 'Approved','Not Found'

        // API to Reject a Book Request
        [HttpPut("bookrequests/ApprovedRequest/{id}")]
        [Authorize(Roles = "Instructor")]
        public IActionResult ApprovedRequest(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Books_Request_To_Add SET State = 'Approved' WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return Ok(new { message = "Request Approved" });
                    }
                }

                return BadRequest(new { message = "Error Approved Request" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }




    }
}
