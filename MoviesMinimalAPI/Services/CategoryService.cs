using AutoMapper;
using MoviesMinimalAPI.DTOs;
using MoviesMinimalAPI.Models;
using MoviesMinimalAPI.Repository;

namespace MoviesMinimalAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDTO>> GetAllAsyncService()
        {
            var categories = await _categoryRepository.GetAllRepository();
            return _mapper.Map<List<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> GetByIdAsyncService(int id)
        {
            var category = await _categoryRepository.GetByIdRepository(id);
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> CreateAsyncService(CategoryCreateDTO categoryCreateDTO)
        {
            if (await ExistsByNameAsyncService(categoryCreateDTO.Name))
            {
                throw new InvalidOperationException("The category already exists");
            }

            var category = _mapper.Map<Category>(categoryCreateDTO);
            category.CreationDate = DateTime.Now;

            await _categoryRepository.AddRepository(category);
            await _categoryRepository.SaveRepository();

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task UpdateAsyncService(int categoryId, CategoryUpdateDTO categoryUpdateDTO)
        {
            if (categoryId != categoryUpdateDTO.IdCategory)
            {
                throw new ArgumentException("ID mismatch");
            }

            var existingCategory = await _categoryRepository.GetByIdRepository(categoryId);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");
            }

            var category = _mapper.Map<Category>(categoryUpdateDTO);
            category.CreationDate = DateTime.Now;

            _categoryRepository.UpdateRepository(category);
            await _categoryRepository.SaveRepository();
        }

        public async Task<bool> DeleteAsyncService(int categoryId)
        {
            var category = await _categoryRepository.GetByIdRepository(categoryId);
            if (category == null)
            {
                return false;
            }

            _categoryRepository.DeleteRepository(category);
            await _categoryRepository.SaveRepository();
            return true;
        }

        public async Task<bool> ExistsByNameAsyncService(string name)
        {
            return await Task.FromResult(_categoryRepository.ExistsByNameRepository(name));
        }

        public async Task<bool> ExistsByIdAsyncService(int id)
        {
            return await Task.FromResult(_categoryRepository.ExistsByIdRepository(id));
        }
    }
}
