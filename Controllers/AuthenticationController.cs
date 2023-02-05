using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RSS_Sub_App.Configurations;
using RSS_Sub_App.DTOs;
using RSS_Sub_App.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace RSS_Sub_App.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;        
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;            
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDTO requestDto)
        {            
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(requestDto.Email);

                if(user != null)
                {
                    return BadRequest(error: new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Email already exist."
                        }
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Username
                };

                var newUserResponse = await _userManager.CreateAsync(newUser, requestDto.Password);

                if (newUserResponse.Succeeded)
                {
                    var token = GenerateJwtToken(newUser);

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = token

                    });
                }

                return BadRequest(error: new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Server error"
                    },
                    Result = false
                });

            }

            return BadRequest();
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDTO loginRequest)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);

                if (user == null)
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid payload."
                        },
                        Result = false

                    });
                var isCorrect = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

                if (!isCorrect)
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid creadentials"
                        },
                        Result = false
                    });


                var jwtToken = GenerateJwtToken(user);

                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                    Result = true
                });

            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid payload."
                },
                Result = false

            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandeler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(type:"Id", value:user.Id),
                    new Claim(type:JwtRegisteredClaimNames.Sub, value:user.Email),
                    new Claim(type:JwtRegisteredClaimNames.Email, value:user.Email),
                    new Claim(type:JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(type:JwtRegisteredClaimNames.Iat, value:DateTime.Now.ToUniversalTime().ToString())
                }),
                
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), algorithm:SecurityAlgorithms.HmacSha256)
            };


            var token = jwtTokenHandeler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandeler.WriteToken(token);


            return jwtToken;
        }
    }
}
