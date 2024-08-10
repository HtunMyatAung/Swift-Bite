using IdentityDemo.Interface;
using IdentityDemo.Models;
using IdentityDemo.Services;
using IdentityDemo.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityDemo.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActionRepository _actionRepository;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager,IActionRepository actionRepostiory)
        {
            _orderService = orderService;
            _userManager = userManager;
            _actionRepository = actionRepostiory;
        }

        [HttpPost]
        public async Task<IActionResult> SendInvoiceEmail(string htmlContent)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Sending invoice with email";
            string responseData = string.Empty;
            string error = string.Empty;            
            
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _orderService.SendInvoiceEmailAsync(htmlContent, currentUser);
                TempData["test"] = "hahaha"; // Test
                ViewBag.Message = "Email sent successfully!"; // Test
                responseData = "Email with invoice is sent successfully";
                return RedirectToAction("SaveInvoice", "Order");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                responseData = error;
                // Log the exception
                return RedirectToAction("Show_error_loading", "Home");
            }
            finally
            {
                var log = new ActionLog
                {
                    LogStatus = string.IsNullOrEmpty(error) ? "INFO" : "ERROR",
                    ActionName = "SendInvoiceEmail",
                    ControllerName = "Order",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        public async Task<IActionResult> Order_confirm(Dictionary<int, int> selectedItems)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = JsonConvert.SerializeObject(selectedItems);
            string responseData = string.Empty;
            string error = string.Empty;
            TempData["SelectedItems"] = JsonConvert.SerializeObject(selectedItems);
            try
            {
                if (selectedItems == null || selectedItems.Count == 0)
                {
                    responseData = "Selected itme is zero or null";
                    return RedirectToAction("HomePageItems", "Item");
                }
                responseData = "Show selected items successfully";
                var invoiceViewModel = await _orderService.PrepareInvoiceAsync(selectedItems, HttpContext);

                // Store InvoiceViewModel in TempData
                TempData["InvoiceViewModel"] = JsonConvert.SerializeObject(invoiceViewModel);

                return View(invoiceViewModel); // Pass the InvoiceViewModel to the view
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
                    ActionName = "Order_confirm",
                    ControllerName = "Order",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }            
        }
        public async Task<IActionResult> Invoice()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Showing invoice with shop,user,items data";
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                if (TempData["SelectedItems"] == null)
                {
                    responseData = "Selected items is null or removed";
                    return RedirectToAction("Show_error_loading", "Home");
                }
                var selectedItemsJson = TempData["SelectedItems"].ToString();
                var selectedItems = JsonConvert.DeserializeObject<Dictionary<int, int>>(selectedItemsJson); // Retrieve invoice dictionary data

                var invoiceViewModel = await _orderService.PrepareInvoiceAsync(selectedItems, HttpContext);

                // Store InvoiceViewModel in TempData
                TempData["InvoiceViewModel"] = JsonConvert.SerializeObject(invoiceViewModel);
                responseData = "Invoice is shown successfully";
                return View(invoiceViewModel); // Pass the InvoiceViewModel to the view
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
                    ActionName = "Invoice",
                    ControllerName = "Order",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }            
        }
        public async Task<IActionResult> SaveInvoice()
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Trying to save inovice";
            string responseData = string.Empty;
            string error = string.Empty;         
            try
            {
                if (TempData["InvoiceViewModel"] == null)
                {
                    responseData = "Invoice data is null or removed";
                    // Handle case where InvoiceViewModel is null
                    return BadRequest("Invalid data received");
                }

                // Example: Save or process the invoiceViewModel data as needed
                var invoiceViewModelJson = TempData["InvoiceViewModel"].ToString();
                InvoiceViewModel invoiceViewModel = JsonConvert.DeserializeObject<InvoiceViewModel>(invoiceViewModelJson);

                await _orderService.SaveOrderAsync(invoiceViewModel);
                responseData = "invoice is saved in order table and order detail table";
                return RedirectToAction("Landing_page2", "Hywm");
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
                    ActionName = "SaveInvoice",
                    ControllerName = "Order",
                    UserId = userid,
                    Timestamp = DateTime.Now,
                    RequestData = requestData,
                    ResponseData = responseData
                };
                await _actionRepository.Add(log);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderid)
        {
            var userid = _userManager.GetUserId(User);
            string requestData = "Try to delete order with id"+orderid;
            string responseData = string.Empty;
            string error = string.Empty;
            try
            {
                responseData = "order is deleted successfully";
                await _orderService.DeleteOrderAsync(orderid);
                return RedirectToAction("Owner_order_list", "Shop");
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
                    ActionName = "DeleteOrder",
                    ControllerName = "Order",
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
