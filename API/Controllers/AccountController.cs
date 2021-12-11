using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController:BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController (DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context=context;
        }
    
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {

            if (await UserExist (registerDTO.Username)) return BadRequest ("Username is taken");

            using var hmac=new HMACSHA512();
            var user = new AppUser
            {
                UserName=registerDTO.Username.ToLower(),
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt=hmac.Key
            };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return new UserDto
        {
            Username=user.UserName,
            Token=_tokenService.createtoken(user)
        };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDTO loginDTO)
        {
            var user=await _context.Users.SingleOrDefaultAsync(x=> x.UserName ==loginDTO.username);

            if (user ==null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var ComputedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.password));

            for (int i=0; i<ComputedHash.Length; i++)
            {
                if (ComputedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto
            {
                Username=user.UserName,
                Token=_tokenService.createtoken(user)
            };
        }

        private async Task <bool> UserExist(string username)
        {
            return await _context.Users.AnyAsync(x=> x.UserName == username.ToLower());
        }
    }
}