using System;

namespace LMMS.Models
{
    public class BookRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? PublishedYear { get; set; }
        public string Email { get; set; }
        public DateTime RequestDate { get; set; }
        public string State { get; set; }

        // Foreign key to BookSection
        public int SectionId { get; set; }
        public string SectionName { get; set; }  // To display the section name in the view
    }
}
