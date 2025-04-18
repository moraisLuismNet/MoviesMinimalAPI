namespace MoviesMinimalAPI.Services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        public FileManagerService(IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task DeleteFile(string route, string folder)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                return;
            }

            try
            {
                var fileName = Path.GetFileName(route);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                string directoryFile = Path.Combine(env.WebRootPath, folder, fileName);

                if (File.Exists(directoryFile))
                {
                    File.Delete(directoryFile);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error deleting file: {ex.Message}");
                throw; 
            }
        }


        public async Task<string> SaveFile(byte[] content, string extension, string folder, string contentType)
        {
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentNullException(nameof(folder), "Folder path cannot be null or empty");

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension), "File extension cannot be null or empty");

            var fileName = $"{Guid.NewGuid()}{extension}";
            string folderF = Path.Combine(env.WebRootPath, folder);

            if (!Directory.Exists(folderF))
            {
                Directory.CreateDirectory(folderF);
            }

            string route = Path.Combine(folderF, fileName);
            await File.WriteAllBytesAsync(route, content);

            var currentUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
            var urlForBD = Path.Combine(currentUrl, folder, fileName).Replace("\\", "/");
            return urlForBD;
        }

        public async Task<string> EditFile(byte[] content, string extension, string folder, string route, string contentType)
        {
            await DeleteFile(route, folder);
            return await SaveFile(content, extension, folder, contentType);
        }
    }
}
