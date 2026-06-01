using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Echoes.Data; // Kendi proje ismine göre namespace'i kontrol et
using Echoes.Models;

namespace Echoes.Controllers
{
    public class LettersController : Controller
    {
        private readonly AppDbContext _context;

        public LettersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var allLetters = await _context.Letters.OrderByDescending(l => l.CreatedDate).ToListAsync();

            // 1. Taslaklar: Henüz mühürlenmemiş olanlar
            ViewBag.Drafts = allLetters.Where(l => l.IsDraft).ToList();

            // 2. Mühürlüler: Taslak olmayan, henüz açılmamış VE bir mühür tarihi olanlar
            ViewBag.Sealed = allLetters.Where(l => !l.IsDraft && !l.IsOpened && l.UnlockDate.HasValue).ToList();

            // 3. Açıklar: Taslak olmayan VE (ya mührü bizzat kırılmış YA DA en başından mühürsüz kaydedilmiş olanlar)
            ViewBag.Unsealed = allLetters.Where(l => !l.IsDraft && (l.IsOpened || !l.UnlockDate.HasValue)).ToList();

            return View();
        }
    }
}