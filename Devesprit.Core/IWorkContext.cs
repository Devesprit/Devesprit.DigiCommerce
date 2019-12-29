using Devesprit.Data.Domain;

namespace Devesprit.Core
{
    public partial interface IWorkContext
    {
        TblLanguages CurrentLanguage { get; }

        TblCurrencies CurrentCurrency { get; }
        
        TblUsers CurrentUser { get; }

        bool IsAdmin { get; }

        bool UserHasPermission(string areaName);
        bool UserHasAllPermissions(params string[] areaNames);
        bool UserHasAtLeastOnePermission(params string[] areaNames);
    }
}
