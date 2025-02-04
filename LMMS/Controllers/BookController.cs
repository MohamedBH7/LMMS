using LMMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LMMS.Controllers
{
    [Authorize] // Ensure only logged-in users can borrow books
    public class BookController : Controller
    {
        private readonly string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-LMMS-7fc7690b-1ba3-432f-9f4f-99c7a2dbeb77;Trusted_Connection=True;MultipleActiveResultSets=true";

       
        public IActionResult Index()
        {
            return View();

        }
        public IActionResult StudentBorrowRequests()
        {
            List<BookViewModel> books = new List<BookViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Title, Author, Quantity FROM Books WHERE Quantity > 0"; // Show only available books
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(new BookViewModel
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Author = reader.GetString(2),
                                Quantity = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return View(books);
        }
        [Authorize]
        public IActionResult MyBorrowedBooks()
        {
            string userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            List<BorrowedBookViewModel> borrowedBooks = new List<BorrowedBookViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                SELECT bb.Id, bb.UserEmail, bb.BookId, bb.BorrowDate, bb.ReturnDate, bb.Status, b.Title AS BookTitle
                FROM BorrowedBooks bb
                JOIN Books b ON bb.BookId = b.Id
                WHERE bb.UserEmail = @UserEmail";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserEmail", userEmail);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            borrowedBooks.Add(new BorrowedBookViewModel
                            {
                                Id = reader.GetInt32(0),
                                UserEmail = reader.GetString(1),
                                BookId = reader.GetInt32(2),
                                BorrowDate = reader.GetDateTime(3),
                                ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                Status = reader.GetString(5),
                                BookTitle = reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return View(borrowedBooks);
        }


        #region Action

        // Request to Borrow a Book
        [HttpPost]
        public IActionResult RequestBorrow(int bookId, string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
                return Json(new { success = false, message = "User not authenticated" });

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Check if the book exists and has a quantity greater than zero
                string bookQuery = "SELECT Quantity FROM Books WHERE Id = @BookId";
                using (SqlCommand bookCmd = new SqlCommand(bookQuery, conn))
                {
                    bookCmd.Parameters.AddWithValue("@BookId", bookId);
                    int quantity = (int)bookCmd.ExecuteScalar();
                    if (quantity <= 0)
                        return Json(new { success = false, message = "Book is not available for borrowing" });
                }

                // Check if the user has already borrowed 3 books
                string checkQuery = "SELECT COUNT(*) FROM BorrowedBooks WHERE UserEmail = @UserEmail AND Status = 'Approved'";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserEmail", userEmail);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count >= 3)
                        return Json(new { success = false, message = "You cannot borrow more than 3 books at the same time" });
                }

                // Insert borrow request
                string query = "INSERT INTO BorrowedBooks (UserEmail, BookId, Status) VALUES (@UserEmail, @BookId, 'Pending')";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    //return MyBorrowedBooks();
                    return RedirectToAction("BorrowedBooks");
                }
            }
        }

        // Approve Borrow Request (Admin Function)
        [HttpPost]
        public IActionResult ApproveBorrow(int borrowId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE BorrowedBooks SET Status = 'Approved' WHERE Id = @BorrowId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return Json(new { success = rowsAffected > 0, message = rowsAffected > 0 ? "Borrow request approved" : "Failed to approve request" });
                }
            }
        }

        // Get Pending Borrow Requests (For Admin)
        public IActionResult BorrowRequests()
        {
            List<BorrowedBookViewModel> borrowRequests = new List<BorrowedBookViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT bb.Id, bb.UserEmail, bb.BorrowDate, bb.Status, b.Title 
                    FROM BorrowedBooks bb
                    INNER JOIN Books b ON bb.BookId = b.Id
                    WHERE bb.Status = 'Pending'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            borrowRequests.Add(new BorrowedBookViewModel
                            {
                                Id = reader.GetInt32(0),
                                UserEmail = reader.GetString(1),
                                BorrowDate = reader.GetDateTime(2),
                                Status = reader.GetString(3),
                                BookTitle = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return View(borrowRequests);
        }


        [HttpGet]
        public IActionResult BorrowedBooks()
        {
            var borrowedBooks = new List<BorrowedBookViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Id, UserEmail, BookId, BorrowDate, ReturnDate, Status FROM BorrowedBooks";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            borrowedBooks.Add(new BorrowedBookViewModel
                            {
                                Id = reader.GetInt32(0),
                                UserEmail = reader.GetString(1),
                                BookId = reader.GetInt32(2),
                                BorrowDate = reader.GetDateTime(3),
                                ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                Status = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return View(borrowedBooks);
        }
        
        [HttpPost]
        public IActionResult ReturnBook(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE BorrowedBooks SET ReturnDate = @ReturnDate, Status = 'Returned' WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Id", bookId);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("BorrowedBooks");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult ApproveBorrowRequests()
        {
            List<AdminBorrowedBookViewModel> borrowRequests = new List<AdminBorrowedBookViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                SELECT bb.Id, bb.UserEmail, bb.BookId, bb.BorrowDate, bb.ReturnDate, bb.Status, b.Title AS BookTitle
                FROM BorrowedBooks bb
                JOIN Books b ON bb.BookId = b.Id
                WHERE bb.Status = 'Pending'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            borrowRequests.Add(new AdminBorrowedBookViewModel
                            {
                                Id = reader.GetInt32(0),
                                UserEmail = reader.GetString(1),
                                BookId = reader.GetInt32(2),
                                BorrowDate = reader.GetDateTime(3),
                                ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                Status = reader.GetString(5),
                                BookTitle = reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return View(borrowRequests);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ApproveRequest(int borrowId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE BorrowedBooks SET Status = 'Approved' WHERE Id = @BorrowId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                    cmd.ExecuteNonQuery();
                }
            }

            //return RedirectToAction("PendingBorrowRequests");
        

            return RedirectToAction("ApproveBorrowRequests");
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public IActionResult MarkAsReturned(int borrowId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE BorrowedBooks SET Status = 'Returned', ReturnDate = @ReturnDate WHERE Id = @BorrowId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("ApproveBorrowRequests");
        }
        #endregion
    }
}
