using MessengerClientMaui.Domain.Entities;
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
        User user = new User();
        //user.Name = NameEntry.Text;
        user.Login = LoginEntry.Text;
        user.PasswordHash = PasswordEntry.Text;
        string? response = Task.Run(async () => await client.Request("SignIn", user)).Result;
        if (response == null || response == "Error")
        {
            DisplayAlert("Информация", "Ошибка входа", "ОК");
        }
        else
        {
            //DisplayAlert("Информация", "Успех", "ОК");
            client.ID = JsonSerializer.Deserialize<User>(response).Id;
            client.nickname = JsonSerializer.Deserialize<User>(response).Name;
            MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(nameof(UserChatsPage)));
            
        }
    }

    private void SignUpClicked(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(nameof(SignUpPage)));
    }
}