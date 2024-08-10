using IdentityDemo.Models;

namespace IdentityDemo.Interface
{
    public interface IWishListRepository
    {
        Task AddWisthItemAsync(WishListModel item);
        Task<WishListModel> GetWishlistItemAsync(int itemId, string userId);
        Task<List<WishListModel>> GetWishlistItemsAsync(string userId);
        Task<bool> RemoveItemAsync(int itemId, string userId);
        Task<List<int>> GetItemIdsByUserIdAsync(string userId);
    }
}
