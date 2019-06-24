using System;
using System.Web;

namespace Devesprit.WebFramework.Localization
{
    public partial class LocalizedMVCString : MarshalByRefObject, IHtmlString
    {
        private readonly string _localized;

        public LocalizedMVCString(string localized)
        {
            _localized = localized;
        }

        public LocalizedMVCString(string localized, string scope, string textHint, object[] args)
        {
            _localized = localized;
            Scope = scope;
            TextHint = textHint;
            Args = args;
        }

        public static LocalizedMVCString TextOrDefault(string text, LocalizedMVCString defaultValue)
        {
            if (string.IsNullOrEmpty(text))
                return defaultValue;
            return new LocalizedMVCString(text);
        }

        public string Scope { get; }

        public string TextHint { get; }

        public object[] Args { get; }

        public string Text => _localized;

        public override string ToString()
        {
            return _localized;
        }

        public string ToHtmlString()
        {
            return _localized;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (_localized != null)
                hashCode ^= _localized.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var that = (LocalizedMVCString)obj;
            return string.Equals(_localized, that._localized);
        }

    }
}
