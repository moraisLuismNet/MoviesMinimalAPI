using Microsoft.EntityFrameworkCore;
using MoviesMinimalAPI.Models;

namespace MoviesMinimalAPI.Repository
{
    public class MovieRepository : IMovieRepository
    {
        private readonly MoviesMinimalAPIDbContext _context;

        public MovieRepository(MoviesMinimalAPIDbContext context)
        {
            _context = context;
        }

        public async Task<List<Movie>> GetAllRepository()
        {
            return await _context.Movies
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Movie> GetByIdRepository(int id)
        {
            return await _context.Movies
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.IdMovie == id);
        }

        public async Task<IEnumerable<Movie>> GetByCategoryRepository(int categoryId)
        {
            return await _context.Movies
                .Include(m => m.Category)
                .Where(m => m.categoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> SearchByNameRepository(string name)
        {
            IQueryable<Movie> query = _context.Movies.Include(m => m.Category);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(m =>
                    m.Name.Contains(name) ||
                    m.Synopsis.Contains(name));
            }

            return await query.ToListAsync();
        }

        public async Task AddRepository(Movie entity)
        {
            entity.CreationDate = DateTime.Now;
            await _context.Movies.AddAsync(entity);
        }

        public void UpdateRepository(Movie entity)
        {
            entity.CreationDate = DateTime.Now;
            var existingEntity = _context.Movies
                .AsTracking()
                .FirstOrDefault(m => m.IdMovie == entity.IdMovie);

            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                _context.Movies.Update(entity);
            }
        }

        public void DeleteRepository(Movie entity)
        {
            _context.Movies.Remove(entity);
        }

        public bool ExistsByIdRepository(int id)
        {
            return _context.Movies.Any(m => m.IdMovie == id);
        }

        public bool ExistsByNameRepository(string name)
        {
            return _context.Movies.Any(m => EF.Functions.Like(m.Name, $"%{name.ToLower()}%"));
        }

        public async Task SaveRepository()
        {
            await _context.SaveChangesAsync();
        }
        
    }
}
