using CommunityToolkit.Maui.Views;
using MessengerClientMaui.Popups;
using MessengerClientMaui.ViewModels;
using System.Diagnostics;

namespace MessengerClientMaui.Pages;

public partial class ChatPage : ContentPage
{
	public ChatPage(ChatViewModel view_model)
	{
		InitializeComponent();
		BindingContext = view_model;
    }

    protected override bool OnBackButtonPressed()
    {
        if (((ChatViewModel)BindingContext).LeftChangeMessage())
        {
            return true;
        }
        return base.OnBackButtonPressed();
    }
}