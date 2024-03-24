using MessengerServer.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Application
{
    public class ApplicationService
    {
        public UserService userService;
        public ChatService chatService;
        public MessageService messageService;

        public ApplicationService(IUnitOfWork unitOfWork)
        {
            userService = new UserService(unitOfWork);
            chatService = new ChatService(unitOfWork);
            messageService = new MessageService(unitOfWork);
        }
    }
}
