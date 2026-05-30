using Microsoft.AspNetCore.Mvc;
using Echoes.Data;
using Echoes.Models;
using Microsoft.EntityFrameworkCore;

namespace Echoes.Controllers;

public class MemoriesController : Controller
{
    private readonly AppDbContext _context;

    public MemoriesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(MemoryStatus? status)
    {
        // status yoksa default good
        var selectedStatus = status ?? MemoryStatus.Goods;
        
        var memories = await _context.Memories
            .Where(m => m.Status == selectedStatus)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();

        ViewBag.CurrentStatus = selectedStatus;
        return View(memories);
    }
}