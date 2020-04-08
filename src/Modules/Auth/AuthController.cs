using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Modules.Auth.Dtos;
using Web.Api.Infrastructure.Guards;
using AutoMapper;
using Web.Api.Infrastructure.Pipes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using System;
using Microsoft.Extensions.Caching.Distributed;
using Web.Api.Infrastructure.Repositories;
using System.Net.Http;

namespace Web.Api.Modules.Auth {

    [Route ("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    [ServiceFilter(typeof(CacheControllerAttribute))]
    public class AuthController : ControllerBase {

        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly Jwt _jwt;

        public AuthController (IAuthService authService, Jwt jwt, IMapper mapper) {
            _authService = authService;
            _jwt = jwt;
            _mapper = mapper;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignupAsync ([FromBody] SignupDto signupDto, [FromServices] IValidator<SignupDto> validator) {

            var validated = await validator.ValidateAsync(signupDto);

            if (!validated.IsValid)
                return BadRequest(validated.Errors.Select(e => new { message = e.ErrorMessage, statusCode = e.ErrorCode }).Distinct());


            var result = await _authService.CreateAsync(signupDto);

            if (!result.Succeeded)
            {
                return Ok(result.Errors.Select(e => new { message = "E-mail already exists", statusCode = 409 }));
            }

            return Ok(new { message = "user created", statusCode = 201 });
        }


        [HttpPost("signin")]
        public async Task<IActionResult> SigninAsync([FromBody] SigninDto signinDto, [FromServices] IValidator<SigninDto> validator)
        {
            var validated = await validator.ValidateAsync(signinDto);

            if (!validated.IsValid)
                return BadRequest(validated.Errors.Select(e => new { message = e.ErrorMessage, statusCode = e.ErrorCode }).Distinct());

            var result = await _authService.AuthenticateAsync(signinDto);

            // busca no cache
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid e-mail or password", StatusCode = 401 });
            }

            // generate token
            var user = await _authService.FindAsync(signinDto);
            user.Last_Login = DateTime.Now;

            var data =_mapper.Map<UserDto>(user);

            return Ok(new {
                message = "user logged!",
                Token = _jwt.GenerateToken(user).AccessToken,
                StatusCode = 200,
                Data = data
            
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result="";


            string url = "https://jsonplaceholder.typicode.com/todos"; // sample url
            using (HttpClient client = new HttpClient())
            {
                result = await client.GetStringAsync(url);
            }


            return Ok(result);
        }
    }
}