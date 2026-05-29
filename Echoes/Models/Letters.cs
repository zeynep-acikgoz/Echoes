using System;

namespace Echoes.Models;

public class Letters
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Mühür açılma tarihi (Boşsa mühürsüzdür)
    public DateTime? UnlockDate { get; set; }

    // Yardımcı kontroller
    public bool IsSealed => UnlockDate.HasValue;
    public bool IsLocked => IsSealed && UnlockDate > DateTime.Now;}