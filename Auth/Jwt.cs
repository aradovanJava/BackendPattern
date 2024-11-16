using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Back.Auth
{
    public class Jwt
    {
        public static SecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superTajniSecretKeyKojegNitkoNikadaNecePogoditi"));
        }

        public static string GenerateJwtToken(int id, int role, string tokenType = "access")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = "mojFrontend",
                Claims = new Dictionary<string, object>(),
                Issuer = "mojBekend",
                Expires = tokenType == "access" ? DateTime.Now.AddMinutes(1) : DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity(new[] { new Claim("id", id.ToString()), new Claim(ClaimTypes.Role, role.ToString()) }),
            };

            var claim = new Claim("tokenType", tokenType == "access" ? "access" : "refresh");
            tokenDescriptor.Claims.Add(new KeyValuePair<string, object>(claim.Type, claim.Value));

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSymmetricSecurityKey(),
                ValidIssuer = "mojBekend",
                ValidAudience = "mojFrontend",
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var securityToken = tokenHandler.ReadToken(token);
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new Exception("Token expired");
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid token", ex);
            }
        }

        public static bool CheckIfSameUserOrAdmin(int id, ClaimsPrincipal token) =>
            token.IsInRole("4") || token.Claims.FirstOrDefault(c => c.Type == "id").Value == id.ToString();
    }
}
