using MessengerClientMaui.ViewModels;

namespace MessengerClientMaui.Pages;

public partial class ChangeUserPage : ContentPage
{
	public ChangeUserPage(ChangeUserViewModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
	}
}