using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.ActionFilters
{

    public class NoCacheResponseFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            // Set cache control headers
            response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            response.Cache.AppendCacheExtension("must-revalidate");
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // This method is not used for cache control
        }
    }
}