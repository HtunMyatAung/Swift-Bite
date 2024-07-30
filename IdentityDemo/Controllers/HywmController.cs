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
        private readonly IShopService _shopService;
        public HywmController(IShopService shopService,IAccountService accountService, IItemService itemService,UserManager<ApplicationUser> userManager,IWishListService wishListService)
        {
            _accountService = accountService;
            _itemService = itemService;
            _userManager = userManager;
            _wishListService = wishListService;
            _shopService = shopService;
        }
        
        public async Task<IActionResult> SingleShopView(int shopid)
        {
            var itemsViewModel = await _itemService.GetItemNShopByShopIdAsync(shopid);
            if (!itemsViewModel.Items.Any())
            {
                TempData["itemsthere"] = "hello";
            }
            return View(itemsViewModel);
        }
        public async Task<IActionResult> Foods()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var wishes = await _wishListService.GetWishlistItemsAsync(user.Id);
            var shops = await _shopService.GetShopsListAsync();
           var shopLookup = shops.ToDictionary(s => s.ShopId, s => s.ShopName);
            var itemsViewModel = await _itemService.GetHomePageItemsAsync();
            var newItemsViewModel = new ItemsViewModel
            {
                Shops = itemsViewModel.Shops,
                Items = itemsViewModel.Items,
                Categories = itemsViewModel.Categories,
                WishlistItems = wishes,
                ShopLookup = shopLookup
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
