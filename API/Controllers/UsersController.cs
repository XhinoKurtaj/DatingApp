using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;   
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<MemberDto>>> GetUser()
        //{
        //    var users = await _userRepository.GetUsersAsync();
        //    var mapUsers = _mapper.Map<IEnumerable<MemberDto>>(users);
        //    return Ok(mapUsers);
        //}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUser()
            => Ok(await _userRepository.GetMembersAsync());

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) 
            => await _userRepository.GetMemberAsync(username);
  
    


    }
}
