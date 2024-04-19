using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PresentationLayer.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin")]

    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index(string SearchValue)
        {
            var roles = Enumerable.Empty<IdentityRole>().ToList();
            if (string.IsNullOrEmpty(SearchValue))
                roles.AddRange(_roleManager.Roles);
            else
            roles = await _roleManager.Roles.Where(user => user.Name.Trim().ToLower().Contains(SearchValue.Trim().ToLower())).ToListAsync();



            return View(roles);

        }

        public IActionResult Create()
        {
            //ViewBag.Departments=_departmemtRepository.GetAll();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole Role)
        {
            if (ModelState.IsValid)
            {
               var result=   await _roleManager.CreateAsync(Role);
                if(result.Succeeded)
                   return RedirectToAction(nameof(Index));

                foreach (var Error in result.Errors)
                {
                    ModelState.AddModelError("", Error.Description);
                }
            }
            return View(Role);
        }

        public async Task<IActionResult> Datails(string id, string ViewName = "Datails")
        {
            if (id == null)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound();

            return View(ViewName, role);
        }



        public async Task<IActionResult> Edit(string id)
        {
            //ViewBag.Departments = _departmemtRepository.GetAll();
            return await Datails(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IdentityRole UpdatedRole, [FromRoute] string Id)
        {
            if (Id != UpdatedRole.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    var role = await _roleManager.FindByIdAsync(Id);
                    role.Name = UpdatedRole.Name;


                    
                    var result =  await _roleManager.UpdateAsync(role);
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
            }
            return View(UpdatedRole);
        }

        public async Task<IActionResult> Delete(string id)
        {
            return await Datails(id, "Delete");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] string Id, IdentityRole DeletedRole)
        {
         
            if (Id != DeletedRole.Id)
                return BadRequest();


            try
            {
                var user = await _roleManager.FindByIdAsync(Id);
                if (user is null)
                    return NotFound();

                var result = await _roleManager.DeleteAsync(user);

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

        public async Task<IActionResult> AddOrRemoveUsers(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role is null)
                return NotFound();

            ViewBag.RoleId = roleId;

            var usersInRole = new List<UserInRoleViewModel>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var UserWithRole = new UserInRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                if(await _userManager.IsInRoleAsync(user,role.Name))
                    UserWithRole.IsSelected = true;
                else
                    UserWithRole.IsSelected = false;


                usersInRole.Add(UserWithRole);

            }
            return View(usersInRole);


        }
        [HttpPost]
        public async Task<IActionResult> AddOrRemoveUsers(string roleId, List<UserInRoleViewModel> users)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
                return NotFound();

            if (ModelState.IsValid)
            {

                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.UserId);


                    if (appUser != null)
                    {
                        if (user.IsSelected && !(await _userManager.IsInRoleAsync(appUser, role.Name)))
                            await _userManager.AddToRoleAsync(appUser, role.Name);
                        else if (!user.IsSelected && (await _userManager.IsInRoleAsync(appUser, role.Name)))
                            await _userManager.RemoveFromRoleAsync(appUser, role.Name);

                    }
                }
                return RedirectToAction("Edit", new { id = roleId });
            }

            return View(users);
        }
       


    }
}
