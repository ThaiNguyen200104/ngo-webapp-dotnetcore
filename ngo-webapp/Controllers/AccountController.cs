using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ngo_webapp.Models;
using ngo_webapp.Models.Entities;

namespace ngo_webapp.Controllers;
public class AccountController : Controller
{
	[HttpGet]
	public IActionResult Signup() => View();

	[HttpPost]
	public async Task<IActionResult> Signup(SignupViewModel model)
	{
		if (ModelState.IsValid)
		{
			using (NgoManagementContext context = new())
			{
				// Check if the username or email already exists
				var existingUserWithUsername = await context.Users
					.AnyAsync(u => u.Username == model.Username);
				var existingUserWithEmail = await context.Users
					.AnyAsync(u => u.Email == model.Email);

				if (existingUserWithUsername)
				{
					ModelState.AddModelError("Username", "Username is already taken.");
				}

				if (existingUserWithEmail)
				{
					ModelState.AddModelError("Email", "Email is already in use.");
				}

				if (existingUserWithUsername || existingUserWithEmail)
				{
					// Return the view with validation errors
					return View(model);
				}

				// Proceed with creating the user if the username and email are unique
				var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
				User user = new()
				{
					Username = model.Username,
					Email = model.Email,
					PasswordHash = hashedPassword,
					RegistrationDate = DateTime.Now,
					Balance = 10000
				};
				context.Users.Add(user);
				await context.SaveChangesAsync();
				return RedirectToAction("Login", "Account");
			}
		}
		return View(model);
	}


	[HttpGet]
	public IActionResult Login() => View();

	[HttpPost]
	public async Task<IActionResult> Login(LoginViewModel model)
	{
		if (ModelState.IsValid)
		{
			using (NgoManagementContext context = new())
			{
				var user = await context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
				if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
				{
					HttpContext.Session.SetString("UserID", user.UserId.ToString());
					HttpContext.Session.SetString("Username", user.Username);

					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					return View(model);
				}
			}
		}
		return View(model);
	}


	[HttpPost]
	public IActionResult Logout()
	{
		HttpContext.Session.Clear();
		return RedirectToAction("Login", "Account");
	}


	public async Task<IActionResult> Profile()
	{
		if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
		{
			var userId = int.Parse(HttpContext.Session.GetString("UserID"));

			using (NgoManagementContext context = new())
			{
				var user = await context.Users.FindAsync(userId);
				if (user != null)
				{
					var donations = await context.Donations
						.Where(d => d.UserId == userId)
						.Include(d => d.Appeals)
						.Select(d => new ProfileViewModel.DonationDetail
						{
							AppealId = d.AppealsId,
							AppealName = d.Appeals.AppealsName,
							Amount = d.Amount,
							DonationDate = d.DonationDate
						}).ToListAsync();

					ProfileViewModel model = new()
					{
						Username = user.Username,
						RegistrationDate = user.RegistrationDate,
						Balance = user.Balance,
						Bio = user.Bio,
						UserImage = user.UserImage == null ? "default.jpg" : user.UserImage,
						Email = user.Email,
						Donations = donations,
						TotalAmount = donations.Sum(d => d.Amount), // Calculate the total donated amount
						ProjectCount = donations.Select(d => d.AppealId).Distinct().Count()
					};
					return View(model);
				}
				return View("NotFoundPage");
			}
		}
		return RedirectToAction("Login", "Account");
	}


	[HttpGet]
	public IActionResult Edit() => View();

	[HttpPost]
	public async Task<IActionResult> Edit(EditProfileViewModel model, IFormFile file)
	{
		if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
		{
			return RedirectToAction("Login", "Account");
		}

		var userId = int.Parse(HttpContext.Session.GetString("UserID"));
		using (NgoManagementContext context = new())
		{
			try
			{
				var user = await context.Users.FindAsync(userId);
				if (user != null)
				{
					if (file != null && file.Length > 0)
					{
						var filename = DateTime.Now.Ticks + file.FileName;
						var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", filename);
						using (var stream = new FileStream(filePath, FileMode.Create)) // upload into the folder
						{
							await file.CopyToAsync(stream);
						}
						user.UserImage = filename;
					}
					user.Username = model.Username;
					user.Bio = model.Bio;

					await context.SaveChangesAsync();
					return RedirectToAction("Profile", "Account");
				}
				return View("NotFoundPage");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("Error", ex.Message);
				return View();
			}
		}
	}


	public async Task<IActionResult> Export(int appealId)
	{
		using (var context = new NgoManagementContext())
		{
			var donations = await context.Donations
				.Where(d => d.AppealsId == appealId)
				.Include(d => d.User)
				.Include(d => d.Appeals)
				.Select(d => new DonateViewModel
				{
					UserName = d.User.Username,
					AppealName = d.Appeals.AppealsName,
					Amount = d.Amount,
					DonationDate = d.DonationDate
				}).ToListAsync();

			// Use "DonationsPdf" Razor view to generate the PDF content !!!!!!!
			return new Rotativa.AspNetCore.ViewAsPdf("DonationsPdf", donations)
			{
				FileName = $"Donations-{DateTime.Now:yyyyMMddHHmmss}.pdf"
			};
		}
	}

	[HttpGet]
	public IActionResult Faucet()
	{
		if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
		{
			return RedirectToAction("Login");
		}
		return View(new FaucetViewModel());
	}


	[HttpPost]
	public async Task<IActionResult> Faucet(FaucetViewModel model)
	{
		if (ModelState.IsValid)
		{
			var userIdString = HttpContext.Session.GetString("UserID");
			if (!string.IsNullOrEmpty(userIdString))
			{
				var userId = int.Parse(userIdString);
				using (var context = new NgoManagementContext())
				{
					var user = await context.Users.FindAsync(userId);
					if (user == null)
					{
						ModelState.AddModelError("", "User not found.");
						return View(model);
					}
					user.Balance += model.AmountToAdd;
					await context.SaveChangesAsync();
				}
				return RedirectToAction("Profile");
			}
			return RedirectToAction("Login");
		}
		return View(model);
	}
}