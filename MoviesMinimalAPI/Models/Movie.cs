using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MoviesMinimalAPI.Models
{
    public class Movie
    {
        [Key]
        // Primary key
        public int IdMovie { get; set; }

        public string Name { get; set; }
        public string Synopsis { get; set; }
        public int Duration { get; set; }
        public string? RouteImage { get; set; }
        public bool AllPublic { get; set; }
        public DateTime CreationDate { get; set; }

        // Relationship with category
        public int categoryId { get; set; }
        [ForeignKey("categoryId")]
        public Category Category { get; set; }
    }
}
