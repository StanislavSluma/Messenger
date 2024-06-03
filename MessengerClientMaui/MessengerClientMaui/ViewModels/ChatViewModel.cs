using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessengerClientMaui.Popups;
using MessengerClientMaui.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessengerClientMaui.Pages;

namespace MessengerClientMaui.ViewModels
{
    [QueryProperty("Selected_chat", "Selected_chat")]

    public partial class ChatViewModel : ObservableObject
    {
        Client client;
        [ObservableProperty]
        bool is_admin = false;

        [ObservableProperty]
        Chat selected_chat = null!;
        [ObservableProperty]
        Message edited_message = null!;
        [ObservableProperty]
        string text = "";


        public ObservableCollection<Message> Messages { get; set; } = new();


        List<Regex> banned_words = new() {
            new Regex(@"[Дд]\s*[иИеЕ]\s*[бБ]\s*[иИ]\s*[лЛ][a-zа-яA-ZА-Я]*"),
            new Regex(@"[дД]\s*[оО]\s*[лЛ]\s*[бБ]\s*[аАоО]\s*[еЕёЁ]\s*[бБ][a-zа-яA-ZА-Я]*"),
            new Regex(@"[нНзЗпП]?\s*[оОаА]?\s*[хХ]\s*[уУ]\s*[йЙ][a-zа-яA-ZА-Я]*"),
            new Regex(@"[бБ]\s*[лЛ]\s*[Яя]\s*[тТдД]\s*[ьЬ]?[a-zа-яA-ZА-Я]*"),
            new Regex(@"[a-zа-яA-ZА-Я]*[пП]\s*[иИ]\s*[зЗ]\s*[дД][a-zа-яA-ZА-Я]*"),
            new Regex(@"[a-zа-яA-ZА-Я]*[еЕ]\s*[бБ]\s*[аА]\s*[тТ]\s*[ьЬ][a-zа-яA-ZА-Я]*"),
        };

        public ChatViewModel()
        {
            client = App.Current?.Handler?.MauiContext?.Services.GetService<Client>() ?? new Client();
            //AddHandlers();
            //curUserId = client.ID;
        }

        [RelayCommand]
        public async Task UpdateMessages()
        {
            AddHandlers();
            Is_admin = (Selected_chat.user_id == client.ID);
            if (Messages.Count != 0)
                return;
            string? response = await client.Request("GetChatMessages?", Selected_chat.Id);
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
            for (int i = 0; i < banned_words.Count; i++)
            {
                Text = banned_words[i].Replace(Text, m =>
                {
                    string newstr = m.Groups[0].Value;
                    string first_letter = newstr[0] + "*";
                    return first_letter + new string('*', newstr.Length - 2);
                });
            }
            if (Edited_message == null)
            {
                Message? message = new Message() { Text = Text, chatId = Selected_chat.Id, date = DateTime.Now, userId = client.ID, nickname = client.nickname };
                string? response = await client.Request("AddMessage?", message, Selected_chat.usersId);
                message = JsonSerializer.Deserialize<Message>(response);
                Messages.Add(message);
            }
            else
            {
                Edited_message.Text = Text;
                string? response = await client.Request("EditMessage?", Edited_message, Selected_chat.usersId);
                Edited_message = null!;
            }
            Text = "";
        }


        [RelayCommand]
        public async Task MessagePopup(Message selected_message)
        {
            var res = await MainThread.InvokeOnMainThreadAsync(async Task<string?> () =>
            {
                if (selected_message.userId == client.ID)
                    return await Shell.Current.CurrentPage.ShowPopupAsync(new MessagePopup()) as string;
                else
                    return await Shell.Current.CurrentPage.ShowPopupAsync(new MessageReactions()) as string;
            });


            if (res == null)
                return;
            if (res == "Edit")
            {
                Edited_message = selected_message;
                Text = selected_message.Text;
                return;
            }
            if (res == "Delete")
            {
                await DeleteMessage(selected_message);
                return;
            }
            await SetReaction(selected_message, res);
        }

        public bool LeftChangeMessage()
        {
            if (Edited_message != null)
            {
                Edited_message = null!;
                Text = "";
                return true;
            }
            return false;
        }

        public async Task DeleteMessage(Message selected_message)
        {
            string? response = await client.Request("DeleteMessage?", selected_message.Id, Selected_chat);
            Messages.Remove(selected_message);
        }

