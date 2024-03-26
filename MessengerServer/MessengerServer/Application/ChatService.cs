using MessengerServer.Domain.Abstractions;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Application
{
    public class ChatService
    {
        IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Chat> AddChat(Chat chat)
        {
            chat = await _unitOfWork.Chat_Repository.CreateAsync(chat);
            List<int> usersId = chat.usersId;
            foreach (var userId in usersId)
            {
                User user = await _unitOfWork.User_Repository.GetByIdAsync(userId);
                user.AddChatById(chat.Id);
                await _unitOfWork.User_Repository.UpdateAsync(user);
            }
            await _unitOfWork.SaveAllAsync();
            return chat;
        }

        public async Task DeleteChat(int chatId)
        {
            Chat chat = await _unitOfWork.Chat_Repository.GetByIdAsync(chatId);
            List<int> usersId = chat.usersId;
            foreach (var userId in usersId)
            {
                User user = await _unitOfWork.User_Repository.GetByIdAsync(userId);
                user.RemoveChatById(chatId);
                await _unitOfWork.User_Repository.UpdateAsync(user);
            }
            await _unitOfWork.Chat_Repository.DeleteAsync(chatId);
            await _unitOfWork.SaveAllAsync();
        }

        public async Task<Chat> AddUser(int chatId, int userId)
        {
            Chat chat = await _unitOfWork.Chat_Repository.GetByIdAsync(chatId);
            chat.AddUserById(userId);
            User user = await _unitOfWork.User_Repository.GetByIdAsync(userId);
            user.AddChatById(chatId);
            await _unitOfWork.User_Repository.UpdateAsync(user);
            await _unitOfWork.Chat_Repository.UpdateAsync(chat);
            await _unitOfWork.SaveAllAsync();
            return chat;
        }

        public async Task<Chat> DeleteUser(int chatId, int userId)
        {
            Chat old_chat = await _unitOfWork.Chat_Repository.GetByIdAsync(chatId);
            old_chat.RemoveUserById(userId);
            User old_user = await _unitOfWork.User_Repository.GetByIdAsync(userId);
            old_user.RemoveChatById(chatId);
            await _unitOfWork.User_Repository.UpdateAsync(old_user);
            await _unitOfWork.Chat_Repository.UpdateAsync(old_chat);
            await _unitOfWork.SaveAllAsync();
            return old_chat;
        }

        public async Task<List<Chat>> GetChats(User user)
        {
            List<Chat> chats = new List<Chat>();
            if (user.chatsId.Count == 0)
            {
                return chats;
            }
            foreach (int chatId in user.chatsId)
            {
                chats.Add(await _unitOfWork.Chat_Repository.GetByIdAsync(chatId));
            }
            return chats;
        }

        public async Task<Chat> GetChat(int chatId)
        {
            return await _unitOfWork.Chat_Repository.GetByIdAsync(chatId);
        }
    }
}
