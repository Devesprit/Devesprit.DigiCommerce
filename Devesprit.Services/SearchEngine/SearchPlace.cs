using System;
using System.ComponentModel;

namespace Devesprit.Services.SearchEngine
{
    [Flags]
    public enum SearchPlace
    {
        [Description("Anywhere")]
        Anywhere = 0,
        [Description("Title")]
        Title = 1,
        [Description("Description")]
        Description = 2,
        [Description("Tags")]
        Tags = 4,
        [Description("Keywords")]
        Keywords = 8
    }
}