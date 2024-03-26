using MessengerServer.Domain.Abstractions;
using MessengerServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Application
{
    public class UserService
    {
        IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int?> GetUserId(string user_name)
        {
            User? user = await _unitOfWork.User_Repository.FirstOrDefaultAsync(x => x.Name == user_name);
            if (user == null)
                return null;
            return user.Id;
        }

        public async Task<User?> SignIn(User user)
        {
            User is_exist_user = await _unitOfWork.User_Repository.FirstOrDefaultAsync(x => x.Login == user.Login);
            if (is_exist_user == null) return null;
            if (is_exist_user.PasswordHash != user.PasswordHash) return null;
            return is_exist_user;
        }

        public async Task<User?> SignUp(User user)
        {
            User exist_user = await _unitOfWork.User_Repository.FirstOrDefaultAsync(x => x.Login == user.Login);
            if (exist_user != null)
                return null;
            User new_user = await _unitOfWork.User_Repository.CreateAsync(user);
            await _unitOfWork.SaveAllAsync();
            return new_user;
        }
    }
}
