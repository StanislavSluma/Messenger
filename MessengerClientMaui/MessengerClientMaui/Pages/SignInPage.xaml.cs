using MessengerClientMaui.Domain.Entities;
using Serializator__Deserializator;
using System.Text.Json;


namespace MessengerClientMaui.Pages;

public partial class SignInPage : ContentPage
{
    Client client;

    public SignInPage()
    {
        InitializeComponent();
        client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
    }

    private void SignInClicked(object sender, EventArgs e)
    {
        /*if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            DisplayAlert("Warning!", "Something went wrong!", "OK");
        }*/
        User user = new User();
        //user.Name = NameEntry.Text;
        user.Login = LoginEntry.Text;
        user.PasswordHash = PasswordEntry.Text;
        string? response = Task.Run(async () => await client.Request("SignIn?", user)).Result;
        List<string> parse_response = RequestSerializer.Deserializer(response);
        if (parse_response[0] == "Error?")
        {
            DisplayAlert("Информация", "Ошибка входа", "ОК");
        }
        else
        {
            client.acces_token = parse_response[1];
            client.ID = JsonSerializer.Deserialize<User>(parse_response[2]).Id;
            client.nickname = JsonSerializer.Deserialize<User>(parse_response[2]).Name;
            MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(nameof(UserChatsPage)));
        }
    }

    private void SignUpClicked(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(nameof(SignUpPage)));
    }
}