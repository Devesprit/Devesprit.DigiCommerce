using System.ComponentModel;

namespace Devesprit.Data.Enums
{
    public enum MatchType
    {
        [Description("Exact Match")]
        Exact,
        [Description("Regular Expressions")]
        Regex,
        [Description("Wildcards")]
        Wildcards
    }
}