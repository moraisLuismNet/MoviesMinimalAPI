namespace MoviesMinimalAPI.DTOs
{
    public class MovieCreateDTO
    {
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public int Duration { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool AllPublic { get; set; } = true;
        public DateTime CreationDate { get; set; }
        public int categoryId { get; set; }
    }
}
