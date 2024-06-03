using MessengerClientMaui.Pages;

namespace MessengerClientMaui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(UserChatsPage), typeof(UserChatsPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(ChangeChatPage), typeof(ChangeChatPage));
        }
    }
}
