using Microsoft.EntityFrameworkCore;
using AccountTimer.Models;
using System.IO;

namespace AccountTimer.Data;

public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountList> AccountLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AccountTimer",
            "accounts.db");

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasOne(a => a.AccountList)
            .WithMany(al => al.Accounts)
            .HasForeignKey(a => a.AccountListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
