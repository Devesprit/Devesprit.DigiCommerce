using System.Linq;
using System.Web.Mvc;
using Devesprit.DigiCommerce.Factories.Interfaces;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class WidgetController : BaseController
    {
        private readonly IWidgetModelFactory _widgetModelFactory;

        public WidgetController(IWidgetModelFactory widgetModelFactory)
        {
            this._widgetModelFactory = widgetModelFactory;
        }

        [ChildActionOnly]
        public virtual ActionResult WidgetsByZone(string widgetZone, object additionalData = null)
        {
            var model = _widgetModelFactory.GetRenderWidgetModels(widgetZone, additionalData);

            //no data?
            if (!model.Any())
                return Content("");

            return PartialView("Partials/_WidgetsByZone", model);
        }
    }
}