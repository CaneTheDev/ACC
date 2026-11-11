using AccountTimer.ViewModels;
using ModernWpf.Controls;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AccountTimer;

public partial class MainWindow
{
    public MainViewModel ViewModel { get; }
    private bool _isSidebarCollapsed = false;

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _isSidebarCollapsed = !_isSidebarCollapsed;

        var duration = new Duration(TimeSpan.FromMilliseconds(250));

        if (_isSidebarCollapsed)
        {
            // Collapse sidebar
            var widthAnimation = new GridLengthAnimation
            {
                From = new GridLength(280),
                To = new GridLength(0),
                Duration = duration
            };
            SidebarColumn.BeginAnimation(System.Windows.Controls.ColumnDefinition.WidthProperty, widthAnimation);
            
            var spacerAnimation = new GridLengthAnimation
            {
                From = new GridLength(24),
                To = new GridLength(0),
                Duration = duration
            };
            SidebarSpacerColumn.BeginAnimation(System.Windows.Controls.ColumnDefinition.WidthProperty, spacerAnimation);
            
            SidebarPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Expand sidebar
            SidebarPanel.Visibility = Visibility.Visible;
            
            var widthAnimation = new GridLengthAnimation
            {
                From = new GridLength(0),
                To = new GridLength(280),
                Duration = duration
            };
            SidebarColumn.BeginAnimation(System.Windows.Controls.ColumnDefinition.WidthProperty, widthAnimation);
            
            var spacerAnimation = new GridLengthAnimation
            {
                From = new GridLength(0),
                To = new GridLength(24),
                Duration = duration
            };
            SidebarSpacerColumn.BeginAnimation(System.Windows.Controls.ColumnDefinition.WidthProperty, spacerAnimation);
        }
    }

    private async void AddListButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Add Account List",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var textBox = new System.Windows.Controls.TextBox
        {
            MinWidth = 300
        };

        dialog.Content = textBox;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await ViewModel.AddAccountListCommand.ExecuteAsync(textBox.Text);
        }
    }

    private async void DeleteListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is AccountListViewModel listVm)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Account List",
                Content = $"Are you sure you want to delete '{listVm.Name}' and all its accounts?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteAccountListCommand.ExecuteAsync(listVm);
            }
        }
    }

    private async void AddAccountsButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedAccountList == null)
            return;

        var dialog = new ContentDialog
        {
            Title = "Add Accounts",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var textBox = new System.Windows.Controls.TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 200,
            MinWidth = 400
        };

        dialog.Content = textBox;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await ViewModel.AddAccountsCommand.ExecuteAsync((ViewModel.SelectedAccountList.Id, textBox.Text));
        }
    }

    private async void GlobalDurationCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Prevent firing during initialization
        if (!IsLoaded || ViewModel.SelectedAccountList == null || GlobalDurationCombo.SelectedItem == null)
            return;

        var item = (System.Windows.Controls.ComboBoxItem)GlobalDurationCombo.SelectedItem;
        var tagValue = item.Tag.ToString();

        if (tagValue == "custom")
        {
            // Show custom duration dialog
            var dialog = new ContentDialog
            {
                Title = "Custom Reset Duration",
                PrimaryButtonText = "Set",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var stackPanel = new System.Windows.Controls.StackPanel();

            var label = new TextBlock
            {
                Text = "Enter number of days:",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var numberBox = new ModernWpf.Controls.NumberBox
            {
                Minimum = 1,
                Maximum = 365,
                Value = 7,
                SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Inline,
                MinWidth = 200
            };

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(numberBox);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int days = (int)numberBox.Value;
                await ViewModel.SetGlobalResetDurationCommand.ExecuteAsync((ViewModel.SelectedAccountList.Id, days));
                
                // Update the combo box text to show the custom value
                item.Content = $"Custom ({days} days)";
            }
            else
            {
                // User cancelled, revert to previous selection
                GlobalDurationCombo.SelectedIndex = 0;
            }
        }
        else
        {
            var days = int.Parse(tagValue ?? "7");
            await ViewModel.SetGlobalResetDurationCommand.ExecuteAsync((ViewModel.SelectedAccountList.Id, days));
        }
    }

    private async void AccountDurationCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Prevent firing during initialization
        if (!IsLoaded || sender is not System.Windows.Controls.ComboBox combo || combo.SelectedItem == null)
            return;

        var item = (System.Windows.Controls.ComboBoxItem)combo.SelectedItem;
        var tagValue = item.Tag.ToString();

        if (tagValue == "custom")
        {
            // Show custom duration dialog
            var dialog = new ContentDialog
            {
                Title = "Custom Duration",
                PrimaryButtonText = "Set",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var stackPanel = new System.Windows.Controls.StackPanel();

            var label = new TextBlock
            {
                Text = "Enter number of days:",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var numberBox = new ModernWpf.Controls.NumberBox
            {
                Minimum = 1,
                Maximum = 365,
                Value = 7,
                SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Inline,
                MinWidth = 200
            };

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(numberBox);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int days = (int)numberBox.Value;
                // Update the combo box text to show the custom value
                item.Content = $"Custom ({days} days)";
                // Here you would update the account's specific duration
                // This would require accessing the account view model from combo.Tag
            }
            else
            {
                // User cancelled, revert to previous selection
                combo.SelectedIndex = 0;
            }
        }
    }
}

// Helper class for GridLength animation
public class GridLengthAnimation : AnimationTimeline
{
    public GridLength From { get; set; }
    public GridLength To { get; set; }

    public override Type TargetPropertyType => typeof(GridLength);

    protected override System.Windows.Freezable CreateInstanceCore()
    {
        return new GridLengthAnimation();
    }

    public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
    {
        double fromVal = From.Value;
        double toVal = To.Value;

        if (animationClock.CurrentProgress.HasValue)
        {
            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, From.GridUnitType);
            }
            else
            {
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, To.GridUnitType);
            }
        }

        return To;
    }
}