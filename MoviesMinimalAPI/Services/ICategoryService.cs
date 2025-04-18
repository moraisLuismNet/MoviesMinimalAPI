using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Services
{
    public interface ICategoryService : IService<CategoryDTO, CategoryCreateDTO, CategoryUpdateDTO, int>
    {
        Task<bool> ExistsByNameAsyncService(string name);
    }
}
