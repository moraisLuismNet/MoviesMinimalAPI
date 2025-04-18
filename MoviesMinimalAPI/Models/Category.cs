using System.ComponentModel.DataAnnotations;

namespace MoviesMinimalAPI.Models
{
    public class Category
    {
        [Key]
        // Indicates that the property is the auto-incrementing primary key
        public int IdCategory { get; set; }

        [Required]
        // Required field
        public string? Name { get; set; }

        [Required]
        // Required field
        public DateTime CreationDate { get; set; }
    }
}
