using System;
using System.Globalization;
using System.Web.Mvc;

namespace Devesprit.WebFramework.ModelBinder
{
    public partial class DateTimeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var name = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(name);
            if (value == null)
                return null;

            if (DateTime.TryParse(value.AttemptedValue, null, DateTimeStyles.AdjustToUniversal, out var result))
                return result;
            else
                return base.BindModel(controllerContext, bindingContext);
        }
    }
}