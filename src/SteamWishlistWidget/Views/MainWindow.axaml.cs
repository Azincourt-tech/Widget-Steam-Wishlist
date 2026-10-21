using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SteamWishlistWidget.Models;
using SteamWishlistWidget.Platform.Windows;
using SteamWishlistWidget.ViewModels;

namespace SteamWishlistWidget.Views;

/// <summary>
/// Só comportamento visual: arrastar, efeito vidro, sombra por tema e reposicionar ao minimizar.
/// Estado e ações ficam no <see cref="WidgetViewModel"/>.
/// </summary>
public partial class MainWindow : Window
{
    private const double PanelRadius = 20;
    private const double ShadowMargin = 14;
    private const double DragThreshold = 4;

    private static readonly BoxShadows PanelShadow = BoxShadows.Parse("0 6 28 0 #47000000");

    private readonly WidgetViewModel _viewModel = null!;

    // Clique num jogo: só abre a loja se o mouse não se mover (senão vira arrasto da janela).
    private PointerPressedEventArgs? _pendingRowPress;
    private GameDeal? _pressedDeal;
    private Point _pressPoint;

    // Borda direita (em pixels) a manter quando o tamanho mudar ao minimizar/abrir.
    private int? _keepRightEdge;

    /// <summary>Usado pelo designer visual do Avalonia.</summary>
    public MainWindow() => InitializeComponent();

