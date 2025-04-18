using Microsoft.EntityFrameworkCore;
using MoviesMinimalAPI.Models;

namespace MoviesMinimalAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly MoviesMinimalAPIDbContext _context;

        public UserRepository(MoviesMinimalAPIDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailUserRepository(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<IEnumerable<User>> GetUserRepository()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> UserExistsUserRepository(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserRepository(User entity)
        {
            await _context.Users.AddAsync(entity);
        }

        public void UpdateUserRepository(User entity)
        {
            var existingUser = _context.Users.AsTracking().FirstOrDefault(u => u.Email == entity.Email);

            if (existingUser != null)
            {
                // If the user exists, we update its values
                _context.Entry(existingUser).CurrentValues.SetValues(entity);
            }
            else
            {
                // If the user does not exist, we add it
                _context.Users.Update(entity);
            }
        }

        public void DeleteUserRepository(User entity)
        {
            _context.Users.Remove(entity);
        }

        public async Task SaveUserRepository()
        {
            await _context.SaveChangesAsync();
        }
    }
}
