using IdentityDemo.Models;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

using System;
using System.Linq;
using Newtonsoft.Json;
using IdentityDemo.Interface;

namespace IdentityDemo.Controllers
{
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWishListService _wishListService;
        private readonly IActionRepository _actionRepository;
        public ItemController(IActionRepository actionRepository,IWishListService wishListService,IItemService itemService, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager,ICategoryRepository categoryRepository)
        {
            _itemService = itemService;
            _environment = environment;
            _userManager = userManager;
            _categoryRepository = categoryRepository;
            _wishListService = wishListService;
            _actionRepository = actionRepository;

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWishlistItem(int itemId, string action)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = action+" wishlist item "+itemId;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (itemId <= 0 || string.IsNullOrEmpty(action))
                {
                    responseData = "fail to save wish item ";
                    return Json(new { success = false, message = "Invalid data." });
                }

                // Your logic to add/remove item from wishlist
                bool success = false;
                string message = string.Empty;
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                try
                {
                    if (action == "add")
                    {
                        success = await _wishListService.AddItemToWishlistAsync(itemId, userId);
                        responseData = "wish item is successfully saved";
                    }
                    else if (action == "remove")
                    {
                        success = await _wishListService.RemoveItemFromWishlistAsync(itemId, userId);
                        responseData = "wish item is successfully removed";
                    }

                    message = success ? "Item updated successfully." : "Failed to update item.";
                }
                catch (Exception ex)
                {
                    message = $"An error occurred: {ex.Message}";
                }

                return Json(new { success = success, message = message });
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
                    ActionName = "SaveWishlistItem",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        public async Task<IActionResult> HomePageItems()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching home page data";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Check if the user is authenticated
                if (user == null)
                {
                    responseData = "Invalid user";
                    // Handle unauthenticated user case (e.g., redirect to login page)
                    return RedirectToAction("Login", "Account");
                }

                // Retrieve the user ID (string type)
                var userId = user.Id;

                // Call the service method to get wishlist items
                var wishes = await _wishListService.GetWishlistItemsAsync(userId);

                // Retrieve items view model
                var itemsViewModel = await _itemService.GetHomePageItemsAsync();

                // Create a new view model with the wishlist items
                var newItemsViewModel = new ItemsViewModel
                {
                    Shops = itemsViewModel.Shops,
                    Items = itemsViewModel.Items,
                    Categories = itemsViewModel.Categories,
                    WishlistItems = wishes
                };
                responseData=JsonConvert.SerializeObject(newItemsViewModel);
                // Return the view with the new view model
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
                    ActionName = "HomePageItems",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        public async Task<IActionResult> ItemByShopid(int shopid)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching items by shopid "+shopid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var itemsViewModel = await _itemService.GetItemNShopByShopIdAsync(shopid);
                if (!itemsViewModel.Items.Any())
                {                    
                    TempData["items"] = "hello";
                }
                responseData = JsonConvert.SerializeObject(itemsViewModel);
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
                    ActionName = "ItemByShopid",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        public async Task<IActionResult> UpdateItem(int itemid)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Update item with item id "+itemid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var updateItemViewModel = await _itemService.GetItemForUpdateAsync(itemid, user.Id);
                if (updateItemViewModel == null)
                {
                    responseData = "Invalid item";
                    return RedirectToAction("Show_error_loading", "Home");
                }
                responseData=JsonConvert.SerializeObject(updateItemViewModel);
                return View(updateItemViewModel);
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
                    ActionName = "UpdateItem",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateItem(SingleItemViewModel updateItem)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(updateItem);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(updateItem);
                }

                string uniqueFileName = null;
                if (updateItem.ItemImage != null)
                {
                    uniqueFileName = await ProcessUploadedFile(updateItem.ItemImage);
                }
                await _itemService.UpdateItemAsync(updateItem, uniqueFileName);
                responseData = "Item is successfully updated";
                return RedirectToAction("Owner_Item_List", "Shop");
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
                    ActionName = "UpdateItem",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        [Authorize(Roles = "Owner")]
        [HttpGet]
        public async Task<IActionResult> CreateItem()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && user.Role != "Owner")
            {
                return RedirectToAction("Login", "Account");
            }
            var viewModel = await _itemService.getSingleItemViewModelAsync(user.ShopId); // Ensure await here

            return View(viewModel); // Pass the resolved SingleItemViewModel to the view
        }
        

        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<IActionResult> CreateItem(SingleItemViewModel item)
        {            
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(item);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    string uniqueFileName = null;
                    if (item.ItemImage != null)
                    {
                        uniqueFileName = await ProcessUploadedFile(item.ItemImage);
                    }
                    else
                    {
                        uniqueFileName = "item_default.png";
                    }
                    //item.Categories = item.Categories;
                    if (item.ItemPrice == 0)
                    {
                        TempData["amount_error"] = "Price amount can't be 0.";
                        item.Categories = await _categoryRepository.GetCategoryNamesAsync();
                        responseData = "Item price can't be 0";
                        return View(item);

                    }
                    else if (item.ItemQuantity == 0)
                    {
                        TempData["amount_error"] = "Quantity amount can't  0.";
                        item.Categories = await _categoryRepository.GetCategoryNamesAsync();
                        responseData = "Item amount can't be 0";
                        return View(item);
                    }
                    await _itemService.AddItemAsync(item, uniqueFileName);
                    responseData = "Item is successfully create";
                    return RedirectToAction("Owner_item_List", "Shop");
                }

                item.Categories = await _categoryRepository.GetCategoryNamesAsync();
                responseData = "Invalid model state";
                return View(item);
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
                    ActionName = "CreateItem",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        [HttpPost]
        [ActionName("DeleteItem")]
        public async Task<IActionResult> DeleteItem(int itemid)
        {
           
            var userid = _userManager.GetUserId(User);
            string requestData = "Trying to delete item with item id "+itemid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                await _itemService.DeleteItemAsync(itemid);
                responseData = "Item is successfully deleted";
                return RedirectToAction("Owner_Item_List", "Shop");
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
                    ActionName = "DeleteItem",
                    ControllerName = "Item",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        private async Task<string> ProcessUploadedFile(IFormFile file)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Upload image file ";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                string uniqueFileName = null;

                if (file != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "img/items");
                    Directory.CreateDirectory(uploadsFolder);
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                responseData = "image is successfully uploaded";
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return error;
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "ProcessUploadedFile",
                    ControllerName = "Item",
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
