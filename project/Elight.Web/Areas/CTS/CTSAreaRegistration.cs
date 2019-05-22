using System.Web.Mvc;

namespace Elight.Web.Areas.CTS
{
    public class CTSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CTS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CTS_default",
                "CTS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}