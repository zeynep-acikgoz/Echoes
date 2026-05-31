using System.ComponentModel.DataAnnotations;

namespace Echoes.Models
{
    public enum MoodLevel
    {
        Awful = 1, Bad = 2, Meh = 3, Good = 4, Rad = 5 
    }

    public class DailyMood
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; } 
        
        [Required]
        public MoodLevel Level { get; set; } 

        // BU SATIRIN ORADA OLDUĞUNDAN EMİN OL:
        public string? Note { get; set; } 
    }
}