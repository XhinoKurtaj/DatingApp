using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<User> GetUserByIdAsync(int id)
            => await _context.Users.FindAsync(id);

        public async Task<User> GetUserByUsernameAsync(string username)
           => await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == username);

        public async Task<IEnumerable<User>> GetUsersAsync()
            => await _context.Users.Include(p => p.Photos).ToListAsync();

        public async Task<bool> SaveAllAsync()
            => await _context.SaveChangesAsync() > 0;

        public void Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }


        public async Task<MemberDto> GetMemberAsync(string username)
            => await _context.Users
                    .Where(x => x.UserName == username)
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();


        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
            => await _context.Users
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

    }
}
