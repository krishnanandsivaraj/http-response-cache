using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Web.Api
{
    public class AFESAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.Response.Headers.Add("authToken", "");
            //throw new UnauthorizedAccessException(); //uncomment this to check for cache when unauthorized
            return;
        }

        
    }
}
