using CommunityToolkit.Maui.Views;
using MessengerServer.Domain.Entities;
using System.Diagnostics;


namespace MessengerClientMaui.Popups;

public partial class MessagePopup : Popup
{
    List<Reaction> reactions { get; set; } = new List<Reaction>() {
        new Reaction() { Name="happy", ImagePath="happy.png" },
        new Reaction() { Name="laugh_tiers", ImagePath="laugh_tiers.png" },
        new Reaction() { Name="handshake", ImagePath="handshake.png" },
        new Reaction() { Name="like", ImagePath="like.png" },
        new Reaction() { Name="heart", ImagePath="heart.png" },
        new Reaction() { Name="happy_hearts", ImagePath="happy_hearts.png" },
        new Reaction() { Name="displasure", ImagePath="displasure.png" },
        new Reaction() { Name="disgust", ImagePath="disgust.png" },
        new Reaction() { Name="demon", ImagePath="demon.png" },
        new Reaction() { Name="clown", ImagePath="clown.png" }
    };

    public MessagePopup()
	{
		InitializeComponent();
        reactionsList.ItemsSource = reactions;
        BindingContext = this;
	}

    private void ChangeMessage(object sender, EventArgs e)
    {
        Close("Edit");
    }

    private void DeleteMessage(object sender, EventArgs e)
    {
        Close("Delete");
    }

    private void ReactionSelected(object sender, TappedEventArgs e)
    {

        Reaction? reaction = ((Frame)sender).BindingContext as Reaction;
        if (reaction != null)
        {
            Close(reaction.Name);
        }
    }
}