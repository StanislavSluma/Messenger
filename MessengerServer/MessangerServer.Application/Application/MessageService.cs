using MessengerServer.Domain.Abstractions;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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
            bool added = false;
            Message mess = await UnsetReaction(messId, userId);
            foreach (var reaction in mess.userReactions)
            {
                if(reaction.Name == react)
                {
                    reaction.users_id.Add(userId);
                    added = true;
                    break;
                }
            }
            if(!added)
            {
                mess.userReactions.Add(new Reaction()
                {
                    Id = Math.Abs(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0)),
                    Name = react,
                    message_id = messId,
                    users_id = new List<int> { userId },
                    ImagePath = react + ".png"
                });
            }
            await _unitOfWork.Message_Repository.UpdateAsync(mess);
            await _unitOfWork.SaveAllAsync();
            return mess;
        }

        public async Task<Message> UnsetReaction(int messId, int userId)
        {
            Message mess = await _unitOfWork.Message_Repository.GetByIdAsync(messId);
            int deleted_index = 0;
            bool deleted = false;
            foreach (var reaction in mess.userReactions)
            {
                for (int i = 0; i < reaction.users_id.Count; i++)
                {
                    if (reaction.users_id[i] == userId)
                    {
                        reaction.users_id.RemoveAt(i);
                        if (reaction.users_id.Count == 0)
                            deleted = true;
                        break;
                    }
                }
                if (deleted) break;
                ++deleted_index;
            }
            if (mess.userReactions.Count != deleted_index)
            {
                mess.userReactions.RemoveAt(deleted_index);
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
