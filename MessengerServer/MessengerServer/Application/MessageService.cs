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

        public async Task<Message> EditMessage(int messId, string messText)
        {
            Message mess = await _unitOfWork.Message_Repository.GetByIdAsync(messId);
            mess.Text = messText;
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
