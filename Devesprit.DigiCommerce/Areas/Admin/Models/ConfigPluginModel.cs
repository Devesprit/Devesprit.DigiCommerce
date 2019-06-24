using System.Web.Routing;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ConfigPluginModel
    {
        public string FriendlyName { get; set; }

        public string ConfigurationActionName { get; set; }
        public string ConfigurationControllerName { get; set; }
        public RouteValueDictionary ConfigurationRouteValues { get; set; }
    }
}