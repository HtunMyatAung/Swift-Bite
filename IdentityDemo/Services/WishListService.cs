using IdentityDemo.Models;
using IdentityDemo.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemo.Services
{
    public class WishListService:IWishListService
    {
        private readonly IWishListRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WishListService> _logger;
        public WishListService(IWishListRepository repository,UserManager<ApplicationUser> userManager, ILogger<WishListService> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<List<int>> GetItemIdsFromWishlistAsync(string userId)
        {
            return await _repository.GetItemIdsByUserIdAsync(userId);
        }
        public async Task<List<WishListModel>> GetWishlistItemsAsync(string userId)
        {
            return await _repository.GetWishlistItemsAsync(userId);
        }
        public async Task<bool> AddItemToWishlistAsync(int itemId, string userId)
        {
            try
            {
                _logger.LogInformation($"Adding item {itemId} to wishlist for user {userId}");

                var existingWishlistItem = await _repository.GetWishlistItemAsync(itemId, userId);
                if (existingWishlistItem != null)
                {
                    _logger.LogInformation($"Item {itemId} already exists in the wishlist for user {userId}");
                    return false;
                }

                var newWishlistItem = new WishListModel
                {
                    ItemId = itemId,
                    UserId = userId
                };
                await _repository.AddWisthItemAsync(newWishlistItem);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding item to wishlist: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RemoveItemFromWishlistAsync(int itemId, string userId)
        {
            try
            {
                // Call the repository method to remove the item from the wishlist
                bool result = await _repository.RemoveItemAsync(itemId, userId);
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while removing item from wishlist: {ex.Message}");
                return false;
            }
        }
    }
}
