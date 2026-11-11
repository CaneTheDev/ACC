using System.ComponentModel.DataAnnotations;

namespace AccountTimer.Models;

public class Account
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Email { get; set; }

    public int AccountListId { get; set; }
    public AccountList? AccountList { get; set; }

    public DateTime? TimerStartedAt { get; set; }
    
    public int? ResetDurationDays { get; set; }

    public DateTime? ResetAt => TimerStartedAt?.AddDays(ResetDurationDays ?? 0);

    public bool IsAvailable => !TimerStartedAt.HasValue || DateTime.Now >= ResetAt;

    public bool IsSuspended { get; set; }

    public int SortOrder { get; set; }
}
