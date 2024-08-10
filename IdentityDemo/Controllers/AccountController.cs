using IdentityDemo.Data;
using IdentityDemo.Filters;
using IdentityDemo.Models;
using IdentityDemo.Services;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using IdentityDemo.Extensions;
using Org.BouncyCastle.Bcpg;
using System.Web.Helpers;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Xml.Linq;
using MySqlX.XDevAPI;
using static System.Net.WebRequestMethods;
using System.Security.Claims;
using IdentityDemo.Interface;

namespace IdentityDemo.Controllers
{
    [ServiceFilter(typeof(LogActionFilter))]
    public class Accountcontroller : Controller
    {
        public readonly IAccountRepository _accountRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        public readonly IAccountService _accountService;
        public readonly IActionRepository _actionRepository;
        public Accountcontroller(IActionRepository actionRepository,IAccountService accountService,UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager, AppDbContext context, RoleManager<IdentityRole> roleManager, IWebHostEnvironment environment, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _environment = environment;
            _emailService = emailService;
            _accountService = accountService;
            _actionRepository = actionRepository;
        }
        public IActionResult SendOTP() => View();
        [HttpGet]
        public IActionResult User_change_password()=> View();
        [HttpGet]
        public IActionResult Confirm_register() => View();
        [HttpPost]
        public async Task<IActionResult> SendOTP(string email)
        {
            string error = string.Empty;
            var userid = _userManager.GetUserId(User);
            string requestData=JsonConvert.SerializeObject(new {email});
            string responseData = string.Empty;
            try
            {
                responseData = "Sent otp successfully";
                await _accountService.SendOTP(email);
                return RedirectToAction("Confirm_register", "Account");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                return RedirectToAction("Show_email_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userid,
                    ActionName = "SendOTP",
                    ControllerName = "Account",
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
            
        }
        public async Task<IActionResult> Save_register()
        {
            var model = HttpContext.Session.GetObject<RegisterViewModel>("Registerviewmodel");
            var userid = _userManager.GetUserId(User);
            string requestData = "Saving registered user data";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(model);
                }

                var result = await _accountService.CreateNewUserAsync(model);

                // Clear session after use
                if (result)
                {
                    responseData = "Registered user's data is successfully saved";
                    HttpContext.Session.Remove("RegisterViewModel");
                    return RedirectToAction("Landing_page2", "Hywm");
                }
                else
                {
                    responseData = "Registered user's data did'nt save";
                    HttpContext.Session.Remove("RegisterViewModel");
                    return RedirectToAction("Show_error_loading", "Home");
                }
            }
            catch (Exception ex) {error = ex.Message; 
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userid,
                    RequestData = requestData,
                    ResponseData = responseData,
                    Timestamp = DateTime.Now,
                    ActionName = "Save_register",
                    ControllerName = "Account",
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
            
        }        
        [HttpPost]
        public async Task<IActionResult> SendChangePasswordEmail(string email)
        {
            var userid = _userManager.GetUserId(User);
            string requestData=JsonConvert.SerializeObject(new {email});
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                var user = await _accountService.FindUserByEmailAsync(email);
                
                if (user!=null) 
                {
                    responseData = "Reset password link is sent to email";
                    _accountService.ChangePasswordSendEmail(user.Email);
                    _accountService.UpdateUserAsync1(user);
                }
                else
                {
                    responseData = "email is not registered";
                    TempData["message"] = "Your email is not registered";
                    Console.WriteLine("********************error***********");

                    
                }
                return RedirectToAction("Login", "Account");
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
                    UserId=userid,
                    RequestData=requestData,
                    ResponseData=responseData,
                    LogStatus=string.IsNullOrEmpty(error)?"INFO":"ERROR",
                    ActionName= "SendChangePasswordEmail",
                    ControllerName="Account",
                    Timestamp= DateTime.Now,
                };
                await _actionRepository.Add(log);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Register() => View();
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            string error = string.Empty;
            var userid = _userManager.GetUserId(User);
            string requestData=JsonConvert.SerializeObject(model);
            string responseData = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if email is already taken
                    var existingEmailUser = await _accountService.FindUserByEmailAsync(model.Email);
                    Console.WriteLine(existingEmailUser);
                    if (existingEmailUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "Email is already taken.");
                        return View(model);
                    }
                    // Check if username is already taken
                    var existingUsernameUser = await _userManager.FindByNameAsync(model.UserName);
                    if (existingUsernameUser != null && existingUsernameUser is ApplicationUser)
                    {
                        ModelState.AddModelError(string.Empty, "Username is already taken.");
                        return View(model);
                    }
                    Random random = new Random();
                    var otp_code = random.Next(100000, 999999).ToString();
                    var otpmodel = new OTPViewModel { OTP = otp_code };
                    TempData["Model"] = JsonConvert.SerializeObject(otpmodel);
                    TempData["otpcode"] = otp_code;
                    try
                    {
                        await _accountService.SendRegisterConfirmEmail(model.Email, otp_code);
                        // Save the model in the session
                        HttpContext.Session.SetObject("Registerviewmodel", model);
                        return RedirectToAction("Confirm_register", "Account");
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("Show_email_error_loading", "Home");
                    }
                }
                // If registration fails, return the registration view with validation errors
                return View(model);
            }
            catch(Exception ex) 
            {
                    error = ex.Message;
                    responseData = error;
                    return RedirectToAction("Show_email_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userid,
                    ActionName = "Register",
                    ControllerName = "Account",
                    Timestamp = DateTime.Now,
                    RequestData = responseData,
                    ResponseData = responseData,
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR"
                };
                await _actionRepository.Add(log);
            }
            
        }
        [HttpGet]
        public IActionResult Login() => View();
        [HttpGet]
        public IActionResult Login_old() => View();
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var userid = _userManager.GetUserId(User);
            string error = string.Empty;
            string requestData=JsonConvert.SerializeObject(model);
            string responseData = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    var isuser = await _userManager.FindByNameAsync(model.UserName);
                    if (result.Succeeded && isuser != null && isuser.Deleted != 1)
                    {
                        // Retrieve the current user
                        ApplicationUser currentUser = await _userManager.GetUserAsync(User);

                        // Pass the user data to the view
                        ViewBag.CurrentUser = currentUser;
                        if (isuser.Role == "Admin")
                        {
                            responseData = "Admin is logged in ";
                            ViewBag.LogUserRole = "Admin";
                            return RedirectToAction("Admin_dashboard", "AdminControl");
                        }
                        else if (isuser.Role == "Owner")
                        {
                            
                            ViewBag.LogUserRole = "Owner";
                            if (model.Role == "user")
                            {
                                responseData = "Owner logged in as user ";
                                return RedirectToAction("User_profile", "Hywm");
                            }
                            else if (model.Role == "owner")
                            {
                                responseData = "Owner logged in as owner";
                                return RedirectToAction("Owner_dashboard", "Shop");
                            }

                        }
                        else
                        {
                            responseData = "Normal user logged in ";
                            return RedirectToAction("User_profile", "Hywm");
                        }
                    }
                    if (result.IsLockedOut)
                    {
                        responseData = "Login fail attempts exceed 5 times";
                        return RedirectToAction("LoginError", "Home");
                    }
                    else
                    {
                        responseData = " Incorrect login attempt";
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }
                responseData = "Invalid model state";
                ModelState.AddModelError(string.Empty, "Please enter valid data");
                return View(model);
            }
            catch(Exception ex)
            {
                error=ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData,
                    ActionName = "Login",
                    ControllerName = "Account",
                    LogStatus=string.IsNullOrEmpty(error)?"INFO":"ERROR"
                };
                await _actionRepository.Add(log);
            }            
        }
        [HttpPost]
        public async Task<IActionResult> Login_old(LoginViewModel model)
        {
            string error = string.Empty;
            string requestedData = JsonConvert.SerializeObject(new { model });
            string responseData = string.Empty;
            var userid = _userManager.GetUserId(User);
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    var isuser = await _userManager.FindByNameAsync(model.UserName);
                    if (result.Succeeded && isuser != null && isuser.Deleted != 1)
                    {
                        // Retrieve the current user
                        ApplicationUser currentUser = await _userManager.GetUserAsync(User);

                        // Pass the user data to the view
                        ViewBag.CurrentUser = currentUser;
                        if (isuser.Role == "Admin")
                        {
                            responseData = "Admin logged in";
                            return RedirectToAction("Admin_dashboard", "AdminControl");
                        }
                        else if (isuser.Role == "Owner")
                        {
                            if (model.Role == "user")
                            {
                                responseData = "Owner logged in as user";
                                return RedirectToAction("User_profile", "Hywm");
                            }
                            else if (model.Role == "owner")
                            {
                                responseData = "Owner logged in as owner";
                                return RedirectToAction("Owner_dashboard", "Shop");
                            }
                        }
                        else
                        {
                            responseData = "Normal user logged in";
                            return RedirectToAction("User_profile", "Hywm");
                        }
                    }
                    if (result.IsLockedOut)
                    {
                        responseData = "Login fail attempts exceed 5 times";
                        return RedirectToAction("LoginError", "Home");
                    }
                    else
                    {
                        responseData = "Login Fail attempt";
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }
                responseData = "Invalid model state";
                ModelState.AddModelError(string.Empty, "Please enter valid data");
                return View(model);
            }
            catch(Exception ex)
            {
                error= ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log=new ActionLog
                {
                    LogStatus=string.IsNullOrEmpty(error)?"INFO":"ERROR",
                    ActionName= "Login_old",
                    ControllerName="Account",
                    RequestData= requestedData,
                    ResponseData=responseData,
                    UserId=userid,
                    Timestamp=DateTime.Now,
                };
                await _actionRepository.Add(log);
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]public IActionResult ResetPassword(){return View();}
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(UpdateUserViewModel model)
        {
            var userid = _userManager.GetUserId(User);
            string requestData=JsonConvert.SerializeObject(model);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(model);
                }

                var user = await _accountService.FindUserByEmailAsync(model.Email);
                if (user == null)
                {
                    responseData = "null user";
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                if (user.Forgot == 0)
                {
                    responseData = "User didn't confirm forgot password";
                    ModelState.AddModelError(string.Empty, "You are not allowed to reset password !!!");
                    return View(model);
                }

                var resetResult = await _accountService.ResetPasswordAsync(user, model.NewPassword);

                if (resetResult)
                {
                    responseData = "Reset password successfully";
                    return RedirectToAction("Landing_page2", "Hywm");
                }
                else
                {
                    responseData = "Incorrect format of password";
                    ModelState.AddModelError(string.Empty, "Password must be 1 captital ,1 smaller ,1 digit and 1 special key");
                    return View(model);
                }
            }
            catch(Exception ex)
            {
                error= ex.Message;
                responseData = error;
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "ResetPassword",
                    ControllerName = "Account",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePasswordFromProfile(ChangePasswordViewModel model)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(model);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return RedirectToAction("UpdatePassword", "Account");
                }
                var userId = _userManager.GetUserId(User);
                var result = await _accountService.ChangePasswordAsync(userId, model);
                if (result.Succeeded)
                {
                    responseData = "Changed password successfully";
                    return RedirectToAction("User_profile", "Hywm");
                }
                responseData = "error in updating password";
                return RedirectToAction("Show_error_loading", "Home");
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
                    ActionName = "UpdatePasswordFromProfile",
                    ControllerName = "Account",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
            
        }
        [HttpGet]
        public async Task<IActionResult> UpdateUser()
        {
            var viewModel = _accountService.GetUpdateUserAsync(User);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(model);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(model);
                }

                var user = await _accountService.GetUserByIdAsync(model.Id);
                if (user == null)
                {
                    responseData = "invalid user";
                    return NotFound();
                }


                bool updateResult = await _accountService.UpdateUserAsync(user, model);


                if (updateResult)
                {
                    responseData = "User successfully update profile";
                    return RedirectToAction("Landing_page2", "Hywm");
                }
                else
                {
                    responseData = "Fail to update profile";
                    ModelState.AddModelError(string.Empty, "Failed To Update User");
                    return View(model);
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
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "UpdateUser",
                    ControllerName = "Account",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
            
        }        
        [HttpGet]
        public async Task<IActionResult> User_profile()
        {
            try
            {
                var profileViewModel = await _accountService.GetUserProfileAsync(User);

                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving user profile.");
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> User_profile(ProfileViewModel model)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(model);
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    responseData = "Invalid model state";
                    return View(model);
                }

                var user = await _accountService.GetUserByIdAsync(model.UserId);
                if (user == null)
                {
                    responseData = "invalid user";
                    return NotFound();
                }

                // Handle user image upload
                if (model.UserImage != null)
                {
                    // Save user image and update user's UserImageName property
                    string newImageName = await _accountService.SaveUserImageAsync(model.UserId, model.UserImage);

                    if (newImageName == null)
                    {
                        responseData = "fail to save image";
                        ModelState.AddModelError(string.Empty, "Failed to save user image.");
                        return View(model);
                    }
                }

                // Update user profile
                bool updateResult = await _accountService.UpdateUserProfileAsync(user, model);

                if (updateResult)
                {
                    responseData = "Updated profile successfully";
                    return RedirectToAction("User_profile", "Hywm");
                }
                else
                {
                    responseData = "Fail to update profile";
                    ModelState.AddModelError(string.Empty, "Failed to update user profile.");
                    return View(model);
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
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "User_profile",
                    ControllerName = "Account",
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
