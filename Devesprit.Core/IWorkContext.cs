using Devesprit.Data.Domain;

namespace Devesprit.Core
{
    public partial interface IWorkContext
    {
        TblLanguages CurrentLanguage { get; }

        TblCurrencies CurrentCurrency { get; }
        
        TblUsers CurrentUser { get; }

        bool IsAdmin { get; }
    }
}
