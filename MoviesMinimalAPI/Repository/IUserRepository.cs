using MoviesMinimalAPI.Models;

namespace MoviesMinimalAPI.Repository
{
    public interface IUserRepository
    {
        // List of Users
        Task<IEnumerable<User>> GetUserRepository();

        // Get User By Email
        Task<User?> GetByEmailUserRepository(string email);

        // Validate if the user exists with that email
        Task<bool> UserExistsUserRepository(string email);

        Task AddUserRepository(User entity);
        void UpdateUserRepository(User entity);
        void DeleteUserRepository(User entity);
        Task SaveUserRepository();
    }
}
