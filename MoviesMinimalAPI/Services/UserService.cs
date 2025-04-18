using AutoMapper;
using MoviesMinimalAPI.DTOs;
using MoviesMinimalAPI.Models;
using MoviesMinimalAPI.Repository;

namespace MoviesMinimalAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly HashService _hashService;
        private readonly IMapper _mapper;

        public List<string> Errors { get; } = new List<string>();

        public UserService(IUserRepository userRepository, HashService hashService, IMapper mapper)
        {
            _userRepository = userRepository;
            _hashService = hashService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetUserService()
        {
            var users = await _userRepository.GetUserRepository();
            return users.Select(user => _mapper.Map<UserDTO>(user));
        }


        public async Task<User?> GetByEmailUserService(string email)
        {
            return await _userRepository.GetByEmailUserRepository(email);
        }


        public bool VerifyPasswordUserService(string password, User user)
        {
            var hashResult = _hashService.Hash(password, user.Salt);
            return user.Password == hashResult.Hash;
        }


        public bool ValidateUserService(UserRegistrationDTO userRegistrationDTO)
        {
            if (string.IsNullOrWhiteSpace(userRegistrationDTO.Email) || string.IsNullOrWhiteSpace(userRegistrationDTO.Password))
            {
                Errors.Add("Email and password required");
                return false;
            }
            return true;
        }


        public async Task<UserDTO> AddUserService(UserRegistrationDTO userRegistrationDTO)
        {
            var resultHash = _hashService.Hash(userRegistrationDTO.Password);
            var user = new User
            {
                Email = userRegistrationDTO.Email,
                Password = resultHash.Hash,
                Salt = resultHash.Salt,
                Role = userRegistrationDTO.Role
            };

            await _userRepository.AddUserRepository(user);
            await _userRepository.SaveUserRepository();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> ChangePasswordUserService(string email, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetByEmailUserRepository(email);
            if (user == null)
            {
                return false;
            }

            bool isValidPassword = VerifyPasswordUserService(oldPassword, user);
            if (!isValidPassword)
            {
                return false;
            }

            var resultHash = _hashService.Hash(newPassword);
            user.Password = resultHash.Hash;
            user.Salt = resultHash.Salt;

            _userRepository.UpdateUserRepository(user);
            await _userRepository.SaveUserRepository();
            return true;
        }


        public async Task<UserDTO> DeleteUserService(string email)
        {
            var user = await _userRepository.GetByEmailUserRepository(email);
            if (user == null)
            {
                return null;
            }

            _userRepository.DeleteUserRepository(user);
            await _userRepository.SaveUserRepository();

            return _mapper.Map<UserDTO>(user);
        }
    }
}
