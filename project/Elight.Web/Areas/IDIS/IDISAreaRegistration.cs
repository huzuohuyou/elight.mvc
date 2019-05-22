using System.Web.Mvc;

namespace Elight.Web.Areas.IDIS
{
    public class IDISAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "IDIS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "IDIS_default",
                "IDIS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}