using IdentityDemo.Data;
using IdentityDemo.Interface;
using IdentityDemo.Models;
using IdentityDemo.Services;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Threading.Tasks;

namespace IdentityDemo.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminControlController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAdminService _adminService;
        private readonly IShopService _shopService;
        private readonly IActionRepository _actionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly ICategoryRepository _categoryRepository;
        public AdminControlController( IAdminService adminService,IShopService shopService,
            UserManager<ApplicationUser> userManager,IActionRepository actionRepository,
            SignInManager<ApplicationUser> signInManager,IAccountService accountService,AppDbContext context,ICategoryRepository categoryRepository)
        {
           
            _adminService = adminService;
            _shopService = shopService;
            _userManager = userManager;
            _actionRepository = actionRepository;
            _signInManager = signInManager;
            _accountService = accountService;
            _context = context;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index() => View();
        [HttpGet]
        public async Task<IActionResult> Admin_category_list(){
            string err=string.Empty;
            var userId = _userManager.GetUserId(User);
            string requestData = "Fetching list of catogries";
            string responseData = string.Empty;
            try
            {
                var model = new CategoryListViewModel
                {
                    List = await _adminService.GetCategoriesList(),
                    NewCategory = new IdentityDemo.Models.CategoryModel()
                };
                responseData = JsonConvert.SerializeObject(model);
                return View(model);
            }
            catch (Exception ex)
            {
                err = ex.Message;
                responseData = err;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "Admin_category_list",
                    ControllerName = "Admin_control",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(err) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);

            }
            
        }
        [HttpPost]
        [ActionName("DeleteCategory")]        
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            string err = string.Empty;
            var userId = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(new { CategoryId = categoryId });
            string responseData = string.Empty;

            try
            {
                await _categoryRepository.DeleteCategoryAsync(categoryId);
                responseData = "Category deleted successfully";
            }
            catch (Exception ex)
            {
                err = ex.Message;
                responseData = err;
                // Redirect to an error page or return an error response
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                // Log the action
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "DeleteCategory",
                    ControllerName = "AdminControl",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(err) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }

            return RedirectToAction("Admin_category_list", "AdminControl");
        }

        [HttpPost]
        public async Task<IActionResult> Admin_add_category(string category_name)
        {
            string requestData = JsonConvert.SerializeObject(new { CategoryName = category_name });
            string responseData = string.Empty;
            var userid= _userManager.GetUserId(User);
            string err=string.Empty;
            try
            {
                var existingCategory = await _categoryRepository.GetCategoryByNameAsync(category_name);
                if (existingCategory != null)
                {
                    responseData = "duplicate category";
                    TempData["error_category"] = "This category is already exsit";
                    return RedirectToAction("Admin_category_List", "AdminControl");
                }
                var model = new CategoryModel()
                {

                    Name = category_name,
                    Item_count = 0
                };
                await _adminService.AddCategoryAsync(model);
                responseData=JsonConvert.SerializeObject(model);
                return RedirectToAction("Admin_category_List", "AdminControl");
            }
            catch (Exception ex)
            {
                err = ex.Message;
                responseData = err;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var Log = new ActionLog
                {
                    UserId=userid,
                    ActionName= "Admin_add_category",
                    ControllerName="Admin_control",
                    Timestamp= DateTime.Now,
                    RequestData= requestData,
                    ResponseData= responseData,
                    LogStatus=string.IsNullOrEmpty(err)?"INFO":"ERROR"
                };
                await _actionRepository.Add(Log);
            }
            
        }

        public async Task<IActionResult> Admin_shop_list()
        {
            string error = string.Empty;
            var userid = _userManager.GetUserId(User);
            string requestData = "Fetching list of shop";
            string responseData = string.Empty;
            try
            {
                TempData["title"] = "Shop list";
                var shopViewModels = await _shopService.GetShopsNOwnersAsync();
                return View(shopViewModels);
            }
            catch(Exception ex) {error=ex.Message;
                responseData = error;
                return RedirectToAction("Show_erro_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userid,
                    ActionName = "Admin_shop_list",
                    ControllerName = "Admin_control",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
            
        }

        public async Task<IActionResult> Admin_forgot_list()
        {
            TempData["title"] = "Forgot password list";
            string err = string.Empty;
            var userId = _userManager.GetUserId(User);
            string requestData = "Fetching list of users who requested password reset";
            string responseData=string.Empty;
            try
            {
                var forgots = await _adminService.GetForgotPasswordUsersAsync();
                responseData=JsonConvert.SerializeObject(forgots);
                return View(forgots);
            }
            catch (Exception ex) { 
                err = ex.Message;
                responseData = err;         
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "Admin_forgot_list",
                    ControllerName = "Admin_control",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(err) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> Admin_update_user_info(string userId)
        {
            TempData["title"] = "Update User information";
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }
            var model = await _accountService.GetUpdateUserViewModelAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admin_update_user_info(UpdateUserViewModel model)
        {
            TempData["title"] = "Update User Information";
            string err = string.Empty;
            var userId = _userManager.GetUserId(User);
            string responseData = string.Empty;
            string requestData = JsonConvert.SerializeObject(model); // Serialize the request data

            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(model);
                }

                await _adminService.AdminUpdateUserAsync(model);
                responseData = "User information updated successfully";

                return RedirectToAction("Admin_user_list");
            }
            catch (Exception ex)
            {
                err = ex.Message;
                responseData = err;

                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                // Log the action
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "Admin_update_user_info",
                    ControllerName = "AdminControl",
                    Timestamp = DateTime.Now,
                    RequestData = requestData, // Log the captured request data
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(err) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
        }

        public async Task<IActionResult> Admin_dashboard()
        {
            TempData["title"] = "Dashboard";            
            string err = string.Empty;
            var userId = _userManager.GetUserId(User);
            string responseData = string.Empty;
            
            try
            {
                var viewModel = await _adminService.GetAdminDashboardDataAsync();
                responseData = JsonConvert.SerializeObject(viewModel);
                return View(viewModel);
            }
            catch (Exception ex) { 
                err = ex.Message;
                responseData = err;
                return RedirectToAction("Show_error_loading", "Home");
            
            }
            finally
            {
                // Log the action
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "Admin_dashboard",
                    ControllerName = "AdminControl",
                    Timestamp = DateTime.Now,
                    RequestData = null,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(err) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
        }
        public async Task<IActionResult> Admin_user_list()
        {
            string error = string.Empty;
            var loginUserId = _userManager.GetUserId(User);
            string responseData = string.Empty;
            try
            {
                TempData["title"] = "User List";
                var users = _accountService.GetAllUser();
                responseData = JsonConvert.SerializeObject(users);

                return View(users);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                // Log the action
                var log = new ActionLog
                {
                    UserId = loginUserId,
                    ActionName = "Admin_user_list",
                    ControllerName = "AdminControl",
                    Timestamp = DateTime.Now,
                    RequestData = null,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log); 
                
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            string error= string.Empty;
            var userid=_userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(new { UserId = userId });
            string responseData = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    responseData = "userid is null";
                    return BadRequest("User ID cannot be null or empty.");
                }
                await _adminService.DeleteUserAsync(userId);
                responseData = "User is successfully deleted by userid";
                return RedirectToAction("Admin_user_list");
            }
            catch (Exception ex) { error = ex.Message; responseData = error; return RedirectToAction("Show_error_loading", "Home"); }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "DeleteUser",
                    ControllerName = "Admin_control",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
              
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            string error= string.Empty;
            string requestData=JsonConvert.SerializeObject(new{ UserId = userId });
            string responseData= string.Empty;
            try
            {
                TempData["title"] = "Reset password";
                if (string.IsNullOrEmpty(userId))
                {
                    responseData = "null user";
                    return BadRequest("User ID cannot be null or empty.");
                }
                await _adminService.ResetPasswordAsync(userId);
                responseData = "User's password is successfully reset";
                return RedirectToAction("Admin_forgot_list");
            }
            catch(Exception ex)
            {
                error = ex.Message; responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userId,
                    RequestData = requestData,
                    ResponseData = responseData,
                    Timestamp = DateTime.Now,
                    ActionName = "ResetPassword",
                    ControllerName = "Admin_control",
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
            
        }

        [HttpGet]
        public IActionResult Admin_change_password() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Admin_change_password(UpdateUserViewModel model)
        {
            string requestData = JsonConvert.SerializeObject(model);
            string error = string.Empty;
            var userId = _userManager.GetUserId(User);
            string responseData = string.Empty;

            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(model);
                }

                var result = await _adminService.ChangePasswordAsync(userId, model);
                if (result.Succeeded)
                {
                    responseData = "Password changed successfully";
                    return RedirectToAction("Admin_user_list");
                }
                else
                {
                    responseData = "Password change failed";
                    return RedirectToAction("Show_error_loading", "Home");
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                // Log the action
                var log = new ActionLog
                {
                    UserId = userId,
                    ActionName = "Admin_change_password",
                    ControllerName = "AdminControl",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
