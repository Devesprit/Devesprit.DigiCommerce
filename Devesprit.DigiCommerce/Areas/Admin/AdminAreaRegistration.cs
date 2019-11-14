using System.Web.Mvc;

namespace Devesprit.DigiCommerce.Areas.Admin
{
    public partial class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get   
            {
                return "Admin"; 
            }
        }  

        public override void RegisterArea(AreaRegistrationContext context) 
        {  
            context.MapRoute(
                "Admin_defaultLocalized",
                "{lang}/Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new { lang = @"(\w{2})|(\w{2}-\w{2})" }   // en or en-US
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}