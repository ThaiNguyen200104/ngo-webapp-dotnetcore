using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ngo_webapp.Models;
using ngo_webapp.Models.Entities;

namespace ngo_webapp.Controllers;
public class AdminController : Controller
{
	public IActionResult LoginAdmin() => View();

	[HttpPost]
	public async Task<IActionResult> LoginAdmin(AdminLoginViewModel model)
	{
		if (ModelState.IsValid)
		{
			try
			{
				using (NgoManagementContext context = new NgoManagementContext())
				{
					var user = await context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
					if (user != null)
					{
						if (BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
						{
							if (user.IsAdmin)
							{
								HttpContext.Session.SetString("UserID", user.UserId.ToString());
								HttpContext.Session.SetString("Is_Admin", user.IsAdmin.ToString());

								return RedirectToAction("Index"); // Redirect to the admin dashboard
							}
							else
							{
								// User is not an admin
								throw new UnauthorizedAccessException("You do not have admin privileges.");
							}
						}
						else
						{
							// Password is incorrect
							throw new ArgumentException("Invalid login attempt.");
						}
					}
					else
					{
						// Email does not exist
						throw new ArgumentException("Invalid login attempt.");
					}
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(model);
			}
		}
		return View(model);
	}


	[HttpPost]
	public IActionResult Logout()
	{
		HttpContext.Session.Clear();
		return RedirectToAction("LoginAdmin", "Admin");
	}


	public IActionResult Dashboard()
	{
		if (HttpContext.Session.GetString("Is_Admin") != "True")
		{
			return RedirectToAction("LoginAdmin"); // Redirect non-admin users to the login page
		}

		return View(); // Return the Dashboard view for admin users
	}
	public IActionResult Index() => View();

}