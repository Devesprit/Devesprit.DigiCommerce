using System.ComponentModel;

namespace Devesprit.Data.Enums
{
    public enum RedirectStatusCode
    {
        [Description("Moved Permanently (301)")]
        MovedPermanently = 301,
        [Description("Found (302)")]
        Found = 302,
        [Description("See Other (303)")]
        SeeOther = 303,
        [Description("Not Modified (304)")]
        NotModified = 304,
        [Description("Use Proxy (305)")]
        UseProxy = 305,
        [Description("Switch Proxy (306)")]
        SwitchProxy = 306,
        [Description("Temporary Redirect (307)")]
        TemporaryRedirect = 307,
        [Description("Permanent Redirect (308)")]
        PermanentRedirect = 308,
    }
}