using Back.Attributes;
using Back.Auth;
using Back.Db;
using Back.Models;
using Back.Utils;
using front.Utils.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace Back.Controllers
{
    [Route("/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MySqlContext _db;
        private readonly UserUpdateProxy _proxy;
        private readonly Pagination<User> _pagination;

        public UsersController(MySqlContext db, UserUpdateProxy proxy)
        {
            _db = db;
            _proxy = proxy;
            _pagination = new Pagination<User>(_db);
        }

        [Authorize]
        [Role("Admin")]
        [HttpGet()]
        public async Task<ActionResult> GetUsers(int page = 1)
        {
            Logger.Instance.Information($"Admin fetched page {page} of users");
            return Ok(_pagination.GetPage(page).Select(x => new { x.Id, x.Username, x.Role}).ToList());
        }

        [HttpGet("count")]
        public async Task<ActionResult> GetUserStats(string username)
        {
            using (var context = new MySqlContext())
            {
                string sql = @"
                SELECT COUNT(i.id) 
                FROM users u 
                INNER JOIN images i ON u.id = i.author 
                WHERE u.username = @username";

                //Injection https://localhost:7299/users/count?username=josip%27%20OR%20%271%27=%271%27;%20INSERT%20INTO%20users%20(username,%20email,%20password,%20role)%20VALUES%20(%27sqlinjector%27,%20%27sql@injector.com%27,%20%27injectTheSql%27,%204);%20--%20

                //string sql = 
                //    $"SELECT COUNT(i.id) " +
                //    $"FROM users u " +
                //    $"INNER JOIN images i " +
                //    $"ON u.id = i.author " +
                //    $"WHERE u.username = '{username}'";

                int count = 0;

                using (var connection = context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Parameters.Add(new MySqlParameter("@username", username));

                        count = Convert.ToInt32((long)await command.ExecuteScalarAsync());
                    }
                }

                return Ok(count);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (!UserExists(id))
            {
                Logger.Instance.Warning($"Someone tried fetching a non existing user {id}");
                return NotFound();
            }
            //Logger.Instance.Information($"Someone fetched user {id}");

            return Ok(new {id = user.Id, username = user.Username});
        }

        [Authorize]
        [UserOrAdmin]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] UserUpdate userUpdate)
        {
            var existingUser = await _db.Users.FindAsync(id);

            if(existingUser == null)
            {
                Logger.Instance.Warning("Unauthorized user tried editing someone who doesn't exist");
                return NotFound("User doesn't exist");
            }

            existingUser.Email = userUpdate.Email ?? existingUser.Email;
            existingUser.Username = userUpdate.Username ?? existingUser.Username;
            if (userUpdate.Role != null && existingUser.Role != userUpdate.Role)
            {
                _proxy.RequestRoleUpdate(id, (int)userUpdate.Role);
                Logger.Instance.Information($"User role change {existingUser.Role} -> {userUpdate.Role} transfered to proxy", id);
            }
            if (userUpdate.Password != null)
            {
                existingUser.Password = userUpdate.Password;
                _db.PasswordExpires.Find(id).ExpirationDate = DateTime.Now.AddDays(30);
            }

            //_db.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
                Logger.Instance.Information($"User updated to Email: {existingUser.Email}, Username: {existingUser.Username}", id);
            }
            catch (DbUpdateException ex)
            {
                Logger.Instance.Warning("User tried changing email or username to an already existing one", id);
                return BadRequest("Email or Username already exists.");
            }
            var updatedUser = _db.Users.Find(id);

            return Ok(new UserBuilder().Username(updatedUser.Username).Email(updatedUser.Email).Role(updatedUser.Role).Build());
        }

        [HttpPost]
        public async Task<ActionResult<Object>> PostUser(RegisterModel user)
        {
            if (await _db.Users.AnyAsync(x => x.Email == user.Email) || await _db.Users.AnyAsync(x => x.Username == user.Username))
            {
                Logger.Instance.Warning("User tried creating a new account with existing email or username!");
                return BadRequest("Email or username already in use");
            }
            else
            {
                _db.Users.Add(
                    new UserBuilder()
                        .Email(user.Email)
                        .Username(user.Username)
                        .Password(user.Password)
                        .Role(user.Role)
                        .Build()
                    );
                await _db.SaveChangesAsync();

                User newUser = _db.Users.Where(x => x.Email == user.Email).FirstOrDefault();

                newUser.RefreshToken = Jwt.GenerateJwtToken(newUser.Id, user.Role, "refresh");
                _db.UserPlanUsages.Add(new UserPlanUsage(newUser.Id));
                _db.PasswordExpires.Add(new PasswordExpire { Id = newUser.Id, ExpirationDate = DateTime.Now.AddDays(30) });
                await _db.SaveChangesAsync();
                Logger.Instance.Information("Created an account", newUser.Id);

                return new
                {
                    id = newUser.Id,
                    email = newUser.Email,
                    username = newUser.Username,
                    role = newUser.Role,
                    accessToken = Jwt.GenerateJwtToken(newUser.Id, newUser.Role),
                    refreshToken = newUser.RefreshToken,
                    userPlanUsage = 0,
                    expirationDate = DateTime.Now.AddDays(30)
                };
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (!UserExists(id))
            {
                Logger.Instance.Warning($"Can't delete user {id} if he doesn't exist");
                return NotFound();
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            Logger.Instance.Information("User deleted", id);

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _db.Users.Any(e => e.Id == id);
        }
    }
}
