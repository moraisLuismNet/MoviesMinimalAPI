using MoviesMinimalAPI.Models;

namespace MoviesMinimalAPI.Repository
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        // Check Category by Name
        bool ExistsByNameRepository(string name);
    }
}
