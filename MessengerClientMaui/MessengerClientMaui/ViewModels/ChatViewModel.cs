using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerClientMaui.ViewModels
{
    [QueryProperty("Selected_chat", "Selected_chat")]

    public partial class ChatViewModel : ObservableObject
    {
        Client client;
/*        [ObservableProperty]
        int curUserId = 0;*/

        [ObservableProperty]
        Chat? selected_chat;
        [ObservableProperty]
        string text = "";
        

        public ObservableCollection<Message> Messages { get; set; } = new();


        public ChatViewModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
            client.MessageReceiveHandler += this.ReceiveMessage;
            //curUserId = client.ID;
        }

        [RelayCommand]
        public async Task UpdateMessages()
        {
            string? response = await client.Request("GetChatMessages?", selected_chat.Id);
            List<Message>? new_messages = JsonSerializer.Deserialize<List<Message>>(response);
            new_messages.Sort((x, y) =>
            {
                if (x.date > y.date)
                    return 1;
                if (x.date < y.date)
                    return -1;
                return 0;
            });
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Messages.Clear();
                foreach (var message in new_messages)
                    Messages.Add(message);
            });
        }

        [RelayCommand]
        public async Task SendMessage()
        {
            if (Text == "")
                return;
            Message message = new Message() { Text = Text, chatId = selected_chat.Id, date = DateTime.Now, userId = client.ID};
            string? response = await client.Request("AddMessage?", message, selected_chat.usersId);
            Messages.Add(message);
            Text = "";
        }


        public void ReceiveMessage(Message? mess)
        {
            if (mess == null)
                return;
            Messages.Add(mess);
        }
    }
}
