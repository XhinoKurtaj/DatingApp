using API.DTOs;
using API.Entities;
using API.Helpers;
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


        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge - 1);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive),
            };

            return await PagedList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider), 
                userParams.PageNumber, 
                userParams.PageSize);
        }

        public async Task<User> GetUserByEmailAsync(string email)
            => await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Email == email);

    }
}
