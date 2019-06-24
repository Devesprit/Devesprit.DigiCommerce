using System.Collections.Generic;
using Devesprit.DigiCommerce.Models.Widget;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface IWidgetModelFactory
    {
        List<RenderWidgetModel> GetRenderWidgetModels(string widgetZone, object additionalData = null);
    }
}
