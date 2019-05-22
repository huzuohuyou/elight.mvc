using System.Web.Mvc;

namespace Elight.Web.Areas.TestCenterSystem
{
    public class TestCenterSystemAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TestCenterSystem";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TestCenterSystem_default",
                "TestCenterSystem/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}