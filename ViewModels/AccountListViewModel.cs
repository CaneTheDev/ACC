using CommunityToolkit.Mvvm.ComponentModel;
using AccountTimer.Models;
using System.Collections.ObjectModel;

namespace AccountTimer.ViewModels;

public partial class AccountListViewModel : ObservableObject
{
    private readonly AccountList _accountList;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private int? globalResetDurationDays;

    [ObservableProperty]
    private ObservableCollection<AccountViewModel> accounts = new();

    public int Id => _accountList.Id;

    public AccountListViewModel(AccountList accountList, MainViewModel mainViewModel)
    {
        _accountList = accountList;
        _mainViewModel = mainViewModel;
        
        name = accountList.Name;
        globalResetDurationDays = accountList.GlobalResetDurationDays;
        
        LoadAccounts();
    }

    private void LoadAccounts()
    {
        Accounts.Clear();
        
        // Sort: Available first, then in-use, suspended last
        var sortedAccounts = _accountList.Accounts
            .OrderBy(a => a.IsSuspended ? 2 : (a.IsAvailable ? 0 : 1))
            .ThenBy(a => a.SortOrder);
        
        foreach (var account in sortedAccounts)
        {
            Accounts.Add(new AccountViewModel(account, _mainViewModel, Id));
        }
    }
}
