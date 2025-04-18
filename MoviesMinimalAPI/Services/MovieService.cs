using AutoMapper;
using MoviesMinimalAPI.DTOs;
using MoviesMinimalAPI.Models;
using MoviesMinimalAPI.Repository;

namespace MoviesMinimalAPI.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMapper _mapper;
        private readonly IFileManagerService _fileManagerService;

        public MovieService(IMovieRepository movieRepository, IMapper mapper, IFileManagerService fileManagerService)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
            _fileManagerService = fileManagerService;
        }

        public async Task<List<MovieDTO>> GetAllAsyncService()
        {
            var movies = await _movieRepository.GetAllRepository();
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        public async Task<MovieDTO> GetByIdAsyncService(int movieId)
        {
            var movie = await _movieRepository.GetByIdRepository(movieId);
            return _mapper.Map<MovieDTO>(movie);
        }

        public async Task<IEnumerable<MovieDTO>> GetByCategoryAsyncService(int categoryId)
        {
            var movies = await _movieRepository.GetByCategoryRepository(categoryId);
            return _mapper.Map<IEnumerable<MovieDTO>>(movies);
        }

        public async Task<IEnumerable<MovieDTO>> SearchByNameAsyncService(string name)
        {
            var movies = await _movieRepository.SearchByNameRepository(name);
            return _mapper.Map<IEnumerable<MovieDTO>>(movies);
        }

        public async Task<MovieDTO> CreateAsyncService(MovieCreateDTO movieCreateDTO)
        {
            if (await ExistsByNameAsyncService(movieCreateDTO.Name))
            {
                throw new InvalidOperationException("The film already exists");
            }

            var movie = _mapper.Map<Movie>(movieCreateDTO);

            if (movieCreateDTO.ImageFile is not null)
            {
                movie.RouteImage = await ProcessImage(movieCreateDTO.ImageFile);
            }
            movie.CreationDate = DateTime.Now;

            await _movieRepository.AddRepository(movie);
            await _movieRepository.SaveRepository();

            return _mapper.Map<MovieDTO>(movie);
        }

        public async Task UpdateAsyncService(int movieId, MovieUpdateDTO movieUpdateDTO)
        {
            if (movieId != movieUpdateDTO.IdMovie)
            {
                throw new ArgumentException("ID mismatch");
            }

            var existingMovie = await _movieRepository.GetByIdRepository(movieId);
            if (existingMovie == null)
            {
                throw new KeyNotFoundException($"Movie with ID {movieId} not found");
            }

            // Map changes to the existing model
            _mapper.Map(movieUpdateDTO, existingMovie);

            if (movieUpdateDTO.ImageFile is not null)
            {
                try
                {
                    // Process new image and delete the old one if it exists
                    existingMovie.RouteImage = await ProcessImage(movieUpdateDTO.ImageFile, existingMovie.RouteImage);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error processing image", ex);
                }
            }

            existingMovie.CreationDate = DateTime.Now;

            _movieRepository.UpdateRepository(existingMovie);
            await _movieRepository.SaveRepository();
        }

        public async Task<bool> DeleteAsyncService(int movieId)
        {
            var movie = await _movieRepository.GetByIdRepository(movieId);
            if (movie == null) return false;

            _movieRepository.DeleteRepository(movie);

            if (!string.IsNullOrWhiteSpace(movie.RouteImage))
            {
                await _fileManagerService.DeleteFile(movie.RouteImage, "img");
            }

            await _movieRepository.SaveRepository();
            return true;
        }

        public async Task<bool> ExistsByIdAsyncService(int movieId)
        {
            return await Task.FromResult(_movieRepository.ExistsByIdRepository(movieId));
        }

        public async Task<bool> ExistsByNameAsyncService(string name)
        {
            return await Task.FromResult(_movieRepository.ExistsByNameRepository(name));
        }

        private async Task<string> ProcessImage(IFormFile imageFile, string existingImagePath = null)
        {
            if (!string.IsNullOrWhiteSpace(existingImagePath))
            {
                try
                {
                    await _fileManagerService.DeleteFile(existingImagePath, "img");
                }
                catch (Exception ex)
                {
                    // Log the error but continue with the process
                    Console.WriteLine($"Error deleting old image: {ex.Message}");
                }
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            var content = memoryStream.ToArray();
            var extension = Path.GetExtension(imageFile.FileName);
            var contentType = imageFile.ContentType;

            return await _fileManagerService.SaveFile(content, extension, "img", contentType);
        }
    }
}