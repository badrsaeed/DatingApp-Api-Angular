using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public AccountController(DataDbContext dbContext, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.UserName)) return BadRequest("User is Taken");

            using var hmac = new HMACSHA512();
            var user = new AppUser()
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PassworSalt = hmac.Key
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);
            if (user is null) return Unauthorized("User not Found");

            var hmac = new HMACSHA512(user.PassworSalt);
            var computedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedPassword.Length; i++)
            {
                if (computedPassword[i] != user.PasswordHash[i]) return Unauthorized("Password Not Valid");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExist(string userName)
            => await _dbContext.Users.AnyAsync(u => u.UserName == userName.ToLower());

    }
}