        public async Task SetReaction(Message selected_message, string react)
        {
            string? response = await client.Request("SetReaction?", selected_message.Id, react);
            if (response == null) return;

            Message? mess = JsonSerializer.Deserialize<Message>(response);
            if (mess == null) return;

            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Id == mess.Id)
                {
                    Messages[i].userReactions = mess.userReactions;
                }
            }
        }

        [RelayCommand]
        public async Task CurrentReaction(Reaction selected_react)
        {
            string? response;
            if (selected_react.users_id.Contains(client.ID))
            {
                response = await client.Request("UnsetReaction?", selected_react.message_id);
            }
            else
            {
                response = await client.Request("SetReaction?", selected_react.message_id, selected_react.Name);
            }
            if (response == null) return;

            Message? mess = JsonSerializer.Deserialize<Message>(response);
            if (mess == null) return;

            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Id == mess.Id)
                {
                    Messages[i].userReactions = mess.userReactions;
                }
            }
        }

        [RelayCommand]
        public async Task GoToChangeChatPage()
        {
            IDictionary<string, object> options = new Dictionary<string, object>() { { "Current_chat", selected_chat } };
            await Shell.Current.GoToAsync(nameof(ChangeChatPage), options);
        }


        [RelayCommand]
        public async Task Leave()
        {
            string? response = await client.Request("LeaveChat?", Selected_chat.Id);
            await Shell.Current.GoToAsync("..");
        }

        public void AddHandlers()
        {
            UnloadedEventHandler();
            client.MessageReceiveHandler += this.ReceiveMessage;
            client.MessageDeleteHandler += this.UserDeleteMessage;
            client.MessageEditHandler += this.UserEditMessage;
            client.SetReactionHandler += this.UserSetReaction;
            client.UnsetReactionHandler += this.UserSetReaction;
            client.UserAddedToChatHandler += this.UserAddedToChat;
            client.UserDeletedFromChatHandler += this.UserDeletedFromChat;
            client.UpdateChatHandler += this.UpdateChat;
            client.DeleteChatHandler += this.DeleteChat;
            client.UserLeaveChatHandler += this.UserLeaveChat;
        }

        [RelayCommand]
        public void UnloadedEventHandler()
        {
            client.MessageReceiveHandler -= this.ReceiveMessage;
            client.MessageDeleteHandler -= this.UserDeleteMessage;
            client.MessageEditHandler -= this.UserEditMessage;
            client.SetReactionHandler -= this.UserSetReaction;
            client.UnsetReactionHandler -= this.UserSetReaction;
            client.UserAddedToChatHandler -= this.UserAddedToChat;
            client.UserDeletedFromChatHandler -= this.UserDeletedFromChat;
            client.UpdateChatHandler -= this.UpdateChat;
            client.DeleteChatHandler -= this.DeleteChat;
            client.UserLeaveChatHandler -= this.UserLeaveChat;
        }

        public void ReceiveMessage(Message? mess)
        {
            if (mess == null)
                return;
            Messages.Add(mess);
        }

        public void UserEditMessage(Message? mess)
        {
            if (mess == null)
                return;
            if (mess.chatId == Selected_chat.Id)
            {
                for (int i = 0; i < Messages.Count; i++)
                {
                    if (Messages[i].Id == mess.Id)
                    {
                        Messages[i].Text = mess.Text;
                    }
                }
            }
        }

        public void UserDeleteMessage(int chat_id, int mess_id)
        {
            if(Selected_chat.Id == chat_id)
            {
                Message? mess = Messages.FirstOrDefault(x => x?.Id == mess_id, null);
                if (mess != null)
                    Messages.Remove(mess);
            }
        }

        public void UserSetReaction(Message? mess)
        {
            if (mess == null)
                return;
            if (mess.chatId == Selected_chat.Id)
            {
                for (int i = 0; i < Messages.Count; i++)
                {
                    if (Messages[i].Id == mess.Id)
                    {
                        Messages[i].userReactions = mess.userReactions;
                    }
                }
            }
        }

        public void UpdateChat(Chat chat)
        {
            if (Selected_chat.Id == chat.Id)
            {
                Selected_chat.Description = chat.Description;
                Selected_chat.Name = chat.Name;
            }
        }

        public void DeleteChat(int chat_id)
        {
            if (Selected_chat.Id == chat_id)
            {
                Shell.Current.GoToAsync("..");
            }
        }

        public void UserAddedToChat(Chat chat, string added_user)
        {
            if (Selected_chat.Id == chat.Id)
            {
                Messages.Add(new Message() { Id = -1, Text = $"{added_user} присоединился к чату!", date = DateTime.Now, userId=-1 });
                Selected_chat.usersId = chat.usersId;
            }
        }

        public void UserDeletedFromChat(Chat chat, string deleted_user, int deleted_user_id)
        {
            if (Selected_chat.Id == chat.Id)
            {
                if (client.ID == deleted_user_id)
                {
                    Shell.Current.GoToAsync("..");
                }
                Messages.Add(new Message() { Id = -1, Text = $"{deleted_user} был удален из чата!", date = DateTime.Now, userId = -1 });
                Selected_chat.usersId = chat.usersId;
            }
        }

        public void UserLeaveChat(int user_id, string user_name, int chat_id)
        {
            if (Selected_chat.Id == chat_id)
            {
                Messages.Add(new Message() { Id = -1, Text = $"{user_name} вышел из чата!", date = DateTime.Now, userId = -1 });
                Selected_chat.usersId.Remove(user_id);
            }
        }
    }
}
