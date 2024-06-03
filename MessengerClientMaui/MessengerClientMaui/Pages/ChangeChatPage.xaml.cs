using MessengerClientMaui.ViewModels;

namespace MessengerClientMaui.Pages;

public partial class ChangeChatPage : ContentPage
{
	public ChangeChatPage(ChangeChatViewModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
	}
}