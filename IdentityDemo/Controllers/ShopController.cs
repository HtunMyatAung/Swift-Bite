// ShopController.cs
using IdentityDemo.Interface;
using IdentityDemo.Models;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace IdentityDemo.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly IItemService _itemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly IActionRepository _actionRepository;
        public ShopController(IActionRepository actionRepository,IShopService shopService,IItemService itemService,UserManager<ApplicationUser> userManager,IOrderService orderService)
        {
            _shopService = shopService;
            _itemService = itemService;
            _userManager = userManager;
            _orderService = orderService;
            _actionRepository = actionRepository;
            
        }

       
        public async Task<IActionResult> Restaurant()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetch all shops which is not deleted";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var shops = _shopService.GetAllShopsAsync();
                responseData = JsonConvert.SerializeObject(shops);
                return View(shops);
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
                    ActionName = "Restaurant",
                    ControllerName = "Shop",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Owner_Item_List()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching items which to owner shop with id"+userid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var items = await _itemService.GetAllItemsByShopIdAsync(user.ShopId);
                if (items == null)
                {
                    responseData = "no items in this shop";
                    return NotFound();
                }
                responseData = JsonConvert.SerializeObject(items);
                return View(items);
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
                    ActionName = "Owner_Item_List",
                    ControllerName = "Shop",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        public async Task<IActionResult> Shop_view(int shopid)
        {            
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching shop items with shopid"+shopid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var shop = await _shopService.GetShopByIdAsync(shopid);
                var user = await _userManager.GetUserAsync(User);
                if (shopid != user.ShopId)
                {
                    responseData = "shopid is not match with current owner shopid ";
                    return RedirectToAction("Show_error_loading", "Home");
                }
                var items = await _itemService.GetItemNShopByShopIdAsync(shopid);
                responseData=JsonConvert.SerializeObject(items);
                return View(items);
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
                    ActionName = "Shop_view",
                    ControllerName = "Shop",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        public async Task<IActionResult> Owner_order_list()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching orders list with user's shopid";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    responseData = "Invalid user";
                    return NotFound();
                }

                var orderViewModels = await _orderService.GetOrderNUserByShopIdAsync(user.ShopId);
                responseData=JsonConvert.SerializeObject(orderViewModels);
                return View(orderViewModels);
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
                    ActionName = "Owner_order_list",
                    ControllerName = "Shop",
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
        public async Task<IActionResult> UpdateShop()
        {
            var user = await _userManager.GetUserAsync(User);
            var shop = await _shopService.GetShopByIdAsync(user.ShopId);
            var shopview = new ShopViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ShopEmail = shop.ShopEmail,
                ShopAddress = shop.ShopAddress,
                ShopPhone = shop.ShopPhone,
                ShopDescription = shop.ShopDescription,
            };

            if (shop == null)
            {
                return NotFound();
            }
            return View(shopview);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<IActionResult> UpdateShop(ShopViewModel shop)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(shop);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (user == null)
                    {
                        responseData = "Invalid user";
                        return NotFound();
                    }

                    await _shopService.UpdateShopAsync(shop);
                    responseData = "Shop is successfully updated";
                    return RedirectToAction("Owner_dashboard", "Shop");
                }
                responseData = "Invalid model state";
                return View(shop);
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
                    ActionName = "UpdateShop",
                    ControllerName = "Shop",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteShop(int shopid)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Delete shop with shopid"+shopid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var shop = await _shopService.GetShopByIdAsync(shopid);
                if (shop == null)
                {
                    responseData = "Invalid shopid ";
                    return NotFound();
                }
                await _shopService.DeleteShopAsync(shopid);
                responseData = "Shop is successfully deleted";
                return View(shop);
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
                    ActionName = "DeleteShop",
                    ControllerName = "Shop",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<IActionResult> DeleteShop(ShopViewModel shop)
        {            
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(shop);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    await _shopService.DeleteShopAsync(shop.ShopId);
                    responseData = "Shop is successfully deleted";
                    return RedirectToAction("Login", "Account");
                }
                responseData = "Fail to delete shop";
                return View();
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
                    ActionName = "DeleteShop",
                    ControllerName = "Shop",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Owner_dashboard()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching owner dashboard data";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.ShopId == null)
                {
                    responseData = "current user is not owner and trying to reach dashboard with route";
                    return NotFound();
                }
                var shop = await _shopService.GetShopByIdAsync(user.ShopId);
                if (shop.Is_confirm == 0)
                {
                    responseData = "Owner first time log in and send to update shop page";
                    return RedirectToAction("UpdateShop", "Shop");
                }
                var dashboard = await _shopService.GetOwnerDashboardAsync(user.Id);
                responseData=JsonConvert.SerializeObject(dashboard);
                return View(dashboard);
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
                    ActionName = "Owner_dashboard",
                    ControllerName = "Shop",
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
