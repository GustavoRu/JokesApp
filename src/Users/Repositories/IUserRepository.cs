using BackendApi.Users.DTOs;
using BackendApi.Users.Models;
namespace BackendApi.Users.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAll();
        Task<UserModel> GetById(int id);
        Task Create(UserModel user);
        void Update(UserModel user);
        void Delete(UserModel user);

        Task Save();
        IEnumerable<UserModel> Search(Func<UserModel, bool> filter);
    }
}