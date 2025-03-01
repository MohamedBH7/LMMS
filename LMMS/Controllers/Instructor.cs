using LMMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LMMS.Controllers
{
    public class Instructor : Controller
    {
        private readonly string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-LMMS-7fc7690b-1ba3-432f-9f4f-99c7a2dbeb77;Trusted_Connection=True;MultipleActiveResultSets=true";
        [Authorize(Roles = "Instructor")]

        public IActionResult Index()
        {
            return View();
        
        }
        [Authorize(Roles = "Instructor")]
        public IActionResult RequestBook()
        {
            var currentUserEmail = User.Identity?.Name;

            // Error handling: Early exit if no user is logged in
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                ViewData["ErrorMessage"] = "User not found.";
                return View();
            }

            try
            {
                List<BookSection> sections = new List<BookSection>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Fetch book sections from the database
                    string sectionQuery = "SELECT * FROM BookSection";
                    using (SqlCommand cmd = new SqlCommand(sectionQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sections.Add(new BookSection
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    SectionName = reader["SectionName"].ToString()
                                });
                            }
                        }
                    }
                }

                // Check if there are any sections available
                if (sections.Count == 0)
                {
                    ViewData["ErrorMessage"] = "No book sections available. Please contact the administrator.";
                }
                else
                {
                    ViewBag.Sections = new SelectList(sections, "Id", "SectionName");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (logging mechanism is not shown here)
                ViewData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
            }

            return View();
        }




        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public IActionResult RequestBook(BookRequest bookRequest)
        {
            
                var currentUserEmail = User.Identity?.Name;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Books_Request_To_Add (Title, Author, PublishedYear, Email, SectionId, State) VALUES (@Title, @Author, @PublishedYear, @Email, @SectionId, 'Pending')", conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", bookRequest.Title);
                        cmd.Parameters.AddWithValue("@Author", bookRequest.Author);
                        cmd.Parameters.AddWithValue("@PublishedYear", (object)bookRequest.PublishedYear ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", currentUserEmail);
                        cmd.Parameters.AddWithValue("@SectionId", bookRequest.SectionId);
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("BookRequests");
            

           // return View(bookRequest);
        }











        #region Instructor After Login
        public IActionResult BookRequests()
        {
            List<BookRequest> requests = new List<BookRequest>();
            var currentUserEmail = User.Identity?.Name;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT br.*, bs.SectionName 
                    FROM Books_Request_To_Add br
                    LEFT JOIN BookSection bs ON br.SectionId = bs.Id
                    WHERE br.Email = @Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", currentUserEmail);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new BookRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                PublishedYear = reader["PublishedYear"] != DBNull.Value ? Convert.ToInt32(reader["PublishedYear"]) : (int?)null,
                                Email = reader["Email"].ToString(),
                                RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                                State = reader["State"].ToString(),
                                SectionId = Convert.ToInt32(reader["SectionId"]),
                                SectionName = reader["SectionName"].ToString()
                            });
                        }
                    }
                }
            }

            return View(requests);
        }




        public IActionResult Requests()
        {
            List<BookRequest> requests = new List<BookRequest>();
            var currentUserEmail = User.Identity?.Name;
            if(currentUserEmail == null) 
                return NotFound("You Have To Login");
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT br.*, bs.SectionName 
                    FROM Books_Request_To_Add br
                    LEFT JOIN BookSection bs ON br.SectionId = bs.Id
                    WHERE br.Email = @Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", currentUserEmail);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new BookRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                PublishedYear = reader["PublishedYear"] != DBNull.Value ? Convert.ToInt32(reader["PublishedYear"]) : (int?)null,
                                Email = reader["Email"].ToString(),
                                RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                                State = reader["State"].ToString(),
                                SectionId = Convert.ToInt32(reader["SectionId"]),
                                SectionName = reader["SectionName"].ToString()
                            });
                        }
                    }
                }
            }

            return View(requests);







        }



        #region API Actions 

        // API to Reject a Book Request
        [HttpPut("api/bookrequests/reject/{id}")]
        [Authorize(Roles = "Instructor")]
        public IActionResult RejectRequest(int id)
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


        // API to Delete a Book Request
        [HttpPut("api/bookrequests/delete/{id}")]
        [Authorize(Roles = "Instructor")]
        public IActionResult DeleteRequest(int id)
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

        [HttpPut("api/bookrequests/ApproveRequest/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRequestModel request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // First, get the request details to add the book
                    string getRequestQuery = "SELECT Title, Author, SectionId FROM Books_Request_To_Add WHERE Id = @Id";
                    using (var cmd = new SqlCommand(getRequestQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return NotFound(new { message = "Request not found or already processed." });
                            }

                            // Read the request details
                            reader.Read();
                            string title = reader["Title"].ToString();
                            string author = reader["Author"].ToString();
                            int sectionId = Convert.ToInt32(reader["SectionId"]);

                            // Update the request state to 'Approved'
                            string updateQuery = "UPDATE Books_Request_To_Add SET State = 'Approved' WHERE Id = @Id";
                            using (var updateCmd = new SqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Id", id);
                                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                                if (rowsAffected <= 0)
                                {
                                    return BadRequest(new { message = "Failed to approve the request." });
                                }
                            }

                            // Now add the book to the Books table
                            string addBookQuery = @"
                        INSERT INTO [dbo].[Books] ([Title], [Author], [Description], [Quantity], [SectionId])
                        VALUES (@Title, @Author, @Description, @Quantity, @SectionId)";

                            using (var insertCmd = new SqlCommand(addBookQuery, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@Title", title);
                                insertCmd.Parameters.AddWithValue("@Author", author);
                                insertCmd.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value); // Using request description
                                insertCmd.Parameters.AddWithValue("@Quantity", request.Quantity); // Using request quantity
                                insertCmd.Parameters.AddWithValue("@SectionId", sectionId);

                                int rowsInserted = await insertCmd.ExecuteNonQueryAsync();

                                if (rowsInserted > 0)
                                {
                                    return Ok(new { message = "Request approved and book added successfully!" });
                                }
                                else
                                {
                                    return BadRequest(new { message = "Failed to add the book." });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", details = ex.Message });
            }
        }

        public class ApproveRequestModel
        {
            public string Description { get; set; }
            public int Quantity { get; set; }
        }




        public class AddBookRequest
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public int SectionId { get; set; }
        }



        #endregion

        #endregion


        #region Admin Part Mange 

        [Authorize(Roles = "Admin")]
        public IActionResult ManageSections()
        {
            List<BookSection> sections = new List<BookSection>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, SectionName FROM BookSection", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sections.Add(new BookSection
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                SectionName = reader["SectionName"].ToString()
                            });
                        }
                    }
                }
            }

            return View(sections);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult AddSection(BookSection section)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO BookSection (SectionName) VALUES (@SectionName)", conn))
                {
                    cmd.Parameters.AddWithValue("@SectionName", section.SectionName);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("ManageSections");
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult UpdateSection(BookSection section)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE BookSection SET SectionName = @SectionName WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", section.Id);
                    cmd.Parameters.AddWithValue("@SectionName", section.SectionName);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("ManageSections");
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult RemoveSection(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM BookSection WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("ManageSections");
        }


        #endregion
    }
}
