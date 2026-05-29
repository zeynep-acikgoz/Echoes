using System;

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
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; } 
    public string? RelatedPerson { get; set; }
    public string? ImagePath { get; set; }
    public string? YoutubeUrl { get; set; } 
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public MemoryStatus Status { get; set; }
}