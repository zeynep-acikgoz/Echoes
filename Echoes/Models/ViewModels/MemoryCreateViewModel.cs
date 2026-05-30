using System.ComponentModel.DataAnnotations;

namespace Echoes.Models.ViewModels;

public class MemoryCreateViewModel
{
    
    public int Id { get; set; } = 0;
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Content { get; set; }
    
    public string? SongUrl { get; set; } // YouTube linki
    
    public int StatusValue { get; set; } // Hangi sekmede olduğumuz (Goods, Bads vb.)

   
    public string SelectedCategories { get; set; } 
    public string SelectedPeople { get; set; }
    
    // Fotoğraflar
    public List<IFormFile>? Photos { get; set; }
}