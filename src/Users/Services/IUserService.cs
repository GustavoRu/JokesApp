using BackendApi.Users.DTOs;
using BackendApi.Users.Models;

namespace BackendApi.Users.Services
{
    public interface IUserService
    {
        public List<string> Errors { get; }
        Task<IEnumerable<UserDto>> GetAll();
        Task<UserDto> GetById(int id);
        Task<UserDto> Create(UserInsertDto userInsertDto);
        Task<UserDto> Update(int id, UserUpdateDto userUpdateDto);
        Task<UserDto> Delete(int id);
        Task<UserModel> GetUserByEmailAsync(string email);
        Task<UserModel> CreateUserAsync(UserModel user);

        bool Validate(UserInsertDto dto);
        bool Validate(UserUpdateDto dto);
    }
}