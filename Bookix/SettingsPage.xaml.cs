using Bookix.Controls;
namespace Bookix;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}
    private async void OnSideMenuButtonTapped(object sender, EventArgs e)
    {
        await SideMenu.Open();
    }
}