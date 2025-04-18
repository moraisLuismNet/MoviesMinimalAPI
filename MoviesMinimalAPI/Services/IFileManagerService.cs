namespace MoviesMinimalAPI.Services
{
    public interface IFileManagerService
    {
        Task<string> EditFile(byte[] content, string extension, string folder, string route,
            string contentType);
        Task DeleteFile(string route, string folder);
        Task<string> SaveFile(byte[] content, string extension, string folder, string contentType);
    }
}
