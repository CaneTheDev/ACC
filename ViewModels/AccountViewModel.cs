using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AccountTimer.Models;
using System.Windows;
using System.Windows.Threading;

namespace AccountTimer.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    private readonly Account _account;
    private readonly MainViewModel _mainViewModel;
    private readonly int _listId;
    private readonly DispatcherTimer _updateTimer;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private DateTime? timerStartedAt;

    [ObservableProperty]
    private int? resetDurationDays;

    [ObservableProperty]
    private bool isAvailable;

    [ObservableProperty]
    private bool isSuspended;

    [ObservableProperty]
    private string statusText = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    public int Id => _account.Id;

    public AccountViewModel(Account account, MainViewModel mainViewModel, int listId)
    {
        _account = account;
        _mainViewModel = mainViewModel;
        _listId = listId;
        
        email = account.Email;
        timerStartedAt = account.TimerStartedAt;
        resetDurationDays = account.ResetDurationDays;
        isSuspended = account.IsSuspended;
        
        // Create a timer that updates every minute
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _updateTimer.Tick += (s, e) => UpdateStatus();
        _updateTimer.Start();
        
        UpdateStatus();
    }

    [RelayCommand]
    private void CopyEmail()
    {
        Clipboard.SetText(Email);
    }

    [RelayCommand]
    private void StartTimer()
    {
        _mainViewModel.StartAccountTimer(_listId, Id);
    }

    [RelayCommand]
    private void ResetTimer()
    {
        _mainViewModel.ResetAccountTimer(_listId, Id);
    }

    [RelayCommand]
    private void CancelTimer()
    {
        _mainViewModel.CancelAccountTimer(_listId, Id);
    }

    [RelayCommand]
    private void ToggleSuspension()
    {
        _mainViewModel.ToggleAccountSuspension(_listId, Id);
    }

    [RelayCommand]
    private void DeleteAccount()
    {
        _mainViewModel.DeleteAccount(_listId, Id);
    }

    private void UpdateStatus()
    {
        var resetAt = TimerStartedAt?.AddDays(ResetDurationDays ?? 0);
        IsAvailable = !TimerStartedAt.HasValue || DateTime.Now >= resetAt;

        if (IsSuspended)
        {
            StatusText = "⛔ Suspended";
        }
        else if (!TimerStartedAt.HasValue)
        {
            StatusText = "✓ Available";
        }
        else if (IsAvailable)
        {
            StatusText = "✓ Reset complete - Available";
        }
        else
        {
            var remaining = resetAt - DateTime.Now;
            if (remaining.HasValue)
            {
                StatusText = $"⏱ {FormatTimeRemaining(remaining.Value)} remaining";
            }
        }
    }

    private string FormatTimeRemaining(TimeSpan remaining)
    {
        var parts = new List<string>();
        
        int weeks = (int)(remaining.TotalDays / 7);
        int days = remaining.Days % 7;
        int hours = remaining.Hours;
        int minutes = remaining.Minutes;
        
        // For longer durations, show weeks and days
        if (weeks > 0)
        {
            parts.Add($"{weeks}w");
            if (days > 0)
                parts.Add($"{days}d");
            if (hours > 0)
                parts.Add($"{hours}h");
            if (minutes > 0)
                parts.Add($"{minutes}m");
        }
        // For medium durations, show days and hours
        else if (days > 0)
        {
            parts.Add($"{days}d");
            if (hours > 0)
                parts.Add($"{hours}h");
            if (minutes > 0)
                parts.Add($"{minutes}m");
        }
        // For short durations, show hours and minutes
        else if (hours > 0)
        {
            parts.Add($"{hours}h");
            if (minutes > 0)
                parts.Add($"{minutes}m");
        }
        // For very short durations, just show minutes
        else
        {
            parts.Add($"{minutes}m");
        }
        
        return string.Join(" ", parts);
    }
}