    public MainWindow(WidgetViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;

        PlaceOnScreen(viewModel.SavedLeft, viewModel.SavedTop);
        ShowCollapsed(viewModel.IsCollapsed);
        PanelHost.Margin = new Thickness(PanelMarginFor(viewModel.Theme));

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        viewModel.CloseRequested += (_, _) => Close();

        // Arrastar por qualquer parte do painel, inclusive perto da borda.
        PanelView.AddHandler(PointerPressedEvent, Panel_PointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        PanelView.AddHandler(PointerMovedEvent, Panel_PointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        PanelView.AddHandler(PointerReleasedEvent, Panel_PointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        IconView.PointerPressed += Icon_PointerPressed;

        Opened += async (_, _) =>
        {
            ApplyThemeVisuals(_viewModel.Theme);
            await _viewModel.InitializeAsync();
        };

        Closing += (_, _) =>
        {
            _viewModel.Shutdown();
            _viewModel.SaveWindowPosition(Position.X, Position.Y);
        };
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(WidgetViewModel.IsCollapsed):
                SetCollapsedKeepingEdge(_viewModel.IsCollapsed);
                break;
            case nameof(WidgetViewModel.Theme):
                ApplyThemeVisuals(_viewModel.Theme);
                break;
            case nameof(WidgetViewModel.Deals):
                DealsScroll.ScrollToHome();
                break;
            case nameof(WidgetViewModel.IsSetupVisible) when _viewModel.IsSetupVisible:
                ProfileInput.Focus();
                break;
        }
    }

    // ---- Posição ----

    private void PlaceOnScreen(double? left, double? top)
    {
        if (left is double x && top is double y && IsOnScreen((int)x, (int)y))
        {
            Position = new PixelPoint((int)x, (int)y);
            return;
        }

        // Primeira execução (ou monitor desconectado): canto superior direito da tela principal.
        if (Screens.Primary is { } screen)
        {
            var area = screen.WorkingArea;
            var width = (int)((PanelView.Width + 2 * ShadowMargin + 24) * screen.Scaling);
            Position = new PixelPoint(area.Right - width, area.Y + (int)(24 * screen.Scaling));
        }
    }

    private bool IsOnScreen(int x, int y) =>
        Screens.All.Any(s => s.WorkingArea.Contains(new PixelPoint(x + 50, y + 50)));

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Depois de minimizar/abrir, a janela muda de tamanho (às vezes em mais de um passo, quando a
        // moldura do Vidro liga/desliga): mantém a borda direita no mesmo lugar a cada mudança.
        if (change.Property == ClientSizeProperty && _keepRightEdge is int right)
            Position = new PixelPoint(right - (int)(ClientSize.Width * DesktopScaling), Position.Y);
    }

    // ---- Aparência ----

    private static bool IsGlass(string theme) => ThemeNames.Normalize(theme) == ThemeNames.Glass;

    private static double PanelMarginFor(string theme) => IsGlass(theme) ? 0 : ShadowMargin;

    private void ShowCollapsed(bool collapsed)
    {
        PanelHost.IsVisible = !collapsed;
        IconView.IsVisible = collapsed;
    }

    private void SetCollapsedKeepingEdge(bool collapsed)
    {
        // Se o widget está do lado direito da tela, a borda direita fica parada.
        var width = (int)(ClientSize.Width * DesktopScaling);
        if (Screens.ScreenFromWindow(this) is { } screen && Position.X + width / 2 > screen.WorkingArea.Center.X)
        {
            _keepRightEdge = Position.X + width;
            DispatcherTimer.RunOnce(() => _keepRightEdge = null, TimeSpan.FromMilliseconds(400));
        }

        ShowCollapsed(collapsed);
        ApplyBackdrop();
    }

    private void ApplyThemeVisuals(string theme)
    {
        // A margem da sombra muda entre os temas; desloca a janela para o painel não "pular".
        var oldMargin = PanelHost.Margin.Left;
        var newMargin = PanelMarginFor(theme);
        if (oldMargin != newMargin)
        {
            PanelHost.Margin = new Thickness(newMargin);
            if (!_viewModel.IsCollapsed)
            {
                var delta = (int)((oldMargin - newMargin) * DesktopScaling);
                Position = new PixelPoint(Position.X + delta, Position.Y + delta);
            }
        }

        var glass = IsGlass(theme);
        PanelView.BoxShadow = glass ? default : PanelShadow;

        // No Windows, o desfoque só respeita o arredondamento nativo (8 px); os cantos do painel acompanham.
        var radius = glass && OperatingSystem.IsWindows() ? WindowsCorners.SystemCornerRadius : PanelRadius;
        PanelView.CornerRadius = new CornerRadius(radius);

        ApplyBackdrop();
    }

    private void ApplyBackdrop()
    {
        // Desfoque só no tema Vidro e com o painel aberto (o ícone redondo não usa).
        var glass = IsGlass(_viewModel.Theme) && !_viewModel.IsCollapsed;
        TransparencyLevelHint = glass
            ? [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur, WindowTransparencyLevel.Transparent]
            : [WindowTransparencyLevel.Transparent];

        // No Vidro, a janela mantém a moldura do sistema (sem barra de título): é ela que arredonda
        // os cantos e corta o desfoque. Nos outros temas, a janela não tem moldura e o painel desenha tudo.
        WindowDecorations = glass ? WindowDecorations.BorderOnly : WindowDecorations.None;

        if (OperatingSystem.IsWindows())
            WindowsCorners.Apply(this, round: glass);
    }

    // ---- Arrastar e clicar ----

    private void Panel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _pendingRowPress = null;
        _pressedDeal = null;

        for (var node = e.Source as Visual; node is not null && node != PanelView; node = node.GetVisualParent())
        {
            // Botões, campo de texto e barra de rolagem continuam funcionando normalmente.
            if (node is Button or TextBox or ScrollBar)
                return;

            // Num jogo, só arrasta se o mouse se mover; um clique simples abre a loja.
            if (node is Control { DataContext: GameDeal deal })
            {
                _pendingRowPress = e;
                _pressedDeal = deal;
                _pressPoint = e.GetPosition(this);
                return;
            }
        }

        BeginMoveDrag(e);
    }

    private void Panel_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pendingRowPress is not { } press || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var delta = e.GetPosition(this) - _pressPoint;
        if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
            return;

        _pendingRowPress = null;
        _pressedDeal = null;
        BeginMoveDrag(press);
    }

    private void Panel_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var deal = _pressedDeal;
        _pendingRowPress = null;
        _pressedDeal = null;
        if (deal is not null)
            _viewModel.OpenDealCommand.Execute(deal);
    }

    private void Icon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        // Arrastar move o ícone; um clique sem arrastar abre a lista.
        var before = Position;
        BeginMoveDrag(e);
        if (Math.Abs(Position.X - before.X) < 3 && Math.Abs(Position.Y - before.Y) < 3)
            _viewModel.IsCollapsed = false;
    }
}
