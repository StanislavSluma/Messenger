using MessengerClientMaui.ViewModels;

namespace MessengerClientMaui.Pages;

public partial class ChatPage : ContentPage
{
	public ChatPage(ChatViewModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
    }

    private void CollectionView_ScrollToRequested(object sender, ScrollToRequestEventArgs e)
    {

    }
}