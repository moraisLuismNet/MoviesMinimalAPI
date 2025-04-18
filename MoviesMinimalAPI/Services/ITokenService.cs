using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Services
{
    public interface ITokenService
    {
        UserLoginResponseDTO GenerateTokenService(UserLoginDTO user);
    }
}
