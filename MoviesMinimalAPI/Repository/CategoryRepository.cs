using Microsoft.EntityFrameworkCore;
using MoviesMinimalAPI.Models;

namespace MoviesMinimalAPI.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MoviesMinimalAPIDbContext _context;

        public CategoryRepository(MoviesMinimalAPIDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllRepository()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category> GetByIdRepository(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.IdCategory == id);
        }

        public async Task AddRepository(Category entity)
        {
            entity.CreationDate = DateTime.Now;
            await _context.Categories.AddAsync(entity);
        }

        public void UpdateRepository(Category entity)
        {
            entity.CreationDate = DateTime.Now;
            var existingEntity = _context.Categories
                .AsTracking()
                .FirstOrDefault(c => c.IdCategory == entity.IdCategory);

            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                _context.Categories.Update(entity);
            }
        }

        public void DeleteRepository(Category entity)
        {
            _context.Categories.Remove(entity);
        }

        public bool ExistsByIdRepository(int id)
        {
            return _context.Categories.Any(c => c.IdCategory == id);
        }

        public bool ExistsByNameRepository(string name)
        {
            return _context.Categories
                .Any(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
        }

        public async Task SaveRepository()
        {
            await _context.SaveChangesAsync();
        }
    }
}