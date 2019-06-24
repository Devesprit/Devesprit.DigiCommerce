using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Services.Languages;

namespace Devesprit.WebFramework.ModelBinder
{
    public partial class LocalizedStringModelBinder: IModelBinder
    {
        public virtual object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;

            var shouldPerformRequestValidation = controllerContext.Controller.ValidateRequest && bindingContext.ModelMetadata.RequestValidationEnabled;
            var form = shouldPerformRequestValidation ? request.Form : request.Unvalidated.Form;

            var defaultValue = form.AllKeys.Contains(bindingContext.ModelName) ? form[bindingContext.ModelName] : "";
            var result = new LocalizedString {{0, defaultValue}};
            var langService = DependencyResolver.Current.GetService<ILanguagesService>();
            var languages = langService.GetAsEnumerable();
            foreach (var language in languages)
            {
                if (form.AllKeys.Contains(bindingContext.ModelName+"_"+language.Id))
                {
                    result.Add(language.Id, form[bindingContext.ModelName + "_" + language.Id]);
                }
                else
                {
                    result.Add(language.Id, "");
                }
            }

            return result;
        }
    }
}