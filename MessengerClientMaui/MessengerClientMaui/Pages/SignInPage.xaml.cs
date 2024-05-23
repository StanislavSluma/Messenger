using MessengerServer.Domain.Entities;
using System.Text.Json;


namespace MessengerClientMaui.Pages;

public partial class SignInPage : ContentPage
{
    Client client;

    public SignInPage()//Client cl)
    {
        InitializeComponent();

        //client = cl;
        //cl.Start();
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
            Navigation.PushModalAsync(new UserChatsPage(new ViewModels.UserChatsViewModel()));
        }
    }

    private void SignUpClicked(object sender, EventArgs e)
    {
        User user = new User();
        user.Name = NameEntry.Text;
        user.Login = LoginEntry.Text;
        user.PasswordHash = PasswordEntry.Text;
        string? response = Task.Run(async () => await client.Request("SignUp", user)).Result;
        if (response == null || response == "Error")
        {
            DisplayAlert("Информация", "Ошибка регистрация", "ОК");
        }
        else
        {
            //DisplayAlert("Информация", "Успех", "ОК");
            client.ID = JsonSerializer.Deserialize<User>(response).Id;
            Navigation.PushModalAsync(new UserChatsPage(new ViewModels.UserChatsViewModel()));
        }
    }
}