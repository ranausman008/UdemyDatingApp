using API.DTOs;
using API.Entities;
using API.Helpers;
using System.Globalization;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<bool> SaveAllAsync();

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUserNameAsync(string usernamename);

        Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);

        Task<MemberDTO> GetMemberAsync(string username);
    }
}
