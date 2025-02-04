namespace LMMS.Models
{
    public class AdminBorrowedBookViewModel
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
        public string BookTitle { get; set; } // Assuming you want to show the book title as well
    }
}
