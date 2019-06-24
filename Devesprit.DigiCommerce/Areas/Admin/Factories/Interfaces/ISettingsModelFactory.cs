using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface ISettingsModelFactory
    {
        SiteSettingModel PrepareSiteSettingModel(SiteSettings setting);
        SiteSettings PrepareSiteSettings(SiteSettingModel setting);
    }
}