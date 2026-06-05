using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Echoes.Data; 
using Echoes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Echoes.Controllers
{
    public class LettersController : Controller
    {
        private readonly AppDbContext _context;

        public LettersController(AppDbContext context)
        {
            _context = context;
        }

        // Ana Liste Sayfası (3 Sekmeli Görünüm)
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

        // YENİ: Butona tıklandığında anında boş bir taslak oluşturup ID'sini döner
        [HttpPost]
        public async Task<IActionResult> CreateDraft()
        {
            var draft = new Letters
            {
                Title = "", // Başlangıçta boş bırakıyoruz, kullanıcı Notion ekranında dolduracak
                Content = "",
                CreatedDate = DateTime.Now,
                IsDraft = true, // Taslak olarak kilitliyoruz
                IsOpened = false
            };

            _context.Letters.Add(draft);
            await _context.SaveChangesAsync();

            // Oluşan taslağın ID'sini frontend'e yolluyoruz ki doğru sayfayı açsın
            return Json(new { success = true, id = draft.Id });
        }

        // YENİ: Taslağı Notion tarzı tam ekran editörde açacak olan sayfa
        [HttpGet]
        public async Task<IActionResult> Write(int id)
        {
            var letter = await _context.Letters.FindAsync(id);
            
            if (letter == null)
            {
                return RedirectToAction("Index"); // Mektup bulunamazsa ana sayfaya geri atmak için
            }

            return View(letter); // Write.cshtml sayfasına mektup verisiyle git go go go
        }
        
        
        [HttpPost]
        public async Task<IActionResult> SaveLetterAuto(Letters updatedLetter)
        {
            var letter = await _context.Letters.FindAsync(updatedLetter.Id);
            if (letter == null) return Json(new { success = false });

            // Sadece doluysa güncelle, Notion tarzı editör boş bırakmaya da izin verir
            letter.Title = updatedLetter.Title ?? "";
            letter.Content = updatedLetter.Content ?? "";
            letter.UnlockDate = updatedLetter.UnlockDate;
            letter.IsDraft = updatedLetter.IsDraft;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        
        // YENİ: Mektup Silme Metodu
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var letter = await _context.Letters.FindAsync(id);
            if (letter != null)
            {
                _context.Letters.Remove(letter);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        
        // YENİ: Dışarıdan Hızlı Mühürleme Metodu
        [HttpPost]
        public async Task<IActionResult> SealLetter(int id, DateTime unlockDate)
        {
            var letter = await _context.Letters.FindAsync(id);
            if (letter != null)
            {
                letter.IsDraft = false; // Artık taslak değil
                letter.UnlockDate = unlockDate; // Seçilen tarihi ata
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        
        [HttpPost]
        public async Task<IActionResult> UnsealLetter(int id)
        {
            var letter = await _context.Letters.FindAsync(id);
    
            if (letter == null) 
            {
                return Json(new { success = false, message = "Letter not found." });
            }

            // Mektubun açılma zamanının gelip gelmediğini son bir kez güvenliğe alıyoruz
            if (letter.UnlockDate.HasValue && letter.UnlockDate.Value.Date <= DateTime.Now.Date)
            {
                letter.IsOpened = true; // Mektup artık açıldı!
                _context.Update(letter);
                await _context.SaveChangesAsync();
        
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "This letter is not ready to be opened yet." });
        }
        
    }
}