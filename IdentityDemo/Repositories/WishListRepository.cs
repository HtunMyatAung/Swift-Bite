using IdentityDemo.Data;
using IdentityDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemo.Repositories
{
    public class WishListRepository:IWishListRepository
    {
        private readonly AppDbContext _context;
        public WishListRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<int>> GetItemIdsByUserIdAsync(string userId)
        {
            return await _context.WishList
                .Where(w => w.UserId == userId)
                .Select(w => w.ItemId)
                .ToListAsync();
        }
        public async Task<bool> RemoveItemAsync(int itemId, string userId)
        {
            try
            {
                // Find the wishlist item for the given user and item
                var wishlistItem = await _context.WishList
                    .FirstOrDefaultAsync(w => w.ItemId == itemId && w.UserId == userId);

                if (wishlistItem == null)
                {
                    return false; // Item not found in the user's wishlist
                }

                _context.WishList.Remove(wishlistItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while removing item from wishlist: {ex.Message}");
                return false;
            }
        }
        public async Task AddWisthItemAsync(WishListModel item)
        {
            await _context.WishList.AddAsync(item);
            await _context.SaveChangesAsync();
        }
        public async Task<WishListModel> GetWishlistItemAsync(int itemId, string userId)
        {

            return await _context.WishList.FirstOrDefaultAsync(wi => wi.ItemId == itemId && wi.UserId == userId);
        }
        public async Task<List<WishListModel>> GetWishlistItemsAsync(string userId)
        {
            return await _context.WishList
                .Where(item => item.UserId == userId)
                .ToListAsync();
        }
    }   

}
