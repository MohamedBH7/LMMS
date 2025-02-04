namespace LMMS.Models
{
    public class ReturnBookViewModel
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
    }
}
