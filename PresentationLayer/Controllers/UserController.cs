using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.Helpers;
using PresentationLayer.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string SearchValue)
        {
            var users = Enumerable.Empty<ApplicationUser>().ToList();
            if (string.IsNullOrEmpty(SearchValue))
                users.AddRange( _userManager.Users);
            else
                users = await _userManager.Users.Where(user =>user.Email.Trim().ToLower().Contains(SearchValue.Trim().ToLower())).ToListAsync();


           
            return View(users);

        }

        //public IActionResult Create()
        //{
        //    //ViewBag.Departments=_departmemtRepository.GetAll();
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(EmployeeViewModel employeeVM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (employeeVM.Image != null)
        //            employeeVM.ImageName = DocumentSettings.UploadFile(employeeVM.Image, "images");

        //        var mappedEmp = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);
        //        await _employeeRepository.Add(mappedEmp);
        //        TempData["Message"] = "Employee is Created Seccessfully";

        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(employeeVM);
        //}

        public async Task<IActionResult> Datails(string id, string ViewName = "Datails")
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();
         
            return View(ViewName, user);
        }



        public async Task<IActionResult> Edit(string id)
        {
            //ViewBag.Departments = _departmemtRepository.GetAll();
            return await Datails(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser UpdatedUser, [FromRoute] string Id)
        {
            if (Id != UpdatedUser.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(Id);
                    user.UserName=UpdatedUser.UserName;
                    user.PhoneNumber=UpdatedUser.PhoneNumber;

                     var result=  await _userManager.UpdateAsync(user);
                    if(result.Succeeded)
                        return RedirectToAction(nameof(Index));
                    foreach (var Error in result.Errors)
                    {
                        ModelState.AddModelError("", Error.Description);
                    }
                }
                catch (System.Exception ex)
                {
                    //log in database
                    //friendly message
                    //ModelState.AddModelError("",ex.Message);
                    ModelState.AddModelError("", ex.Message);

                }
            }
            return View(UpdatedUser);
        }

        public async Task<IActionResult> Delete(string id)
        {
            return await Datails(id, "Delete");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] string Id, ApplicationUser DeletedUser)
        {
            if (Id != DeletedUser.Id)
                return BadRequest();

           
                try
                {
                    var user = await _userManager.FindByIdAsync(Id);
                    if (user is null)
                        return NotFound();

                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                        return RedirectToAction(nameof(Index));
                    foreach (var Error in result.Errors)
                    {
                        ModelState.AddModelError("", Error.Description);
                    }
                }
                catch (System.Exception ex)
                {
                    //log in database
                    //friendly message
                    //ModelState.AddModelError("",ex.Message);
                    ModelState.AddModelError("", ex.Message);

                }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
