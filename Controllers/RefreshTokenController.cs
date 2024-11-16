using Back.Auth;
using Back.Db;
using Back.Models;
using front.Utils.Logger;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Controllers
{
    [Route("/refreshToken")]
    [ApiController]
    public class RefreshTokenController : ControllerBase
    {
        private readonly MySqlContext _db;

        public RefreshTokenController(MySqlContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateNewAccessToken(RefreshToken refreshToken)
        {
            try
            {
                var principal = Jwt.GetPrincipalFromToken(refreshToken.Token);
                var storedRefreshToken = _db.Users.Where(x => x.Id == int.Parse(principal.FindFirstValue("id"))).First().RefreshToken;
                if (storedRefreshToken == refreshToken.Token)
                {
                    Logger.Instance.Information($"Creating new JWT access token for {int.Parse(principal.FindFirstValue("id"))}");
                    return Ok(Jwt.GenerateJwtToken(int.Parse(principal.FindFirstValue("id")), int.Parse(principal.FindFirstValue(ClaimTypes.Role))));
                }
                else
                {
                    return BadRequest("Lose kompa");
                }
            }
            catch
            {
                return BadRequest("Lose kompa");
            }
        }
    }
}
