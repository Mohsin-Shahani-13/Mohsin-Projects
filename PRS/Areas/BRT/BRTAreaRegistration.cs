using System.Web.Mvc;

namespace IP.Areas.BRT
{
    public class BRTAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BRT";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BRT_default",
                "BRT/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}