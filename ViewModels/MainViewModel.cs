using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AccountTimer.Data;
using AccountTimer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace AccountTimer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AccountListViewModel> accountLists = new();

    [ObservableProperty]
    private AccountListViewModel? selectedAccountList;

    public MainViewModel()
    {
        try
        {
            InitializeDatabaseAsync();
            LoadAccountListsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error initializing: {ex.Message}\n\n{ex.StackTrace}", "Startup Error");
        }
    }

    private async void InitializeDatabaseAsync()
    {
        using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async void LoadAccountListsAsync()
    {
        using var db = new AppDbContext();
        var lists = await db.AccountLists
            .Include(al => al.Accounts)
            .ToListAsync();

        AccountLists.Clear();
        foreach (var list in lists)
        {
            AccountLists.Add(new AccountListViewModel(list, this));
        }

        if (AccountLists.Any())
        {
            SelectedAccountList = AccountLists.First();
        }
    }

    [RelayCommand]
    private async Task AddAccountListAsync(string listName)
    {
        if (string.IsNullOrWhiteSpace(listName))
            return;

        using var db = new AppDbContext();
        var newList = new AccountList { Name = listName };
        db.AccountLists.Add(newList);
        await db.SaveChangesAsync();

        LoadAccountListsAsync();
    }

    [RelayCommand]
    private async Task DeleteAccountListAsync(AccountListViewModel listVm)
    {
        using var db = new AppDbContext();
        var list = await db.AccountLists.FindAsync(listVm.Id);
        if (list != null)
        {
            db.AccountLists.Remove(list);
            await db.SaveChangesAsync();
        }

        LoadAccountListsAsync();
    }

    [RelayCommand]
    private async Task AddAccountsAsync((int listId, string accountsText) args)
    {
        if (string.IsNullOrWhiteSpace(args.accountsText))
            return;

        var emails = args.accountsText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct() // Remove duplicates from input
            .ToList();

        using var db = new AppDbContext();
        var list = await db.AccountLists
            .Include(al => al.Accounts)
            .FirstAsync(al => al.Id == args.listId);

        // Filter out emails that already exist
        var existingEmails = list.Accounts.Select(a => a.Email.ToLower()).ToHashSet();
        var newEmails = emails.Where(e => !existingEmails.Contains(e.ToLower())).ToList();

        if (newEmails.Count == 0)
        {
            MessageBox.Show("All accounts already exist in this list.", "No New Accounts", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var maxOrder = list.Accounts.Any() ? list.Accounts.Max(a => a.SortOrder) : 0;

        foreach (var email in newEmails)
        {
            maxOrder++;
            list.Accounts.Add(new Account
            {
                Email = email,
                AccountListId = list.Id,
                SortOrder = maxOrder
            });
        }

        await db.SaveChangesAsync();
        
        var skipped = emails.Count - newEmails.Count;
        if (skipped > 0)
        {
            MessageBox.Show($"Added {newEmails.Count} new account(s).\n{skipped} duplicate(s) skipped.", 
                "Accounts Added", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        LoadAccountListsAsync();
    }

    [RelayCommand]
    private async Task SetGlobalResetDurationAsync((int listId, int days) args)
    {
        using var db = new AppDbContext();
        var list = await db.AccountLists.FindAsync(args.listId);
        if (list != null)
        {
            list.GlobalResetDurationDays = args.days;
            await db.SaveChangesAsync();
        }

        LoadAccountListsAsync();
    }

    public async void StartAccountTimer(int listId, int accountId)
    {
        using var db = new AppDbContext();
        var list = await db.AccountLists
            .Include(al => al.Accounts)
            .FirstAsync(al => al.Id == listId);
        
        var account = list.Accounts.First(a => a.Id == accountId);
        
        var resetDays = account.ResetDurationDays ?? list.GlobalResetDurationDays ?? 7;
        
        account.TimerStartedAt = DateTime.Now;
        account.ResetDurationDays = resetDays;
        
        // Move to bottom
        var maxOrder = list.Accounts.Max(a => a.SortOrder);
        account.SortOrder = maxOrder + 1;
        
        await db.SaveChangesAsync();
        LoadAccountListsAsync();
    }

    public async void ResetAccountTimer(int listId, int accountId)
    {
        using var db = new AppDbContext();
        var list = await db.AccountLists
            .Include(al => al.Accounts)
            .FirstAsync(al => al.Id == listId);
        
        var account = list.Accounts.First(a => a.Id == accountId);
        
        // Clear timer and move to top
        account.TimerStartedAt = null;
        account.ResetDurationDays = null;
        
        // Move to top
        var minOrder = list.Accounts.Min(a => a.SortOrder);
        account.SortOrder = minOrder - 1;
        
        await db.SaveChangesAsync();
        LoadAccountListsAsync();
    }

    public async void CancelAccountTimer(int listId, int accountId)
    {
        using var db = new AppDbContext();
        var list = await db.AccountLists
            .Include(al => al.Accounts)
            .FirstAsync(al => al.Id == listId);
        
        var account = list.Accounts.First(a => a.Id == accountId);
        
        // Clear timer and move to top (same as reset but semantically different action)
        account.TimerStartedAt = null;
        account.ResetDurationDays = null;
        
        // Move to top
        var minOrder = list.Accounts.Min(a => a.SortOrder);
        account.SortOrder = minOrder - 1;
        
        await db.SaveChangesAsync();
        LoadAccountListsAsync();
    }

    public async void ToggleAccountSuspension(int listId, int accountId)
    {
        using var db = new AppDbContext();
        var list = await db.AccountLists
            .Include(al => al.Accounts)
            .FirstAsync(al => al.Id == listId);
        
        var account = list.Accounts.First(a => a.Id == accountId);
        
        account.IsSuspended = !account.IsSuspended;
        
        if (account.IsSuspended)
        {
            // Move to absolute bottom
            var maxOrder = list.Accounts.Max(a => a.SortOrder);
            account.SortOrder = maxOrder + 1000; // Large gap to ensure it stays at bottom
            
            // Clear any active timer
            account.TimerStartedAt = null;
            account.ResetDurationDays = null;
        }
        else
        {
            // Move to top when unsuspended
            var minOrder = list.Accounts.Min(a => a.SortOrder);
            account.SortOrder = minOrder - 1;
        }
        
        await db.SaveChangesAsync();
        LoadAccountListsAsync();
    }

    public async void DeleteAccount(int listId, int accountId)
    {
        using var db = new AppDbContext();
        var account = await db.Accounts.FindAsync(accountId);
        if (account != null)
        {
            db.Accounts.Remove(account);
            await db.SaveChangesAsync();
        }
        
        LoadAccountListsAsync();
    }

    [RelayCommand]
    private async Task DeleteSelectedAccountsAsync(AccountListViewModel listVm)
    {
        var selectedAccounts = listVm.Accounts.Where(a => a.IsSelected).ToList();
        
        if (selectedAccounts.Count == 0)
        {
            MessageBox.Show("No accounts selected.", "Delete Accounts", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to delete {selectedAccounts.Count} selected account(s)?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            using var db = new AppDbContext();
            var accountIds = selectedAccounts.Select(a => a.Id).ToList();
            var accountsToDelete = await db.Accounts.Where(a => accountIds.Contains(a.Id)).ToListAsync();
            
            db.Accounts.RemoveRange(accountsToDelete);
            await db.SaveChangesAsync();
            
            LoadAccountListsAsync();
        }
    }
}
