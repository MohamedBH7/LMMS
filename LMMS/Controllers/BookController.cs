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

        private readonly string _connectionString = "Server=DESKTOP-8EVQE8E;Database=aspnet-LMMS-7fc7690b-1ba3-432f-9f4f-99c7a2dbeb77;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true;";

       
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
        //[Authorize]
        //public IActionResult MyBorrowedBooks()
        //{
        //    string userEmail = User.Identity?.Name;

        //    if (string.IsNullOrEmpty(userEmail))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    List<BorrowedBookViewModel> borrowedBooks = new List<BorrowedBookViewModel>();

        //    using (SqlConnection conn = new SqlConnection(_connectionString))
        //    {
        //        conn.Open();
        //        string query = @"
        //        SELECT bb.Id, bb.UserEmail, bb.BookId, bb.BorrowDate, bb.ReturnDate, bb.Status, b.Title AS BookTitle
        //        FROM BorrowedBooks bb
        //        JOIN Books b ON bb.BookId = b.Id
        //        WHERE bb.UserEmail = @UserEmail";

        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@UserEmail", userEmail);

        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    borrowedBooks.Add(new BorrowedBookViewModel
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        UserEmail = reader.GetString(1),
        //                        BookId = reader.GetInt32(2),
        //                        BorrowDate = reader.GetDateTime(3),
        //                        ReturnDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
        //                        Status = reader.GetString(5),
        //                        BookTitle = reader.GetString(6)
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return View(borrowedBooks);
        //}


        #region Action

        // Request to Borrow a Book
        [Authorize]
        [HttpPost]
        public IActionResult RequestBorrow(int bookId, string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "User not authenticated";
                return RedirectToAction("StudentBorrowRequests");
            }

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
                    {
                        TempData["ErrorMessage"] = "Book is not available for borrowing";
                        return RedirectToAction("StudentBorrowRequests");
                    }
                }

                //// Check if the user has already borrowed 3 books Approved || Pending
                //string checkQuery = "SELECT COUNT(*) FROM BorrowedBooks WHERE UserEmail = @UserEmail AND (Status = 'Approved' OR Status = 'Pending')";
                //using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                //{
                //    checkCmd.Parameters.AddWithValue("@UserEmail", userEmail);
                //    int count = (int)checkCmd.ExecuteScalar();
                //    if (count >= 3)
                //    {
                //        TempData["ErrorMessage"] = "You cannot borrow more than 3 books at the same time.";
                //        return RedirectToAction("StudentBorrowRequests");
                //    }
                //}


                //
                // Check the number of approved books
                string approvedQuery = "SELECT COUNT(*) FROM BorrowedBooks WHERE UserEmail = @UserEmail AND Status = 'Approved'";
                int approvedCount;
                using (SqlCommand approvedCmd = new SqlCommand(approvedQuery, conn))
                {
                    approvedCmd.Parameters.AddWithValue("@UserEmail", userEmail);
                    approvedCount = (int)approvedCmd.ExecuteScalar();
                }

                // Check the total number of approved and pending books
                string totalQuery = "SELECT COUNT(*) FROM BorrowedBooks WHERE UserEmail = @UserEmail AND (Status = 'Approved' OR Status = 'Pending')";
                int totalCount;
                using (SqlCommand totalCmd = new SqlCommand(totalQuery, conn))
                {
                    totalCmd.Parameters.AddWithValue("@UserEmail", userEmail);
                    totalCount = (int)totalCmd.ExecuteScalar();
                }

                if (approvedCount >= 3)
                {
                    TempData["ErrorMessage"] = "You cannot borrow more than 3 approved books at the same time.";
                    return RedirectToAction("StudentBorrowRequests");
                }
                else if (totalCount >= 3)
                {
                    // Allow the request but inform the user that they have reached the limit
                    TempData["WarningMessage"] = "Your borrow request has been submitted, but you have now reached the limit of 3 books (approved and pending).";
                     ;
                }
                // Insert borrow request
                string query = "INSERT INTO BorrowedBooks (UserEmail, BookId, Status) VALUES (@UserEmail, @BookId, 'Pending')";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0 && totalCount <= 3)
                    {
                        TempData["SuccessMessage"] = "Borrow request submitted successfully";
                    }
                    else if(rowsAffected ==0 || rowsAffected == null)
                    {
                        TempData["ErrorMessage"] = "Failed to submit borrow request";
                    }
                    return RedirectToAction("MyBorrowedBooks");
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

        [Authorize(Roles = "Admin")]

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

        
        [Authorize(Roles = "Admin")]

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

            // Sorting the list by Status: "Pending" first, then "Approved", then "Returned"
            var sortedBorrowedBooks = borrowedBooks
                .OrderBy(book => book.Status == "Pending" ? 1 : book.Status == "Approved" ? 2 : 3)
                .ThenBy(book => book.BorrowDate)
                .ToList();

            return View(sortedBorrowedBooks);
        }
        [HttpPost]
        public IActionResult ReturnBook(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Begin a transaction to ensure both updates are applied atomically
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Update the status and return date of the borrow request
                        string updateBorrowQuery = "UPDATE BorrowedBooks SET ReturnDate = @ReturnDate, Status = 'Returned' WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(updateBorrowQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Id", bookId);
                            cmd.ExecuteNonQuery();
                        }

                        // Increase the quantity of the book by 1
                        string updateBookQuery = @"
                UPDATE Books
                SET Quantity = Quantity + 1
                WHERE Id = (SELECT BookId FROM BorrowedBooks WHERE Id = @Id)";
                        using (SqlCommand cmd = new SqlCommand(updateBookQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", bookId);
                            cmd.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction if any error occurs
                        transaction.Rollback();
                        throw;
                    }
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

                // Begin a transaction to ensure both updates are applied atomically
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Update the status of the borrow request
                        string updateBorrowQuery = "UPDATE BorrowedBooks SET Status = 'Approved' WHERE Id = @BorrowId";
                        using (SqlCommand cmd = new SqlCommand(updateBorrowQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        // Decrease the quantity of the book by 1
                        string updateBookQuery = @"
                        UPDATE Books
                        SET Quantity = Quantity - 1
                        WHERE Id = (SELECT BookId FROM BorrowedBooks WHERE Id = @BorrowId)";
                        using (SqlCommand cmd = new SqlCommand(updateBookQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction if any error occurs
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return RedirectToAction("ApproveBorrowRequests");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult MarkAsReturned(int borrowId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Begin a transaction to ensure both updates are applied atomically
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Update the status and return date of the borrow request
                        string updateBorrowQuery = "UPDATE BorrowedBooks SET Status = 'Returned', ReturnDate = @ReturnDate WHERE Id = @BorrowId";
                        using (SqlCommand cmd = new SqlCommand(updateBorrowQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        // Increase the quantity of the book by 1
                        string updateBookQuery = @"
                        UPDATE Books
                        SET Quantity = Quantity + 1
                        WHERE Id = (SELECT BookId FROM BorrowedBooks WHERE Id = @BorrowId)";
                        using (SqlCommand cmd = new SqlCommand(updateBookQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction if any error occurs
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return RedirectToAction("ApproveBorrowRequests");
        }



        [HttpPost]
        public IActionResult RemovePendingRequest(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "DELETE FROM BorrowedBooks WHERE Id = @BookId AND Status = 'Pending'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Pending borrow request removed successfully";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to remove pending borrow request";
                    }
                }
            }
            return RedirectToAction("MyBorrowedBooks");
        }

        [HttpGet]
        public IActionResult MyBorrowedBooks()
        {
            var borrowedBooks = new List<BorrowedBookViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                SELECT bb.Id, bb.UserEmail, bb.BookId, bb.BorrowDate, bb.ReturnDate, bb.Status, b.Title AS BookTitle
                FROM BorrowedBooks bb
                JOIN Books b ON bb.BookId = b.Id
                WHERE bb.UserEmail = @UserEmail"; using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserEmail", User.Identity.Name);
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
                               Status = reader.GetString(5),
                               BookTitle = reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return View(borrowedBooks);
        }
        #endregion
    }
}
