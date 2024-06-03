using MessengerClientMaui.Domain.Entities;
using Serializator__Deserializator;
using System.Text.Json;

namespace MessengerClientMaui.Pages;

public partial class SignUpPage : ContentPage
{
    Client client;

	public SignUpPage()
	{
		InitializeComponent();
        client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
    }

    private void SignUpClicked(object sender, EventArgs e)
    {
        if (NameEntry.Text == "" || LoginEntry.Text == "" || PasswordEntry.Text == "" || PasswordAcceptEntry.Text == "")
        {
            DisplayAlert("Информация", "Все поля должны быть заполнены", "ОК");
            return;
        }
        if (PasswordAcceptEntry.Text != PasswordEntry.Text)
        {
            DisplayAlert("Информация", "Пароли должны совпадать!", "ОК");
            return;
        }
        User user = new User();
        user.Name = NameEntry.Text;
        user.Login = LoginEntry.Text;
        user.PasswordHash = PasswordEntry.Text;
        string? response = Task.Run(async () => await client.Request("SignUp", user)).Result;
        List<string> parse_response = RequestSerializer.Deserializer(response);
        if (parse_response[0] == "Error?")
        {
            DisplayAlert("Информация", "Логин должен быть уникален!", "ОК");
        }
        else
        {
            client.acces_token = parse_response[1];
            client.ID = JsonSerializer.Deserialize<User>(parse_response[2]).Id;
            client.nickname = JsonSerializer.Deserialize<User>(parse_response[2]).Name;
            MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync(nameof(UserChatsPage)));
        }
    }
}