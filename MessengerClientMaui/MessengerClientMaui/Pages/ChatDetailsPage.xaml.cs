using MessengerClientMaui.ViewModels;

namespace MessengerClientMaui.Pages;

public partial class ChatDetailsPage : ContentPage
{
	public ChatDetailsPage(ChatDetailsViewModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
	}
}