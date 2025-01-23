using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.ActionFilters
{
    public class UpdateLastActivityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session != null && filterContext.HttpContext.Session["LastActivity"] == null)
            {
                filterContext.HttpContext.Session["LastActivity"] = DateTime.Now;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}