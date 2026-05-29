using Microsoft.EntityFrameworkCore;
using Echoes.Models;

namespace Echoes.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Memories tablosu
    public DbSet<Memories> Memories { get; set; }
    
    // Letters tablosu
    public DbSet<Letters> Letters { get; set; }
}