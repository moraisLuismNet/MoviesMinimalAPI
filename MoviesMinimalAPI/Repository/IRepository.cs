namespace MoviesMinimalAPI.Repository
{
    public interface IRepository<TEntity, TKey>
    {
        Task<List<TEntity>> GetAllRepository();
        Task <TEntity> GetByIdRepository(TKey id);
        Task AddRepository(TEntity entity);
        void UpdateRepository(TEntity entity);
        void DeleteRepository(TEntity entity);
        bool ExistsByIdRepository(TKey id);
        Task SaveRepository();
    }
}
