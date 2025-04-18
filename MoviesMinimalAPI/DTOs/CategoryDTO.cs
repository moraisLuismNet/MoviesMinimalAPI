using System.ComponentModel.DataAnnotations;

namespace MoviesMinimalAPI.DTOs
{
    public class CategoryDTO
    {
        /* To view the Category List, view a category by ID, check a category by ID, check a category 
        by name, update a category, and delete a category */
        public int IdCategory { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "The maximum number of characters is 100!")]
        public string? Name { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
