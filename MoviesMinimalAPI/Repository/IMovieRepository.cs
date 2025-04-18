using MoviesMinimalAPI.Models;

namespace MoviesMinimalAPI.Repository
{
    public interface IMovieRepository : IRepository<Movie, int>
    {
        // List of Movies by Category
        Task <IEnumerable<Movie>> GetByCategoryRepository(int categoryId);

        // Search Movie by Name
        Task <IEnumerable<Movie>> SearchByNameRepository(string name);

        // Check Movie By Name
        bool ExistsByNameRepository(string name);
    }
}
