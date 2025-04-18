namespace MoviesMinimalAPI.Services
{
    public interface IService<TDto, TCreateDto, TUpdateDto, TKey>
    {
        Task<List<TDto>> GetAllAsyncService();
        Task<TDto> GetByIdAsyncService(TKey id);
        Task<TDto> CreateAsyncService(TCreateDto createDto);
        Task UpdateAsyncService(TKey id, TUpdateDto dto);
        Task<bool> DeleteAsyncService(TKey id);
        Task<bool> ExistsByIdAsyncService(TKey id);
    }
}
