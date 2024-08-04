using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using H4H.Application.Interfaces;

namespace H4H.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid UserId)
        {
            return await _userRepository.GetByIdAsync(UserId);
        }

        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid UserId)
        {
            var user = await _userRepository.GetByIdAsync(UserId);
            if (user != null)
            {
                await _userRepository.DeleteAsync(user);
            }
        }
    }
}
