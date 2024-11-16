using Back.Auth;
using front.Utils.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Back.Attributes
{
    public class UserOrAdminAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            int id = ExtractIdFromPath(context.HttpContext.Request.Path.ToString());

            string token = context.HttpContext.Request.Headers["Authorization"].ToString().Split(' ')[1];

            if (!Jwt.CheckIfSameUserOrAdmin(id, Jwt.GetPrincipalFromToken(token)))
            {
                Logger.Instance.Warning("Unauthorized user tried editing someone else");
                return;
            }
        }

        private int ExtractIdFromPath(string path)
        {
            var match = Regex.Match(path, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int id))
            {
                return id;
            }
            throw new ArgumentException("No valid ID found in the URL.");
        }
    }
}
