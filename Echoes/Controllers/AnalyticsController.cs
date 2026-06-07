using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Echoes.Data;   
using Echoes.Models; 

namespace Echoes.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- KATEGORİ GRAFİKLERİ İÇİN VERİ ÇEKEN METOT ---
        [HttpGet]
        public async Task<IActionResult> GetCategoryStats()
        {
            var memories = await _context.Memories.ToListAsync();

            // İYİ KATEGORİLER (Status == Goods)
            // SelectMany ve Split ile "Aile, İş" gibi virgüllü gelenleri ayırıp tek tek sayıyoruz
            var goodCategories = memories
                .Where(m => m.Status == MemoryStatus.Goods && !string.IsNullOrWhiteSpace(m.Category))
                .SelectMany(m => m.Category.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim())
                .GroupBy(c => c)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

            // KÖTÜ KATEGORİLER (Status == Bads)
            var badCategories = memories
                .Where(m => m.Status == MemoryStatus.Bads && !string.IsNullOrWhiteSpace(m.Category))
                .SelectMany(m => m.Category.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim())
                .GroupBy(c => c)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

            return Json(new { success = true, good = goodCategories, bad = badCategories });
        }

        // --- KİŞİ GRAFİKLERİ İÇİN VERİ ÇEKEN METOT ---
        [HttpGet]
        public async Task<IActionResult> GetPeopleStats()
        {
            var memories = await _context.Memories.ToListAsync();

            // ENERJİ VERENLER (Status == Goods ve RelatedPerson dolu olanlar)
            var goodPeople = memories
                .Where(m => m.Status == MemoryStatus.Goods && !string.IsNullOrWhiteSpace(m.RelatedPerson))
                .SelectMany(m => m.RelatedPerson.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(p => p.Trim())
                .GroupBy(p => p)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

            // ENERJİ EMENLER (Status == Bads ve RelatedPerson dolu olanlar)
            var badPeople = memories
                .Where(m => m.Status == MemoryStatus.Bads && !string.IsNullOrWhiteSpace(m.RelatedPerson))
                .SelectMany(m => m.RelatedPerson.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(p => p.Trim())
                .GroupBy(p => p)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

            return Json(new { success = true, good = goodPeople, bad = badPeople });
        }
    }
}