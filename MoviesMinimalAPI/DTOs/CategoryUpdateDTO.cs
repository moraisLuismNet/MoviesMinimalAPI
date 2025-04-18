using System.ComponentModel.DataAnnotations;

namespace MoviesMinimalAPI.DTOs
{
    public class CategoryUpdateDTO
    {
        public int IdCategory { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "The maximum number of characters is 100!")]
        public string? Name { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
