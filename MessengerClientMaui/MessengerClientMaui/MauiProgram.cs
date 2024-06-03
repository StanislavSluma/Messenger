using CommunityToolkit.Maui;
using MessengerClientMaui.Pages;
using MessengerClientMaui.ViewModels;
using Microsoft.Extensions.Logging;

namespace MessengerClientMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            Client client = new Client();
            client.Start();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services
                .AddSingleton(client)
                .AddTransient<UserChatsPage>()
                .AddTransient<ChatPage>()
                .AddTransient<ChangeChatPage>()
                .AddTransient<UserChatsViewModel>()
                .AddTransient<ChatViewModel>()
                .AddTransient<ChangeChatViewModel>()
                ;
#endif
            return builder.Build();
        }
    }
}
