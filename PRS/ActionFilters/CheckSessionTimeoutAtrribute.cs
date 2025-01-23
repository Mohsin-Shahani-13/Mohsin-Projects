using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.ActionFilters
{
    public class CheckSessionTimeoutAtrribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session != null)
            {
                var lastActivity = filterContext.HttpContext.Session["LastActivity"] as DateTime?;
                var sessionTimeout = filterContext.HttpContext.Session.Timeout; // Timeout in minutes

                if (lastActivity.HasValue && (DateTime.Now - lastActivity.Value).TotalMinutes > sessionTimeout)
                {
                    filterContext.HttpContext.Session.Abandon();
                    filterContext.Result = new RedirectResult("~/Home/Logout"); // Redirect to login page
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}