using System;
using System.ComponentModel.DataAnnotations;

namespace Echoes.Models;

public class Letters
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Opsiyonel mühür tarihi. Boşsa en başından beri mühürsüzdür.
    public DateTime? UnlockDate { get; set; }

    // YENİ EKLENEN: Mektup henüz taslak aşamasında mı? (Varsayılan: Evet)
    public bool IsDraft { get; set; } = true;

    // YENİ EKLENEN: Zamanı geldiğinde mühür kullanıcı tarafından kırıldı mı? (Varsayılan: Hayır)
    public bool IsOpened { get; set; } = false;
}