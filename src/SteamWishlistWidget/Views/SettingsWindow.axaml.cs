using Avalonia.Controls;
using Avalonia.Input;
using SteamWishlistWidget.ViewModels;

namespace SteamWishlistWidget.Views;

/// <summary>
/// Só comportamento visual: arrastar pelo título e fechar com o resultado pedido pelo ViewModel.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>Usado pelo designer visual do Avalonia.</summary>
    public SettingsWindow() => InitializeComponent();

    public SettingsWindow(SettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, saved) => Close(saved);
        TitleBar.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };
    }
}
