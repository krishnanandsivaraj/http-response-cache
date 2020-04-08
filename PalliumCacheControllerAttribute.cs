using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Web.Api.Infrastructure.Repositories;

namespace Web.Api
{
    public class PalliumCacheControllerAttribute : ActionFilterAttribute
    {
        private readonly IRepositoryFacade _repositoryFacade;
        public IConfiguration Configuration { get; }

        public PalliumCacheControllerAttribute(IRepositoryFacade repositoryFacade)
        {
            this._repositoryFacade = repositoryFacade;
        }
        /// <summary>
        /// OnActionExecuting(ActionExecutingContext filterContext) - at the begining of a controller action 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string name = "api_name."+Convert.ToString(filterContext.RouteData.Values["controller"]) + "." + Convert.ToString(filterContext.RouteData.Values["action"]);

            //expiry logic should go here

            var cache = _repositoryFacade.GetCache(name);

            if (cache != null)
            {
                var json_serializer = JObject.Parse(cache);
                foreach (var property in json_serializer.Properties())
                {
                    if (property.Name == "Value") { filterContext.Result = new JsonResult(property.Value); }
                }
                
            }
        }

        /// <summary>
        /// Last change before a controller action returns its result to client 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string name = "api_name." + Convert.ToString(filterContext.RouteData.Values["controller"]) + "." + Convert.ToString(filterContext.RouteData.Values["action"]);
            
            _repositoryFacade.SetCache(
                 name, JsonConvert.SerializeObject(filterContext.Result),null);
        }
    }

}
