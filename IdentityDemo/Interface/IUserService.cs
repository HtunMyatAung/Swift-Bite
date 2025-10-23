using IdentityDemo.Models;

namespace IdentityDemo.Interface
{
    public interface IUserService
    {
        IEnumerable<ApplicationUser> GetAllUser();

        Task<int> GetUserCount();

        Task DeleteUserAsync(string userId);

    }
}
