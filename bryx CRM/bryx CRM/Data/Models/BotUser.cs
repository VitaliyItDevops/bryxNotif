using System.ComponentModel.DataAnnotations;

namespace bryx_CRM.Data.Models;

public class BotUser
{
    public int Id { get; set; }

    [StringLength(100)]
    public string? ChatId { get; set; }

    [StringLength(100)]
    public string? Username { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsConfirmed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
