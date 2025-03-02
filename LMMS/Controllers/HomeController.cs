using System.Diagnostics;
using LMMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LMMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString = "Server=DESKTOP-8EVQE8E;Database=aspnet-LMMS-7fc7690b-1ba3-432f-9f4f-99c7a2dbeb77;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true;";
        bool IsDevelopment = false;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public IActionResult Index()
        {
            IsDevelopment = false;
            if (IsDevelopment)
            {
                #region Only For Dev Make it true
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Books')BEGIN CREATE TABLE Books (Id INT IDENTITY(1,1) PRIMARY KEY,Title NVARCHAR(255) NOT NULL,Author NVARCHAR(255) NOT NULL,Description NVARCHAR(MAX),Quantity INT NOT NULL);END";
                    // Example SQL query in development
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        int count = (int)cmd.ExecuteScalar();
                        ViewBag.TableCrearte = count; // Store result for debugging
                    }
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO Books (Title, Author, Description, Quantity) VALUES('The Great Gatsby', 'F. Scott Fitzgerald', 'A novel set in the 1920s about the American dream.', 5),('To Kill a Mockingbird', 'Harper Lee', 'A story about racial injustice in the Deep South.', 3),('1984', 'George Orwell', 'A dystopian novel about totalitarianism and surveillance.', 4),('Pride and Prejudice', 'Jane Austen', 'A romantic novel that critiques the British landed gentry.', 2),('The Catcher in the Rye', 'J.D. Salinger', 'A tale of teenage angst and alienation.', 6),('Moby Dick', 'Herman Melville', 'An epic tale of obsession and revenge.', 2),('War and Peace', 'Leo Tolstoy', 'A historical novel that intertwines the lives of several families.', 1),('The Odyssey', 'Homer', 'An ancient Greek epic poem about Odysseus'' journey home.', 3),('The Hobbit', 'J.R.R. Tolkien', 'A fantasy novel about a hobbit''s adventure.', 7),('Fahrenheit 451', 'Ray Bradbury', 'A dystopian novel about a future where books are banned.', 4),('Jane Eyre', 'Charlotte Brontë', 'A novel about a young governess and her growth.', 5),('The Picture of Dorian Gray', 'Oscar Wilde', 'A story about vanity and moral duplicity.', 3),('Brave New World', 'Aldous Huxley', 'A dystopian novel about a genetically engineered society.', 2),('The Brothers Karamazov', 'Fyodor Dostoevsky', 'A philosophical novel exploring morality and faith.', 1),('The Alchemist', 'Paulo Coelho', 'A novel about a shepherd''s journey to find his personal legend.', 6);";
                    // Example SQL query in development
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        int DataInsert = (int)cmd.ExecuteScalar();
                        ViewBag.DataInsert = DataInsert; // Store result for debugging
                    }
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BorrowedBooks') BEGIN CREATE TABLE BorrowedBooks (Id INT IDENTITY(1,1) PRIMARY KEY, UserEmail NVARCHAR(255) NOT NULL, BookId INT NOT NULL, BorrowDate DATETIME DEFAULT GETDATE(), ReturnDate DATETIME NULL, Status NVARCHAR(50) CHECK (Status IN ('Pending', 'Approved', 'Returned')) NOT NULL DEFAULT 'Pending', FOREIGN KEY (BookId) REFERENCES Books(Id)); END";
                    // Example SQL query in development
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        int BorrowedBooks = (int)cmd.ExecuteScalar();
                        ViewBag.BorrowedBooks = BorrowedBooks; // Store result for debugging
                    }
                }
                #endregion
            }
            return View();
        }

        public IActionResult Profile()   // profile Page
        {
            return View();
        }
        public IActionResult Report()   // Report Page
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Manage()   // manage Page
        {

     
                List<BookViewModel> books = new List<BookViewModel>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Id, Title, Author, Description, Quantity FROM Books";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(new BookViewModel
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Author = reader.GetString(2),
                                    Description = reader.GetString(3),
                                    Quantity = reader.GetInt32(4)
                                });
                            }
                        }
                    }
                }

                return View(books);
        }

        public IActionResult Books()   // Books Page
        {
            return View();
        }
        public IActionResult RentedBooks()   // Books Page
        {
            return View();
        }
        
        public IActionResult Dashboard() // Dashboard Page
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Actions 

        // Manage Book (Insert, Update, Delete)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult ManageBook(string title, string author, string description, int quantity, string action, int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = string.Empty;
                if (action == "Insert")
                {
                    query = "INSERT INTO Books (Title, Author, Description, Quantity) VALUES (@Title, @Author, @Description, @Quantity)";
                    TempData["SuccessMessage"] = "Book inserted successfully";
                }
                else if (action == "Update")
                {
                    query = "UPDATE Books SET Title = @Title, Author = @Author, Description = @Description, Quantity = @Quantity WHERE Id = @Id";
                    TempData["WarningMessage"] = "Book updated successfully";
                }
                else if (action == "Delete")
                {
                    query = "DELETE FROM Books WHERE Id = @Id";
                    TempData["ErrorMessage"] = "Book deleted successfully";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Author", author);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    if (action == "Update" || action == "Delete")
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Manage");
        }

        #endregion
    }
}
