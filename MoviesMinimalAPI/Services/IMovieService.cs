using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Services
{
    public interface IMovieService : IService<MovieDTO, MovieCreateDTO, MovieUpdateDTO, int>
    {
        Task<IEnumerable<MovieDTO>> GetByCategoryAsyncService(int categoryId);
        Task<IEnumerable<MovieDTO>> SearchByNameAsyncService(string name);
        Task<bool> ExistsByNameAsyncService(string name);
    }
}
