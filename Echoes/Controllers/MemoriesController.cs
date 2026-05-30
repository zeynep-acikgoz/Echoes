using Microsoft.AspNetCore.Mvc;
using Echoes.Data;
using Echoes.Models;
using Echoes.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 

namespace Echoes.Controllers;

public class MemoriesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public MemoriesController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(MemoryStatus? status)
    {
        var selectedStatus = status ?? MemoryStatus.Goods;
        
        var memories = await _context.Memories
            .Where(m => m.Status == selectedStatus)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();

        var allMemories = await _context.Memories.ToListAsync();

        var allCategories = allMemories
            .Where(m => !string.IsNullOrWhiteSpace(m.Category))
            .SelectMany(m => m.Category.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var allPeople = allMemories
            .Where(m => !string.IsNullOrWhiteSpace(m.RelatedPerson))
            .SelectMany(m => m.RelatedPerson.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        ViewBag.AllCategories = allCategories;
        ViewBag.AllPeople = allPeople;
        ViewBag.CurrentStatus = selectedStatus;
        
        return View(memories);
    }

    [HttpPost]
    public async Task<IActionResult> Create(MemoryCreateViewModel model)
    {
        if (!ModelState.IsValid) 
            return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });

        try
        {
            Memories memoryToSave;

            if (model.Id > 0)
            {
                // DÜZENLEME (UPDATE)
                memoryToSave = await _context.Memories.FindAsync(model.Id);
                if (memoryToSave == null) 
                    return Json(new { success = false, message = "Kayıt bulunamadı!" });

                memoryToSave.Title = model.Title;
                memoryToSave.Content = model.Content;
                memoryToSave.SongUrl = model.SongUrl;
                memoryToSave.Category = model.SelectedCategories;
                memoryToSave.RelatedPerson = model.SelectedPeople;
            }
            else
            {
                // YENİ KAYIT (INSERT)
                memoryToSave = new Memories
                {
                    Title = model.Title,
                    Content = model.Content,
                    SongUrl = model.SongUrl,
                    Status = (MemoryStatus)model.StatusValue,
                    CreatedDate = DateTime.Now,
                    Category = model.SelectedCategories,
                    RelatedPerson = model.SelectedPeople
                };
                _context.Memories.Add(memoryToSave);
            }

            // --- FOTOĞRAF İŞLEMLERİ (YENİLENMİŞ GÜVENLİ YAPI) ---
            if (model.Photos != null && model.Photos.Count > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadFolder)) 
                    Directory.CreateDirectory(uploadFolder);

                List<string> fileNames = new List<string>();
                foreach (var photo in model.Photos)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadFolder, fileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }
                    fileNames.Add(fileName);
                }
                
                string newPhotos = string.Join(";", fileNames);

                // Edit modundaysak ve önceden fotoğraf varsa ESKİLERİN YANINA EKLE
                if (model.Id > 0 && !string.IsNullOrWhiteSpace(memoryToSave.PhotoPath))
                {
                    // Boşlukları ve fazla noktalı virgülleri temizleyerek birleştir
                    memoryToSave.PhotoPath = memoryToSave.PhotoPath.TrimEnd(';') + ";" + newPhotos;
                }
                else
                {
                    // Eskiden fotoğraf yoksa veya yeni kayıtsa direkt ata
                    memoryToSave.PhotoPath = newPhotos;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, int newStatus)
    {
        var memory = await _context.Memories.FindAsync(id);
        if (memory == null) 
            return Json(new { success = false, message = "Kayıt bulunamadı!" });

        memory.Status = (MemoryStatus)newStatus;
        await _context.SaveChangesAsync();
        
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var memory = await _context.Memories.FindAsync(id);
        if (memory == null) 
            return Json(new { success = false, message = "Kayıt bulunamadı!" });

        _context.Memories.Remove(memory);
        await _context.SaveChangesAsync();
        
        return Json(new { success = true });
    }
}