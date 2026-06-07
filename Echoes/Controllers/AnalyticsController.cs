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

        // --- 1. KATEGORİ GRAFİKLERİ İÇİN VERİ ÇEKEN METOT ---
        [HttpGet]
        public async Task<IActionResult> GetCategoryStats()
        {
            var memories = await _context.Memories.ToListAsync();

            var goodCategories = memories
                .Where(m => m.Status == MemoryStatus.Goods && !string.IsNullOrWhiteSpace(m.Category))
                .SelectMany(m => m.Category.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(c => c.Trim())
                .GroupBy(c => c)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

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

        // --- 2. KİŞİ GRAFİKLERİ İÇİN VERİ ÇEKEN METOT ---
        [HttpGet]
        public async Task<IActionResult> GetPeopleStats()
        {
            var memories = await _context.Memories.ToListAsync();

            var goodPeople = memories
                .Where(m => m.Status == MemoryStatus.Goods && !string.IsNullOrWhiteSpace(m.RelatedPerson))
                .SelectMany(m => m.RelatedPerson.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(p => p.Trim())
                .GroupBy(p => p)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

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
        
        // --- 3. MOOD FLOW (AYLIK RUH HALİ AKIŞI) ---
        [HttpGet]
        public async Task<IActionResult> GetMoodFlowStats(int? year, int? month)
        {
            // Eğer yıl/ay gönderilmemişse şu anki tarihi kullan
            int targetYear = year ?? DateTime.Now.Year;
            int targetMonth = month ?? DateTime.Now.Month;
            int daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);

            var thisMonthMoods = await _context.DailyMoods 
                .Where(m => m.Date.Year == targetYear && m.Date.Month == targetMonth)
                .ToListAsync();

            var dailyData = new List<int?>(); 

            for (int i = 1; i <= daysInMonth; i++)
            {
                var dayMood = thisMonthMoods.FirstOrDefault(m => m.Date.Day == i);
                dailyData.Add(dayMood != null ? (int)dayMood.Level : null); 
            }

            var weeklyData = new List<double?>();
            weeklyData.Add(CalculateAverage(dailyData.Skip(0).Take(7)));   
            weeklyData.Add(CalculateAverage(dailyData.Skip(7).Take(7)));   
            weeklyData.Add(CalculateAverage(dailyData.Skip(14).Take(7)));  
            weeklyData.Add(CalculateAverage(dailyData.Skip(21)));          

            return Json(new { success = true, daily = dailyData, weekly = weeklyData, daysInMonth = daysInMonth });
        }

        // Haftalık ortalama hesaplamak için yardımcı metot
        private double? CalculateAverage(IEnumerable<int?> days)
        {
            var validDays = days.Where(d => d.HasValue).ToList();
            if (!validDays.Any()) return null;
            return Math.Round(validDays.Average(d => d.Value), 1);
        }

        // --- 4. HEATMAP (SON 1 YILLIK AKTİVİTE) ---
        [HttpGet]
        public async Task<IActionResult> GetHeatmapStats()
        {
            var startDate = DateTime.Now.Date.AddDays(-364); // Son 1 yıl

            // 1. Anı Tarihleri
            var memoryDates = await _context.Memories
                .Where(m => m.CreatedDate >= startDate)
                .Select(m => m.CreatedDate.Date)
                .ToListAsync();

            // 2. Mektup Tarihleri
            var letterDates = await _context.Letters
                .Where(l => l.CreatedDate >= startDate)
                .Select(l => l.CreatedDate.Date)
                .ToListAsync();

            // 3. Mod Tarihleri
            var moodDates = await _context.DailyMoods
                .Where(m => m.Date >= startDate)
                .Select(m => m.Date.Date)
                .ToListAsync();

            // Hepsini tek bir listede birleştiriyoruz
            var allActivityDates = memoryDates.Concat(letterDates).Concat(moodDates).ToList();

            // Hangi tarihte kaç tane işlem yapılmış sayıyoruz
            var heatmapData = allActivityDates
                .GroupBy(d => d)
                .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                .ToDictionary(x => x.Date, x => x.Count);

            return Json(new { success = true, heatmap = heatmapData });
        }
    }
}