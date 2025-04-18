namespace MoviesMinimalAPI.DTOs
{
    public class UserDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "User";
        public byte[]? Salt { get; set; }
    }
}
