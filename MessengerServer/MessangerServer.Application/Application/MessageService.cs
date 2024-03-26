using MessengerServer.Domain.Abstractions;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Application
{
    public class MessageService
    {
        IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Message> AddMessage(Message mess)
        {
            mess = await _unitOfWork.Message_Repository.CreateAsync(mess);
            await _unitOfWork.SaveAllAsync();
            return mess;
        }

        public async Task RemoveMessage(int messId)
        {
            await _unitOfWork.Message_Repository.DeleteAsync(messId);
            await _unitOfWork.SaveAllAsync();
        }

        public async Task<Message> EditMessage(Message mess)//int messId, string messText)
        {
            //Message mess = await _unitOfWork.Message_Repository.GetByIdAsync(messId);
            //mess.Text = messText;
            await _unitOfWork.Message_Repository.UpdateAsync(mess);
            await _unitOfWork.SaveAllAsync();
            return mess;
        }

        public async Task<Message> SetReaction(int messId, int userId, string react)
        {
            Message mess = await _unitOfWork.Message_Repository.GetByIdAsync(messId);
            foreach (string emot in mess.userReactions.Keys.ToList())
            {
                foreach (var user_id in mess.userReactions[emot])
                {
                    if(user_id == userId)
                    {
                        mess.userReactions[emot].Remove(user_id);
                        if (mess.userReactions[emot].Count == 0)
                            mess.userReactions.Remove(emot);
                        break;
                    }
                }
            }
            if (mess.userReactions.ContainsKey(react))
                mess.userReactions[react].Add(userId);
            else
                mess.userReactions.Add(react, new List<int> { userId });
            await _unitOfWork.Message_Repository.UpdateAsync(mess);
            await _unitOfWork.SaveAllAsync();
            return mess;
        }

        public async Task<Message> UnsetReaction(int messId, int userId)
        {
            Message mess = await _unitOfWork.Message_Repository.GetByIdAsync(messId);
            foreach (string emot in mess.userReactions.Keys.ToList())
            {
                foreach (var user_id in mess.userReactions[emot])
                {
                    if (user_id == userId)
                    {
                        mess.userReactions[emot].Remove(user_id);
                        if(mess.userReactions[emot].Count == 0)
                            mess.userReactions.Remove(emot);
                        break;
                    }
                }
            }
            await _unitOfWork.Message_Repository.UpdateAsync(mess);
            await _unitOfWork.SaveAllAsync();
            return mess;
        }

        public async Task<List<Message>> GetMessages(int chatId)
        {
            List<Message> messages = (await _unitOfWork.Message_Repository.ListAsync(x => x.chatId == chatId)).ToList();
            return messages;
        }
    }
}
