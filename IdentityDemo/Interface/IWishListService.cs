using IdentityDemo.Models;

namespace IdentityDemo.Interface
{
    public interface IWishListService
    {
        Task<bool> AddItemToWishlistAsync(int itemId, string userId);
        Task<List<WishListModel>> GetWishlistItemsAsync(string userId);
        Task<bool> RemoveItemFromWishlistAsync(int itemId, string userId);
        Task<List<int>> GetItemIdsFromWishlistAsync(string userId);
    }
}
