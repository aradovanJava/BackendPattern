using Back.Auth;
using Back.Db;
using Back.Models;
using front.Utils.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Back.Controllers
{
    [Route("login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly MySqlContext _db;
        private static Dictionary<int, int> _loginAttempts = new();
        private static Dictionary<int, DateTime> _blockedUsers = new();

        public LoginController(MySqlContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(x => x.Email == login.Email);

                if (user == null)
                {
                    Logger.Instance.Error($"User tried logging in with invalid username or password");
                    return Unauthorized("Invalid e-mail or password");
                }

                if (_loginAttempts.ContainsKey(user.Id) && _loginAttempts[user.Id] == 3)
                {
                    if (!_blockedUsers.ContainsKey(user.Id))
                    {
                        _blockedUsers.Add(user.Id, DateTime.Now.AddMinutes(1));
                        Logger.Instance.Error($"Acount disabled", user.Id);
                        return Unauthorized("Too many attempts to login, account is temporary disabled");
                    }
                    else if (_blockedUsers[user.Id] > DateTime.Now)
                    {
                        Logger.Instance.Error($"Acount disabled", user.Id);
                        return Unauthorized("Too many attempts to login, account is temporary disabled");
                    }
                    else if (_blockedUsers[user.Id] < DateTime.Now)
                    {
                        _loginAttempts.Remove(user.Id);
                        _blockedUsers.Remove(user.Id);
                    }
                }
                if(_db.PasswordExpires.Find(user.Id).ExpirationDate < DateTime.Now)
                {
                    return Unauthorized("Password expired");
                }
                if (login.Password != user.Password)
                {
                    if (_loginAttempts.ContainsKey(user.Id))
                        _loginAttempts[user.Id]++;
                    else
                        _loginAttempts.Add(user.Id, 1);
                    Logger.Instance.Error($"User tried logging in with invalid username or password");
                    return Unauthorized("Invalid e-mail or password");
                }

                string accessToken = Jwt.GenerateJwtToken(user.Id, user.Role, tokenType: "access");

                string refreshToken = Jwt.GenerateJwtToken(user.Id, user.Role, tokenType: "refresh");

                int userPlanUsage = _db.UserPlanUsages.FirstOrDefault(x => x.UserId == user.Id).Usages;

                DateTime expirationDate = _db.PasswordExpires.FirstOrDefault(x => x.Id == user.Id).ExpirationDate.Value;

                var response = new
                {
                    id = user.Id,
                    email = user.Email,
                    username = user.Username,
                    role = user.Role,
                    accessToken,
                    refreshToken,
                    userPlanUsage,
                    expirationDate
                };

                _db.Users.Where(x => x.Email == login.Email).First().RefreshToken = refreshToken;
                _db.SaveChangesAsync();

                Logger.Instance.Information("Logged in", response.id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPatch("recover")]
        public async Task<IActionResult> Recover(List<string> request)
        {
            var user = _db.Users.Where(x => x.Email == request[0]).FirstOrDefault();
            if (user == null)
            {
                return BadRequest();
            }
            string answer = _db.SecretAnswers.Where(x => x.Id == user.Id).First().Answer;
            if (answer == request[1])
            {
                user.Password = request[2];
                _db.PasswordExpires.Find(user.Id).ExpirationDate = DateTime.Now.AddDays(30);
                _db.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }
    }
}
