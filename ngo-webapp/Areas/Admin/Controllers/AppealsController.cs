using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ngo_webapp.Models.Entities;

namespace ngo_webapp.Areas.Admin.Controllers;

[Area("Admin")]
public class AppealsController(NgoManagementContext dbContext, ILogger<AppealsController> logger, IConfiguration configuration) : Controller
{
	private readonly ILogger<AppealsController> _logger = logger;
	public readonly NgoManagementContext _dbContext = dbContext;
	private readonly IConfiguration _configuration = configuration;

	// GET: Admin/Appeals
	public async Task<IActionResult> List()
	{
		var appeals = await _dbContext.Appeals.ToListAsync();
		return View(appeals);
	}

	// GET: Admin/Appeals/Details/5
	public async Task<IActionResult> Details(int? id)
	{
		if (id != null)
		{
			var appeal = await _dbContext.Appeals.FirstOrDefaultAsync(m => m.AppealsId == id);
			return appeal != null ? View(appeal) : NotFound();
		}
		return NotFound();
	}

	// GET: Admin/Appeals/Create
	public IActionResult Create() => View();

	// POST: Admin/Appeals/Create
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateEvent([Bind("AppealsId,AppealsName,Organization,Description,CreationDate,EndDate,Amount,Status,AppealsImage")] Appeal appeal, IFormFile file)
	{
		if (ModelState.IsValid)
		{
			if (file != null && file.Length > 0)
			{
				var fileName = DateTime.Now.ToString("yyyymmddhhmmss") + file.FileName;
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
				using (var streams = new FileStream(filePath, FileMode.Create)) // upload a pic into the folder
				{
					await file.CopyToAsync(streams);
				}
				appeal.AppealsImage = fileName;
			}
			_dbContext.Add(appeal);
			await _dbContext.SaveChangesAsync();
			return RedirectToAction(nameof(List));
		}
		return View(appeal);
	}

	// GET: Admin/Appeals/Edit/5
	public async Task<IActionResult> Update(int? id)
	{
		if (id != null)
		{
			var appeal = await _dbContext.Appeals.FindAsync(id);
			return appeal != null ? View(appeal) : NotFound();
		}
		return NotFound();
	}

	// POST: Admin/Appeals/Edit/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> UpdateEvent(int id, [Bind("AppealsId,AppealsName,Organization,Description,CreationDate,EndDate,Amount,Status,AppealsImage")] Appeal appeal, IFormFile file)
	{
		if (id == appeal.AppealsId)
		{
			if (ModelState.IsValid)
			{
				try
				{
					if (file != null && file.Length > 0)
					{
						var fileName = DateTime.Now.ToString("yyyymmddhhmmss") + file.FileName;
						var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
						using (var streams = new FileStream(filePath, FileMode.Create)) // upload a pic into the folder
						{
							await file.CopyToAsync(streams);
						}
						appeal.AppealsImage = fileName;
					}
					_dbContext.Update(appeal);
					await _dbContext.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (AppealExists(appeal.AppealsId))
					{
						throw;
					}
					return NotFound();
				}
				return RedirectToAction(nameof(List));
			}
			return View(appeal);
		}
		return NotFound();
	}

	// GET: Admin/Appeals/Delete/5
	public async Task<IActionResult> Delete(int? id)
	{
		if (id != null)
		{
			var appeal = await _dbContext.Appeals.FirstOrDefaultAsync(m => m.AppealsId == id);
			return appeal != null ? View(appeal) : NotFound();
		}
		return NotFound();
	}

	// POST: Admin/Appeals/Delete/5
	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var appeal = await _dbContext.Appeals.FindAsync(id);
		_dbContext.Appeals.Remove(appeal);

		await _dbContext.SaveChangesAsync();
		return RedirectToAction(nameof(List));
	}
	private bool AppealExists(int id) => _dbContext.Appeals.Any(e => e.AppealsId == id);
}