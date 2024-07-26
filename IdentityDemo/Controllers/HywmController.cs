using IdentityDemo.Models;
using IdentityDemo.Services;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Near_foods.Controllers
{
    public class HywmController : Controller
    {
        public readonly IAccountService _accountService;
        private readonly IItemService _itemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWishListService _wishListService;
        public HywmController(IAccountService accountService, IItemService itemService,UserManager<ApplicationUser> userManager,IWishListService wishListService)
        {
            _accountService = accountService;
            _itemService = itemService;
            _userManager = userManager;
            _wishListService = wishListService;
        }
        public async Task<IActionResult> Foods()
        {
            var user = await _userManager.GetUserAsync(User);
            var wishes = await _wishListService.GetWishlistItemsAsync(user.Id);

            var itemsViewModel = await _itemService.GetHomePageItemsAsync();
            var newItemsViewModel = new ItemsViewModel
            {
                Shops = itemsViewModel.Shops,
                Items = itemsViewModel.Items,
                Categories = itemsViewModel.Categories,
                WishlistItems = wishes
            };

            return View(newItemsViewModel);
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SignUp() => View();
        public IActionResult Login() => View();
        public IActionResult Landing_page() => View();
        
        
        public async Task<IActionResult> Landing_page2()
        {
            var itemsViewModel = await _itemService.GetHomePageItemsAsync();
            return View(itemsViewModel);
        }
        public IActionResult Otp() => View();
        public async Task<IActionResult> User_profile()
        {
            try
            {
                
                var profileViewModel = await _accountService.GetUserProfileAsync(User);
                TempData["title"] = "profile";
                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving user profile.");
            }
        }
        
    }
}
