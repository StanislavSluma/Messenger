using CommunityToolkit.Maui.Views;
using MessengerClientMaui.Domain.Entities;
using MessengerClientMaui.ViewModels;
using System.Text.Json;

namespace MessengerClientMaui.Popups;

public partial class FindUsersPopup : Popup
{
	public FindUsersPopup(FindUsersPopupModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
	}

	private async void Search(object sender, EventArgs e)
	{
		await ((FindUsersPopupModel)BindingContext).SearchUsers(FindUserEntry.Text);
	}

    private void AddUser(object sender, TappedEventArgs e)
    {
        User? user = ((Image)sender).BindingContext as User;
        if (user != null)
        {
            Close(JsonSerializer.Serialize(user));
        }
    }
}