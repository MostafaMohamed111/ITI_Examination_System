using ITI.DAL.Entities.Identity;
using ITIExaminationSystemMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITIExaminationSystemMVC.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUsers> _signInManager;
    private readonly UserManager<AppUsers> _userManager;

    public AccountController(SignInManager<AppUsers> signInManager, UserManager<AppUsers> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Instructor"))
                    {
                        return RedirectToAction("Index", "Instructor");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Student"))
                    {
                        return RedirectToAction("Index", "Student");
                    }
                }
                
                return RedirectToAction("Index", "Home"); 
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}
