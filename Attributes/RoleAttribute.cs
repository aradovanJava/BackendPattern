using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Attributes
{
    public class RoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public RoleAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var claimsPrincipal = context.HttpContext.User;

            if (!claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                context.Result = new ForbidResult(JwtBearerDefaults.AuthenticationScheme);
                return;
            }

            bool hasRole = _role.Any(role => claimsPrincipal.Claims.Any(c => c.Type == ClaimTypes.Role && RoleHierarchy.HasPrivilege(int.Parse(c.Value), _role)));

            if (!hasRole)
            {
                context.Result = new ForbidResult(JwtBearerDefaults.AuthenticationScheme);
            }
        }
    }

    public class RoleHierarchy
    {
        public static readonly string[] Roles = { "Normal", "Pro", "Gold", "Admin" };

        public static bool HasPrivilege(int userRole, string requiredRole)
        {
            return userRole >= Array.IndexOf(Roles, requiredRole) + 1;
        }
    }
}
