using System.Collections.Generic;

namespace Devesprit.Services.Widget
{
    public partial interface IWidgetService
    {
        IList<IWidgetPlugin> LoadWidgetsByWidgetZone(string widgetZone);

        IWidgetPlugin LoadWidgetBySystemName(string systemName);

        IList<IWidgetPlugin> LoadAllWidgets();
    }
}
