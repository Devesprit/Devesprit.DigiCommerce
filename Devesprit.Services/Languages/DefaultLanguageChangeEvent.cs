using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Languages
{
    public partial class DefaultLanguageChangeEvent: IEvent
    {
        public TblLanguages Record { get; }

        public DefaultLanguageChangeEvent(TblLanguages record)
        {
            Record = record;
        }
    }
}