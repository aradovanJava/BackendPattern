using Back.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Back.Attributes
{
    public class AuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authorizationHeader = context.HttpContext.Request.Headers["Authorization"];
            var token = authorizationHeader.ToString().Split(' ')[1];

            try
            {
                Jwt.GetPrincipalFromToken(token);
            }
            catch (Exception ex)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}
