using System.Configuration;
using System.Data;
using System.Windows;

namespace AccountTimer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Global exception handler
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            MessageBox.Show($"Unhandled exception:\n{ex.ExceptionObject}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };
        
        DispatcherUnhandledException += (s, ex) =>
        {
            var exMsg = ex.Exception.InnerException != null
                ? $"{ex.Exception.Message}\n\nInner Exception:\n{ex.Exception.InnerException.Message}"
                : ex.Exception.Message;
            MessageBox.Show($"UI exception:\n{exMsg}\n\nStack:\n{ex.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };
    }
}

