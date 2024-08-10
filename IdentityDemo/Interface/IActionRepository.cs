using IdentityDemo.Models;

namespace IdentityDemo.Interface
{
    public interface IActionRepository
    {
        Task Add(ActionLog log);
    }
}
