using System.Threading;
namespace Bookix.Controls;

public partial class SideMenuControl : ContentView
{
    private bool _isGenresExpanded = false;
    private bool _animatedGenresToggle = true;
    public SideMenuControl()
    {
        InitializeComponent();
    }

    public async Task Open()
    {
        DarkOverlay.IsVisible = true;
        _ = DarkOverlay.FadeTo(0.4, 250);
        await SideMenuPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
    }

    public async Task Close()
    {
        _ = DarkOverlay.FadeTo(0, 250);
        await SideMenuPanel.TranslateTo(-300, 0, 300, Easing.CubicIn);
        DarkOverlay.IsVisible = false;
    }

    private async void OnCloseMenuTapped(object sender, TappedEventArgs e) => await Close();

    private void OnToggleGenresClicked(object sender, EventArgs e)
    {
        _isGenresExpanded = !_isGenresExpanded;
        GenresList.IsVisible = _isGenresExpanded;
        if(_animatedGenresToggle)
        {
            GenresDropdownButton.RotateTo(_isGenresExpanded ? 180 : 0, 200);
            GenresDropdownButton.TranslateTo(0, _isGenresExpanded ? 2 : -2, 200);
        }
        else
        {
            GenresDropdownButton.Text = _isGenresExpanded ? "▲" : "▼";
        }
    }
    private async void OnOpenSettingsButtonTapped(object sender, EventArgs e)
    {
        await Close();
        if (Application.Current.MainPage is not null)
        {
            //await Navigation.PushAsync(new SettingsPage());
            await Shell.Current.GoToAsync("//SettingsPage"); //Page fix
        }
    }
    private async void OnOpenMainpageButtonClicked(object sender, EventArgs e)
    {
        await Close();
        if (Application.Current.MainPage is not null)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}