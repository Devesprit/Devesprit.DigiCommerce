using System.ComponentModel;

namespace Devesprit.Data.Enums
{
    public enum ResponseType
    {
        [Description("Redirect")]
        Redirect,
        [Description("Rewrite")]
        Rewrite
    }
}