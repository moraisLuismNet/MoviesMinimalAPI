namespace MoviesMinimalAPI.DTOs
{
    public class MovieUpdateDTO
    {
        public int IdMovie { get; set; }
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public int Duration { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool AllPublic { get; set; }
        public DateTime CreationDate { get; set; }
        public int categoryId { get; set; }
    }
}
