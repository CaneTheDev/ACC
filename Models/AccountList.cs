using System.ComponentModel.DataAnnotations;

namespace AccountTimer.Models;

public class AccountList
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public int? GlobalResetDurationDays { get; set; }

    public List<Account> Accounts { get; set; } = new();
}
