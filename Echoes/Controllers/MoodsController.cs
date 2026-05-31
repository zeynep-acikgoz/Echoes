using Microsoft.AspNetCore.Mvc;
using Echoes.Data;
using Echoes.Models;
using Microsoft.EntityFrameworkCore;

namespace Echoes.Controllers
{
    public class MoodsController : Controller
    {
        private readonly AppDbContext _context;

        public MoodsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Sayfayı Yükleme Metodu
        public IActionResult Index()
        {
            return View();
        }

        // 2. Takvimdeki günleri renklendirmek için verileri AJAX ile çekecek metot
        [HttpGet]
        public async Task<IActionResult> GetMoodsForMonth(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var moods = await _context.DailyMoods
                .Where(m => m.Date >= startDate && m.Date <= endDate)
                .Select(m => new { 
                    date = m.Date.ToString("yyyy-MM-dd"), 
                    level = (int)m.Level,
                    note = m.Note // NOTU DA ÖNYÜZE GÖNDERİYORUZ
                })
                .ToListAsync();

            return Json(moods);
        }

        // 3. Minik baloncuktan (inline menu) renk seçildiğinde çalışacak Kaydetme Metodu
        [HttpPost]
        public async Task<IActionResult> SaveMood(DateTime date, int level)
        {
            try
            {
                var pureDate = date.Date; 
                var existingMood = await _context.DailyMoods.FirstOrDefaultAsync(m => m.Date == pureDate);

                // EĞER LEVEL 0 GELDİYSE (TEMİZLEME İŞLEMİ)
                if (level == 0)
                {
                    if (existingMood != null)
                    {
                        _context.DailyMoods.Remove(existingMood);
                        await _context.SaveChangesAsync();
                    }
                    return Json(new { success = true });
                }

                // NORMAL EKLEME / GÜNCELLEME İŞLEMİ
                if (existingMood != null)
                {
                    existingMood.Level = (MoodLevel)level;
                }
                else
                {
                    var newMood = new DailyMood { Date = pureDate, Level = (MoodLevel)level };
                    _context.DailyMoods.Add(newMood);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetChartData(int? year, int? month)
        {
            IQueryable<DailyMood> query = _context.DailyMoods;

            // Eğer yıl ve ay geldiyse filtrele, gelmediyse (All Time) hepsini al
            if (year.HasValue && month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                query = query.Where(m => m.Date >= startDate && m.Date <= endDate);
            }

            // Veritabanında mood'ları grupla ve say
            var data = await query
                .GroupBy(m => m.Level)
                .Select(g => new { level = (int)g.Key, count = g.Count() })
                .ToListAsync();

            // Hiç girilmemiş mood'lar için 0 (Sıfır) değerini garantile ki grafik bozulmasın
            var result = Enumerable.Range(1, 5).Select(i => new {
                level = i,
                count = data.FirstOrDefault(d => d.level == i)?.count ?? 0
            });

            return Json(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveNote(DateTime date, string note)
        {
            var pureDate = date.Date;
            var existingMood = await _context.DailyMoods.FirstOrDefaultAsync(m => m.Date == pureDate);

            if (existingMood != null)
            {
                // Zaten o güne bir renk seçilmişse sadece notunu güncelle
                existingMood.Note = note;
            }
            else
            {
                // Renk seçmeden direkt not yazdıysa, varsayılan olarak "Meh (3)" rengini ata ve notu kaydet
                var newMood = new DailyMood { Date = pureDate, Level = MoodLevel.Meh, Note = note };
                _context.DailyMoods.Add(newMood);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        
    }
}