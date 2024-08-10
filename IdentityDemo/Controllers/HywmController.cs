using IdentityDemo.Data;
using IdentityDemo.Interface;
using IdentityDemo.Models;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Near_foods.Controllers
{
    public class HywmController : Controller
    {
        public readonly IAccountService _accountService;
        private readonly IItemService _itemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWishListService _wishListService;
        private readonly IShopService _shopService;
        private readonly AppDbContext _context;
        private readonly IActionRepository _actionRepository;
        public HywmController(IActionRepository actionRepository,AppDbContext context,IShopService shopService,IAccountService accountService, IItemService itemService,UserManager<ApplicationUser> userManager,IWishListService wishListService)
        {
            _accountService = accountService;
            _itemService = itemService;
            _userManager = userManager;
            _wishListService = wishListService;
            _shopService = shopService;
            _context= context;
            _actionRepository= actionRepository;
        }
        public IActionResult HowToUse() => View();
        public async Task<IActionResult> Search(string searchQuery)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching items that conatins "+searchQuery;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var items = _context.Items
                    .Where(i => i.ItemName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                responseData = JsonConvert.SerializeObject(items);
                // Return partial view with the filtered items
                return PartialView("_SearchResults", items);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "Search",
                    ControllerName = "Hywm",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        public async Task<IActionResult> SingleShopView(int shopid)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching single shop view with "+shopid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var itemsViewModel = await _itemService.GetItemNShopByShopIdAsync(shopid);
                if (!itemsViewModel.Items.Any())
                {
                    TempData["itemsthere"] = "hello";
                }
                responseData= JsonConvert.SerializeObject(itemsViewModel);
                return View(itemsViewModel);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "SingleShopView",
                    ControllerName = "Hywm",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        public async Task<IActionResult> Foods()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching all items from all shops which are not deleted";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    responseData = "Invalid user";
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
                responseData=JsonConvert.SerializeObject(newItemsViewModel);
                return View(newItemsViewModel);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "Foods",
                    ControllerName = "Hywm",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
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
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching homepage items";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var itemsViewModel = await _itemService.GetHomePageItemsAsync();
                responseData=JsonConvert.SerializeObject(itemsViewModel);
                return View(itemsViewModel);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "Landing_page2",
                    ControllerName = "Hywm",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        public IActionResult Otp() => View();
        public async Task<IActionResult> User_profile()
        {    
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching user profile data";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {

                var profileViewModel = await _accountService.GetUserProfileAsync(User);
                TempData["title"] = "profile";
                responseData = JsonConvert.SerializeObject(profileViewModel);
                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving user profile.");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "User_profile",
                    ControllerName = "Hywm",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        
    }
}
