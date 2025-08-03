using BackendApi.Users.DTOs;
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


        bool Validate(UserInsertDto dto);
        bool Validate(UserUpdateDto dto);
    }
}