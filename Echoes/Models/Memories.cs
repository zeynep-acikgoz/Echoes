using System;
using System.ComponentModel.DataAnnotations;

namespace Echoes.Models;

public enum MemoryStatus
{
    Goods = 1,
    Bads = 2,
    Pasts = 3,
    Ghosts = 4
}

public class Memories
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    // Çoklu seçimleri "Kategori1, Kategori2" şeklinde virgülle saklayacağız
    public string? Category { get; set; } 
    public string? RelatedPerson { get; set; }
    
    // Birden fazla fotoğraf yolu için (Örn: "img1.jpg,img2.png")
    public string? ImagePaths { get; set; }
    
    public string? SongUrl { get; set; } 
    public string? SongTitle { get; set; } // Kartta görünecek olan şarkı adı
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public MemoryStatus Status { get; set; }
    
    public string? PhotoPath { get; set; } 
}