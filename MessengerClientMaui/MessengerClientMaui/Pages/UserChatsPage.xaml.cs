using MessengerClientMaui.ViewModels;

namespace MessengerClientMaui.Pages;

public partial class UserChatsPage : ContentPage
{
	public UserChatsPage(UserChatsViewModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
	}
}